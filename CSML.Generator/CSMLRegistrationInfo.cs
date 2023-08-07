using CSML.Compiler;
using Microsoft.CodeAnalysis;

namespace CSML.Generator;

public record struct CSMLRegistrationInfo(SyntaxToken TypeToCreate, CSMLRawCode CSMLCode);