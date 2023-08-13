namespace CSML.Generator.SyntaxRepresentation;

public sealed class TagSelfClosingSyntax : CSMLSyntaxNode, ITagDeclarationSyntax
{
    private readonly CSMLSyntaxToken[] _tokens;
    private readonly List<CSMLSyntaxNode> _directChildren;

    public override IEnumerable<CSMLSyntaxNode> DirectChildren => _directChildren;
    public override IReadOnlyList<CSMLSyntaxToken> Tokens => _tokens;

    public string Type { get; }
    public string? Name { get; }

    public TagSelfClosingSyntax(CSMLSyntaxToken[] tokens, List<CSMLSyntaxNode> directChildren, string type, string? name)
    {
        Type = type;
        Name = name;
        _tokens = tokens;
        _directChildren = directChildren;
    }

    public override IEnumerable<CSMLSyntaxNode> DescendingNodes()
    {
        return DefaultDescendingNodesImpl(_directChildren);
    }
}