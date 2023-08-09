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
                var csmlInvocationInfo = CSMLCsharpCodeAnalizer.GetInfoFromCSMLRegistration(translatorInvocation);

                // Analizing CSML Code
                var compiler = new CSMLCompiler(context);
                var csmlSyntaxTrees = compiler.GetCompilation(csmlInvocationInfo);

                if (csmlSyntaxTrees is null) {
                    return;
                }

                // Generate Code
                var classesAsTexts = CSMLClassCreator.CreateClasses(csmlSyntaxTrees);
                var classesAsText = String.Join("\n\n", classesAsTexts);

                var finalCode = CSMLClassCreator.CreateFinalCode(classesAsText);
                context.AddSource("CSMLTranslator.generated.cs", finalCode);
            });
    }
}
