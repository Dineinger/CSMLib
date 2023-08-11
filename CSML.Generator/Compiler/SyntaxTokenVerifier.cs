using CSML.Generator.CodeBuilding.SyntaxErrors;
using CSML.Generator.SyntaxRepresentation;

namespace CSML.Compiler;

internal class SyntaxTokenVerifier
{
    public bool VerifyTokensFor_TagOpeningSyntax(ReadOnlySpan<CSMLSyntaxToken> tokensUnchecked, out SyntaxError? syntaxError)
    {
        static SyntaxError errorBuilder(string text) => new UnknownSyntaxSyntaxError(text);

        if (tokensUnchecked[0].SyntaxType != SyntaxType.LessThanToken) {
            syntaxError = errorBuilder("trying verifying tag opening syntax: less than symbole missing");
            return false;
        }

        if (tokensUnchecked[1].SyntaxType != SyntaxType.Identifier) {
            syntaxError = errorBuilder("trying verifying tag opening syntax: identifier symbole missing");
            return false;
        }

        if (tokensUnchecked[2].SyntaxType != SyntaxType.GreaterThanToken) {
            if (tokensUnchecked[2].SyntaxType != SyntaxType.WhitespaceTrivia) {
                syntaxError = errorBuilder("trying verifying tag opening syntax: whitespace symbole missing");
                return false;
            }

            if (tokensUnchecked[3].SyntaxType != SyntaxType.Hashtag) {
                syntaxError = errorBuilder("trying verifying tag opening syntax: hashtag symbole missing");
                return false;
            }

            if (tokensUnchecked[4].SyntaxType != SyntaxType.Identifier) {
                syntaxError = errorBuilder("trying verifying tag opening syntax: identifier symbole for name missing");
                return false;
            }

            if (tokensUnchecked[5].SyntaxType != SyntaxType.GreaterThanToken) {
                syntaxError = errorBuilder("trying verifying tag opening syntax: greater than symbole missing");
                return false;
            }

            syntaxError = null;
            return true;
        }

        syntaxError = null;
        return true;
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