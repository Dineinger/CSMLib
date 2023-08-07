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

    public static ImmutableArray<InvocationExpressionSyntax> GetTranslatorInvocations(Compilation compilation)
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
            ).ToImmutableArray();
    }

    public static CSMLRegistrationInfo[] GetInfoFromCSMLRegistration(ImmutableArray<InvocationExpressionSyntax> translatorInvocation)
    {
        return translatorInvocation
            .Select(ti =>
            {
                var types = ti
                    .DescendantNodes()
                    .OfType<MemberAccessExpressionSyntax>()
                    .Select(maex => GetGenericParameterSyntaxToken(maex))
                    .Where(x => x.HasValue)
                    .Select(x => x!.Value);
                if (types.Count() != 1) {
                    throw new Exception("""There was more than one generic type given.""");
                }
                var type = types.First();

                var codes = ti
                    .DescendantNodes()
                    .OfType<LiteralExpressionSyntax>()
                    .SelectMany(les => les
                        .DescendantTokens()
                        .Where(token => token.IsKind(SyntaxKind.MultiLineRawStringLiteralToken))
                        .Select(token => (string?)token.Value)
                        .Where(x => x is not null).Select(x => x!)
                    );
                if (codes.Count() != 1) {
                    throw new Exception("""There was more than one multi line raw string literal given.""");
                }

                var code = new CSMLRawCode(codes.First());

                return new CSMLRegistrationInfo(type, code);
            })
            .ToArray();
    }
}
