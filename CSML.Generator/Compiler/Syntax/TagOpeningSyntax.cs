using System;
using System.Collections.Generic;

namespace CSML.Compiler.Syntax;

public sealed class TagOpeningSyntax : CSMLSyntaxNode
{
    private readonly CSMLSyntaxToken[] _tokens;
    private readonly List<CSMLSyntaxNode> _directChildren;

    public override IEnumerable<CSMLSyntaxNode> DirectChildren => _directChildren;

    public string Type { get; }
    public string? Name { get; }
    public IReadOnlyList<CSMLSyntaxToken> Tokens => _tokens;

    public TagOpeningSyntax(CSMLSyntaxToken[] tokens, List<CSMLSyntaxNode> directChildren, string type, string? name)
    {
        Type = type;
        Name = name;
        _tokens = tokens;
        _directChildren = directChildren;
    }

    public override IEnumerable<CSMLSyntaxNode> DescendingNodes() => DefaultDescendingNodesImpl(_directChildren);
}
