using Microsoft.CodeAnalysis;
using CSML.Compiler;
using CSML.Generator.CsharpAnalizer;
using CSML.Generator.CodeBuilding;
using CSML.Generator.SyntaxRepresentation;

namespace CSML.Generator;

[Generator]
public class CSMLGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(context.CompilationProvider,
            static (context, compilation) =>
            {
                DebugReporter.Clear();

                context.AddSource("CSMLBasics.generated.cs", CodeSnippets.BASIC_CODE);

                var compiler = new CSMLCompiler(context);

                var csmlCompilation = compiler.GetCompilation(compilation, out var syntaxError,
                        CSMLCsharpCodeAnalizers.Attribute,
                        CSMLCsharpCodeAnalizers.Translator
                    );

                if (csmlCompilation is null) {
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            new DiagnosticDescriptor(
                                "CSML0",
                                "Syntax Error",
                                $"""
                                {syntaxError?.Message}
                                """,
                                "CSML.SyntaxError",
                                DiagnosticSeverity.Error,
                                        true),
                                        Location.None)
                                        );
                    return;
                }

                var finalCodes = CSMLClassCreator.CreateClasses(csmlCompilation);

                foreach (var (TypeName, Code) in finalCodes) {
                    context.AddSource($"{TypeName}.generated.cs", Code);
                }
            });
    }
}
