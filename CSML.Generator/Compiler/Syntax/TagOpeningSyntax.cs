using System;
using System.Collections.Generic;

namespace CSML.Compiler.Syntax;

public sealed class TagOpeningSyntax : CSMLSyntaxNode
{
    private readonly CSMLSyntaxToken[] _tokens;
    private readonly List<CSMLSyntaxNode> _directChildTokens = new();

    public string Type { get; }

    public TagOpeningSyntax(CSMLSyntaxToken[] tokens, string type)
    {
        Type = type;
        _tokens = tokens;
    }

    public override IEnumerable<CSMLSyntaxNode> DescendingNodes() => DefaultDescendingNodesImpl(_directChildTokens);
}

public sealed class TagClosingSyntax : CSMLSyntaxNode
{
    private readonly CSMLSyntaxToken[] _tokens;
    private readonly List<CSMLSyntaxNode> _directChildren;
    private readonly string _type;

    public TagClosingSyntax(CSMLSyntaxToken[] verifiedTokens, List<CSMLSyntaxNode> directChildren, string type)
    {
        _tokens = verifiedTokens;
        _directChildren = directChildren;
        _type = type;
    }

    public override IEnumerable<CSMLSyntaxNode> DescendingNodes() => DefaultDescendingNodesImpl(_directChildren);
}