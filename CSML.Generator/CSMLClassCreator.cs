using CSML.Compiler;
using CSML.Compiler.Syntax;
using Microsoft.CodeAnalysis;
using System.Text;

namespace CSML.Generator;

internal static class CSMLClassCreator
{
    [Obsolete("use New() for setup")]
    public static string CreateSetupMethods(CSMLRegistrationInfo[] registrationInfo)
    {
        StringBuilder sb = new();

        foreach (var info in registrationInfo)
        {
            var typeToCreate = info.TypeToCreate.Text;
            _ = sb.Append("    private static void Setup_")
                .Append(typeToCreate)
                .Append("(")
                .Append(typeToCreate)
                .AppendLine(" result)")
                .AppendLine("    {")
                .AppendLine("""        throw new NotImplementedException("Method 'From' is not implemented by the Generator yet.");""")
                .AppendLine("    }");
        }

        return sb.ToString();
    }

    public static string CreateFromCases(CSMLRegistrationInfo[] registrationInfo)
    {
        StringBuilder sb = new();

        foreach (var info in registrationInfo)
        {
            var typeToCreate = info.TypeToCreate.Text;
            _ = sb.Append("            case ")
                .Append(typeToCreate)
                .Append(" x: Setup_")
                .Append(typeToCreate)
                .AppendLine("(x); break;");
        }

        return sb.ToString();
    }

    public static string[] CreateClasses(CSMLCompilation compilation)
    {
        var syntaxTrees = compilation.SyntaxTrees;

        var result = new string[syntaxTrees.Count];
        for (var a = 0; a < syntaxTrees.Count; a++)
        {
            var syntaxTree = syntaxTrees[a];
            var info = syntaxTree.RegistrationInfo;
            var typeToCreate = info.TypeToCreate.Text;
            StringBuilder sb = new();
            _ = sb.AppendLine("/// generated because a method called CSMLTranslator.From was used")
                .Append("public sealed class ")
                .Append(typeToCreate)
                .Append(" : object, ICSMLClass<")
                .Append(typeToCreate)
                .AppendLine(">")
                .AppendLine("{")
                .Append("    public static ")
                .Append(typeToCreate)
                .AppendLine(" New()")
                .AppendLine("    {");
            AppendSetupCode(sb, syntaxTree);

              _ = sb.AppendLine("        return new();")
                .AppendLine("    }")
                .AppendLine("}");

            result[a] = sb.ToString();
        }

        return result;
    }

    private static void AppendSetupCode(StringBuilder sb, CSMLSyntaxTree syntaxTree)
    {
        var root = syntaxTree.GetRoot();
        _ = sb.Append("// syntax tree: ").AppendLine(String.Join("|", root.DescendingNodes()));
        foreach (var node in root.DirectChildren) {
            if (node is CSMLComponentOpeningSyntax componentSyntax) {
                var children = componentSyntax.DirectChildren;
                foreach (var n in children) {
                    _ = sb.Append("// n: ").AppendLine(n.GetType().ToString());
                    if (n is TagOpeningSyntax tag) {
                        _ = sb.Append("// adding type: ").Append(tag.Type);
                    }
                }
            }
        }
    }

    public static string CreateFinalCode(string classesAsText, string debug)
    {
        return $$"""
                    /*
                    {{debug}}
                    */

                    /// generated code
                    namespace CSML;

                    public interface ICSMLClass<T>
                        where T : ICSMLClass<T>
                    {
                        static abstract T New();
                    }

                    public class CSMLTranslator
                    {
                        public static T From<T>(string csml)
                            where T : ICSMLClass<T>
                        {
                            var result = T.New();

                            return result;
                        }
                    }

                    {{classesAsText}}
                    """;
    }
}
