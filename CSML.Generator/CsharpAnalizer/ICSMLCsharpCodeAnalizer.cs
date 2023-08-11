using CSML.Generator.SyntaxRepresentation;
using Microsoft.CodeAnalysis;

namespace CSML.Generator.CsharpAnalizer;
internal interface ICSMLCsharpCodeAnalizer
{
    IReadOnlyList<CSMLInfo> GetCSMLInfo(Compilation compilation);
}
