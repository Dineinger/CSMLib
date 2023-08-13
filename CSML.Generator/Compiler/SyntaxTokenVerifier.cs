using CSML.Generator.SyntaxRepresentation;
using System.Data.Common;
using System.Linq.Expressions;

namespace CSML.Compiler;

internal class SyntaxTokenVerifier
{
    public readonly SyntaxNodeVerification TagOpeningSyntaxVerification = new (SyntaxNodeVerificationTokenItem.FromList(x => x
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
    ),
    syntaxNodeType: "tag opening syntax");

    public readonly SyntaxNodeVerification TagClosingSyntax = new(SyntaxNodeVerificationTokenItem.FromList(x => x
        .Add(SyntaxType.LessThanToken)
        .Add(SyntaxType.SlashToken)
        .Add(SyntaxType.Identifier)
        .Add(SyntaxType.GreaterThanToken)
    ),
    syntaxNodeType: "tag closing syntax");
}