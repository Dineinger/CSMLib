using System.Collections.Generic;
using CSML.Compiler.Syntax;

namespace CSML.Compiler;

public class CSMLCompilationUnit : CSMLSyntaxNode
{
    private List<CSMLSyntaxNode> _directChildren { get; }

    public CSMLCompilationUnit(List<CSMLSyntaxNode> directChildren)
    {
        _directChildren = directChildren;
    }

    public override IEnumerable<CSMLSyntaxNode> DescendingNodes() => DefaultDescendingNodesImpl(_directChildren);
}


