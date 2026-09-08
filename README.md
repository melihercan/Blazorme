# Blazorme
Utility component libraries for Blazor apps. Four libraries are published to NuGet:

* [Blazorme.Diff:](Diff/README.md) Renders the diff of two input strings in several output display formats.
It can also be used as a library to get diff or html diff output strings from code behind. [![NuGet](https://img.shields.io/nuget/v/Blazorme.Diff.svg)](https://www.nuget.org/packages/Blazorme.Diff)
* [Blazorme.Split:](Split/README.md) Resizeable split views (panes). [![NuGet](https://img.shields.io/nuget/v/Blazorme.Split.svg)](https://www.nuget.org/packages/Blazorme.Split)
* [Blazorme.StreamSaver:](StreamSaver/README.md) Download files by writing incoming data chunks directly to the file instead of accumulating them in memory, which prevents memory overflows for very large files. [![NuGet](https://img.shields.io/nuget/v/Blazorme.StreamSaver.svg)](https://www.nuget.org/packages/Blazorme.StreamSaver)
* [Blazorme.TestHost:](TestHost/README.md) Helpers for Blazor component unit testing. Consider [bUnit](https://github.com/bUnit-dev/bUnit) first for new test suites. [![NuGet](https://img.shields.io/nuget/v/Blazorme.TestHost.svg)](https://www.nuget.org/packages/Blazorme.TestHost)

`FFmpeg/` is an unfinished stub and is not published.

## Requirements
**.NET 10.** Earlier releases targeted `netstandard2.1` and `net5.0`; those are no longer supported.

## Getting started

Install the packages you want, and add `@using Blazorme` to your `_Imports.razor`.

```powershell
dotnet add package Blazorme.Diff
dotnet add package Blazorme.Split
dotnet add package Blazorme.StreamSaver
```

**Diff and Split need nothing added to `index.html` or `_Host.cshtml`.** Since 26.9.9 they ship
jsdiff, diff2html and Split.js and load them on demand. Older versions required CDN script tags,
and leaving one out failed at runtime with a JS interop error; if your app still has them they stay
harmless, because the loader skips a library whose global is already defined.

### Blazorme.Diff

Register the service in `Program.cs`:

```csharp
builder.Services.AddDiff();
```

Then use the component:

```razor
<Diff FirstInput="the quick brown fox"
      SecondInput="the slow brown fox" />

@* Row and Column also take titles and a word/char style *@
<Diff FirstInput="@first" SecondInput="@second"
      FirstTitle="Before" SecondTitle="After"
      OutputFormat="DiffOutputFormat.Column"
      Style="DiffStyle.Word" />
```

`OutputFormat` is `Inline` (default), `Row` or `Column`. Inline is rendered in process by
htmldiff.net and ignores the titles and style; Row and Column go through diff2html.

The same work is available from code behind by injecting `IDiff`:

```csharp
[Inject] private IDiff Diff { get; set; } = default!;

var patch = await Diff.GetAsync(first, second);
var html  = await Diff.GetHtmlAsync(first, second, "Before", "After",
                                    DiffOutputFormat.Row, DiffStyle.Word);
```

### Blazorme.Split

No service registration needed — just the components:

```razor
<div style="height: 600px">
    <Split GutterSize="5">
        <SplitPane SizeInPercentage="30">Left</SplitPane>
        <SplitPane SizeInPercentage="70">Right</SplitPane>
    </Split>
</div>
```

`Split` takes `Direction`, `GutterSize`, `GutterAlign`, `GutterColor`, `DefaultMinSize`,
`SnapOffset`, `DragInterval`, `ExpandToMin` and `Cursor`; `SplitPane` takes `SizeInPercentage` and `MinSize`.
Nest a `Split` inside a `SplitPane` for a grid.

### Blazorme.StreamSaver

```csharp
builder.Services.AddStreamSaver();
```

**This one does need script tags**, because StreamSaver.js relies on a service worker:

```html
<script src="_content/Blazorme.StreamSaver/polyfill.min.js"></script>
<script src="_content/Blazorme.StreamSaver/StreamSaver.min.js"></script>
```

Then write to the file as an ordinary `Stream`, chunk by chunk — nothing is buffered in memory,
and the download completes when the stream is disposed:

```csharp
[Inject] private IStreamSaver StreamSaver { get; set; } = default!;

await using var file = await StreamSaver.CreateWritableFileStreamAsync("report.csv");
foreach (var chunk in chunks)
{
    await file.WriteAsync(chunk);
}
```

Intended for Blazor WebAssembly. It runs on Blazor Server too, but every chunk crosses the SignalR
circuit, which defeats the point for the large files this exists for.

See each package's README for the full parameter tables.

## Building and testing
```powershell
dotnet build Blazorme.sln
dotnet test
dotnet run --project DemoApp
```

The build is expected to be clean: **0 warnings, 0 errors**.

`dotnet test` runs on Microsoft.Testing.Platform, opted into via `global.json`, because the test
project uses xUnit v3. **Do not pass `--nologo`** — MTP forwards it to the test executable, which
rejects it with exit code 5 and "Zero tests ran".

You can run the [Demo](https://melihercan.github.io/) here.

![alt text](https://github.com/melihercan/Blazorme/blob/master/doc/Blazorme.gif)
