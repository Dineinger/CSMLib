using System.Collections.Generic;
using CSML.Compiler.Syntax;

namespace CSML.Compiler;

public class CSMLCompilation
{
    public IReadOnlyList<CSMLSyntaxTree> SyntaxTrees { get; }

    public CSMLCompilation(IReadOnlyList<CSMLSyntaxTree> syntaxTrees)
    {
        SyntaxTrees = syntaxTrees;
    }
}
