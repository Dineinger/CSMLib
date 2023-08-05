using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CSML.Compiler.Syntax;

namespace CSML.Compiler;

public class CSMLCompiler
{
    private static readonly Regex _openTagSyntax = new(@"<[A-z]+>");

    public static CSMLCompilation GetSyntaxTrees(CSMLRawCode[] csmlCodes)
    {
        List<CSMLSyntaxNode> syntaxNodes = new();

        foreach (var csmlCode in csmlCodes)
        {
            var code = csmlCode.Value;

            var codeParts = GetCodeParts(code);

            foreach (var part in codeParts)
            {
                var match = _openTagSyntax.Match(part);

                if (match.Success)
                {
                    var openTagText = match.Value;

                    var tokensUnchecked = GetUncheckedTokens(openTagText);

                    var (Verified, CheckedTokens) = VerifyTokensFor_TagOpeningSyntax(tokensUnchecked);

                    if (Verified == false) {
                        //throw new InvalidOperationException("CSML seems to be wrong at an opening tag");
                        continue;
                    }
                    var tokens = CheckedTokens;

                    syntaxNodes.Add(new TagOpeningSyntax(tokens));
                }
            }
            //throw new Exception($"|||{code}|||");
        }

        return new CSMLCompilation(new List<CSMLSyntaxTree>()
        {
            new CSMLSyntaxTree(new CSMLCompilationUnit(syntaxNodes))
        });

        static CSMLSyntaxToken[] GetUncheckedTokens(string text)
        {
            List<CSMLSyntaxToken> tokens = new();

            string buffer = "";
            bool addedToBuffer = false;
            for (int a = 0; a < text.Length; a++)
            {
                addedToBuffer = false;

                switch (text[a])
                {
                    case '<': tokens.Add(CSMLSyntaxToken.LessThan); break;
                    case '>': tokens.Add(CSMLSyntaxToken.GreaterThan); break;
                    case (> 'A' and < 'Z') or (> 'a' and < 'z'):
                        addedToBuffer = true;
                        buffer += text[a]; // TODO: remove allocations
                        break;
                    default: throw new NotImplementedException();
                }

                if (addedToBuffer is false)
                {
                    tokens.Add(CSMLSyntaxToken.Literal(buffer));
                    buffer = "";
                }
            }

            if (!string.IsNullOrEmpty(buffer))
            {
                tokens.Add(CSMLSyntaxToken.Literal(buffer));
                buffer = "";
            }

            return tokens.ToArray();
        }
    }

    private static (bool Verified, CSMLSyntaxToken[] CheckedTokens) VerifyTokensFor_TagOpeningSyntax(CSMLSyntaxToken[] tokensUnchecked)
    {

        if (tokensUnchecked[0].SyntaxType != SyntaxType.LessThanToken) {
            return (false, new CSMLSyntaxToken[0]);
        }

        if (tokensUnchecked[1].SyntaxType != SyntaxType.Literal) {
            return (false, new CSMLSyntaxToken[0]);
        }

        if (tokensUnchecked[2].SyntaxType != SyntaxType.GreaterThanToken) {
            return (false, new CSMLSyntaxToken[0]);
        }

        var result = new CSMLSyntaxToken[]
        {
            tokensUnchecked[0],
            CSMLSyntaxToken.TypeToken((string)tokensUnchecked[1].Value!),
            tokensUnchecked[2],
        };

        return (true, result);
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

        return codeParts;
    }
}
