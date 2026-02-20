# Using Html2Text MCP Server with LM Studio

This guide explains how to install and configure the **Html2Text MCP Server** for use with **LM Studio**.

The server runs locally and allows LM Studio to convert HTML to plain text via structured tool calls.

## Download the Binary

Go to the [GitHub Releases](https://github.com/pavlosmcg/Html2Text.Net/releases) page and download the archive for your platform:

- `html2text-mcp-win-x64.zip` (Windoze)
- `html2text-mcp-linux-x64.tar.gz` (Linux)
- `html2text-mcp-osx-arm64.tar.gz` (MacOS)

Extract the archive.

You should now have:

- Windows: `html2text-mcp.exe`
- Linux/macOS: `html2text-mcp`

## Place the Binary on Your PATH

LM Studio launches MCP servers by running a command.  
That command must be available on your system `PATH`.

### Windows

Windoze does not include a `bin` directory by default, so you have to create one, e.g. `C:\Tools\bin`. Add it to your path through the usual **Edit the system environment variables** malarkey. Then copy `html2text-mcp.exe` into that directory.

To test it, run:

```powershell
html2text-mcp
```

### Linux and MacOS

Move the binary to a common user bin directory, e.g.:

```bash
chmod +x html2text-mcp
mkdir -p ~/.local/bin
mv html2text-mcp ~/.local/bin/
```

Ensure `~/.local/bin` is on your PATH.
Restart your terminal and test:

```bash
html2text-mcp
```

## Configure LM Studio

Update LM Studio's `mcp.json` configuration file to include the following (note the slight difference in naming for discoverability, "2" -> "To", but you are free to call this what you like):

```json
{
  "mcpServers": {
    "HtmlToText": {
      "command": "html2text-mcp"
    }
  }
}
```

The file is usually found in either `~/.lmstudio/mcp.json` or `%USERPROFILE%\.lmstudio\mcp.json` on Windows and there is now a UI button for it on the developer tab (CTRL+2).

## Verify it works

1. Restart **LM Studio** (if it was already running).
2. Open a new chat session. Load your favourite model trained for tool use.
3. Enable the tool `mcp/html-to-text` by clicking the "Integrations" hammer icon.
4. Enter:
   ```text
   Convert this HTML to text please: <h1>Hello</h1><p>World</p>
   ```
5. LM Studio should automatically:
   - Discover the `mcp/html-to-text` tools, `convert_html_to_text` and `convert_html_file_to_text`
   - Call the `convert_html_to_text` method
   - Return the plain text output

   Expected result:

   ```text
   Hello
   World
   ```

If the response appears without errors, the MCP server is configured correctly.

## The tools

There are currently two methods in the MCP server project, which are the tools exposed when you hook it up:

- `convert_html_to_text`: This method takes a string and returns a string. Fine for small amounts of html, since it requires dealing with the input document and the returned output in the agent's context window.
- `convert_html_file_to_text`: This method takes a file name and writes the plain text output to a file with a `.txt` extension. This is much more useful for saving token budgets, especially when bulk processing. All that is required is a single file name because conversion and output writing happens inside the MCP tool.
