using System.Collections.Immutable;
using CSML.Generator.Compiler;
using CSML.Generator.SyntaxRepresentation;
using CSML.Generator.SyntaxRepresentation.SyntaxErrors;
using Microsoft.CodeAnalysis;

namespace CSML.Compiler;

internal class SyntaxTreeCreator
{
    private readonly SourceProductionContext _context;
    private readonly TokenCreator _tokenCreator;
    private readonly SyntaxTokenVerifiers _tokenVerifier;

    public SyntaxTreeCreator(SourceProductionContext context, TokenCreator tokenCreator, SyntaxTokenVerifiers tokenVerifier)
    {
        _context = context;
        _tokenCreator = tokenCreator;
        _tokenVerifier = tokenVerifier;
    }

    public bool GetSyntaxTreesUnverified(IReadOnlyList<CSMLInfo> info, out ImmutableArray<(CSMLInfo Info, CSMLSyntaxTree CSMLSyntaxTree)> syntaxTree)
    {
        List<(CSMLInfo, CSMLSyntaxTree)> trees = new();
        foreach (var csmlSourceInfo in info) {
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

    private void AddBadTokenDiagnostic(CSMLInfo csmlSourceInfo, SyntaxError? syntaxError)
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

    private bool GetSyntaxTreeUnverified(CSMLInfo info, out CSMLSyntaxTree? syntaxTree, out SyntaxError? syntaxError)
    {
        var code = info.CSMLCode.Value;
        var uncheckedTokens = _tokenCreator.GetUncheckedTokens(code);
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

    private bool BuildSyntaxTreeUnverified(TokenQueue tokens, SyntaxTreeBuilder builder, out SyntaxError? syntaxError)
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

    private bool BuildSyntaxNodesFromTagTokens(SyntaxTreeBuilder builder, ReadOnlySpan<CSMLSyntaxToken> tokens, out SyntaxError? syntaxError)
    {
        var selfClosingSyntaxError = _tokenVerifier.TagSelfClosingSyntax.Verify(tokens);
        if (selfClosingSyntaxError is null) {
            var verifiedTokens = tokens.ToArray();
            builder.AddAndStay(new SyntaxNodeBuilder(SyntaxNodeKind.TagSelfClosingSyntax, verifiedTokens));
            syntaxError = null;
            return true;
        }

        var openingSyntaxError = _tokenVerifier.TagOpeningSyntaxVerification.Verify(tokens);
        if (openingSyntaxError is null) {
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

        var closingSyntaxError = _tokenVerifier.TagClosingSyntax.Verify(tokens);
        if (closingSyntaxError is null) {
            var verifiedTokens = tokens.ToArray();
            builder.AddAndOut(new SyntaxNodeBuilder(SyntaxNodeKind.TagClosingSyntax, verifiedTokens));
            syntaxError = null;
            return true;
        }

        syntaxError =
            selfClosingSyntaxError
            ?? openingSyntaxError
            ?? closingSyntaxError
            ?? new TagSyntaxError($"""
                Tag does not fit any allowed syntax
                """);
        return false;
    }
}
