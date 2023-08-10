using CSML.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace CSML.Generator;

internal static class CSMLCsharpCodeAnalizer
{
    private const string CSML_TRANSLATOR_CLASS = "CSMLTranslator";
    private const string CSMLTRANSLATOR_FROM = "From";

    public readonly static CSMLAttributeAnalizer Attribute = new();
    public readonly static CSMLTranslatorAnalizer Translator = new();

    public class CSMLTranslatorAnalizer
    {
        private static SyntaxToken? GetGenericParameterSyntaxToken(MemberAccessExpressionSyntax maex)
        {
            var children = maex.DescendantNodes();
            var hasTranslatorClass = children.Any(x => x is IdentifierNameSyntax id && (id.Identifier.Text == CSML_TRANSLATOR_CLASS));

            if (hasTranslatorClass) {
                foreach (var child in children) {
                    if (child is not GenericNameSyntax genericName) {
                        continue;
                    }

                    if (genericName.Identifier.Text != CSMLTRANSLATOR_FROM) {
                        continue;
                    }

                    return genericName.DescendantNodes()
                        .OfType<TypeArgumentListSyntax>()
                        .Select(tal => tal
                        .DescendantNodes()
                        .OfType<IdentifierNameSyntax>()
                        .Select(id => id.Identifier).First()).First();
                }
            }

            return null;
        }

        public ImmutableArray<(SyntaxTree SyntaxTree, InvocationExpressionSyntax InvocationExpression)> GetTranslatorInvocations(Compilation compilation)
        {
            return compilation.SyntaxTrees
                .SelectMany(st => st
                    .GetRoot()
                    .DescendantNodes()
                    .OfType<InvocationExpressionSyntax>()
                    .Where(invocationExp => invocationExp
                        .DescendantNodes()
                        .OfType<IdentifierNameSyntax>()
                        .Any(id => id.Identifier.Text == CSML_TRANSLATOR_CLASS)
                    )
                    .Select(x => (st, x))
                )
                .ToImmutableArray();
        }

        public IReadOnlyList<CSMLInfo> GetInfoFromCSMLRegistration(Compilation compilation)
        {
            var translatorInvocation = GetTranslatorInvocations(compilation);

            return translatorInvocation
                .Select(ti =>
                {
                    var types = ti.InvocationExpression
                        .DescendantNodes()
                        .OfType<MemberAccessExpressionSyntax>()
                        .Select(maex => GetGenericParameterSyntaxToken(maex))
                        .Where(x => x.HasValue)
                        .Select(x => x!.Value);
                    if (types.Count() != 1) {
                        throw new Exception("""There was more than one generic type given.""");
                    }

                    var type = types.First();

                    var codes = ti.InvocationExpression
                        .DescendantNodes()
                        .OfType<LiteralExpressionSyntax>()
                        .SelectMany(les =>
                        {
                            var textSpan = les.FullSpan;
                            return les
                                .DescendantTokens()
                                .Where(token => token.IsKind(SyntaxKind.MultiLineRawStringLiteralToken))
                                .Select(token => ((string?)token.Value, token, token.Span))
                                .Where(x => x.Item1 is not null).Select(x => (x.Item1!, x.token, x.Span));
                        });
                    if (codes.Count() != 1) {
                        throw new Exception("""There was more than one multi line raw string literal given.""");
                    }

                    var firstCode = codes.First();
                    var code = new CSMLRawCode(firstCode.Item1, firstCode.token, firstCode.Span);

                    var syntaxTree = ti.SyntaxTree;
                    var @namespace = syntaxTree
                        .GetRoot()
                        .DescendantNodes()
                        .OfType<FileScopedNamespaceDeclarationSyntax>()
                        .First()
                        .DescendantTokens()
                        .Where(x => x.IsKind(SyntaxKind.IdentifierToken))
                        .Select(x => x.ValueText)
                        .First();
                    return new CSMLInfo(syntaxTree, new(type.ValueText, @namespace, CSMLSourceLocation.CSMLTranslator), code);
                })
                .ToArray();
        }
    }

    public class CSMLAttributeAnalizer
    {
        private const string CSML_ATTRIBUTE_IDENTIFIER = "CSMLCode";

        internal IReadOnlyList<CSMLInfo> GetCSMLInfo(Compilation compilation)
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

                    var stringTokens = needed
                        .DescendantTokens()
                        .Where(token =>
                            token.IsKind(SyntaxKind.StringLiteralToken)
                            || token.IsKind(SyntaxKind.SingleLineRawStringLiteralToken)
                            || token.IsKind(SyntaxKind.MultiLineRawStringLiteralToken));

                    if (stringTokens.Count() != 1) {
                        throw new InvalidOperationException("The C# code for the CSMLAttribute seems to be wrong or CSML Compiler bug");
                    }

                    var stringToken = stringTokens.First();
                    var value = stringToken.ValueText;

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

                    return new CSMLInfo(attr.SyntaxTree, new(typeToCreate, "CSML.Tests" /* TODO: Namespace*/, CSMLSourceLocation.CSMLAttribute), new CSMLRawCode(value, stringToken, stringToken.Span));
                })
                .ToImmutableArray();
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
}
