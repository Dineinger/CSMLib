using System.Collections.Generic;
using CSML.Compiler.Syntax;

namespace CSML.Compiler;

public class CSMLCompilationUnit : CSMLSyntaxNode
{
    private readonly List<CSMLSyntaxNode> _directChildren;

    public override IEnumerable<CSMLSyntaxNode> DirectChildren => _directChildren;

    public CSMLCompilationUnit(List<CSMLSyntaxNode> directChildren)
    {
        _directChildren = directChildren;
    }

    public override IEnumerable<CSMLSyntaxNode> DescendingNodes() => DefaultDescendingNodesImpl(_directChildren);
}
