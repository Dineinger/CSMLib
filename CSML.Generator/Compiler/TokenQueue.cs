using CSML.Generator.SyntaxRepresentation;

namespace CSML.Compiler;

public class TokenQueue
{
    private readonly ReadOnlyMemory<CSMLSyntaxToken> _tokens;
    private int Count => _tokens.Length;
    private int Index = 0;

    public bool IsAtEnd => Index >= Count;

    public CSMLSyntaxToken Next => _tokens.Span[Index];

    public TokenQueue(CSMLSyntaxToken[] tokens)
    {
        _tokens = tokens;
    }

    public bool IsNextOfKind(SyntaxType syntaxType)
    {
        if (IsAtEnd) {
            return (false);
        }

        if (Next.SyntaxType != syntaxType) {
            return false;
        }

        return true;
    }

    public ReadOnlySpan<CSMLSyntaxToken> GetUntilOrEndAndMove(SyntaxType syntaxType)
    {
        int indexOfSyntaxToken = -1;
        var tokens = _tokens.Span;
        for (int a = Index; a < Count; a++) {
            var item = tokens[a];

            if (item.SyntaxType == syntaxType) {
                indexOfSyntaxToken = a;
                break;
            }
        }

        var index = Index;
        if (indexOfSyntaxToken == -1) {
            Index = Count;
            return tokens.Slice(index);
        }

        Index = indexOfSyntaxToken + 1;
        return tokens.Slice(index, indexOfSyntaxToken - index + 1);
    }

    internal bool IsNextAnyOfKinds(params SyntaxType[] syntaxTypes)
    {
        foreach (var x in syntaxTypes) {
            if (Next.SyntaxType == x) {
                return true;
            }
        }

        return false;
    }

    internal bool MoveNext()
    {
        Index++;
        return ! IsAtEnd;
    }
}
