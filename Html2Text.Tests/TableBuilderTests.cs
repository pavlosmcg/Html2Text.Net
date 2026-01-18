using Html2Text.Rendering.Tables;
using NUnit.Framework;
using System.Text;

namespace Html2Text.Tests;

[TestFixture]
public class TableBuilderTests
{
    // TODO datatable detector should get datatable if we have th cells
    // TODO datatable should also check number of rows - 1 is not a datatable

    [Test]
    public void Render_Returns_SimpleRowCorrectly()
    {
        // arrange
        var unit = new TableBuilder();

        unit.AppendCell();
        unit.Append('A');

        unit.AppendCell();
        unit.Append('B');

        unit.AppendCell();
        unit.Append('C');

        // act
        var result = unit.Build();

        // assert
        Assert.That(result,
            Is.EqualTo("| A | B | C |"));
    }

    [Test]
    public void Render_Returns_MultipleRowsCorrectly()
    {
        // arrange
        var unit = new TableBuilder()
            .AppendRow() // first row
            .AppendCell("A")
            .AppendCell("B")
            .AppendCell("C")
            .AppendRow() // second row
            .AppendCell("D")
            .AppendCell("E")
            .AppendCell("F");

        // act
        var result = unit.Build();

        // assert
        Assert.That(result,
            Is.EqualTo(
                """
                | A | B | C |
                | D | E | F |
                """));
    }

    [Test]
    public void Render_Returns_RowsWithEmptyCells()
    {
        // arrange
        var unit = new TableBuilder()
            .AppendRow() // first row
            .AppendCell()
            .AppendCell("B")
            .AppendCell("C")
            .AppendRow() // second row
            .AppendCell("D")
            .AppendCell()
            .AppendCell("F");

        // act
        var result = unit.Build();

        // assert
        Assert.That(result,
            Is.EqualTo(
                """
                |   | B | C |
                | D |   | F |
                """));
    }

    [Test]
    public void Render_Returns_TableHeadersWithRowSeparator()
    {
        // arrange
        var unit = new TableBuilder()
            .AppendRow() // header row
            .AppendHeaderCell("A")
            .AppendHeaderCell("B")
            .AppendHeaderCell("C")
            .AppendRow() // data row
            .AppendCell("1")
            .AppendCell("2")
            .AppendCell("3");

        // act
        var result = unit.Build();

        // assert
        Assert.That(result,
            Is.EqualTo(
                """
                | A | B | C |
                | - | - | - |
                | 1 | 2 | 3 |
                """));
    }

    [Test]
    public void Render_Returns_ColumnsFormattedToContentWidth()
    {
        // arrange
        var unit = new TableBuilder()
            .AppendRow() // header row
            .AppendHeaderCell("Column A")
            .AppendHeaderCell("Column B")
            .AppendHeaderCell("Column C")
            .AppendRow() // data row
            .AppendCell("blorg")
            .AppendCell("fester")
            .AppendCell("framistan-bedoulia");

        // act
        var result = unit.Build();

        // assert
        Assert.That(result,
            Is.EqualTo(
                """
                | Column A | Column B | Column C           |
                | -------- | -------- | ------------------ |
                | blorg    | fester   | framistan-bedoulia |
                """));
    }

    [Test] public void Render_Returns_RowsPaddedToMaxColumnCount()
    {
        // arrange
        var unit = new TableBuilder()
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

        // act
        var result = unit.Build();

        // assert
        Assert.That(result,
            Is.EqualTo(
                """
                | Column A | Column B | Column C |            |
                | -------- | -------- | -------- | ---------- |
                | data 1   | data 2   | data 3   |            |
                | data 4   | data 5   | data 6   | extra data |
                """));
    }

    [Test]
    public void Render_Returns_RowsIncludingEmpty()
    {
        // arrange
        var unit = new TableBuilder()
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

        // act
        var result = unit.Build();

        // assert
        Assert.That(result,
            Is.EqualTo(
                """
                | Column A | Column B | Column C |
                | -------- | -------- | -------- |
                |          |          |          |
                | second 1 | second 2 | second 3 |
                """));
    }

    [Test]
    public void Render_Returns_ColumnsIncludingEmpty()
    {
        // arrange
        var unit = new TableBuilder()
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

        // act
        var result = unit.Build();

        // assert
        Assert.That(result,
            Is.EqualTo(
                """
                | Column A | Column B |   | Column D |
                | -------- | -------- | - | -------- |
                | first 1  | first 2  |   | first 4  |
                | second 1 | second 2 |   | second 4 |
                """));
    }

    [Test]
    public void Render_Returns_MultipleHeaderRows_IfThatIsWhatYouReallyReallyWant()
    {
        // arrange
        var unit = new TableBuilder()
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

        // act
        var result = unit.Build();

        // assert
        Assert.That(result,
            Is.EqualTo(
                """
                | Header A | Header B | Header C |
                | -------- | -------- | -------- |
                | data 1   | data 2   | data 3   |
                | data 4   | data 5   | data 6   |
                | Header D | Header E | Header F |
                | -------- | -------- | -------- |
                | data 7   | data 8   | data 9   |
                | data 10  | data 11  | data 12  |
                """));
    }
}