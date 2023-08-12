﻿using System;

namespace CSML.Generator.SyntaxRepresentation;

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

    public static CSMLSyntaxToken Hashtag => new(SyntaxType.Hashtag);

    public static CSMLSyntaxToken At => new(SyntaxType.AtToken);

    public bool IsTrivia => SyntaxType is SyntaxType.EndOfFileTrivia or SyntaxType.EndOfLineTrivia or SyntaxType.WhitespaceTrivia;

    internal static CSMLSyntaxToken Identifier(string buffer) =>
        new(SyntaxType.Identifier)
        {
            Value = buffer,
        };
}
