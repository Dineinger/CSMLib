using CSML.Generator;
using CSML.Generator.SyntaxRepresentation;
using CSML.Generator.SyntaxRepresentation.SyntaxErrors;

namespace CSML.Compiler;

internal sealed class SyntaxNodeVerification
{
    private readonly SyntaxNodeVerificationTokenItem _item;
    private readonly string _syntaxNodeType;
    private readonly TriviaPolicy _triviaPolicy;

    public SyntaxNodeVerification(SyntaxNodeVerificationTokenItem item, string syntaxNodeType, TriviaPolicy triviaPolicy = TriviaPolicy.IgnoreAll)
    {
        _item = item;
        _syntaxNodeType = syntaxNodeType;
        _triviaPolicy = triviaPolicy;
    }

    public SyntaxError? Verify(ReadOnlySpan<CSMLSyntaxToken> tokensUnchecked)
    {
        var error = Verify(_item, ref tokensUnchecked, _syntaxNodeType);

        return error;
    }

    private SyntaxError? Verify(SyntaxNodeVerificationTokenItem item, ref ReadOnlySpan<CSMLSyntaxToken> tokensUnchecked, string syntaxNodeType)
    {
        if (tokensUnchecked.Length == 0) {
            return new UnknownSyntaxSyntaxError("token list was empty but need more items");
        }

        return item.Kind switch
        {
            VerificationTokenItemKind.Item => VerifyItem(item, ref tokensUnchecked, syntaxNodeType),
            VerificationTokenItemKind.List => VerifyList(item, ref tokensUnchecked, syntaxNodeType),
            VerificationTokenItemKind.ListOfOption => VerifyListOfOption(item, ref tokensUnchecked, syntaxNodeType),
            _ => throw new NotImplementedException($"{nameof(VerificationTokenItemKind)} has an not implemented kind"),
        };
    }

    private SyntaxError? VerifyListOfOption(SyntaxNodeVerificationTokenItem item, ref ReadOnlySpan<CSMLSyntaxToken> tokensUnchecked, string syntaxNodeType)
    {
        SyntaxError? error  = null;
        foreach (var option in item.Options!) {
            error = Verify(option, ref tokensUnchecked, syntaxNodeType);

            if (error is null) {
                return null;
            }
        }

        var optionsPossible = String.Join(" | ", item.Options!.Select(x =>
        {
            return x.Kind switch
            {
                VerificationTokenItemKind.Item => "Item: " + x.SyntaxType,
                VerificationTokenItemKind.ListOfOption => "Option",
                VerificationTokenItemKind.List => "List",
                _ => throw new NotImplementedException(),
            };
        }));

        return new UnknownSyntaxSyntaxError($"trying verifying {syntaxNodeType}: Not allowed symbol: {tokensUnchecked[0].SyntaxType}. First option is: {optionsPossible}. ### Inner error: {error?.Message ?? "none"}");
    }

    private SyntaxError? VerifyList(SyntaxNodeVerificationTokenItem item, ref ReadOnlySpan<CSMLSyntaxToken> tokensUnchecked, string syntaxNodeType)
    {
        foreach (var listItem in item.List!) {
            if (tokensUnchecked.Length == 0) {
                return new UnknownSyntaxSyntaxError("token list had not enough items");
            }

            var error = Verify(listItem, ref tokensUnchecked, syntaxNodeType);

            if (error is not null) {
                return error;
            }

            if (tokensUnchecked.Length == 0) {
                tokensUnchecked = ReadOnlySpan<CSMLSyntaxToken>.Empty;
                continue;
            }

            tokensUnchecked = tokensUnchecked.Slice(1);
        }

        return null;
    }

    private SyntaxError? VerifyItem(SyntaxNodeVerificationTokenItem item, ref ReadOnlySpan<CSMLSyntaxToken> tokensUnchecked, string syntaxNodeType)
    {
        var requiredSyntaxType = item.SyntaxType!;

        while (tokensUnchecked[0].SyntaxType != requiredSyntaxType) {
            if (! (_triviaPolicy is TriviaPolicy.IgnoreAll && tokensUnchecked[0].IsTrivia)) {
                return new UnknownSyntaxSyntaxError($"trying verifying {syntaxNodeType}: {requiredSyntaxType} symbole missing.");
            }

            if (tokensUnchecked.Length <= 0) {
                return new UnknownSyntaxSyntaxError($"ran out of tokens verifying {_syntaxNodeType}.");
            }

            tokensUnchecked = tokensUnchecked.Slice(1);
        }

        return null;
    }
}
