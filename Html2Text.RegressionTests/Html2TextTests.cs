namespace Html2Text.RegressionTests;

[TestFixture]
public class Html2TextTests
{
    public static IEnumerable<TestCaseData> HtmlFiles()
    {
        foreach (var file in Directory.GetFiles("Samples", "*.html"))
        {
            yield return new TestCaseData(file)
                .SetName($"Html2Text_Returns_ExpectedOutputFor({Path.GetFileName(file)})");
        }
    }

    [Test]
    [TestCaseSource(nameof(HtmlFiles))]
    public async Task Html2Text_Returns_ExpectedOutputFor(string filePath)
    {
        // arrange
        string input = File.ReadAllText(filePath);

        // act
        string text = Html2Text.Convert(input);

        // assert
        await Verify(text)
            .UseParameters(Path.GetFileNameWithoutExtension(filePath));
    }
}