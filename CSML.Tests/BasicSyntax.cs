namespace CSML.Tests;

public class BasicSyntax
{
    [Fact]
    public void SmallFile()
    {
        MainPage c = CSMLTranslator.From<MainPage>(
            """
            <MainPage>
                <C>
                    <C>
                        <C>
                        </C>
                    </C>
                    <C>
                    </C>
                </C>
            </MainPage>
            """);

        _ = CSMLTranslator.From<C>(
            """
            <C>
            </C>
            """);

        Assert.Equal(typeof(MainPage), c.GetType());
    }
}
