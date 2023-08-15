using System;

namespace CSML.Generator.SyntaxRepresentation;

public struct CSMLSyntaxToken
{
    public int DebugIndex { get; set; }

    public SyntaxType SyntaxType { get; }

    public string? Value { get; private set; }

    public CSMLSyntaxToken(SyntaxType syntaxType, string value)
    {
        SyntaxType = syntaxType;
        Value = value;
    }

    public static CSMLSyntaxToken LessThan => new(SyntaxType.LessThanToken, "<");

    public static CSMLSyntaxToken GreaterThan => new(SyntaxType.GreaterThanToken, ">");

    public static CSMLSyntaxToken SlashToken => new(SyntaxType.SlashToken, "/");

    public static CSMLSyntaxToken EndOfFileTrivia => new(SyntaxType.EndOfFileTrivia, "");

    public static CSMLSyntaxToken WhitespaceTrivia => new(SyntaxType.WhitespaceTrivia, " "); // TODO: using more than one char for whitespace token

    public static CSMLSyntaxToken EndOfLineTrivia => new(SyntaxType.EndOfLineTrivia, "\r\n");

    public static CSMLSyntaxToken Hashtag => new(SyntaxType.Hashtag, "#");

    public static CSMLSyntaxToken At => new(SyntaxType.AtToken, "@");

    public readonly bool IsTrivia => SyntaxType is SyntaxType.EndOfFileTrivia or SyntaxType.EndOfLineTrivia or SyntaxType.WhitespaceTrivia;

    public static CSMLSyntaxToken EqualSign => new(SyntaxType.EqualSignToken, "=");

    internal static CSMLSyntaxToken Identifier(string buffer) =>
        new(SyntaxType.Identifier, buffer);
}
