using System.Threading;

namespace CSML.Generator.SyntaxRepresentation;

public class CSMLSyntaxTree
{
    private readonly CSMLSyntaxNode _root;
    private readonly CSMLInfo _registrationInfo;

    public CSMLInfo CSMLInfo => _registrationInfo;

    public CSMLSyntaxTree(CSMLSyntaxNode root, CSMLInfo registrationInfo)
    {
        _root = root;
        _registrationInfo = registrationInfo;
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
