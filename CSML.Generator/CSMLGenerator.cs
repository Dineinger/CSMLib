using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OneOf;
using OneOf.Types;
using System.Collections.Immutable;

namespace CSML.Generator;

[Generator]
public class CSMLGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(context.CompilationProvider,
            static (context, compilation) =>
            {
                // Analizing C# Code
                var translatorInvocation = CSMLCsharpCodeAnalizer.GetTranslatorInvocations(compilation);
                var typesToCreate = CSMLCsharpCodeAnalizer.GetTypesToCreate(translatorInvocation);
                var csmlCodes = GetCSMLCode(translatorInvocation);

                // Analizing CSML Code

                // Generate Code
                var classesAsTexts = CSMLClassCreator.CreateClasses(typesToCreate);
                var fromCases = CSMLClassCreator.CreateFromCases(typesToCreate);
                var setupMethods = CSMLClassCreator.CreateSetupMethods(typesToCreate);
                var classesAsText = string.Join("\n\n", classesAsTexts);

                var finalCode = CSMLClassCreator.CreateFinalCode(fromCases, setupMethods, classesAsText);
                context.AddSource("CSMLTranslator.generated.cs", finalCode);
            });
    }

    private static CSMLRawCode[] GetCSMLCode(ImmutableArray<InvocationExpressionSyntax> translatorInvocation)
    {
        CSMLRawCode[] result = new CSMLRawCode[translatorInvocation.Length];

        for (int i = 0; i < result.Length; i++)
        {
            var item = translatorInvocation[i];

            item.DescendantNodes();
        }

        return result;
    }
}

public record struct CSMLRawCode(string Value);
