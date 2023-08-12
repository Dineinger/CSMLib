using CSML.Generator.SyntaxRepresentation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace CSML.Generator.CsharpAnalizer;

internal class CSMLAttributeAnalizer : ICSMLCsharpCodeAnalizer
{
    private const string CSML_ATTRIBUTE_IDENTIFIER = "CSMLCode";

    public IReadOnlyList<CSMLInfo> GetCSMLInfo(Compilation compilation)
    {
        var attributes = GetClassesWithAttribute(compilation.SyntaxTrees);
        var infos = GetInfoFromAttributes(attributes);
        return infos;
    }

    private IReadOnlyList<CSMLInfo> GetInfoFromAttributes(IReadOnlyList<(ClassDeclarationSyntax Class, AttributeSyntax Attribute)> classes)
    {
        return classes
            .Select(c =>
            {
                var attributeSyntax = c.Attribute;
                var classSyntax = c.Class;
                var syntaxTree = classSyntax.SyntaxTree;

                var stringLiteralExpressions = attributeSyntax.DescendantNodes()
                    .OfType<ExpressionSyntax>()
                    .Where(x => x.IsKind(SyntaxKind.StringLiteralExpression));

                var needed = stringLiteralExpressions.First(); // optional/additional parameters should always came after

                var stringTokens = GetStringTokens(needed);

                if (stringTokens.Count() != 1) {
                    throw new InvalidOperationException("The C# code for the CSMLAttribute seems to be wrong or CSML Compiler bug");
                }

                var stringToken = stringTokens.First();
                var value = stringToken.ValueText;

                var typeToCreate = GetTypeToCreate(classSyntax);
                var @namespace = GetNamespace(syntaxTree);

                return new CSMLInfo(syntaxTree, new(typeToCreate, @namespace, CSMLSourceLocation.CSMLAttribute), new CSMLRawCode(value, stringToken, stringToken.Span));
            })
            .ToImmutableArray();
    }

    private string GetTypeToCreate(ClassDeclarationSyntax classSyntax)
    {
        return classSyntax.ChildTokens().First(x => x.IsKind(SyntaxKind.IdentifierToken)).ValueText;
    }

    private string GetNamespace(SyntaxTree syntaxTree)
    {
        return syntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<FileScopedNamespaceDeclarationSyntax>()
            .First() // only one in one file / syntax tree, other namespaces could have more than one block (?)
            .ChildNodes()
            .OfType<QualifiedNameSyntax>()
            .First()
            .GetText()
            .ToString();
    }

    private static IEnumerable<SyntaxToken> GetStringTokens(ExpressionSyntax needed)
    {
        return needed
            .DescendantTokens()
            .Where(token =>
                token.IsKind(SyntaxKind.StringLiteralToken)
                || token.IsKind(SyntaxKind.SingleLineRawStringLiteralToken)
                || token.IsKind(SyntaxKind.MultiLineRawStringLiteralToken));
    }

    private IReadOnlyList<(ClassDeclarationSyntax, AttributeSyntax)> GetClassesWithAttribute(IEnumerable<SyntaxTree> syntaxTrees)
    {
        var attributes = syntaxTrees
            .SelectMany(st => st
                .GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Where(x => x
                    .DescendantNodes()
                    .OfType<AttributeSyntax>()
                    .Any(attr => attr
                        .DescendantNodes()
                        .OfType<IdentifierNameSyntax>()
                        .Any(gns => gns.Identifier.ValueText == CSML_ATTRIBUTE_IDENTIFIER)
                    )
                )
                .Select(c => (c, c
                    .DescendantNodes()
                    .OfType<AttributeSyntax>()
                    .Where(attr => attr
                        .DescendantNodes()
                        .OfType<IdentifierNameSyntax>()
                        .Any(gns => gns.Identifier.ValueText == CSML_ATTRIBUTE_IDENTIFIER)
                    )
                    .First() // should always be one
                )
            )
        );

        return attributes.ToImmutableArray();
    }
}
