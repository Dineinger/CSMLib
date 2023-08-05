namespace CSML.Generator;

internal class TagOpeningSyntax : CSMLSyntaxNode
{
    public string Type { get; }

    public TagOpeningSyntax(string type)
    {
        Type = type;
    }

    public override IReadOnlyList<CSMLSyntaxNode> DescendingNodes()
    {
        return Array.Empty<CSMLSyntaxNode>();
    }
}
