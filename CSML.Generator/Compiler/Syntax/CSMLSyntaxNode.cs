using System.Collections.Generic;

namespace CSML.Compiler.Syntax;

public abstract class CSMLSyntaxNode
{
    public abstract IReadOnlyList<CSMLSyntaxNode> DescendingNodes();
}


