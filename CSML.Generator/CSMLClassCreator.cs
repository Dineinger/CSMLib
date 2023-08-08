﻿using CSML.Compiler;
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
                .AppendLine("    public readonly List<object> Children = new();")
                .AppendLine()
                .Append("    public static ")
                .Append(typeToCreate)
                .AppendLine(" New()")
                .AppendLine("    {")
                .Append("        var self = new ").Append(typeToCreate).AppendLine("();");

            AppendSetupCode(sb, syntaxTree);

              _ = sb.AppendLine("        return self;")
                .AppendLine("    }")
                .AppendLine("}");

            result[a] = sb.ToString();
        }

        return result;
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
        var typeVar = "@_" + type + varCount;
        _ = sb.Append("        var ").Append(typeVar).Append(" = ").Append(type).AppendLine(".New();");
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

    public static string CreateFinalCode(string classesAsText)
    {
        return $$"""
// generated code
                    
/*******************************************************************************
* Generated by CSML Source Generator created by Dineinger                      *
* Auther: https://github.com/Dineinger                                         *
* Repo:https://github.com/Dineinger/CSMLib                                     *
*******************************************************************************/

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
