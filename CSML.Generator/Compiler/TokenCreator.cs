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
            if (addedToBuffer is BufferStatus.AddedToBufferAndKeepBuffer) {
                continue;
            }

            if (addedToBuffer is BufferStatus.AddAndClearBuffer && !String.IsNullOrWhiteSpace(buffer)) {
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

    private BufferStatus GetToken(char c, ref string buffer, out CSMLSyntaxToken tokenToAdd)
    {
        switch (c) {
            case (>= 'A' and <= 'Z') or (>= 'a' and <= 'z'):
                buffer += c; // TODO: remove allocations
                tokenToAdd = default;
                return BufferStatus.AddedToBufferAndKeepBuffer;
            case '"':
                if (String.IsNullOrEmpty(buffer)) {
                    buffer = "\"";
                    tokenToAdd = default;
                    return BufferStatus.AddedToBufferAndKeepBuffer;
                }

                buffer += "\"";
                tokenToAdd = default;
                return BufferStatus.AddAndClearBuffer;
            case '<': tokenToAdd = CSMLSyntaxToken.LessThan;
                return BufferStatus.AddAndClearBuffer;
            case '>': tokenToAdd = CSMLSyntaxToken.GreaterThan;
                return BufferStatus.AddAndClearBuffer;
            case '/': tokenToAdd = CSMLSyntaxToken.SlashToken;
                return BufferStatus.AddAndClearBuffer;
            case '\n' or '\r': tokenToAdd = CSMLSyntaxToken.EndOfLineTrivia;
                return BufferStatus.AddAndClearBuffer; // TODO: "\r\n" implementieren
            case ' ': tokenToAdd = CSMLSyntaxToken.WhitespaceTrivia;
                return BufferStatus.AddAndClearBuffer;
            case '#': tokenToAdd = CSMLSyntaxToken.Hashtag;
                return BufferStatus.AddAndClearBuffer;
            case '@': tokenToAdd = CSMLSyntaxToken.At;
                return BufferStatus.AddAndClearBuffer;
            case '=': tokenToAdd = CSMLSyntaxToken.EqualSign;
                return BufferStatus.AddAndClearBuffer;
            default: throw new NotImplementedException($"""Symbol not implemented when creating tokens: "{c}" """);
        }
    }
}

internal enum BufferStatus
{
    AddedToBufferAndKeepBuffer,
    AddAndClearBuffer
}