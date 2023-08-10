using CSML.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSML.Generator;

public record struct CSMLInfo(SyntaxTree SyntaxTree, CSMLMetadata Metadata, CSMLRawCode CSMLCode);

public record struct CSMLMetadata(string TypeToCreate, string Namespace, CSMLSourceLocation From);

public enum CSMLSourceLocation
{
    CSMLTranslator,
    CSMLAttribute,
}

//public record struct CSMLRegistrationInfo(SyntaxTree SyntaxTree, CSMLMetadata Metadata, CSMLRawCode CSMLCode) : CSMLInfo;
//public record struct CSMLAttributeInfo(SyntaxTree SyntaxTree, CSMLMetadata Metadata, CSMLRawCode CSMLCode) : CSMLInfo;
