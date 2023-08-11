using CSML.Compiler;
using CSML.Generator.SyntaxRepresentation;
using Microsoft.CodeAnalysis;
using System.Text;

namespace CSML.Generator.CodeBuilding.FromTranslator;

internal static class CSMLFromTranslatorClassCreator
{
    public static (string TypeName, string Code) CreateFinalCode(CSMLSyntaxTree syntaxTree)
    {
        var info = syntaxTree.CSMLInfo;
        var typeToCreate = info.Metadata.TypeToCreate;

        var typeToCreateMembersFor = CSMLClassCreatorCommonTools.GetTypesToCreateMembersFor(syntaxTree);

        StringBuilder sb = new();
        _ = sb
            .AppendLine(CodeSnippets.GENERATED_CODE_COMMENT_HEADER)
            .AppendLine()
            .AppendLine("namespace CSML;")
            .AppendLine()
            .AppendLine("/// generated because a method called CSMLTranslator.From was used")
            .AppendLine(CodeSnippets.GENERATED_CODE_ATTRIBUTE)
            .Append("public sealed class ").Append(typeToCreate).Append(" : object, ICSMLClass<").Append(typeToCreate).AppendLine(">")
            .AppendLine("{")
            .AppendLine("    public readonly List<object> Children = new();");

        CSMLClassCreatorCommonTools.AppendMembersOfNamedTags(sb, typeToCreateMembersFor);

        _ = sb.AppendLine()
            .Append("    ").AppendLine(CodeSnippets.GENERATED_CODE_ATTRIBUTE)
            .Append("    public static ").Append(typeToCreate).AppendLine(" New()")
            .AppendLine("    {")
            .Append("        var self = new ").Append(typeToCreate).AppendLine("();");

        CSMLClassCreatorCommonTools.AppendSetupCode(sb, syntaxTree);

        _ = sb.AppendLine("        return self;")
        .AppendLine("    }")
        .AppendLine("}");

        return (typeToCreate, sb.ToString());
    }
}
