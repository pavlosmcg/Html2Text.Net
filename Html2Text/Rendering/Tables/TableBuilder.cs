using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Html2Text.Rendering.Tables;

internal class TableCell(bool isHeader)
{
    public readonly StringBuilder Text = new StringBuilder();
    public string GetText() => Text.ToString();
    public bool IsHeader { get; } = isHeader;
}

internal class TableBuilder
{
    private readonly List<List<TableCell>> _rows = new List<List<TableCell>>();
    private List<TableCell>? _currentRow;
    private TableCell? _currentCell;
    private readonly StringBuilder _caption = new();
    private bool _pendingNewLine = false;
    private bool _atCellStart = true;
    private int _columnCount = 0;

    private bool HasCaption => _caption.Length > 0;

    public TableBuilder Append(char c)
    {
        if (_atCellStart && char.IsWhiteSpace(c)) return this;

        _currentCell?.Text.Append(c);
        _atCellStart = false;
        return this;
    }

    public TableBuilder AppendToCaption(char c)
    {
        _caption.Append(c);
        return this;
    }

    public TableBuilder AppendRow()
    {
        BeginRow();
        return this;
    }

    public TableBuilder AppendCell(bool isHeader = false)
    {
        BeginCell(isHeader);
        return this;
    }

    public string Build()
    {
        StringBuilder output = new StringBuilder();

        if (HasCaption)
        {
            output.AppendLine(_caption.ToString());
            output.AppendLine();
        }

        // find column count
        foreach (List<TableCell> row in _rows)
        {
            _columnCount = Math.Max(_columnCount, row.Count);
        }
        int[] columnWidths = Enumerable.Repeat(1, _columnCount).ToArray();

        // find column widths
        for (int rowIndex = 0; rowIndex < _rows.Count; rowIndex++)
        {
            for (int columnIndex = 0; columnIndex < _columnCount; columnIndex++)
            {
                var cellText = GetCellText(rowIndex, columnIndex);
                columnWidths[columnIndex] = Math.Max(columnWidths[columnIndex], cellText.Length);
            }
        }

        for (int r = 0; r < _rows.Count; r++)
        {
            WriteRow(output, r, _rows[r], columnWidths);
        }

        return output.ToString();
    }

    private void BeginRow()
    {
        _currentRow = new List<TableCell>();
        _rows.Add(_currentRow);
    }

    private void BeginCell(bool isHeader = false)
    {
        _atCellStart = true;

        if (_currentRow == null)
        {
            BeginRow();
        }
        _currentCell = new TableCell(isHeader);
        _currentRow!.Add(_currentCell);
    }

    private void WriteRow(StringBuilder output, int rowIndex, List<TableCell> row, int[] columnWidths)
    {
        // if any cells are headers, format whole row
        bool isHeaderRow = row.Any(c => c.IsHeader);

        // write out row contents
        FlushPendingNewLine(output);
        output.Append('|');
        for (int columnIndex = 0; columnIndex < _columnCount; columnIndex++)
        {
            output.Append(' ');
            output.Append(GetFormattedCellText(rowIndex, columnIndex, columnWidths));
            output.Append(' ');
            output.Append('|');
        }
        QueueNewLine();

        if (!isHeaderRow) return;

        // write out header separator
        FlushPendingNewLine(output);
        output.Append('|');
        for (int columnIndex = 0; columnIndex < _columnCount; columnIndex++)
        {
            output.Append(' ');
            output.Append('-', columnWidths[columnIndex]);
            output.Append(' ');
            output.Append('|');
        }
        QueueNewLine();
    }

    private string GetCellText(int row, int column)
    {
        return column < _rows[row].Count
            ? _rows[row][column].GetText()
            : string.Empty;
    }

    private string GetFormattedCellText(int row, int column, int[] columnWidths)
    {
        string text = GetCellText(row, column);
        return PadStringToLength(text, columnWidths[column]);
    }

    private string PadStringToLength(string text, int columnWidth)
    {
        return text.Length > columnWidth
            ? text
            : text.PadRight(columnWidth);
    }

    private void QueueNewLine()
    {
        _pendingNewLine = true;
    }

    private void FlushPendingNewLine(StringBuilder outputBuilder)
    {
        if (_pendingNewLine)
        {
            outputBuilder.AppendLine();
            _pendingNewLine = false;
        }
    }
}
