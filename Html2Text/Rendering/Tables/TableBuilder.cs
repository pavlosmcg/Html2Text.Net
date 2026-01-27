using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Html2Text.Extensions;

namespace Html2Text.Rendering.Tables;

internal class TableCell(bool isHeader)
{
    public readonly StringBuilder Text = new StringBuilder();
    public bool IsHeader { get; } = isHeader;
}

internal class TableBuilder(IBufferWriter<char> output)
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

    public TableBuilder Append(string? s)
    {
        if (s == null) return this;
        foreach (var c in s) Append(c);
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

    public TableBuilder AppendCell(string text)
    {
        BeginCell(false);
        Append(text);
        return this;
    }

    public TableBuilder AppendHeaderCell(string text)
    {
        BeginCell(true);
        Append(text);
        return this;
    }

    public void Build()
    {
        if (HasCaption)
        {
            output.Append(_caption);
            output.AppendLine();
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
            var row = _rows[rowIndex];
            for (int columnIndex = 0; columnIndex < row.Count; columnIndex++)
            {
                columnWidths[columnIndex] = Math.Max(columnWidths[columnIndex], row[columnIndex].Text.Length);
            }
        }

        for (int r = 0; r < _rows.Count; r++)
        {
            WriteRow(r, _rows[r], columnWidths);
        }
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

    private void WriteRow(int rowIndex, List<TableCell> row, int[] columnWidths)
    {
        // if any cells are headers, format whole row
        bool isHeaderRow = row.Any(c => c.IsHeader);

        // write out row contents
        FlushPendingNewLine();
        output.Append('|');
        for (int columnIndex = 0; columnIndex < _columnCount; columnIndex++)
        {
            output.Append(' ');
            WriteFormattedCellText(rowIndex, columnIndex, columnWidths);
            output.Append(' ');
            output.Append('|');
        }
        QueueNewLine();

        if (!isHeaderRow) return;

        // write out header separator
        FlushPendingNewLine();
        output.Append('|');
        for (int columnIndex = 0; columnIndex < _columnCount; columnIndex++)
        {
            output.Append(' ');
            output.AppendRepeated('-', columnWidths[columnIndex]);
            output.Append(' ');
            output.Append('|');
        }
        QueueNewLine();
    }

    private void WriteFormattedCellText(int rowIndex, int columnIndex, int[] columnWidths)
    {
        int targetWidth = columnWidths[columnIndex];
        if (columnIndex < _rows[rowIndex].Count)
        {
            var cell = _rows[rowIndex][columnIndex];
            output.Append(cell.Text);
            if (cell.Text.Length < targetWidth)
            {
                output.AppendRepeated(' ', targetWidth - cell.Text.Length);
            }
        }
        else
        {
            output.AppendRepeated(' ', targetWidth);
        }
    }

    private void QueueNewLine()
    {
        _pendingNewLine = true;
    }

    private void FlushPendingNewLine()
    {
        if (_pendingNewLine)
        {
            output.AppendLine();
            _pendingNewLine = false;
        }
    }
}
