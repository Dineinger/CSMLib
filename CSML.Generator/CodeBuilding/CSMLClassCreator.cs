using CSML.Generator.SyntaxRepresentation;
using Microsoft.CodeAnalysis;
using System.Text;

namespace CSML.Generator.CodeBuilding;

internal static class CSMLClassCreator
{
    public static IReadOnlyList<(string TypeName, string Code)> CreateClasses(CSMLCompilation compilation, Func<CSMLSourceLocation, Func<CSMLSyntaxTree, (string, string)>> locations)
    {
        List<(string, string)> result = new();

        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            result.Add(locations(syntaxTree.CSMLInfo.Metadata.From)(syntaxTree));
        }

        return result;
    }

    public static IReadOnlyList<FinalCode> CreateClasses(CSMLCompilation compilation)
    {
        List<FinalCode> result = new();

        foreach (var syntaxTree in compilation.SyntaxTrees) {
            var info = syntaxTree.CSMLInfo;
            var typeToCreate = info.Metadata.TypeToCreate;

            var @namespace = syntaxTree.CSMLInfo.Metadata.Namespace;

            var typesToCreateMembersFor = CSMLClassCreatorCommonTools.GetTypesToCreateMembersFor(syntaxTree);

            StringBuilder sb = new();

            _ = sb.AppendLine(CodeSnippets.GENERATED_CODE_COMMENT_HEADER)
                .AppendLine()
                .Append("namespace ").Append(@namespace).AppendLine(";")
                .AppendLine()
                .AppendLine(CodeSnippets.GENERATED_CODE_ATTRIBUTE);

            _ = sb.Append("public ");
            _ = sb.Append("sealed ");
            _ = sb.Append("partial ");
            _ = sb.Append("class ").Append(typeToCreate).Append(" : ICSMLClass<").Append(typeToCreate).AppendLine(">")
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

            result.Add(new FinalCode(typeToCreate, sb.ToString()));
        }

        return result;
    }
}

internal record struct FinalCode(string TypeName, string Code);

internal sealed class CSMLClassCreatorOptions
{
    public Accessibility TypeAccessibility { get; set; } = Accessibility.Internal;
    public bool IsSealed { get; set; } = true;
    public bool IsPartial { get; set; } = true;
}
