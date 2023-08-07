using System;
using System.Collections.Generic;

namespace CSML.Compiler.Syntax;

public sealed class TagOpeningSyntax : CSMLSyntaxNode
{
    private readonly CSMLSyntaxToken[] _tokens;
    private readonly List<CSMLSyntaxNode> _directChildren = new();

    public override IEnumerable<CSMLSyntaxNode> DirectChildren => _directChildren;

    public string Type { get; }

    public TagOpeningSyntax(CSMLSyntaxToken[] tokens, string type)
    {
        Type = type;
        _tokens = tokens;
    }

    public override IEnumerable<CSMLSyntaxNode> DescendingNodes() => DefaultDescendingNodesImpl(_directChildren);
}
