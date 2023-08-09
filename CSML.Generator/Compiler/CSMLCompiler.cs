﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using CSML.Generator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace CSML.Compiler;

public class CSMLCompiler
{
    private readonly SourceProductionContext _context;
    private readonly SyntaxTreeCreator _syntaxTreeCreator;
    private readonly CSMLSyntaxTreeVerifier _treeVerifier;
    private readonly SyntaxTokenVerifier _tokenVerifier;
    private readonly TokenCreator _tokenCreator;

    public CSMLCompiler(SourceProductionContext context)
    {
        _context = context;
        _treeVerifier = new CSMLSyntaxTreeVerifier(context);
        _tokenVerifier = new SyntaxTokenVerifier();
        _tokenCreator = new TokenCreator();
        _syntaxTreeCreator = new SyntaxTreeCreator(_context, _tokenCreator, _tokenVerifier);
    }

    public CSMLCompilation? GetSyntaxTrees(CSMLRegistrationInfo[] csmlCodes)
    {
        var success = _syntaxTreeCreator.GetSyntaxTreesUnverified(csmlCodes, out var syntaxTreesUnverified);
        if (success is false) {
            return null;
        }

        _treeVerifier.VerifySyntaxTrees(syntaxTreesUnverified);

        var syntaxTreesVerified = syntaxTreesUnverified;

        return new CSMLCompilation(syntaxTreesVerified.Select(x => x.CSMLSyntaxTree).ToImmutableArray());
    }
}
