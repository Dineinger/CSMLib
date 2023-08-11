namespace CSML.Generator.SyntaxRepresentation;

public sealed class TagClosingSyntax : CSMLSyntaxNode
{
    private readonly CSMLSyntaxToken[] _tokens;
    private readonly List<CSMLSyntaxNode> _directChildren;
    private readonly string _type;

    public string Type => _type;
    public IReadOnlyList<CSMLSyntaxToken> Tokens => _tokens;

    public override IEnumerable<CSMLSyntaxNode> DirectChildren => _directChildren;

    public TagClosingSyntax(CSMLSyntaxToken[] verifiedTokens, List<CSMLSyntaxNode> directChildren, string type)
    {
        _tokens = verifiedTokens;
        _directChildren = directChildren;
        _type = type;
    }

    public override IEnumerable<CSMLSyntaxNode> DescendingNodes() => DefaultDescendingNodesImpl(_directChildren);
}