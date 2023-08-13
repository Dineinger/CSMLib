using System.Collections.Generic;

namespace CSML.Generator.SyntaxRepresentation;

public abstract class CSMLSyntaxNode
{
    public abstract IEnumerable<CSMLSyntaxNode> DirectChildren { get; }

    public abstract IEnumerable<CSMLSyntaxNode> DescendingNodes();

    protected IEnumerable<CSMLSyntaxNode> DefaultDescendingNodesImpl(IEnumerable<CSMLSyntaxNode> directChildTokens)
    {
        foreach (var child in directChildTokens)
        {
            yield return child;
            foreach (var grandchilds in child.DescendingNodes())
            {
                yield return grandchilds;
            }
        }
    }
}
