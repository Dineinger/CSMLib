using CSML.Generator.SyntaxRepresentation;
using System.Data.Common;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace CSML.Compiler;

internal class SyntaxNodeVerifiers
{
    private static readonly Regex _tagOpeningSyntaxRegex = new(@"<\s*[A-z]+(\s+#[A-z]+)?\s*>");
    private static readonly Regex _tagClosingSyntaxRegex = new(@"<\s*\/\s*[A-z]+\s*>");
    private static readonly Regex _tagSelfClosingSyntaxRegex = new(@"<\s*[A-z]+(\s+#[A-z]+)?\s+\/\s*>");

    public readonly SyntaxNodeVerifier TagOpeningSyntaxVerification = new (_tagOpeningSyntaxRegex, syntaxNodeType: "tag opening syntax");

    public readonly SyntaxNodeVerifier TagClosingSyntax = new(_tagClosingSyntaxRegex, syntaxNodeType: "tag closing syntax");

    public readonly SyntaxNodeVerifier TagSelfClosingSyntax = new(_tagSelfClosingSyntaxRegex, "tag self closing syntax");
}