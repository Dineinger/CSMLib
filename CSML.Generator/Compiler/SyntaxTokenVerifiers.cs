using CSML.Generator.SyntaxRepresentation;
using System.Data.Common;
using System.Linq.Expressions;

namespace CSML.Compiler;

internal class SyntaxTokenVerifiers
{
    public readonly SyntaxNodeVerifier TagOpeningSyntaxVerification = new (x => x
            .Add(SyntaxType.LessThanToken)
            .Add(SyntaxType.Identifier)
            .AddOption(x => x
                .Add(SyntaxType.GreaterThanToken)
                .AddList(x => x
                    .Add(SyntaxType.Hashtag)
                    .Add(SyntaxType.Identifier)
                    .Add(SyntaxType.GreaterThanToken)
                )
            )
        ,
        syntaxNodeType: "tag opening syntax");

    public readonly SyntaxNodeVerifier TagClosingSyntax = new(x => x
            .Add(SyntaxType.LessThanToken)
            .Add(SyntaxType.SlashToken)
            .Add(SyntaxType.Identifier)
            .Add(SyntaxType.GreaterThanToken)
        ,
        syntaxNodeType: "tag closing syntax");

    public readonly SyntaxNodeVerifier TagSelfClosingSyntax = new(x => x
            .Add(SyntaxType.LessThanToken)
            .Add(SyntaxType.Identifier)
            .AddOption(x => x
                .AddList(x => x
                    .Add(SyntaxType.SlashToken)
                    .Add(SyntaxType.GreaterThanToken)
                )
                .AddList(x => x
                    .Add(SyntaxType.Hashtag)
                    .Add(SyntaxType.Identifier)
                    .Add(SyntaxType.SlashToken)
                    .Add(SyntaxType.GreaterThanToken)
                )
            )
        ,
        "tag self closing syntax");
}