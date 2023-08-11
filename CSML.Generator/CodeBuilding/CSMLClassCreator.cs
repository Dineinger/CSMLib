﻿using CSML.Generator.SyntaxRepresentation;

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
}