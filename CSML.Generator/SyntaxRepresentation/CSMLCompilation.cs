using System.Collections.Generic;

namespace CSML.Generator.SyntaxRepresentation;

public class CSMLCompilation
{
    public IReadOnlyList<CSMLSyntaxTree> SyntaxTrees { get; }

    public CSMLCompilation(IReadOnlyList<CSMLSyntaxTree> syntaxTrees)
    {
        SyntaxTrees = syntaxTrees;
    }
}
