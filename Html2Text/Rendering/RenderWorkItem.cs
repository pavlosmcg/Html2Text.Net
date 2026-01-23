using Html2Text.Parsing;

namespace Html2Text.Rendering;

internal class RenderWorkItem
{
    internal RenderWorkItem(RenderWorkType workType, Node node)
    {
        WorkType = workType;
        Node = node;
    }

    public RenderWorkType WorkType { get; set; }
    public Node Node { get; set; }
}
