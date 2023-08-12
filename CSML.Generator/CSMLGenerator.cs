using Microsoft.CodeAnalysis;
using CSML.Compiler;
using CSML.Generator.CsharpAnalizer;
using CSML.Generator.CodeBuilding;

namespace CSML.Generator;

[Generator]
public class CSMLGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(context.CompilationProvider,
            static (context, compilation) =>
            {
                context.AddSource("CSMLBasics.generated.cs", CodeSnippets.BASIC_CODE);

                var compiler = new CSMLCompiler(context);

                var csmlCompilation = compiler.GetCompilation(compilation,
                        CSMLCsharpCodeAnalizers.Attribute,
                        CSMLCsharpCodeAnalizers.Translator
                    )
                    ?? throw new NotImplementedException("compilation from CSML compiler was null");

                var finalCodes = CSMLClassCreator.CreateClasses(csmlCompilation);

                foreach (var (TypeName, Code) in finalCodes) {
                    context.AddSource($"{TypeName}.generated.cs", Code);
                }
            });
    }
}
