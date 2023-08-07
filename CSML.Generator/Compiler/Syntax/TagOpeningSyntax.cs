using System;
using System.Collections.Generic;

namespace CSML.Compiler.Syntax;

public class TagOpeningSyntax : CSMLSyntaxNode
{
    private readonly CSMLSyntaxToken[] _tokens;

    public string Type { get; }

    public TagOpeningSyntax(CSMLSyntaxToken[] tokens)
    {
        Type = (string)tokens[1].Value!;
        _tokens = tokens;
    }

    public override IReadOnlyList<CSMLSyntaxNode> DescendingNodes()
    {
        throw new NotImplementedException();
    }
}

public sealed class CSMLComponentOpeningSyntax : CSMLSyntaxNode
{
    private readonly CSMLSyntaxToken[] _tokens;
    private readonly string _type;
    private readonly string _baseType;

    public CSMLComponentOpeningSyntax(CSMLSyntaxToken[] tokens, string type, string baseType)
    {
        _tokens = tokens;
        _type = type;
        _baseType = baseType;
    }

    public override IReadOnlyList<CSMLSyntaxNode> DescendingNodes()
    {
        throw new NotImplementedException();
    }
}