using System;

namespace CSML.Compiler.Syntax;

public struct CSMLSyntaxToken
{
    public SyntaxType SyntaxType { get; }

    public object? Value { get; private set; }

    public CSMLSyntaxToken(SyntaxType syntaxType)
    {
        SyntaxType = syntaxType;
    }

    public static CSMLSyntaxToken LessThan => new(SyntaxType.LessThanToken);

    public static CSMLSyntaxToken GreaterThan => new(SyntaxType.GreaterThanToken);

    internal static CSMLSyntaxToken Literal(string buffer) =>
        new(SyntaxType.Literal)
        {
            Value = buffer,
        };

    internal static CSMLSyntaxToken TypeToken(string v) =>
        new(SyntaxType.TypeToken)
        {
            Value = v,
        };
}
