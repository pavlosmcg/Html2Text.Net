using System.Text;
using Html2Text.Compatibility;
using Html2Text.Rendering.Tables;
using NUnit.Framework;
using static Html2Text.Tests.TestHelpers;

namespace Html2Text.Tests;

[TestFixture]
public class TableBuilderTests
{
    // TODO datatable detector should get datatable if we have th cells
    // TODO datatable should also check number of rows - 1 is not a datatable
    private StringBuilder _builder;
    private StringBuilderBufferWriter _writer;
    private TableBuilder _unit;

    [SetUp]
    public void SetUp()
    {
        _builder = new StringBuilder();
        _writer = new StringBuilderBufferWriter(_builder);
        _unit = new TableBuilder(_writer);
    }

    private string GetResult()
    {
        _unit.Build();
        return _builder.ToString();
    }

    [Test]
    public void Render_Returns_SimpleRowCorrectly()
    {
        // arrange
        _unit.AppendCell();
        _unit.Append('A');

        _unit.AppendCell();
        _unit.Append('B');

        _unit.AppendCell();
        _unit.Append('C');

        // act
        var result = GetResult();

        // assert
        Assert.That(result,
            Is.EqualTo("| A | B | C |"));
    }

    [Test]
    public void Render_Returns_MultipleRowsCorrectly()
    {
        // arrange / act
        _unit
            .AppendRow() // first row
            .AppendCell("A")
            .AppendCell("B")
            .AppendCell("C")
            .AppendRow() // second row
            .AppendCell("D")
            .AppendCell("E")
            .AppendCell("F");

        var result = GetResult();

        // assert
        var expected = """
                       | A | B | C |
                       | D | E | F |
                       """;
        AssertAreEqualNormalised(result, expected);
    }

    [Test]
    public void Render_Returns_RowsWithEmptyCells()
    {
        // arrange / act
        _unit
            .AppendRow() // first row
            .AppendCell()
            .AppendCell("B")
            .AppendCell("C")
            .AppendRow() // second row
            .AppendCell("D")
            .AppendCell()
            .AppendCell("F");

        var result = GetResult();

        // assert
        var expected = """
                       |   | B | C |
                       | D |   | F |
                       """;
        AssertAreEqualNormalised(result, expected);
    }

    [Test]
    public void Render_Returns_TableHeadersWithRowSeparator()
    {
        // arrange / act
        _unit
            .AppendRow() // header row
            .AppendHeaderCell("A")
            .AppendHeaderCell("B")
            .AppendHeaderCell("C")
            .AppendRow() // data row
            .AppendCell("1")
            .AppendCell("2")
            .AppendCell("3");

        var result = GetResult();

        // assert
        var expected = """
                       | A | B | C |
                       | - | - | - |
                       | 1 | 2 | 3 |
                       """;
        AssertAreEqualNormalised(result, expected);
    }

    [Test]
    public void Render_Returns_ColumnsFormattedToContentWidth()
    {
        // arrange / act
        _unit
            .AppendRow() // header row
            .AppendHeaderCell("Column A")
            .AppendHeaderCell("Column B")
            .AppendHeaderCell("Column C")
            .AppendRow() // data row
            .AppendCell("blorg")
            .AppendCell("fester")
            .AppendCell("framistan-bedoulia");

        var result = GetResult();

        // assert
        var expected = """
                       | Column A | Column B | Column C           |
                       | -------- | -------- | ------------------ |
                       | blorg    | fester   | framistan-bedoulia |
                       """;
        AssertAreEqualNormalised(result, expected);
    }

    [Test]
    public void Render_Returns_RowsPaddedToMaxColumnCount()
    {
        // arrange / act
        _unit
            .AppendRow() // header row
            .AppendHeaderCell("Column A")
            .AppendHeaderCell("Column B")
            .AppendHeaderCell("Column C")
            .AppendRow() // data row
            .AppendCell("data 1")
            .AppendCell("data 2")
            .AppendCell("data 3")
            .AppendRow() // longer data row
            .AppendCell("data 4")
            .AppendCell("data 5")
            .AppendCell("data 6")
            .AppendCell("extra data");

        var result = GetResult();

        // assert
        var expected = """
                       | Column A | Column B | Column C |            |
                       | -------- | -------- | -------- | ---------- |
                       | data 1   | data 2   | data 3   |            |
                       | data 4   | data 5   | data 6   | extra data |
                       """;
        AssertAreEqualNormalised(result, expected);
    }

    [Test]
    public void Render_Returns_RowsIncludingEmpty()
    {
        // arrange / act
        _unit
            .AppendRow() // header row
            .AppendHeaderCell("Column A")
            .AppendHeaderCell("Column B")
            .AppendHeaderCell("Column C")
            .AppendRow() // empty data row
            .AppendCell()
            .AppendCell()
            .AppendCell()
            .AppendRow() // second data row
            .AppendCell("second 1")
            .AppendCell("second 2")
            .AppendCell("second 3");

        var result = GetResult();

        // assert
        var expected = """
                       | Column A | Column B | Column C |
                       | -------- | -------- | -------- |
                       |          |          |          |
                       | second 1 | second 2 | second 3 |
                       """;
        AssertAreEqualNormalised(result, expected);
    }

    [Test]
    public void Render_Returns_ColumnsIncludingEmpty()
    {
        // arrange / act
        _unit
            .AppendRow()
            .AppendHeaderCell("Column A")
            .AppendHeaderCell("Column B")
            .AppendHeaderCell(string.Empty) //empty column
            .AppendHeaderCell("Column D")
            .AppendRow()
            .AppendCell("first 1")
            .AppendCell("first 2")
            .AppendCell() //empty column
            .AppendCell("first 4")
            .AppendRow()
            .AppendCell("second 1")
            .AppendCell("second 2")
            .AppendCell() //empty column
            .AppendCell("second 4");

        var result = GetResult();

        // assert
        var expected = """
                       | Column A | Column B |   | Column D |
                       | -------- | -------- | - | -------- |
                       | first 1  | first 2  |   | first 4  |
                       | second 1 | second 2 |   | second 4 |
                       """;
        AssertAreEqualNormalised(result, expected);
    }

    [Test]
    public void Render_Returns_MultipleHeaderRows_IfThatIsWhatYouReallyReallyWant()
    {
        // arrange / act
        _unit
            .AppendRow() // header row
            .AppendHeaderCell("Header A")
            .AppendHeaderCell("Header B")
            .AppendHeaderCell("Header C")
            .AppendRow() // data row
            .AppendCell("data 1")
            .AppendCell("data 2")
            .AppendCell("data 3")
            .AppendRow() // data row
            .AppendCell("data 4")
            .AppendCell("data 5")
            .AppendCell("data 6")
            .AppendRow() // another header row
            .AppendHeaderCell("Header D")
            .AppendHeaderCell("Header E")
            .AppendHeaderCell("Header F")
            .AppendRow() // data row
            .AppendCell("data 7")
            .AppendCell("data 8")
            .AppendCell("data 9")
            .AppendRow() // data row
            .AppendCell("data 10")
            .AppendCell("data 11")
            .AppendCell("data 12");

        var result = GetResult();

        // assert
        var expected = """
                       | Header A | Header B | Header C |
                       | -------- | -------- | -------- |
                       | data 1   | data 2   | data 3   |
                       | data 4   | data 5   | data 6   |
                       | Header D | Header E | Header F |
                       | -------- | -------- | -------- |
                       | data 7   | data 8   | data 9   |
                       | data 10  | data 11  | data 12  |
                       """;
        AssertAreEqualNormalised(result, expected);
    }
}