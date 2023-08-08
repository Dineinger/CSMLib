using System;

namespace CSML.Compiler.Syntax;

public struct CSMLSyntaxToken
{
    public int DebugIndex { get; set; }

    public SyntaxType SyntaxType { get; }

    public object? Value { get; private set; }

    public CSMLSyntaxToken(SyntaxType syntaxType)
    {
        SyntaxType = syntaxType;
    }

    public static CSMLSyntaxToken LessThan => new(SyntaxType.LessThanToken);

    public static CSMLSyntaxToken GreaterThan => new(SyntaxType.GreaterThanToken);

    public static CSMLSyntaxToken SlashToken => new(SyntaxType.SlashToken);

    public static CSMLSyntaxToken EndOfFileTrivia => new(SyntaxType.EndOfFileTrivia);

    public static CSMLSyntaxToken WhitespaceTrivia => new(SyntaxType.WhitespaceTrivia);

    public static CSMLSyntaxToken EndOfLineTrivia => new(SyntaxType.EndOfLineTrivia);

    internal static CSMLSyntaxToken Identifier(string buffer) =>
        new(SyntaxType.Identifier)
        {
            Value = buffer,
        };
}
