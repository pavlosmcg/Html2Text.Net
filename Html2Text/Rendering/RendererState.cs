namespace Html2Text.Rendering;

internal struct RendererState
{
    public RendererState()
    {
    }

    // position tracking
    public bool AtLineStart { get; set; } = true;
    public bool AtDocumentStart { get; set; } = true;

    // output
    public int PendingNewLines { get; set; } = 0;
    public bool PendingSpace { get; set; } = false;
    public bool HasPendingNewLines => PendingNewLines > 0;

    // depth context
    public int VerbatimTagDepth { get; set; } = 0;
    public int ListNestingDepth { get; set; } = 0;
    public int TableNestingDepth { get; set; } = 0;
    public bool InsideTableCell { get; set; } = false;
    public bool InsideTableCaption { get; set; } = false;

    // formatting helpers based on context
    public bool RenderingVerbatimBlock => VerbatimTagDepth > 0;
    public bool RenderingList => ListNestingDepth > 0;
    public bool RenderingTable => TableNestingDepth > 0;
}
