using System.Collections.Generic;
using CSML.Compiler.Syntax;

namespace CSML.Compiler;

public class CSMLCompilationUnit : CSMLSyntaxNode
{
    private IReadOnlyList<CSMLSyntaxNode> _descendingNodes { get; }

    public CSMLCompilationUnit(List<CSMLSyntaxNode> descendingNodes)
    {
        _descendingNodes = descendingNodes;
    }

    public override IReadOnlyList<CSMLSyntaxNode> DescendingNodes()
    {
        return _descendingNodes;
    }
}


