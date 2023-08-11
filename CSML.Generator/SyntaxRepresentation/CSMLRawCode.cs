using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace CSML.Generator.SyntaxRepresentation;

public record struct CSMLRawCode(string Value, SyntaxToken Token, TextSpan TextSpan);
