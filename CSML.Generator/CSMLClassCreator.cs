using CSML.Compiler;

namespace CSML.Generator;

internal static class CSMLClassCreator
{
    public static IReadOnlyList<(string TypeName, string Code)> CreateClasses(params CSMLCompilation?[] compilations)
    {
        List<(string, string)> result = new ();

        foreach (var compilation in compilations) {
            if (compilation is null) { continue; }

            foreach (var syntaxTree in compilation.SyntaxTrees) {
                switch (syntaxTree.CSMLInfo.Metadata.From) {
                    case CSMLSourceLocation.CSMLTranslator:
                        result.Add(CSMLFromTranslatorClassCreator.CreateFinalCode(syntaxTree));
                        break;
                    case CSMLSourceLocation.CSMLAttribute:
                        result.Add(CSMLFromAttributeClassCreator.CreateFinalCode(syntaxTree));
                        break;
                    default: throw new NotImplementedException("code source not implemented when creating C# code");
                }
            }
        }

        return result;
    }
}
