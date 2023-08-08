using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace CSML.Compiler;

public record struct CSMLRawCode(string Value, SyntaxToken token, TextSpan TextSpan);
