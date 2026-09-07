# Modernization Log

The repository sat untouched from June 2021 until the .NET 10 modernization. This is what changed,
phase by phase, and what the work turned up.

Each phase is one commit on `master`.

## Phase 0 — characterization tests

No production code changed. The point was to pin **current** behaviour, defects included, before
touching anything.

A `net10.0` xUnit v3 project with bUnit was added, plus `PublicApi.approved.txt` as the additive-only
baseline. A spike settled what was testable and what was not:

| Library | Could it be exercised before migration? |
|---|---|
| Diff | Yes — renders under bUnit despite being built against Components 3.1.15 |
| Split | Yes |
| TestHost | **No** — throws `MissingFieldException` |
| StreamSaver | **No** — `net5.0` only, unreferenceable from `net10.0` |
| FFmpeg | **No** — same |

So StreamSaver went into the migration with the API baseline as its only safety net. That was stated
rather than papered over, and Phase 1 closed the gap.

Seven defects were pinned. Six were spotted by reading the code; **one was not**: `Blazorme.TestHost`
1.0.0 was never actually compiled against .NET 5 (see [Design Notes](Design-Notes#blazormetesthost)).

## Phase 1 — .NET 10

All six projects moved from `netstandard2.1`/`net5.0` to `net10.0`. The public surface came through
**byte-identical**, proven by the baseline rather than asserted.

Two things went better than planned:

- **TestHost was fixed by the retarget alone**, with no source change. Both its defect pins were
  rewritten as `FIXED_` regressions.
- **StreamSaver became testable**, gaining nine tests where Phase 0 could give it none.

One thing was harder: the phase could not reach a green build without replacing StreamSaver's dead
interop, because `DemoApp` references it and a `net5.0` app cannot reference `net10.0` libraries.
That work was pulled forward deliberately rather than leaving master unbuildable.

**A live bug was found and fixed in passing**: `WriteAsync` ignored `offset`/`count`, so every
download had trailing bytes from a pooled buffer appended.

The vendored `System.Private.Runtime.InteropServices.JavaScript.dll` was deleted.

## Phase 4 — code improvement

Four of the seven pinned defects fixed, each defect test rewritten in place:

| Defect | Fix |
|---|---|
| `Diff` fetched its html twice per mount | Dropped the duplicate `OnInitializedAsync` override |
| `Split` overwrote its own `Cursor` parameter | Cursor derived at point of use |
| Culture-sensitive `ToLower()` | `ToLowerInvariant()` |
| `SplitPane` threw a bare `Exception` | `InvalidOperationException` |

`Nullable` was enabled on all five libraries — 24 warnings, all fixed rather than suppressed, and no
baseline change, since the annotations are metadata-only. `WritableFileStream` was brought in line
with the `Stream` contract.

This phase produced the **only approved baseline change** of the modernization: two `protected`
lifecycle overrides removed, because deleting them is the fix.

## Phase 5 — package metadata

All four packages moved to date-based `26.09.07` (NuGet: `26.9.7`) from `1.0.x`, and gained
`PackageReadmeFile`, `PackageProjectUrl` and a current copyright. Verified by unzipping the actual
`.nupkg` files rather than trusting the build log.

Release notes said "Added .NET 5 support.", "Creation" and "First version". They now name the
specific defect each release fixes — bluntly, in TestHost's case, because its broken 1.0.0 is still
on nuget.org.

`Blazorme.FFmpeg` was marked `IsPackable=false`; it had been quietly producing a `.nupkg` on every
build despite never being published.

Two claims were corrected rather than repeated: StreamSaver is no longer described as WASM-only, and
the root README no longer says "three component libraries" while listing four.

## Phase 6 — CI and publishing

`ci.yml` and `publish.yml` added; `GeneratePackageOnBuild` removed now that CI owns packaging. See
[Publishing](Publishing).

## What is still open

- **The demo site is five years stale** and deployed from a different repository by hand. There is
  no Pages workflow here.
- **`Blazorme.FFmpeg` is an empty stub** — `IFFmpeg` has no members. It builds and is excluded from
  packaging; nothing more.
- **`Blazorme.TestHost` is superseded by bUnit**, which grew out of the same prototype. It works on
  .NET 10 now, and its README and description say to prefer bUnit for new work.
- **The two `PERMANENT_` warts** — the `FFmpegerviceExtensions` typo and the public `_Imports`
  classes — need a major version to fix, if ever.
- **No XML documentation**, so `GenerateDocumentationFile` stays off.
