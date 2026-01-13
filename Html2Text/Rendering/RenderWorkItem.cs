using Html2Text.Parsing;

namespace Html2Text.Rendering;

internal record RenderWorkItem(RenderWorkType WorkType, Node Node);
