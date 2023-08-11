using CSML.Compiler;
using CSML.Generator.CodeBuilding;
using CSML.Generator.SyntaxRepresentation;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace CSML.Generator.Compiler.FromAttribute;

internal class CSMLFromAttributeClassCreator
{
    internal static (string TypeName, string Code) CreateFinalCode(CSMLSyntaxTree syntaxTree)
    {
        StringBuilder sb = new();

        var info = syntaxTree.CSMLInfo;
        var typeToCreate = info.Metadata.TypeToCreate;

        var typesToCreateMembersFor = CSMLClassCreatorCommonTools.GetTypesToCreateMembersFor(syntaxTree);

        _ = sb.AppendLine(CodeSnippets.GENERATED_CODE_COMMENT_HEADER)
            .AppendLine()
            .Append("namespace ").Append(syntaxTree.CSMLInfo.Metadata.Namespace).AppendLine(";")
            .AppendLine()
            .AppendLine(CodeSnippets.GENERATED_CODE_ATTRIBUTE)
            .Append("public partial class ").Append(typeToCreate).Append(" : ICSMLClass<").Append(typeToCreate).AppendLine(">")
            .AppendLine("{")
            .AppendLine("    public readonly List<object> Children = new();");

        CSMLClassCreatorCommonTools.AppendMembersOfNamedTags(sb, typesToCreateMembersFor);

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
