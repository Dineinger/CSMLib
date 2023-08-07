using CSML.Compiler;
using Microsoft.CodeAnalysis;

namespace CSML.Generator;

public record struct CSMLRegistrationInfo(SyntaxTree SyntaxTree, SyntaxToken TypeToCreate, CSMLRawCode CSMLCode);