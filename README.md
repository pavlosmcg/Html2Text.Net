# Html2Text.Net

[![CI](https://github.com/pavlosmcg/Html2Text.Net/actions/workflows/CI.yml/badge.svg)](https://github.com/pavlosmcg/Html2Text.Net/actions/workflows/CI.yml)
[![Benchmarks](https://img.shields.io/badge/benchmarks-charts-blue)](https://pavlosmcg.github.io/Html2Text.Net/dev/bench/)
[![License](https://img.shields.io/github/license/pavlosmcg/Html2Text.Net)](LICENSE.txt)

**Just fast HTML -> plain text.**

Lightweight, hand rolled, high-performance HTML to plain text conversion for .NET.

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

<img width="1670" height="836" alt="image" src="https://github.com/user-attachments/assets/2bdcca01-644f-4053-9b32-ff20ee475f9b" />

## How it works

### Pipeline
```
HTML document -> Lexer (tokens) -> Parser (AST nodes) -> Renderer (string text)
```

- Text nodes are emitted in document order.
- Basic block separation is preserved (e.g., paragraphs/headings insert newlines).
- Whitespace is normalized to produce readable plain text.

Minimal formatting is added to make the plain text output readable:
- HTML tables are given cell separators (|) and horizontal lines (---) under column headers .
- The `<hr/>` element adds a horizontal line of dashes (---).
- The `<title>` element also gets a horizontal underline.

Formatting logic can be found in [Html2Text/Rendering](Html2Text/Rendering). 

### Goals
This project is focused on:
- High performance: designed for low allocations and fast throughput.
- Text extraction only: get the words from the page/document.
- No dependencies: Lightweight, not an embedded browser engine. No dependencies other than .NET itself.

### Non-goals (by design)
The following are intentionally out of scope so the library can excel at the goals above:
- Respecting CSS, computed styles, `display:none`, or visibility.
- Pixel-accurate layout, whitespace mirroring, or browser-equivalent rendering.
- Executing JavaScript or loading remote resources.

## Performance notes

[![Benchmarks](https://img.shields.io/badge/benchmarks-charts-blue)](https://pavlosmcg.github.io/Html2Text.Net/dev/bench/)

High performance is a goal of this project. This library:
- designed for converting many documents quickly (batch processing, indexing, search pipelines).
- avoids DOM dependencies.
- uses a lightweight, hand rolled lexer/parser/renderer pipeline.

Benchmarks are in `Html2Text.PerfTests` and can be run locally with:
```
dotnet run -c Release --project Html2Text.PerfTests
```

Or check out the latest automated perf test results here: https://pavlosmcg.github.io/Html2Text.Net/dev/bench/

<img width="1028" height="487" alt="image" src="https://github.com/user-attachments/assets/4cbea709-71a2-4bee-a29a-d9493b0841da" />

<img width="1638" height="732" alt="Screenshot 2026-01-18 213121" src="https://github.com/user-attachments/assets/43cea001-ab45-4ddd-9cd2-27396d80dbfe" />

## Install, build, test

When I've published to NuGet (coming soon!), you will be able to:
```
dotnet add package Html2Text
```

Or, for now, download or submodule the repo and reference the project directly.

Build with: 
```
dotnet build
```

Run unit tests and regression tests: 
```
dotnet test
```

## Regression tests

Each file in the `Samples/` directory acts as an acceptance/regression test. The results of converting these HTML files to plain text are saved in `Html2Text.RegressionTests/*.verified.txt`:

```
Samples/<file-name>.html -> Html2Text.Convert(<file-contents>) -> <file-name>.verified.txt
```

For example [scottallen.html](Samples/scottallen.html) -> [scottallen.verified.txt](Html2Text.RegressionTests/Html2TextTests.Html2Text_Returns_ExpectedOutputFor_filePath=scottallen.verified.txt)

`Html2Text.RegressionTests` uses [Verify](https://github.com/VerifyTests/Verify) to make test assertions against verified output snapshots. If you need to update the outputs please see the Verify docs for snapshot management.

## Projects in this repository

- `Html2Text/`: core library
- `Html2Text.Example/`: small example app
- `Html2Text.Tests/`: unit tests
- `Html2Text.RegressionTests/`: regression/acceptance tests
- `Html2Text.PerfTests/`: performance benchmarking console app
- `Samples/`: sample HTML files used during development and automated regression testing

## Target frameworks

- .NET 8+

## License

MPL-2.0 see [LICENSE.txt](LICENSE.txt)
