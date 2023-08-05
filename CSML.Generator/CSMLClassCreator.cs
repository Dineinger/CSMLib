using Microsoft.CodeAnalysis;
using System.Text;

namespace CSML.Generator;

internal static class CSMLClassCreator
{
    public static string CreateSetupMethods(IEnumerable<SyntaxToken> typesToCreate)
    {
        StringBuilder sb = new();

        foreach (var type in typesToCreate)
        {
            sb.Append("    private static void Setup_");
            sb.Append(type.Text);
            sb.Append("(");
            sb.Append(type.Text);
            sb.AppendLine(" result)");
            sb.AppendLine("    {");
            sb.AppendLine("""        throw new NotImplementedException("Method 'From' is not implemented by the Generator yet.");""");
            sb.AppendLine("    }");
        }

        return sb.ToString();
    }

    public static string CreateFromCases(IEnumerable<SyntaxToken> typesToCreate)
    {
        StringBuilder sb = new();

        foreach (SyntaxToken t in typesToCreate)
        {
            sb.Append("            case ");
            sb.Append(t.Text);
            sb.Append(" x: Setup_");
            sb.Append(t.Text);
            sb.AppendLine("(x); break;");
        }

        return sb.ToString();
    }

    public static string[] CreateClasses(IList<SyntaxToken> typesToCreate)
    {
        var result = new string[typesToCreate.Count];
        for (var a = 0; a < typesToCreate.Count; a++)
        {
            var type = typesToCreate[a];
            StringBuilder sb = new();
            sb.AppendLine("/// generated because a method called CSMLTranslator.From was used");
            sb.Append("public sealed class ");
            sb.Append(type.Text);
            sb.Append(" : object, ICSMLClass<");
            sb.Append(type.Text);
            sb.AppendLine(">");
            sb.AppendLine("{");
            sb.Append("    public static ");
            sb.Append(type.Text);
            sb.AppendLine(" New()");
            sb.AppendLine("    {");
            sb.AppendLine("        return new();");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            result[a] = sb.ToString();
        }

        return result;
    }

    public static string CreateFinalCode(string fromCases, string setupMethods, string classesAsText, string debug)
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

                            switch (result)
                            {
                    {{fromCases}}
                                default: throw new InvalidOperationException();
                            }

                            return result;
                        }

                    {{setupMethods}}
                    }

                    {{classesAsText}}
                    """;
    }
}
