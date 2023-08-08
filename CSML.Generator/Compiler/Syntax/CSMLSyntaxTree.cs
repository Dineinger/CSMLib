using CSML.Generator;
using System.Threading;

namespace CSML.Compiler.Syntax;

public class CSMLSyntaxTree
{
    private readonly CSMLSyntaxNode _root;
    private readonly CSMLRegistrationInfo _registrationInfo;

    public CSMLRegistrationInfo RegistrationInfo => _registrationInfo;

    public CSMLSyntaxTree(CSMLSyntaxNode root, CSMLRegistrationInfo registrationInfo)
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
