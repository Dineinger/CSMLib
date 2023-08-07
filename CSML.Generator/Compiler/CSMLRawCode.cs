using Microsoft.CodeAnalysis.Text;

namespace CSML.Compiler;

public record struct CSMLRawCode(string Value, TextSpan TextSpan);
