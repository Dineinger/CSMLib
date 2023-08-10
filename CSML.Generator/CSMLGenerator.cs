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
using System.Dynamic;

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

                var csmlCompilation = GetCompilation(context, compilation,
                    x => CSMLCsharpCodeAnalizer.CSMLAttribute.GetCSMLInfo(compilation),
                    x =>
                    {
                        var translatorInvocation = CSMLCsharpCodeAnalizer.GetTranslatorInvocations(compilation);
                        var csmlInvocationInfo = CSMLCsharpCodeAnalizer.GetInfoFromCSMLRegistration(translatorInvocation);
                        return csmlInvocationInfo;
                    });

                var finalCode = CSMLClassCreator.CreateClasses(csmlCompilation);

                foreach (var (TypeName, Code) in finalCode) {
                    context.AddSource($"{TypeName}.generated.cs", Code);
                }
            });
    }

    private static CSMLCompilation? GetCompilation(SourceProductionContext context, Compilation compilation, params Func<Compilation, IReadOnlyList<CSMLInfo>>[] csmlGetter)
    {
        List<CSMLInfo> csmlInfos = new();

        foreach (var getter in csmlGetter)
        {
            csmlInfos.AddRange(getter(compilation));
        }

        var compiler = new CSMLCompiler(context);
        var csmlSyntaxTrees = compiler.GetCompilation(csmlInfos);
        return csmlSyntaxTrees;
    }
}
