# Html2Text.Net

[![Live Demo](https://img.shields.io/badge/Live_Demo-🚀-blue)](https://html2text.net)
[![CI](https://github.com/pavlosmcg/Html2Text.Net/actions/workflows/CI.yml/badge.svg)](https://github.com/pavlosmcg/Html2Text.Net/actions/workflows/CI.yml)
[![Benchmarks](https://img.shields.io/badge/benchmarks-charts-blue)](https://pavlosmcg.github.io/Html2Text.Net/dev/bench/)
[![NuGet](https://img.shields.io/nuget/v/Html2Text.Net.svg)](https://www.nuget.org/packages/Html2Text.Net/)
[![License](https://img.shields.io/github/license/pavlosmcg/Html2Text.Net)](LICENSE.txt)

**Just fast HTML -> plain text.**

Lightweight, hand rolled, high-performance HTML to plain text conversion for .NET.

- [Usage](#usage)
- [Install, build, test](#install-build-test)
- [How it works](#how-it-works)
  - [Goals](#goals)
- [Performance notes](#performance-notes)
- [Regression tests](#regression-tests)

## Usage

Simple as possible:
```csharp
using Html2Text;

string html = "<h1>Hello</h1><p>World</p>";

string text = Html2Text.Convert(html);
```

Output:
```text
Hello

World
```

Check out the [LIVE DEMO](https://html2text.net)!

## Install, build, test, contribute

[![NuGet](https://img.shields.io/nuget/v/Html2Text.Net.svg)](https://www.nuget.org/packages/Html2Text.Net/)

Install using NuGet (recommended):
```
dotnet add package Html2Text.Net
```

### Supported frameworks
- .Net 8+
- .Net Framework 4.6.2+
- .Net Standard 2.0 for compatibility with other frameworks, including .Net 5/6/7

For .Net Framework users, PackageReference style dependencies are recommended. Also ensure binding redirects are enabled.

### Contributing

Contributions and pull requests are welcome! With .Net 10 SDK installed, to build locally: 
```
dotnet build
```

To run unit and regression tests: 
```
(windows): dotnet test
(linux/mac): dotnet test -f net10.0
```

To run the example console app:
```
dotnet build
dotnet run --project Html2Text.Example Samples/scottallen.html
```

## Use cases

- **Search / indexing pipelines**: Strip HTML down to text for full-text search, indexing, classification, or deduping.
  - Example: convert HTML to text before indexing in Elasticsearch / OpenSearch
- **Batch processing**: Convert large archives of HTML (docs, KB articles, CMS exports) into text efficiently.
- **Email & notification processing**: Get a readable text version of HTML emails for previews, logs, or plain-text fallbacks.
- **LLM / NLP preprocessing**: Normalize HTML into clean text before chunking, embedding, or extraction.
- **Logging / auditing**: Store a text representation of HTML content for review or compliance.

## How it works

### Pipeline
```
HTML document -> Lexer (tokens) -> Parser (AST nodes) -> Renderer (string text)
```

- Text nodes are emitted in document order.
- Basic block separation is preserved (e.g., paragraphs/headings insert newlines).
- Whitespace is normalized to produce readable plain text.

Minimal formatting is added to make the plain text output readable in only 4 cases:
1. HTML tables are given cell separators `|` and horizontal lines `---` under column headers:
```text
| Chart                  | Record Holder     | Record       |
| ---------------------- | ----------------- | ------------ |
| Opening Days           | Avengers: Endgame | $157,461,641 |
| Top Single Day Grosses | Avengers: Endgame | $157,461,641 |
```
2. Lists and nested lists are indented and given a leading `-` like so:
```text
 - 1 Early life
 - 2 Enigma machine
 - 3 Solving the wiring
 - Toggle Solving the wiring subsection
   - 3.1 French help
 - 4 Solving daily settings
 - Toggle Solving daily settings subsection
   - 4.1 Early methods
   - 4.2 Bomba and sheets
   - 4.3 Allies informed
```
3. In preformatted areas `<pre>` whitespace is preserved:
```text
private int GetSmallestNonNegative(int x, int y) {
    return x < 0 && y < 0 ? 0
        : x < 0 ? y
        : y < 0 ? x
        : Math.Min(x, y);
}
```
4. The `<hr/>` element adds a horizontal line of dashes `---`.

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

![perf-test-console](https://raw.githubusercontent.com/pavlosmcg/Html2Text.Net/master/perftests-console.png)
![perf-test-chart](https://raw.githubusercontent.com/pavlosmcg/Html2Text.Net/master/perftests-chart.png)

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

_Distributed under MPL-2.0 see [LICENSE.txt](LICENSE.txt)_