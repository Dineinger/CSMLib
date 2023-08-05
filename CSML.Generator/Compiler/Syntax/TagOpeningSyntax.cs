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
        return Array.Empty<CSMLSyntaxNode>();
    }
}
