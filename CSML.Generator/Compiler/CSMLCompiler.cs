using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using CSML.Compiler.Syntax;
using CSML.Generator;
using CSML.Generator.Compiler;
using CSML.Generator.Compiler.SyntaxErrors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace CSML.Compiler;

public class CSMLCompiler
{
    private readonly SourceProductionContext _context;

    public CSMLCompiler(SourceProductionContext context)
    {
        _context = context;
    }

    public CSMLCompilation? GetSyntaxTrees(CSMLRegistrationInfo[] csmlCodes)
    {
        var success = GetSyntaxTreesUnverified(csmlCodes, out var syntaxTreesUnverified);
        if (success is false) {
            return null;
        }

        VerifySyntaxTrees(syntaxTreesUnverified);

        var syntaxTreesVerified = syntaxTreesUnverified;

        return new CSMLCompilation(syntaxTreesVerified.Select(x => x.CSMLSyntaxTree).ToImmutableArray());
    }

    private void VerifySyntaxTrees(ImmutableArray<(CSMLRegistrationInfo Info, CSMLSyntaxTree CSMLSyntaxTree)> syntaxTreesUnverified)
    {
        foreach (var syntaxTree in syntaxTreesUnverified) {
            if (VerifySyntaxTree(syntaxTree.CSMLSyntaxTree, out var syntaxError) == false) {
                _context.ReportDiagnostic(
                    Diagnostic.Create(
                        new DiagnosticDescriptor(
                            "CSML0",
                            "Syntax Error",
                            $"""
                            There was an error verifying the syntax: {syntaxError?.GetType()} with message: {syntaxError?.Message}"
                            """,
                            "CSML.SyntaxError",
                            DiagnosticSeverity.Error,
                            true),
                        Location.Create(syntaxTree.Info.SyntaxTree, syntaxTree.Info.CSMLCode.TextSpan))
                    );

            }
        }
    }

    private static bool VerifySyntaxTree(CSMLSyntaxTree syntaxTree, out SyntaxError? syntaxError)
    {
        Stack<CSMLSyntaxNode> openTags = new();

        var success = VerifySyntaxNodes(openTags, syntaxTree.RegistrationInfo, syntaxTree.GetRoot().DirectChildren, out var innerSyntaxError);

        if (success is false) {
            syntaxError = innerSyntaxError;
            return false;
        }

        syntaxError = null;
        return true;
    }

    private static bool VerifySyntaxNode(Stack<CSMLSyntaxNode> openTags, CSMLRegistrationInfo info, CSMLSyntaxNode node, out SyntaxError? syntaxError)
    {
        if (node is CSMLComponentOpeningSyntax componentOpeningSyntax) {
            if (componentOpeningSyntax.Type != info.TypeToCreate.ValueText) {
                syntaxError = new BadTypeSyntaxError($"""
                    The component level tag has a type that is different from the generic parameter provided by the C# code
                    """);
                return false;
            }

            openTags.Push(componentOpeningSyntax);
            syntaxError = null;
            return true;
        }

        if (node is TagOpeningSyntax tagOpening) {
            openTags.Push(tagOpening);
            syntaxError = null;
            return true;
        }

        if (node is TagClosingSyntax tagClosing) {
            if (openTags.Peek() is TagOpeningSyntax tagOpeningWhenTagClosing) {
                if (tagClosing.Type != tagOpeningWhenTagClosing.Type) {
                    syntaxError = new TypeOfOpenAndCloseTagDoNotMatchSyntaxError($"""
                        Opening tag type: {tagOpeningWhenTagClosing.Type} | Closing type: {tagClosing.Type}
                        """);
                    return false;
                }

                _ = openTags.Pop();
                syntaxError = null;
                return true;
            }

            if (openTags.Peek() is CSMLComponentOpeningSyntax componentOpeningWhenTagClosing) {
                if (tagClosing.Type != componentOpeningWhenTagClosing.Type) {
                    syntaxError = new TypeOfOpenAndCloseTagDoNotMatchSyntaxError($"""
                        Opening Component type: {componentOpeningWhenTagClosing.Type} | Closing type: {tagClosing.Type}
                        """);
                    return false;
                }

                _ = openTags.Pop();
                syntaxError = null;
                return true;
            }
            syntaxError = new ClosingTagUnableToCloseAnythingSyntaxError($"""
                        Last type on stack: {openTags.Peek()}
                        """);
            return false;
        }

        syntaxError = new UnknownSyntaxSyntaxError($"""
            While verifing syntax tree there was a syntax node which couldn't be verified: {node}
            """);
        return false;
    }

    private static bool VerifySyntaxNodes(Stack<CSMLSyntaxNode> openTags, CSMLRegistrationInfo info, IEnumerable<CSMLSyntaxNode> nodes, out SyntaxError? syntaxError)
    {
        foreach (var node in nodes) {
            var success = VerifySyntaxNode(openTags, info, node, out var innerSyntaxError);
            if (success is false) {
                syntaxError = innerSyntaxError;
                return false;
            }

            var childrenSuccess = VerifySyntaxNodes(openTags, info, node.DirectChildren, out var innerChildSyntaxError);
            if (childrenSuccess is false) {
                syntaxError = innerChildSyntaxError;
                return false;
            }
        }

        syntaxError = null;
        return true;
    }

    private static bool GetSyntaxTreeUnverified(CSMLRegistrationInfo info, out CSMLSyntaxTree? syntaxTree, out SyntaxError? syntaxError)
    {
        var code = info.CSMLCode.Value;
        var uncheckedTokens = GetUncheckedTokens(code);
        var tokens = new TokenQueue(uncheckedTokens);

        var builder = new SyntaxTreeBuilder(new SyntaxNodeBuilder(SyntaxNodeKind.CompilationUnit, new CSMLSyntaxToken[0]));

        var success = BuildSyntaxTreeUnverified(tokens, builder, out var innerSyntaxError);

        if (success is false) {
            syntaxTree = null;
            syntaxError = innerSyntaxError;
            return false;
        }

        syntaxError = null;
        syntaxTree = new CSMLSyntaxTree(builder.Build(), info);
        return true;
    }

    private static bool BuildSyntaxNodesFromTagTokens(SyntaxTreeBuilder builder, ReadOnlySpan<CSMLSyntaxToken> tokens, out SyntaxError? syntaxError)
    {
        var isOpeningSyntax = VerifyTokensFor_TagOpeningSyntax(tokens);
        if (isOpeningSyntax) {
            var verifiedTokens = tokens.ToArray();

            if (builder.Contains(SyntaxNodeKind.CSMLComponentOpeningSyntax)) {
                builder.InAndAdd(new SyntaxNodeBuilder(SyntaxNodeKind.TagOpeningSyntax, verifiedTokens));
                syntaxError = null;
                return true;
            }

            builder.InAndAdd(new SyntaxNodeBuilder(SyntaxNodeKind.CSMLComponentOpeningSyntax, verifiedTokens));
            syntaxError = null;
            return true;
        }

        var isClosingSyntax = VerifyTokensFor_TagClosingSyntax(tokens);
        if (isClosingSyntax) {
            var verifiedTokens = tokens.ToArray();
            builder.AddAndOut(new SyntaxNodeBuilder(SyntaxNodeKind.TagClosingSyntax, verifiedTokens));
            syntaxError = null;
            return true;
        }

        syntaxError = new TagSyntaxError($"""
            Tag does not fit any allowed syntax
            """);
        return false;
    }

    private static bool BuildSyntaxTreeUnverified(TokenQueue tokens, SyntaxTreeBuilder builder, out SyntaxError? syntaxError)
    {
        while (true) {
            if (tokens.IsAtEnd) {
                break;
            }

            if (tokens.IsNextOfKind(SyntaxType.LessThanToken)) {
                var success = BuildSyntaxNodesFromTagTokens(builder, tokens.GetUntilOrEndAndMove(SyntaxType.GreaterThanToken), out var innerSyntaxError);
                if (success == false) {
                    syntaxError = innerSyntaxError;
                    return false;
                }

                continue;
            }

            var isTrivia = tokens.IsNextAnyOfKinds(SyntaxType.EndOfLineTrivia, SyntaxType.WhitespaceTrivia, SyntaxType.EndOfFileTrivia);
            if (isTrivia) {
                if (tokens.MoveNext() == false) {
                    break;
                }

                continue;
            }

            syntaxError = new UnknownSyntaxSyntaxError($"""Not Implemented or allowed from token "{tokens.Next.SyntaxType}".""");
            return false;
        }

        syntaxError = null;
        return true;
    }

    private bool GetSyntaxTreesUnverified(CSMLRegistrationInfo[] csmlCodes, out ImmutableArray<(CSMLRegistrationInfo Info, CSMLSyntaxTree CSMLSyntaxTree)> syntaxTree)
    {
        List<(CSMLRegistrationInfo, CSMLSyntaxTree)> trees = new();
        foreach (var csmlSourceInfo in csmlCodes) {
            var success = GetSyntaxTreeUnverified(csmlSourceInfo, out var innerSyntaxTree, out var innerSyntaxError);
            if (success is false) {
                AddBadTokenDiagnostic(csmlSourceInfo, innerSyntaxError);
                return false;
            }

            trees.Add((csmlSourceInfo, innerSyntaxTree!));
        }

        syntaxTree = trees.ToImmutableArray();
        return true;
    }

    private void AddBadTokenDiagnostic(CSMLRegistrationInfo csmlSourceInfo, SyntaxError? syntaxError)
    {
        var syntaxTree = csmlSourceInfo.SyntaxTree;
        var CSMLCode = csmlSourceInfo.CSMLCode;
        var message = syntaxError is not null ?
            $"""
            There was an error verifying the syntax: {syntaxError?.GetType()} with message: {syntaxError?.Message}"
            """ :
            $"""
            The code reported an error, but the error object was null (Compiler error)
            """;
        _context.ReportDiagnostic(
            Diagnostic.Create(
                new DiagnosticDescriptor(
                    "CSML0",
                    "Syntax Error",
                    message,
                    "CSML.SyntaxError",
                    DiagnosticSeverity.Error,
                    true),
                Location.Create(syntaxTree, CSMLCode.TextSpan))
            );
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
}
