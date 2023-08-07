using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using CSML.Compiler.Syntax;
using CSML.Generator;

namespace CSML.Compiler;

public class CSMLCompiler
{
    public static CSMLCompilation GetSyntaxTrees(CSMLRegistrationInfo[] csmlCodes)
    {
        List<CSMLSyntaxNode> syntaxNodes = new();

        foreach (var csmlSourceInfo in csmlCodes)
        {
            var requiredCsharpType = csmlSourceInfo.TypeToCreate;
            var code = csmlSourceInfo.CSMLCode.Value;

            var uncheckedTokens = GetUncheckedTokens(code);
            //var debug = string.Join("|", uncheckedTokens.Select(x => (x.SyntaxType.ToString(), x.Value?.ToString() ?? "null")));
            //throw new Exception(debug);
            var tokens = new TokenQueue(uncheckedTokens);

            while (true) {
                if (tokens.IsAtEnd) {
                    break;
                }

                if (tokens.IsNextOfKind(SyntaxType.LessThanToken)) {
                    var tagSyntaxToken = tokens.GetUntilOrEndAndMove(SyntaxType.GreaterThanToken);
                    //throw new Exception($"1: {tagSyntaxToken[0].SyntaxType}, 2: {tagSyntaxToken[1].SyntaxType}, 3: {tagSyntaxToken[2].SyntaxType}");

                    var verified = VerifyTokensFor_TagOpeningSyntax(tagSyntaxToken);
                    if (verified == false) {
                        //throw new InvalidOperationException("CSML seems to be wrong at an opening tag");
                        continue;
                    }

                    var verifiedTokens = tagSyntaxToken.ToArray();

                    if (syntaxNodes.Any(x => x is TagOpeningSyntax)) {
                        syntaxNodes.Add(new TagOpeningSyntax(verifiedTokens));
                        continue;
                    }

                    var typeToken = verifiedTokens.First(x => x.SyntaxType == SyntaxType.Identifier);
                    syntaxNodes.Add(new CSMLComponentOpeningSyntax(verifiedTokens, (string)typeToken.Value!, "object"));
                    continue;
                }

                var isTrivia = tokens.IsNextAnyOfKinds(SyntaxType.EndOfLineTrivia, SyntaxType.WhitespaceTrivia, SyntaxType.EndOfFileTrivia);
                if (isTrivia) {
                    if (tokens.MoveNext() == false) {
                        break;
                    }

                    continue;
                }

                throw new NotImplementedException($"""Not Implemented from token "{tokens.Next.SyntaxType}".""");
            }
        }

        return new CSMLCompilation(new List<CSMLSyntaxTree>()
        {
            new CSMLSyntaxTree(new CSMLCompilationUnit(syntaxNodes))
        });
    }

    private static CSMLSyntaxToken[] GetUncheckedTokens(string text)
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

        if (!string.IsNullOrEmpty(buffer)) {
            tokens.Add(CSMLSyntaxToken.Identifier(buffer));
            buffer = "";
        }

        tokens.Add(CSMLSyntaxToken.EndOfFileTrivia);

        // debug
        var result = new CSMLSyntaxToken[tokens.Count];

        for (var i = 0; i < tokens.Count; i++) {
            var token = tokens[i];
            token.debugIndex = i;
            result[i] = token;
        }

        return result.ToArray();
    }

    private static bool GetToken(char c, ref string buffer, out CSMLSyntaxToken tokenToAdd)
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
            default: throw new NotImplementedException($"""Symbol not implemented: "{c}" """);
        }
    }

    private static bool VerifyTokensFor_TagOpeningSyntax(ReadOnlySpan<CSMLSyntaxToken> tokensUnchecked)
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

    private static IReadOnlyList<string> GetCodeParts(string code)
    {
        var codeParts = new List<string>();

        var lastClosingTag = 0;
        for (int a = 0; a < code.Length; a++)
        {
            var c = code[a];

            if (c == '>')
            {
                var part = code.Substring(lastClosingTag, a - lastClosingTag + 1);
                codeParts.Add(part);
                lastClosingTag = a + 1;
            }
        }
        var lastPart = code.Substring(lastClosingTag, code.Length - lastClosingTag);
        codeParts.Add(lastPart);

        return codeParts.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
    }
}
