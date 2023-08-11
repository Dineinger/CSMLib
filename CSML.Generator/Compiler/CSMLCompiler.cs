using System.Collections.Immutable;
using CSML.Generator.SyntaxRepresentation;
using Microsoft.CodeAnalysis;

namespace CSML.Compiler;

public class CSMLCompiler
{
    private readonly SourceProductionContext _context;
    private readonly SyntaxTreeCreator _syntaxTreeCreator;
    private readonly CSMLSyntaxTreeVerifier _treeVerifier;
    private readonly SyntaxTokenVerifier _tokenVerifier;
    private readonly TokenCreator _tokenCreator;

    public CSMLCompiler(SourceProductionContext context)
    {
        _context = context;
        _treeVerifier = new CSMLSyntaxTreeVerifier(context);
        _tokenVerifier = new SyntaxTokenVerifier();
        _tokenCreator = new TokenCreator();
        _syntaxTreeCreator = new SyntaxTreeCreator(_context, _tokenCreator, _tokenVerifier);
    }

    public CSMLCompilation? GetCompilation(IReadOnlyList<CSMLInfo> csmlCodes)
    {
        var success = _syntaxTreeCreator.GetSyntaxTreesUnverified(csmlCodes, out var syntaxTreesUnverified);
        if (success is false) {
            return null;
        }

        _treeVerifier.VerifySyntaxTrees(syntaxTreesUnverified);

        var syntaxTreesVerified = syntaxTreesUnverified;

        return new CSMLCompilation(syntaxTreesVerified.Select(x => x.CSMLSyntaxTree).ToImmutableArray());
    }

    public CSMLCompilation? GetCompilation(Compilation compilation, params Func<Compilation, IReadOnlyList<CSMLInfo>>[] csmlGetter)
    {
        List<CSMLInfo> csmlInfos = new();

        foreach (var getter in csmlGetter) {
            csmlInfos.AddRange(getter(compilation));
        }

        var csmlSyntaxTrees = GetCompilation(csmlInfos);
        return csmlSyntaxTrees;
    }
}
