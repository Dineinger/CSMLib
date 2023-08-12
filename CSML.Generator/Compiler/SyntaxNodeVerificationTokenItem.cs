using CSML.Generator.SyntaxRepresentation;

namespace CSML.Compiler;

internal sealed class SyntaxNodeVerificationTokenItem
{
    public VerificationTokenItemKind Kind;

    public SyntaxNodeVerificationTokenItem[]? Options;

    public SyntaxNodeVerificationTokenItem[]? List;

    public SyntaxType? SyntaxType;

    public SyntaxNodeVerificationTokenItem(SyntaxNodeVerificationTokenItem[]? list)
    {
        Kind = VerificationTokenItemKind.List;
        List = list;
    }

    public SyntaxNodeVerificationTokenItem(SyntaxType syntaxType)
    {
        Kind = VerificationTokenItemKind.Item;
        SyntaxType = syntaxType;
    }

    public SyntaxNodeVerificationTokenItem(SyntaxNodeVerificationTokenOptions options)
    {
        Kind = VerificationTokenItemKind.ListOfOption;
        Options = options.ToArray();
    }

    public static SyntaxNodeVerificationTokenItem FromList(Action<SyntaxNodeVerificationTokenList> listOption)
    {
        var list = new SyntaxNodeVerificationTokenList();
        listOption(list);
        return new(list.ToArray());
    }

    public static SyntaxNodeVerificationTokenItem FromSyntaxType(SyntaxType syntaxType) => new (syntaxType);

    internal static SyntaxNodeVerificationTokenItem FromOptions(Action<SyntaxNodeVerificationTokenOptions> action)
    {
        var options = new SyntaxNodeVerificationTokenOptions();
        action(options);
        return new(options);
    }
}
