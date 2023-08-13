using System.Collections.Immutable;
using CSML.Generator.CsharpAnalizer;
using CSML.Generator.SyntaxRepresentation;
using CSML.Generator.SyntaxRepresentation.SyntaxErrors;
using Microsoft.CodeAnalysis;

namespace CSML.Compiler;

internal class CSMLCompiler
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

    public CSMLCompilation? GetCompilation(IReadOnlyList<CSMLInfo> csmlCodes, out SyntaxError? syntaxError)
    {
        var success = _syntaxTreeCreator.GetSyntaxTreesUnverified(csmlCodes, out var syntaxTreesUnverified);
        if (success is false) {
            syntaxError = new SyntaxError("could not create a syntax tree");
            return null;
        }

        //syntaxError = new SyntaxError(string.Join("|||", syntaxTreesUnverified.Select(x => x.CSMLSyntaxTree).First().GetRoot().DescendingNodes().Select(x => string.Join("|", x.Tokens.Select(x => x.SyntaxType)))));
        //return null;

        _treeVerifier.VerifySyntaxTrees(syntaxTreesUnverified);

        var syntaxTreesVerified = syntaxTreesUnverified;

        syntaxError = null;
        return new CSMLCompilation(syntaxTreesVerified.Select(x => x.CSMLSyntaxTree).ToImmutableArray());
    }

    public CSMLCompilation? GetCompilation(Compilation compilation, out SyntaxError? syntaxError, params ICSMLCsharpCodeAnalizer[] csmlGetter)
    {
        List<CSMLInfo> csmlInfos = new();

        foreach (var getter in csmlGetter) {
            csmlInfos.AddRange(getter.GetCSMLInfo(compilation));
        }

        var csmlSyntaxTrees = GetCompilation(csmlInfos, out syntaxError);
        return csmlSyntaxTrees;
    }
}
