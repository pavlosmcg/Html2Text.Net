# Html2Text.Net

Just fast HTML -> Text

Lightweight, hand rolled, high-performance HTML to plain text conversion for .NET.

This library focuses on extracting the text content of a page as quickly and predictably as possible. No attempt is undertaken to interpret layout, CSS, visibility, or rendering rules, other than applying some basic formatting for readability to table headings and table data rows to make them look nice in plain text.

## Goals

- **High performance**: designed for low allocations and fast throughput.
- **Text extraction only**: get the words from the page/document.
- **No dependencies**: Lightweight, not an embedded browser engine. No dependencies other than .NET itself.

## Out of scope

- Respecting CSS, computed styles, `display:none`, or visibility.
- Pixel-accurate layout, whitespace mirroring, or browser-equivalent rendering.
- Executing JavaScript or loading remote resources.

## Target frameworks

- .NET 8+

## Install

When I've published to NuGet (coming soon!), you will be able to:

- `dotnet add package Html2Text`

Or, for now, download or submodule the repo and reference the project directly.

## Usage

Simple as possible:
```csharp
using Html2Text;

string html = "<h1>Hello</h1><p>World</p>";

string text = Html2Text.Convert(html);

// Hello
//
// World
```

## Output rules (high-level)

- Text nodes are emitted in document order.
- Basic block separation is preserved (e.g., paragraphs/headings insert newlines).
- Whitespace is normalized to produce readable plain text.

Exact behavior is defined by the classes in `Html2Text\Rendering`.

## Performance notes

- Designed for converting many documents quickly (batch processing, indexing, search pipelines).
- Avoids DOM dependencies.
- uses a lightweight, hand rolled lexer/parser/renderer pipeline.

Benchmarks are in `Html2Text.PerfTests`.

## Projects in this repository

- `Html2Text/`: core library
- `Html2Text.Tests/`: unit tests
- `Html2Text.Example/`: small example app
- `Html2Text.PerfTests/`: benchmarks
- `Samples/`: sample HTML files used for testing/manual inspection

## Build & test commands

Build with: `dotnet build`

Run unit tests: `dotnet test`

Run performance benchmarks: `dotnet run -c Release --project Html2Text.PerfTests`

## License

MPL-2.0 see [LICENSE.txt](LICENSE.txt)
