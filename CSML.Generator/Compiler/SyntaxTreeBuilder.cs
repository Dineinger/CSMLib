using CSML.Generator.SyntaxRepresentation;
using System.Collections;

namespace CSML.Generator.Compiler;
internal class SyntaxTreeBuilder
{
    private readonly SyntaxNodeBuilder _root;
    private SyntaxNodeBuilder _last;

    public SyntaxTreeBuilder(SyntaxNodeBuilder root)
    {
        _last = root;
        _root = root;
    }

    public void InAndAdd(SyntaxNodeBuilder node)
    {
        _last.Children.Add(node);
        _last = node;
    }

    public void AddAndOut(SyntaxNodeBuilder node)
    {
        if (_last.Parent is null) {
            throw new NullReferenceException(nameof(_last.Parent));
        }

        _last.Children.Add(node);
        _last = _last.Parent;
    }

    public bool Contains(SyntaxNodeKind nodeKind)
    {
        return Contains(nodeKind, _root);
    }
    
    private bool Contains(SyntaxNodeKind nodeKind, SyntaxNodeBuilder node)
    {
        if (node.SyntaxNodeKind == nodeKind) {
            return true;
        }

        return node.Children.Any(x => x.SyntaxNodeKind == nodeKind);
    }

    public CSMLSyntaxNode Build()
    {
        return Build(_root);
    }

    private CSMLSyntaxNode Build(SyntaxNodeBuilder builder)
    {
        var tokens = builder.Tokens;
        var children = builder.Children.Select(Build).ToList();
        CSMLSyntaxNode node = builder.SyntaxNodeKind switch
        {
            SyntaxNodeKind.TagOpeningSyntax => new TagOpeningSyntax(tokens, children, GetTagOpeningSyntaxType(tokens), GetTagOpeningSyntaxName(tokens)),
            SyntaxNodeKind.TagClosingSyntax => new TagClosingSyntax(tokens, children, GetTagClosingSyntaxType(tokens)),
            SyntaxNodeKind.CSMLComponentOpeningSyntax => new CSMLComponentOpeningSyntax(tokens, children, GetComponentType(tokens), "object"),
            SyntaxNodeKind.CompilationUnit => new CSMLCompilationUnit(children),
            _ => throw new NotImplementedException("syntax builder couldn't find the correct syntax node for the enum SyntaxNodeKind")
        };

        return node;
    }

    private string? GetTagOpeningSyntaxName(CSMLSyntaxToken[] tokens)
    {
        for (var i = 0; i < tokens.Length; i++) {
            var token = tokens[i];
            if (token.SyntaxType == SyntaxType.Hashtag) {
                var nextToken = tokens[i + 1];
                if (nextToken.SyntaxType != SyntaxType.Identifier) {
                    throw new NotImplementedException("There should be an identifier after an #, diagnostic handling has to be handled in future");
                }

                return (string)nextToken.Value!;
            }
        }

        return null;
    }

    private string GetComponentType(CSMLSyntaxToken[] tokens)
    {
        var typeToken = tokens.First(x => x.SyntaxType == SyntaxType.Identifier);
        return (string)typeToken.Value!;
    }

    private static string GetTagClosingSyntaxType(CSMLSyntaxToken[] tokens)
    {
        var typeToken = tokens.First(x => x.SyntaxType == SyntaxType.Identifier);
        return (string)typeToken.Value!;
    }

    private static string GetTagOpeningSyntaxType(CSMLSyntaxToken[] tokens)
    {
        var typeToken = tokens.First(x => x.SyntaxType == SyntaxType.Identifier);
        return (string)typeToken.Value!;
    }
}

internal enum SyntaxNodeKind
{
    TagOpeningSyntax,
    TagClosingSyntax,
    CSMLComponentOpeningSyntax,
    CompilationUnit
}

internal class SyntaxNodeBuilder
{
    public SyntaxNodeBuilder(SyntaxNodeKind syntaxNodeKind, CSMLSyntaxToken[] tokens)
    {
        Children = new(this);
        SyntaxNodeKind = syntaxNodeKind;
        Tokens = tokens;
    }

    public SyntaxNodeBuilder? Parent { get; set; }
    public SyntaxNodeBuilderChildren Children { get; }
    public SyntaxNodeKind SyntaxNodeKind { get; set; }
    public CSMLSyntaxToken[] Tokens { get; }
}

internal class SyntaxNodeBuilderChildren : IEnumerable<SyntaxNodeBuilder>
{
    private readonly SyntaxNodeBuilder _owner;

    public SyntaxNodeBuilderChildren(SyntaxNodeBuilder owner)
    {
        _owner = owner;
    }

    private readonly List<SyntaxNodeBuilder> _children = new();

    public void Add(SyntaxNodeBuilder node)
    {
        node.Parent = _owner;
        _children.Add(node);
    }

    public IEnumerator<SyntaxNodeBuilder> GetEnumerator()
    {
        return ((IEnumerable<SyntaxNodeBuilder>)_children).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_children).GetEnumerator();
    }
}