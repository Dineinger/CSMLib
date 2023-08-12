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
        var attributes = GetAttribute(compilation.SyntaxTrees);
        var infos = GetInfoFromAttributes(attributes);
        return infos;
    }

    private IReadOnlyList<CSMLInfo> GetInfoFromAttributes(IReadOnlyList<AttributeSyntax> attributes)
    {
        return attributes
            .Select(attr =>
            {
                var stringLiteralExpressions = attr.DescendantNodes()
                    .OfType<ExpressionSyntax>()
                    .Where(x => x.IsKind(SyntaxKind.StringLiteralExpression));

                var needed = stringLiteralExpressions.First(); // optional/additional parameters should always came after

                var stringTokens = GetStringTokens(needed);

                if (stringTokens.Count() != 1) {
                    throw new InvalidOperationException("The C# code for the CSMLAttribute seems to be wrong or CSML Compiler bug");
                }

                var stringToken = stringTokens.First();
                var value = stringToken.ValueText;

                var typeToCreate = GetTypeToCreate(attr);
                var @namespace = GetNamespace(attr.SyntaxTree);

                return new CSMLInfo(attr.SyntaxTree, new(typeToCreate, @namespace, CSMLSourceLocation.CSMLAttribute), new CSMLRawCode(value, stringToken, stringToken.Span));
            })
            .ToImmutableArray();
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

    private static string GetTypeToCreate(AttributeSyntax attr)
    {
        var typeToCreate = attr
            .ChildNodes()
            .OfType<GenericNameSyntax>()
            .First() // should be exactly one always
            .DescendantNodes()
            .OfType<TypeArgumentListSyntax>()
            .First() // should be exactly one always
            .DescendantNodes()
            .OfType<IdentifierNameSyntax>()
            .First()
            .Identifier
            .ValueText;
        return typeToCreate;
    }

    private IReadOnlyList<AttributeSyntax> GetAttribute(IEnumerable<SyntaxTree> syntaxTrees)
    {
        var attributes = syntaxTrees
            .SelectMany(st => st
                .GetRoot()
                .DescendantNodes()
                .OfType<AttributeSyntax>()
                .Where(attr => attr
                    .DescendantNodes()
                    .OfType<GenericNameSyntax>()
                    .Any(gns => gns.Identifier.ValueText == CSML_ATTRIBUTE_IDENTIFIER)));

        return attributes.ToImmutableArray();
    }
}
