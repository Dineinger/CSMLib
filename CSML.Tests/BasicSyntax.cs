using CSML;

namespace CSML.Tests;

public class BasicSyntax
{
    [Fact]
    public void SmallFile()
    {
        MainPage c = CSMLTranslator.From<MainPage>(
            """
            <MainPage>
            </C>
            """);
    }
}
