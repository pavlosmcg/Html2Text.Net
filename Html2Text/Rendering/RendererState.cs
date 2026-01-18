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

    // formatting helpers based on context
    public bool InsideVerbatimBlock => VerbatimTagDepth > 0;
    public bool InsideList => ListNestingDepth > 0;
    public bool InsideTable => TableNestingDepth == 1;  // we only format top level tables
}