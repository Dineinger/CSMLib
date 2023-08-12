using CSML.Generator.SyntaxRepresentation;
using CSML.Generator.SyntaxRepresentation.SyntaxErrors;
using System.Data.Common;
using System.Linq.Expressions;

namespace CSML.Compiler;

internal sealed class SyntaxNodeVerification
{
    private readonly SyntaxNodeVerificationTokenItem _item;
    private readonly string _syntaxNodeType;

    public SyntaxNodeVerification(SyntaxNodeVerificationTokenItem item, string syntaxNodeType)
    {
        _item = item;
        _syntaxNodeType = syntaxNodeType;
    }

    public SyntaxError? Verify(ReadOnlySpan<CSMLSyntaxToken> tokensUnchecked)
    {
        var error = Verify(_item, ref tokensUnchecked, _syntaxNodeType);

        return error;
    }

    private static SyntaxError? Verify(SyntaxNodeVerificationTokenItem item, ref ReadOnlySpan<CSMLSyntaxToken> tokensUnchecked, string syntaxNodeType)
    {
        if (tokensUnchecked.Length == 0) {
            return new UnknownSyntaxSyntaxError("token list was empty but need more items");
        }

        switch (item.Kind) {
            case VerificationTokenItemKind.Item:
                var firstToken = tokensUnchecked[0];
                var requiredSyntaxType = item.SyntaxType!;
                if (firstToken.SyntaxType != requiredSyntaxType) {
                    return new UnknownSyntaxSyntaxError($"trying verifying {syntaxNodeType}: {requiredSyntaxType} symbole missing.");
                }

                return null;
            case VerificationTokenItemKind.List:
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

            case VerificationTokenItemKind.ListOfOption:
                foreach (var option in item.Options!) {
                    var error = Verify(option, ref tokensUnchecked, syntaxNodeType);

                    if (error is null) {
                        return null;
                    }
                }

                return new UnknownSyntaxSyntaxError($"$trying verifying {syntaxNodeType}: Not allowed symbol: {tokensUnchecked[0].SyntaxType}.");

            default:
                throw new NotImplementedException($"{nameof(VerificationTokenItemKind)} has an not implemented kind");
        }
    }
}

internal enum VerificationTokenItemKind
{
    Item,
    ListOfOption,
    List,
}

internal class SyntaxTokenVerifier
{
    SyntaxNodeVerification tagOpeningSyntaxVerification = new (SyntaxNodeVerificationTokenItem.FromList(x => x
        .Add(SyntaxType.LessThanToken)
        .Add(SyntaxType.Identifier)
        .AddOption(x => x
            .Add(SyntaxType.GreaterThanToken)
            .AddList(x => x
                .Add(SyntaxType.WhitespaceTrivia)
                .Add(SyntaxType.Hashtag)
                .Add(SyntaxType.Identifier)
                .Add(SyntaxType.GreaterThanToken)
            )
        )
    ),
    syntaxNodeType: "tag opening syntax");

    public bool VerifyTokensFor_TagOpeningSyntax(ReadOnlySpan<CSMLSyntaxToken> tokensUnchecked, out SyntaxError? syntaxError)
    {
        var error = tagOpeningSyntaxVerification.Verify(tokensUnchecked);

        if (error is not null) {
            syntaxError = error;
            return false;
        }

        syntaxError = null;
        return true;

        //static SyntaxError errorBuilder(string text) => new UnknownSyntaxSyntaxError(text);

        //if (tokensUnchecked[0].SyntaxType != SyntaxType.LessThanToken) {
        //    syntaxError = errorBuilder("trying verifying tag opening syntax: less than symbole missing");
        //    return false;
        //}

        //if (tokensUnchecked[1].SyntaxType != SyntaxType.Identifier) {
        //    syntaxError = errorBuilder("trying verifying tag opening syntax: identifier symbole missing");
        //    return false;
        //}

        //if (tokensUnchecked[2].SyntaxType != SyntaxType.GreaterThanToken) {
        //    if (tokensUnchecked[2].SyntaxType != SyntaxType.WhitespaceTrivia) {
        //        syntaxError = errorBuilder("trying verifying tag opening syntax: whitespace symbole missing");
        //        return false;
        //    }

        //    if (tokensUnchecked[3].SyntaxType != SyntaxType.Hashtag) {
        //        syntaxError = errorBuilder("trying verifying tag opening syntax: hashtag symbole missing");
        //        return false;
        //    }

        //    if (tokensUnchecked[4].SyntaxType != SyntaxType.Identifier) {
        //        syntaxError = errorBuilder("trying verifying tag opening syntax: identifier symbole for name missing");
        //        return false;
        //    }

        //    if (tokensUnchecked[5].SyntaxType != SyntaxType.GreaterThanToken) {
        //        syntaxError = errorBuilder("trying verifying tag opening syntax: greater than symbole missing");
        //        return false;
        //    }

        //    syntaxError = null;
        //    return true;
        //}

        //syntaxError = null;
        //return true;
    }

    public bool VerifyTokensFor_TagClosingSyntax(ReadOnlySpan<CSMLSyntaxToken> tokensUnchecked)
    {

        if (tokensUnchecked[0].SyntaxType != SyntaxType.LessThanToken) {
            return false;
        }

        if (tokensUnchecked[1].SyntaxType != SyntaxType.SlashToken) {
            return false;
        }

        if (tokensUnchecked[2].SyntaxType != SyntaxType.Identifier) {
            return false;
        }

        if (tokensUnchecked[3].SyntaxType != SyntaxType.GreaterThanToken) {
            return false;
        }

        return true;
    }
}