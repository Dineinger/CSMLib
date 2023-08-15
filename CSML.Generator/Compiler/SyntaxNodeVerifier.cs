using CSML.Generator;
using CSML.Generator.SyntaxRepresentation;
using CSML.Generator.SyntaxRepresentation.SyntaxErrors;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace CSML.Compiler;

internal sealed class SyntaxNodeVerifier
{
    private readonly Regex _pattern;
    private readonly string _syntaxNodeType;
    private readonly TriviaPolicy _triviaPolicy;

    public SyntaxNodeVerifier(Regex pattern, string syntaxNodeType)
    {
        _pattern = pattern;
        _syntaxNodeType = syntaxNodeType;
    }

    public SyntaxError? Verify(string tokensUnchecked)
    {
        if (_pattern.IsMatch(tokensUnchecked)) {
            return null;
        }

        return new UnknownSyntaxSyntaxError($"could not verify syntax: {_syntaxNodeType} with text: {tokensUnchecked}");
    }
}
