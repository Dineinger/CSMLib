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

    public static SyntaxToken? GetGenericParameterSyntaxToken(MemberAccessExpressionSyntax maex)
    {
        var children = maex.DescendantNodes();
        var hasTranslatorClass = children.Any(x => x is IdentifierNameSyntax id && (id.Identifier.Text == CSML_TRANSLATOR_CLASS));

        if (hasTranslatorClass)
        {
            foreach (var child in children)
            {
                if (child is not GenericNameSyntax genericName)
                {
                    continue;
                }

                if (genericName.Identifier.Text != CSMLTRANSLATOR_FROM)
                {
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

    public static ImmutableArray<(SyntaxTree SyntaxTree, InvocationExpressionSyntax InvacationExpression)> GetTranslatorInvocations(Compilation compilation)
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

    public static CSMLRegistrationInfo[] GetInfoFromCSMLRegistration(ImmutableArray<(SyntaxTree SyntaxTree, InvocationExpressionSyntax InvocationExpression)> translatorInvocation)
    {
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
                            .Select(token => (string?)token.Value)
                            .Where(x => x is not null).Select(x => (x!, textSpan));
                    });
                if (codes.Count() != 1) {
                    throw new Exception("""There was more than one multi line raw string literal given.""");
                }

                var firstCode = codes.First();
                var code = new CSMLRawCode(firstCode.Item1, firstCode.textSpan);

                return new CSMLRegistrationInfo(ti.SyntaxTree, type, code);
            })
            .ToArray();
    }
}
