using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;
using CSML.Compiler;
using CSML.Compiler.Syntax;
using System.CodeDom.Compiler;

namespace CSML.Generator;

[Generator]
public class CSMLGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(context.CompilationProvider,
            static (context, compilation) =>
            {
                context.AddSource("CSMLBasics.generated.cs", CodeSnippets.CreateBasicCode());

                GenerateCodeForCSMLTranslator(context, compilation);
                GenerateCodeForCSMLAttribute(context, compilation);
            });
    }

    private static void GenerateCodeForCSMLAttribute(SourceProductionContext context, Compilation compilation)
    {
        var csmlInfo = CSMLCsharpCodeAnalizer.CSMLAttribute.GetCSMLInfo(compilation);

        var compiler = new CSMLCompiler(context);
        var csmlSyntaxTrees = compiler.GetCompilation(csmlInfo);

        if (csmlSyntaxTrees is null) {
            throw new NotImplementedException();
        }

        var finalCode = CSMLFromAttributeClassCreator.CreateFinalCode(csmlSyntaxTrees);

        foreach (var (TypeName, Code) in finalCode) {
            context.AddSource($"{TypeName}.generated.cs", Code);
        }
    }

    private static void GenerateCodeForCSMLTranslator(SourceProductionContext context, Compilation compilation)
    {
        // Analizing C# Code
        var translatorInvocation = CSMLCsharpCodeAnalizer.GetTranslatorInvocations(compilation);
        var csmlInvocationInfo = CSMLCsharpCodeAnalizer.GetInfoFromCSMLRegistration(translatorInvocation);

        // Analizing CSML Code
        var compiler = new CSMLCompiler(context);
        var csmlSyntaxTrees = compiler.GetCompilation(csmlInvocationInfo);

        if (csmlSyntaxTrees is null) {
            return;
        }

        // Generate Code
        var classesAsTexts = CSMLFromTranslatorClassCreator.CreateClasses(csmlSyntaxTrees);
        var classesAsText = String.Join("\n\n", classesAsTexts);

        var finalCode = CSMLFromTranslatorClassCreator.CreateFinalCode(classesAsText);
        context.AddSource("CSMLTranslator.generated.cs", finalCode);
    }
}
