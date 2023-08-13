namespace CSML.Generator.SyntaxRepresentation;

public sealed class CSMLComponentOpeningSyntax : CSMLSyntaxNode
{
    private readonly CSMLSyntaxToken[] _tokens;
    private readonly List<CSMLSyntaxNode> _directChildren;
    private readonly string _type;
    private readonly string _baseType;

    public string Type => _type;
    public string BaseType => _baseType;
    public override IReadOnlyList<CSMLSyntaxToken> Tokens => _tokens;

    public override IEnumerable<CSMLSyntaxNode> DirectChildren => _directChildren;

    public CSMLComponentOpeningSyntax(CSMLSyntaxToken[] tokens, List<CSMLSyntaxNode> directChildren, string type, string baseType)
    {
        _tokens = tokens;
        _directChildren = directChildren;
        _type = type;
        _baseType = baseType;
    }

    public override IEnumerable<CSMLSyntaxNode> DescendingNodes() => DefaultDescendingNodesImpl(_directChildren);
}