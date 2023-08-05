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
                var typesToCreate = CSMLCsharpCodeAnalizer.GetTypesToCreate(translatorInvocation);
                //var csmlCodes = CSMLCsharpCodeAnalizer.GetCSMLCode(translatorInvocation);

                // Analizing CSML Code
                //var csmlSyntaxTrees = CSMLCompiler.GetSyntaxTrees(csmlCodes);

                // Generate Code
                var classesAsTexts = CSMLClassCreator.CreateClasses(typesToCreate);
                var fromCases = CSMLClassCreator.CreateFromCases(typesToCreate);
                var setupMethods = CSMLClassCreator.CreateSetupMethods(typesToCreate);
                var classesAsText = string.Join("\n\n", classesAsTexts);

                var debug = ""; //string.Join("\n\n", csmlSyntaxTrees
                //    .SyntaxTrees
                //    .First()
                //    .GetRoot()
                //    .DescendingNodes()
                //    .OfType<TagOpeningSyntax>()
                //    .Select(x => x.Type));

                var finalCode = CSMLClassCreator.CreateFinalCode(fromCases, setupMethods, classesAsText, debug);
                context.AddSource("CSMLTranslator.generated.cs", finalCode);
            });
    }
}
