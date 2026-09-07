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
