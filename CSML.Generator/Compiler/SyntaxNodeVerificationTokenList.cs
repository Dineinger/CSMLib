using CSML.Generator.SyntaxRepresentation;

namespace CSML.Compiler;

internal sealed class SyntaxNodeVerificationTokenList
{
    private readonly List<SyntaxNodeVerificationTokenItem> _items = new();

    public SyntaxNodeVerificationTokenList Add(SyntaxType item)
    {
        _items.Add(SyntaxNodeVerificationTokenItem.FromSyntaxType(item));
        return this;
    }

    public SyntaxNodeVerificationTokenList AddList(Action<SyntaxNodeVerificationTokenList> action)
    {
        _items.Add(SyntaxNodeVerificationTokenItem.FromList(action));
        return this;
    }

    public SyntaxNodeVerificationTokenList AddOption(Action<SyntaxNodeVerificationTokenOptions> action)
    {
        _items.Add(SyntaxNodeVerificationTokenItem.FromOptions(action));
        return this;
    }

    public SyntaxNodeVerificationTokenItem[] ToArray() => _items.ToArray();
}
