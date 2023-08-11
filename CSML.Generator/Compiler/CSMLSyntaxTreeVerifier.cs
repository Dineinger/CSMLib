using System.Collections.Immutable;
using CSML.Generator.CodeBuilding.SyntaxErrors;
using CSML.Generator.SyntaxRepresentation;
using Microsoft.CodeAnalysis;

namespace CSML.Compiler;

public class CSMLSyntaxTreeVerifier
{
    private readonly SourceProductionContext _context;

    public CSMLSyntaxTreeVerifier(SourceProductionContext context)
    {
        _context = context;
    }

    public void VerifySyntaxTrees(ImmutableArray<(CSMLInfo Info, CSMLSyntaxTree CSMLSyntaxTree)> syntaxTreesUnverified)
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

        var success = VerifySyntaxNodes(openTags, syntaxTree.CSMLInfo, syntaxTree.GetRoot().DirectChildren, out var innerSyntaxError);

        if (success is false) {
            syntaxError = innerSyntaxError;
            return false;
        }

        syntaxError = null;
        return true;
    }

    private static bool VerifySyntaxNodes(Stack<CSMLSyntaxNode> openTags, CSMLInfo info, IEnumerable<CSMLSyntaxNode> nodes, out SyntaxError? syntaxError)
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

    private static bool VerifySyntaxNode(Stack<CSMLSyntaxNode> openTags, CSMLInfo info, CSMLSyntaxNode node, out SyntaxError? syntaxError)
    {
        if (node is CSMLComponentOpeningSyntax componentOpeningSyntax) {
            if (componentOpeningSyntax.Type != info.Metadata.TypeToCreate) {
                syntaxError = new BadTypeSyntaxError($"""
                    The component level tag has a type that is different from the generic parameter provided by the C# code: string: {componentOpeningSyntax.Type}, C#: {info.Metadata.TypeToCreate}
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
}
