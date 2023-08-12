using CSML.Generator.SyntaxRepresentation;

namespace CSML.Compiler;

internal sealed class SyntaxNodeVerificationTokenOptions
{
    private readonly List<SyntaxNodeVerificationTokenItem> _items = new();

    public SyntaxNodeVerificationTokenOptions Add(SyntaxType item)
    {
        _items.Add(SyntaxNodeVerificationTokenItem.FromSyntaxType(item));
        return this;
    }

    public SyntaxNodeVerificationTokenOptions AddList(Action<SyntaxNodeVerificationTokenList> action)
    {
        _items.Add(SyntaxNodeVerificationTokenItem.FromList(action));
        return this;
    }

    public SyntaxNodeVerificationTokenOptions AddOption(Action<SyntaxNodeVerificationTokenOptions> action)
    {
        _items.Add(SyntaxNodeVerificationTokenItem.FromOptions(action));
        return this;
    }

    public SyntaxNodeVerificationTokenItem[] ToArray() => _items.ToArray();
}