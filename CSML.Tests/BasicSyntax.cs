namespace CSML.Tests;

[CSMLCode("""
    <App>
        <MainPage 
            #MainPage>
            <Label #x>
            </Label>
        </MainPage>
    </App>
    """)]
public partial class App
{

}

public class BasicSyntax
{
    [Fact]
    public void SmallFile()
    {
        MainPage c = CSMLTranslator.From<MainPage>(
            """
            <MainPage>
                <Div #One>
                    <Div #OneOne>
                        <Label #Text></Label>
                    </Div>
                    <Div #OneTwo>
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
