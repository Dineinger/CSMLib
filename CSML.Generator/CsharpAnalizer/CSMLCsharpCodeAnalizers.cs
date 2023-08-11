using CSML.Compiler;

namespace CSML.Generator.CsharpAnalizer;

internal static class CSMLCsharpCodeAnalizers
{
    public readonly static CSMLAttributeAnalizer Attribute = new();
    public readonly static CSMLTranslatorAnalizer Translator = new();
}
