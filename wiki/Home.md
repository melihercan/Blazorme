# Blazorme

Utility component libraries for Blazor apps, published to NuGet as independent packages.

| Package | Version | Targets | Docs |
|---|---|---|---|
| [Blazorme.Diff](https://www.nuget.org/packages/Blazorme.Diff) | 26.9.7 | `net10.0` | [README](https://github.com/melihercan/Blazorme/blob/master/Diff/README.md) |
| [Blazorme.Split](https://www.nuget.org/packages/Blazorme.Split) | 26.9.7 | `net10.0` | [README](https://github.com/melihercan/Blazorme/blob/master/Split/README.md) |
| [Blazorme.StreamSaver](https://www.nuget.org/packages/Blazorme.StreamSaver) | 26.9.7 | `net10.0` | [README](https://github.com/melihercan/Blazorme/blob/master/StreamSaver/README.md) |
| [Blazorme.TestHost](https://www.nuget.org/packages/Blazorme.TestHost) | 26.9.7 | `net10.0` | [README](https://github.com/melihercan/Blazorme/blob/master/TestHost/README.md) |

The libraries are unrelated and share no code. `FFmpeg/` is an unfinished stub and is not published.

**Per-package usage — install, parameters, examples — lives in each package's README**, which is
also what ships inside the `.nupkg` and shows on nuget.org. This wiki deliberately does not repeat
it; two copies of the same install instructions drift. The wiki covers what the READMEs do not: how
the repository is built and tested, the invariants a change must preserve, and how releases happen.

## The libraries in one line each

**Blazorme.Diff** renders the difference between two strings, inline, line-by-line, or side-by-side.
The same work is available from code behind through `IDiff`.

**Blazorme.Split** provides resizeable split panes, wrapping
[split.js](https://github.com/nathancahill/split).

**Blazorme.StreamSaver** wraps
[StreamSaver.js](https://github.com/jimmywarting/StreamSaver.js) so a download is written chunk by
chunk straight to the file, instead of being accumulated in memory.

**Blazorme.TestHost** packages Steve Sanderson's
[BlazorUnitTestingPrototype](https://github.com/SteveSandersonMS/BlazorUnitTestingPrototype) for
component unit testing. [bUnit](https://github.com/bUnit-dev/bUnit) grew out of the same prototype
and is the maintained option — prefer it for new test suites.

## Repository

- [Building and Testing](Building-and-Testing) — commands, and the `dotnet test` setup traps.
- [Design Notes](Design-Notes) — the additive-only API guarantee, how it is enforced, and the
  warts that are staying.
- [Publishing](Publishing) — Trusted Publishing, the tag scheme, and the version-spelling trap.
- [Modernization Log](Modernization-Log) — what the 2021 → .NET 10 modernization changed, and what
  it found along the way.

## Requirements

**.NET 10.** Releases before 26.9.7 targeted `netstandard2.1` and `net5.0`; those are no longer
supported. Because `26.9.7` sorts above `1.0.2`, it is the version NuGet resolves by default,
including for projects still on the old frameworks, where it will not restore.

## Licence

MIT.
