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

    public static string[] CreateClasses(CSMLRegistrationInfo[] registrationInfo)
    {
        var result = new string[registrationInfo.Length];
        for (var a = 0; a < registrationInfo.Length; a++)
        {
            var info = registrationInfo[a];
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
                .AppendLine("    {")
                .AppendLine("        return new();")
                .AppendLine("    }")
                .AppendLine("}");

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
