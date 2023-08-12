﻿using CSML.Generator.SyntaxRepresentation;
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

            var typesToCreateMembersFor = GetTypesToCreateMembersFor(syntaxTree);

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

            AppendMembersOfNamedTags(sb, typesToCreateMembersFor);

            _ = sb.AppendLine()
                .Append("    ").AppendLine(CodeSnippets.GENERATED_CODE_ATTRIBUTE)
                .Append("    public static ").Append(typeToCreate).AppendLine(" New()")
                .AppendLine("    {")
                .Append("        var self = new ").Append(typeToCreate).AppendLine("();");

            AppendSetupCode(sb, syntaxTree);

            _ = sb.AppendLine("        return self;")
                .AppendLine("    }")
                .AppendLine("}");

            result.Add(new FinalCode(typeToCreate, sb.ToString()));
        }

        return result;
    }

    private static TagOpeningSyntax[] GetTypesToCreateMembersFor(CSMLSyntaxTree syntaxTree)
    {
        return syntaxTree
            .GetRoot()
            .DescendingNodes()
            .OfType<TagOpeningSyntax>()
            .Where(x => x.Name is not null)
            .ToArray();
    }

    private static void AppendMembersOfNamedTags(StringBuilder sb, TagOpeningSyntax[] typeToCreateMembersFor)
    {
        _ = sb.AppendLine("    // generated by using #Identifier syntax inside an tag");
        foreach (var type in typeToCreateMembersFor) {
            _ = sb.Append("    private ").Append(type.Type).Append(" ").Append(type.Name).AppendLine(";")
                  .AppendLine();
        }
    }

    private static void AppendSetupCode(StringBuilder sb, CSMLSyntaxTree syntaxTree)
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

internal record struct FinalCode(string TypeName, string Code);

internal sealed class CSMLClassCreatorOptions
{
    public Accessibility TypeAccessibility { get; set; } = Accessibility.Internal;
    public bool IsSealed { get; set; } = true;
    public bool IsPartial { get; set; } = true;
}
