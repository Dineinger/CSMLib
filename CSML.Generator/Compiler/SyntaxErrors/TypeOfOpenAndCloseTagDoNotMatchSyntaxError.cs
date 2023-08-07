﻿using System;
using System.Collections.Generic;
using System.Text;

namespace CSML.Generator.Compiler.SyntaxErrors;

internal class SyntaxError
{
    public string Message { get; }

    public SyntaxError(string message)
    {
        Message = message;
    }
}

internal class TypeOfOpenAndCloseTagDoNotMatchSyntaxError : SyntaxError
{
    public TypeOfOpenAndCloseTagDoNotMatchSyntaxError(string message) : base(message) { }
}

internal class ClosingTagUnableToCloseAnythingSyntaxError : SyntaxError
{
    public ClosingTagUnableToCloseAnythingSyntaxError(string message) : base(message) { }
}

internal class TagAtBadPositionSyntaxError : SyntaxError
{
    public TagAtBadPositionSyntaxError(string message) : base(message) { }
}