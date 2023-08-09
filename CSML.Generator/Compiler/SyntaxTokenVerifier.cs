using CSML.Compiler.Syntax;

namespace CSML.Compiler;

public class SyntaxTokenVerifier
{
    public bool VerifyTokensFor_TagOpeningSyntax(ReadOnlySpan<CSMLSyntaxToken> tokensUnchecked)
    {

        if (tokensUnchecked[0].SyntaxType != SyntaxType.LessThanToken) {
            return false;
        }

        if (tokensUnchecked[1].SyntaxType != SyntaxType.Identifier) {
            return false;
        }

        if (tokensUnchecked[2].SyntaxType != SyntaxType.GreaterThanToken) {
            return false;
        }

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