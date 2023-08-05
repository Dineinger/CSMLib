using System.Threading;

namespace CSML.Compiler.Syntax;

public class CSMLSyntaxTree
{
    private readonly CSMLSyntaxNode _root;

    public CSMLSyntaxTree(CSMLSyntaxNode root)
    {
        _root = root;
    }

    protected CSMLSyntaxNode GetRootCore(CancellationToken cancellationToken)
    {
        return _root;
    }

    public CSMLSyntaxNode GetRoot(CancellationToken cancellationToken = default)
    {
        return GetRootCore(cancellationToken);
    }
}


