namespace CSML.Tests;

public class BasicSyntax
{
    [Fact]
    public void SmallFile()
    {
        MainPage c = CSMLTranslator.From<MainPage>(
            """
            <MainPage>
                <Div>
                    <Div>
                        <Label></Label>
                    </Div>
                    <Div>
                        <Label></Label>
                    </Div>
                </Div>
            </MainPage>
            """);

        _ = CSMLTranslator.From<Div>(
            """
            <Div>
            </Div>
            """);

        _ = CSMLTranslator.From<Label>("""
            <Label>
            </Label>
            """);

        Assert.Equal(typeof(MainPage), c.GetType());
    }
}
