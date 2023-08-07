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
        var syntaxTreesUnverified = GetSyntaxTreesUnverified(csmlCodes);

        var syntaxTreesVerified = VerifySyntaxTrees(syntaxTreesUnverified);

        return new CSMLCompilation(syntaxTreesVerified);
    }

    private static IReadOnlyList<CSMLSyntaxTree> VerifySyntaxTrees(ImmutableArray<CSMLSyntaxTree> syntaxTreesUnverified)
    {
        return syntaxTreesUnverified;
    }

    private static CSMLSyntaxTree GetSyntaxTreeUnverified(CSMLRegistrationInfo info)
    {
        List<CSMLSyntaxNode> syntaxNodes = new();
        var code = info.CSMLCode.Value;

        var uncheckedTokens = GetUncheckedTokens(code);
        var tokens = new TokenQueue(uncheckedTokens);

        CreateAndAddSyntaxNodesFromTokens(syntaxNodes, tokens);

        return new CSMLSyntaxTree(new CSMLCompilationUnit(syntaxNodes));
    }

    private static void CreateAndAddSyntaxNodesFromTagTokens(List<CSMLSyntaxNode> syntaxNodes, ReadOnlySpan<CSMLSyntaxToken> tagSyntaxToken)
    {
        var isOpeningSyntax = VerifyTokensFor_TagOpeningSyntax(tagSyntaxToken);
        if (isOpeningSyntax) {
            var verifiedTokens = tagSyntaxToken.ToArray();

            if (syntaxNodes.Any(x => x is CSMLComponentOpeningSyntax)) {
                syntaxNodes.Add(new TagOpeningSyntax(verifiedTokens, (string)verifiedTokens[1].Value!));
                return;
            }

            var typeToken = verifiedTokens.First(x => x.SyntaxType == SyntaxType.Identifier);
            syntaxNodes.Add(new CSMLComponentOpeningSyntax(verifiedTokens, new List<CSMLSyntaxNode>(), (string)typeToken.Value!, "object"));
            return;
        }

        var isClosingSyntax = VerifyTokensFor_TagClosingSyntax(tagSyntaxToken);
        if (isClosingSyntax) {
            var verifiedTokens = tagSyntaxToken.ToArray();

            var typeTokens = verifiedTokens.First(x => x.SyntaxType == SyntaxType.Identifier);
            syntaxNodes.Add(new TagClosingSyntax(verifiedTokens, new List<CSMLSyntaxNode>(), (string)typeTokens.Value!));
        }

        return;
    }

    private static void CreateAndAddSyntaxNodesFromTokens(List<CSMLSyntaxNode> syntaxNodes, TokenQueue tokens)
    {
        while (true) {
            if (tokens.IsAtEnd) {
                break;
            }

            if (tokens.IsNextOfKind(SyntaxType.LessThanToken)) {
                CreateAndAddSyntaxNodesFromTagTokens(syntaxNodes, tokens.GetUntilOrEndAndMove(SyntaxType.GreaterThanToken));
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

    private static ImmutableArray<CSMLSyntaxTree> GetSyntaxTreesUnverified(CSMLRegistrationInfo[] csmlCodes)
    {
        List<CSMLSyntaxTree> trees = new();
        foreach (var csmlSourceInfo in csmlCodes) {
            trees.Add(GetSyntaxTreeUnverified(csmlSourceInfo));
        }

        return trees.ToImmutableArray();
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

    private static bool VerifyTokensFor_TagClosingSyntax(ReadOnlySpan<CSMLSyntaxToken> tokensUnchecked)
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
