using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;

namespace CSML.Generator;

[Generator]
public class CSMLGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterSourceOutput(context.CompilationProvider,
            static (context, compilation) =>
            {
                // Analizing C# Code
                var translatorInvocation = CSMLCsharpCodeAnalizer.GetTranslatorInvocations(compilation);
                var typesToCreate = CSMLCsharpCodeAnalizer.GetTypesToCreate(translatorInvocation);
                var csmlCodes = CSMLCsharpCodeAnalizer.GetCSMLCode(translatorInvocation);

                // Analizing CSML Code
                var csmlSyntaxTrees = GetSyntaxTrees(csmlCodes);

                // Generate Code
                var classesAsTexts = CSMLClassCreator.CreateClasses(typesToCreate);
                var fromCases = CSMLClassCreator.CreateFromCases(typesToCreate);
                var setupMethods = CSMLClassCreator.CreateSetupMethods(typesToCreate);
                var classesAsText = string.Join("\n\n", classesAsTexts);

                var debug = string.Join("\n\n", csmlSyntaxTrees.SyntaxTrees.First().GetRoot().DescendingNodes());

                var finalCode = CSMLClassCreator.CreateFinalCode(fromCases, setupMethods, classesAsText, debug);
                context.AddSource("CSMLTranslator.generated.cs", finalCode);
            });
    }

    private static readonly Regex _openTagSyntax = new(@"<[A-z]+>");

    private static CSMLCompilation GetSyntaxTrees(CSMLRawCode[] csmlCodes)
    {
        List<CSMLSyntaxNode> syntaxNodes = new();

        foreach (var csmlCode in csmlCodes)
        {
            var code = csmlCode.Value;

            var codeParts = GetCodeParts(code);

            foreach (var part in codeParts)
            {
                var match = _openTagSyntax.Match(part);

                if (match.Success)
                {
                    var openTagText = match.Value;

                    var openTagType = openTagText.TrimStart('<').TrimEnd('>');

                    syntaxNodes.Add(new TagOpeningSyntax(openTagType));
                }
            }
        }

        return new CSMLCompilation(new List<CSMLSyntaxTree>()
        {
            new CSMLSyntaxTree(new CSMLCompilationUnit(syntaxNodes))
        });
    }

    private static IReadOnlyList<string> GetCodeParts(string code)
    {
        var codeParts = new List<string>();

        var lastClosingTag = 0;
        for (int a = 0; a < code.Length; a++)
        {
            var c = code[a];

            if (c == '>')
            {
                var part = code.Substring(lastClosingTag, a - lastClosingTag + 1);
                codeParts.Add(part);
                lastClosingTag = a;
            }
        }
        var lastPart = code.Substring(lastClosingTag, code.Length - lastClosingTag);
        codeParts.Add(lastPart);

        return codeParts;
    }
}

internal class CSMLCompilation
{
    public IReadOnlyList<CSMLSyntaxTree> SyntaxTrees { get; }

    public CSMLCompilation(IReadOnlyList<CSMLSyntaxTree> syntaxTrees)
    {
        SyntaxTrees = syntaxTrees;
    }
}

internal class CSMLSyntaxTree
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

internal abstract class CSMLSyntaxNode
{
    public abstract IReadOnlyList<CSMLSyntaxNode> DescendingNodes();
}

internal class CSMLCompilationUnit : CSMLSyntaxNode
{
    private IReadOnlyList<CSMLSyntaxNode> _descendingNodes { get; }

    public CSMLCompilationUnit(List<CSMLSyntaxNode> descendingNodes)
    {
        _descendingNodes = descendingNodes;
    }

    public override IReadOnlyList<CSMLSyntaxNode> DescendingNodes()
    {
        return _descendingNodes;
    }
}

file class CSMLSyntaxTreeReference : SyntaxTree
{
    public override string FilePath => throw new NotImplementedException();

    public override bool HasCompilationUnitRoot => throw new NotImplementedException();

    public override int Length => throw new NotImplementedException();

    public override Encoding? Encoding => throw new NotImplementedException();

    protected override ParseOptions OptionsCore => throw new NotImplementedException();

    public override IList<TextSpan> GetChangedSpans(SyntaxTree syntaxTree)
    {
        throw new NotImplementedException();
    }

    public override IList<TextChange> GetChanges(SyntaxTree oldTree)
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<Diagnostic> GetDiagnostics(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<Diagnostic> GetDiagnostics(SyntaxNode node)
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<Diagnostic> GetDiagnostics(SyntaxToken token)
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<Diagnostic> GetDiagnostics(SyntaxTrivia trivia)
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<Diagnostic> GetDiagnostics(SyntaxNodeOrToken nodeOrToken)
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<LineMapping> GetLineMappings(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override FileLinePositionSpan GetLineSpan(TextSpan span, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Location GetLocation(TextSpan span)
    {
        throw new NotImplementedException();
    }

    public override FileLinePositionSpan GetMappedLineSpan(TextSpan span, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override SyntaxReference GetReference(SyntaxNode node)
    {
        throw new NotImplementedException();
    }

    public override SourceText GetText(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override bool HasHiddenRegions()
    {
        throw new NotImplementedException();
    }

    public override bool IsEquivalentTo(SyntaxTree tree, bool topLevel = false)
    {
        throw new NotImplementedException();
    }

    public override bool TryGetText(out SourceText text)
    {
        throw new NotImplementedException();
    }

    public override SyntaxTree WithChangedText(SourceText newText)
    {
        throw new NotImplementedException();
    }

    public override SyntaxTree WithFilePath(string path)
    {
        throw new NotImplementedException();
    }

    public override SyntaxTree WithRootAndOptions(SyntaxNode root, ParseOptions options)
    {
        throw new NotImplementedException();
    }

    protected override Task<SyntaxNode> GetRootAsyncCore(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    protected override SyntaxNode GetRootCore(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    protected override bool TryGetRootCore(out SyntaxNode root)
    {
        throw new NotImplementedException();
    }
}
