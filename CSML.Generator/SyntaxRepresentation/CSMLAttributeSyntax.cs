using System;
using System.Collections.Generic;
using System.Text;

namespace CSML.Generator.SyntaxRepresentation;
internal class CSMLAttributeSyntax : CSMLSyntaxNode
{
    private readonly CSMLSyntaxToken[] _tokens;
    private readonly List<CSMLSyntaxNode> _directChildren;

    public override IEnumerable<CSMLSyntaxNode> DirectChildren => _directChildren;

    public override IReadOnlyList<CSMLSyntaxToken> Tokens => _tokens;

    public CSMLAttributeSyntax(CSMLSyntaxToken[] tokens, List<CSMLSyntaxNode> directChildren)
    {
        _tokens = tokens;
        _directChildren = directChildren;
    }

    public override IEnumerable<CSMLSyntaxNode> DescendingNodes()
    {
        return DefaultDescendingNodesImpl(_directChildren);
    }
}
