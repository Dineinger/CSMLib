using System.Collections.Generic;

namespace CSML.Generator.SyntaxRepresentation;

public abstract class CSMLSyntaxNode
{
    public abstract IEnumerable<CSMLSyntaxNode> DirectChildren { get; }

    public abstract IEnumerable<CSMLSyntaxNode> DescendingNodes();

    protected IEnumerable<CSMLSyntaxNode> DefaultDescendingNodesImpl(IEnumerable<CSMLSyntaxNode> directChildTokens)
    {
        yield return this;
        foreach (var child in directChildTokens)
        {
            foreach (var grandchilds in child.DescendingNodes())
            {
                yield return grandchilds;
            }
        }
    }
}
