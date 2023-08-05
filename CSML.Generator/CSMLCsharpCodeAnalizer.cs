using CSML.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace CSML.Generator;

internal static class CSMLCsharpCodeAnalizer
{
    private const string CSMLTranslatorClass = "CSMLTranslator";
    private const string CSMLTranslator_From = "From";

    public static SyntaxToken[] GetTypesToCreate(IEnumerable<InvocationExpressionSyntax> translatorInvocation)
    {
        return translatorInvocation
            .SelectMany(ti => ti
                .DescendantNodes()
                .OfType<MemberAccessExpressionSyntax>()
                .Select(maex =>
                {
                    return GetGenericParameterSyntaxToken(maex);
                })
                .Where(x => x.HasValue)
                .Select(x => x!.Value))
            .ToArray();
    }


    public static SyntaxToken? GetGenericParameterSyntaxToken(MemberAccessExpressionSyntax maex)
    {
        var children = maex.DescendantNodes();
        var hasTranslatorClass = children.Any(x => x is IdentifierNameSyntax id && (id.Identifier.Text == CSMLTranslatorClass));

        if (hasTranslatorClass)
        {
            foreach (var child in children)
            {
                if (child is not GenericNameSyntax genericName)
                {
                    continue;
                }

                if (genericName.Identifier.Text != CSMLTranslator_From)
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
                    .Any(id => id.Identifier.Text == CSMLTranslatorClass)
                )
            ).ToImmutableArray();
    }

    public static CSMLRawCode[] GetCSMLCode(ImmutableArray<InvocationExpressionSyntax> translatorInvocation)
    {
        CSMLRawCode[] result = new CSMLRawCode[translatorInvocation.Length];

        for (int i = 0; i < result.Length; i++)
        {
            var item = translatorInvocation[i];

            string[] code = item.DescendantNodes()
                .OfType<LiteralExpressionSyntax>()
                .SelectMany(les => les
                    .DescendantTokens()
                    .Where(token => token.IsKind(SyntaxKind.MultiLineRawStringLiteralToken))
                    .Select(token => (string?)token.Value)
                    .Where(x => x is not null).Select(x => x!)
                )
                .ToArray();

            if (code.Length != 1)
            {
                throw new InvalidOperationException("There was more than one multi line raw string literal token found. There is no string iterpolation supported yet.");
            }

            result[i] = new CSMLRawCode(code[0]);
        }

        return result;
    }
}
