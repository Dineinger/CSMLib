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

        if (_triviaPolicy is TriviaPolicy.IgnoreAll && tokensUnchecked[0].IsTrivia) {
            return null;
        }

        return item.Kind switch
        {
            VerificationTokenItemKind.Item => VerifyItem(item, tokensUnchecked, syntaxNodeType),
            VerificationTokenItemKind.List => VerifyList(item, ref tokensUnchecked, syntaxNodeType),
            VerificationTokenItemKind.ListOfOption => VerifyListOfOption(item, ref tokensUnchecked, syntaxNodeType),
            _ => throw new NotImplementedException($"{nameof(VerificationTokenItemKind)} has an not implemented kind"),
        };
    }

    private SyntaxError? VerifyListOfOption(SyntaxNodeVerificationTokenItem item, ref ReadOnlySpan<CSMLSyntaxToken> tokensUnchecked, string syntaxNodeType)
    {
        foreach (var option in item.Options!) {
            var error = Verify(option, ref tokensUnchecked, syntaxNodeType);

            if (error is null) {
                return null;
            }
        }

        return new UnknownSyntaxSyntaxError($"$trying verifying {syntaxNodeType}: Not allowed symbol: {tokensUnchecked[0].SyntaxType}.");
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

    private static SyntaxError? VerifyItem(SyntaxNodeVerificationTokenItem item, ReadOnlySpan<CSMLSyntaxToken> tokensUnchecked, string syntaxNodeType)
    {
        var firstToken = tokensUnchecked[0];
        var requiredSyntaxType = item.SyntaxType!;
        if (firstToken.SyntaxType != requiredSyntaxType) {
            return new UnknownSyntaxSyntaxError($"trying verifying {syntaxNodeType}: {requiredSyntaxType} symbole missing.");
        }

        return null;
    }
}
