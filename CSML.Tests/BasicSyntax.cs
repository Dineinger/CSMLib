using CSML;
using CSML.Compiler;

namespace CSML.Tests;

public class BasicSyntax
{
    [Fact]
    public void SmallFile()
    {
        MainPage c = CSMLTranslator.From<MainPage>(
            """
            <C>
            </C>
            """);
    }
}

public class CompilerTests
{
    [Fact]
    public void SimpleCode()
    {
        var x = CSMLCompiler.GetSyntaxTrees(new[]
        {
            new CSMLRawCode(
                    """
                    <C>
                    </C>
                    """)
        });
    }
}

