using CSML.Generator.SyntaxRepresentation;

namespace CSML.Compiler;

internal class TokenQueue
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

    public CSMLSyntaxTokensInfo GetUntilOrEndAndMove(SyntaxType syntaxType)
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
            var fromBeginning = tokens.Slice(index).ToArray();
            return new CSMLSyntaxTokensInfo(fromBeginning, String.Concat(fromBeginning.Select(x => x.Value)));
        }

        Index = indexOfSyntaxToken + 1;
        var inBetween = tokens.Slice(index, indexOfSyntaxToken - index + 1).ToArray();
        return new(inBetween, String.Concat(inBetween.Select(x => x.Value)));
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
