using Microsoft.CodeAnalysis;
using CSML.Compiler;
using CSML.Generator.Compiler.FromAttribute;
using CSML.Generator.Compiler.FromTranslator;

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

                var compiler = new CSMLCompiler(context);

                var csmlCompilation = compiler.GetCompilation(compilation,
                    CSMLCsharpCodeAnalizer.Attribute.GetCSMLInfo,
                    CSMLCsharpCodeAnalizer.Translator.GetInfoFromCSMLRegistration);

                if (csmlCompilation is null) {
                    throw new NotImplementedException("compilation from CSML compiler was null");
                }

                var finalCode = CSMLClassCreator.CreateClasses(csmlCompilation,
                    from => from switch
                    {
                        CSMLSourceLocation.CSMLTranslator => CSMLFromTranslatorClassCreator.CreateFinalCode,
                        CSMLSourceLocation.CSMLAttribute => CSMLFromAttributeClassCreator.CreateFinalCode,
                        _ => throw new NotImplementedException("code source not implemented when creating C# code"),
                    });

                foreach (var (TypeName, Code) in finalCode) {
                    context.AddSource($"{TypeName}.generated.cs", Code);
                }
            });
    }
}
