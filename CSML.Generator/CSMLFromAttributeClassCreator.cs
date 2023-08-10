﻿using CSML.Compiler;
using CSML.Compiler.Syntax;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace CSML.Generator;

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

internal class CSMLClassCreatorCommonTools
{
    public static TagOpeningSyntax[] GetTypesToCreateMembersFor(CSMLSyntaxTree syntaxTree)
    {
        return syntaxTree
            .GetRoot()
            .DescendingNodes()
            .OfType<TagOpeningSyntax>()
            .Where(x => x.Name is not null)
            .ToArray();
    }

    public static void AppendMembersOfNamedTags(StringBuilder sb, TagOpeningSyntax[] typeToCreateMembersFor)
    {
        _ = sb.AppendLine("    // generated by using #Identifier syntax inside an tag");
        foreach (var type in typeToCreateMembersFor) {
            _ = sb.Append("    public ").Append(type.Type).Append(" ").Append(type.Name).AppendLine(" { get; set; }")
                  .AppendLine();
        }
    }

    public static void AppendSetupCode(StringBuilder sb, CSMLSyntaxTree syntaxTree)
    {
        var root = syntaxTree.GetRoot();
        var varCount = 100;
        foreach (var node in root.DirectChildren) {
            if (node is CSMLComponentOpeningSyntax componentSyntax) {
                var children = componentSyntax.DirectChildren;
                foreach (var n in children) {
                    if (n is TagOpeningSyntax tag) {
                        AppendSetupCodeFromNode(sb, tag, "self", ref varCount);
                        continue;
                    }

                    if (n is TagClosingSyntax) {
                        break;
                    }

                    throw new NotImplementedException(n.ToString());
                }
            }
        }
    }

    private static void AppendSetupCodeFromNode(StringBuilder sb, TagOpeningSyntax syntaxNode, string lastCreatedNode, ref int varCount)
    {
        var type = syntaxNode.Type;
        string? typeVar;
        if (syntaxNode.Name is null) {
            typeVar = "@_" + type + varCount;
            _ = sb.Append("        var ").Append(typeVar).Append(" = ").Append(type).AppendLine(".New();");
        }
        else {
            typeVar = "self." + syntaxNode.Name;
            _ = sb.Append("        ").Append(typeVar).Append(" = ").Append(type).AppendLine(".New();");
        }

        foreach (var child in syntaxNode.DirectChildren) {
            if (child is TagOpeningSyntax tag) {
                varCount++;
                AppendSetupCodeFromNode(sb, tag, typeVar, ref varCount);
                continue;
            }

            if (child is TagClosingSyntax) {
                break;
            }

            throw new NotImplementedException($"node couldn't be turned into C# code: {child}");
        }

        _ = sb.Append("        ").Append(lastCreatedNode).Append(".Children.Add(").Append(typeVar).AppendLine(");");
    }
}