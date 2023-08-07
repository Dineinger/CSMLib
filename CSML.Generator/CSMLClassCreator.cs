using Microsoft.CodeAnalysis;
using System.Text;

namespace CSML.Generator;

internal static class CSMLClassCreator
{
    public static string CreateSetupMethods(CSMLRegistrationInfo[] registrationInfo)
    {
        StringBuilder sb = new();

        foreach (var info in registrationInfo)
        {
            var typeToCreate = info.TypeToCreate.Text;
            sb.Append("    private static void Setup_");
            sb.Append(typeToCreate);
            sb.Append("(");
            sb.Append(typeToCreate);
            sb.AppendLine(" result)");
            sb.AppendLine("    {");
            sb.AppendLine("""        throw new NotImplementedException("Method 'From' is not implemented by the Generator yet.");""");
            sb.AppendLine("    }");
        }

        return sb.ToString();
    }

    public static string CreateFromCases(CSMLRegistrationInfo[] registrationInfo)
    {
        StringBuilder sb = new();

        foreach (var info in registrationInfo)
        {
            var typeToCreate = info.TypeToCreate.Text;
            sb.Append("            case ");
            sb.Append(typeToCreate);
            sb.Append(" x: Setup_");
            sb.Append(typeToCreate);
            sb.AppendLine("(x); break;");
        }

        return sb.ToString();
    }

    public static string[] CreateClasses(CSMLRegistrationInfo[] registrationInfo)
    {
        var result = new string[registrationInfo.Length];
        for (var a = 0; a < registrationInfo.Length; a++)
        {
            var info = registrationInfo[a];
            var typeToCreate = info.TypeToCreate.Text;
            StringBuilder sb = new();
            sb.AppendLine("/// generated because a method called CSMLTranslator.From was used");
            sb.Append("public sealed class ");
            sb.Append(typeToCreate);
            sb.Append(" : object, ICSMLClass<");
            sb.Append(typeToCreate);
            sb.AppendLine(">");
            sb.AppendLine("{");
            sb.Append("    public static ");
            sb.Append(typeToCreate);
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
