using CSML.Generator.SyntaxRepresentation;

namespace CSML.Compiler;

public class TokenCreator
{
    public CSMLSyntaxToken[] GetUncheckedTokens(string text)
    {
        List<CSMLSyntaxToken> tokens = new();

        string buffer = "";
        for (int a = 0; a < text.Length; a++) {
            var addedToBuffer = GetToken(text[a], ref buffer, out var tokenToAdd);
            if (addedToBuffer) {
                continue;
            }

            if (addedToBuffer is false && !String.IsNullOrWhiteSpace(buffer)) {
                tokens.Add(CSMLSyntaxToken.Identifier(buffer));
                buffer = "";
            }

            tokens.Add(tokenToAdd);
            continue;
        }

        if (!String.IsNullOrEmpty(buffer)) {
            tokens.Add(CSMLSyntaxToken.Identifier(buffer));
            buffer = "";
        }

        tokens.Add(CSMLSyntaxToken.EndOfFileTrivia);

        // debug
        var result = new CSMLSyntaxToken[tokens.Count];

        for (var i = 0; i < tokens.Count; i++) {
            var token = tokens[i];
            token.DebugIndex = i;
            result[i] = token;
        }

        return result.ToArray();
    }

    public bool GetToken(char c, ref string buffer, out CSMLSyntaxToken tokenToAdd)
    {
        switch (c) {
            case (>= 'A' and <= 'Z') or (>= 'a' and <= 'z'):
                buffer += c; // TODO: remove allocations
                tokenToAdd = default;
                return true;
            case '<': tokenToAdd = CSMLSyntaxToken.LessThan; return false;
            case '>': tokenToAdd = CSMLSyntaxToken.GreaterThan; return false;
            case '/': tokenToAdd = CSMLSyntaxToken.SlashToken; return false;
            case '\n' or '\r': tokenToAdd = CSMLSyntaxToken.EndOfLineTrivia; return false; // TODO: "\r\n" implementieren
            case ' ': tokenToAdd = CSMLSyntaxToken.WhitespaceTrivia; return false;
            case '#': tokenToAdd = CSMLSyntaxToken.Hashtag; return false;
            case '@': tokenToAdd = CSMLSyntaxToken.At; return false;
            default: throw new NotImplementedException($"""Symbol not implemented when creating tokens: "{c}" """);
        }
    }
}
