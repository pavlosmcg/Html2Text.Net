using Html2Text.Rendering.Tables;

namespace Html2Text.Tests;

internal static class TableBuilderExtensions
{
    public static TableBuilder AppendHeaderCell(this TableBuilder tableBuilder, string content)
    {
        tableBuilder.AppendCell(content, true);
        return tableBuilder;
    }

    public static TableBuilder AppendCell(this TableBuilder tableBuilder, string content, bool isHeader = false)
    {
        tableBuilder.AppendCell(isHeader);
        foreach (char c in content)
        {
            tableBuilder.Append(c);
        }
        return tableBuilder;
    }
}