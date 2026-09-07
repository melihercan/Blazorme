# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository layout

Four independent Blazor component libraries, an unfinished stub, a demo app and one test project,
tied together by `Blazorme.sln`:

- `Diff/` → package `Blazorme.Diff`, public namespace `Blazorme` (internals in `BlazormeDiff`)
- `Split/` → package `Blazorme.Split`, public namespace `Blazorme` (internals in `BlazormeSplit`)
- `StreamSaver/` → package `Blazorme.StreamSaver`, public namespace `Blazorme` (internals in `BlazormeStreamSaver`)
- `TestHost/` → package `Blazorme.TestHost`, public namespace `Blazorme` (internals in `BlazormeTestHost`)
- `FFmpeg/` → **unfinished stub, never published, `IsPackable=false`**
- `DemoApp/` → Blazor WebAssembly app referencing Diff, Split, StreamSaver and TestHost
- `Blazorme.Tests/` → xUnit v3 + bUnit tests for the whole solution

The libraries do not reference each other. Note the shared public namespace `Blazorme`: a type added
to one library can collide with another in a consuming app that references both.

## Commands

```powershell
dotnet build Blazorme.sln
dotnet test
dotnet test --project Blazorme.Tests/Blazorme.Tests.csproj
dotnet run --project DemoApp
dotnet pack Diff/Diff.csproj -c Release
```

**`dotnet test` runs in Microsoft.Testing.Platform (MTP) mode**, opted in via the `test.runner`
section of `global.json`. xUnit v3 test projects are executables and the .NET 10 SDK refuses to run
them through the legacy VSTest path, so that file is required — deleting it breaks the suite with
"Testing with VSTest target is no longer supported".

Two consequences worth knowing before you fight the CLI:

- **Do not pass `--nologo`** (or other VSTest-era flags). MTP forwards unrecognised arguments to the
  test executable, which rejects them with exit code 5 / "Zero tests ran" — a failure that looks
  like broken tests but is a bad command line.
- Target a single project with **`--project <path>`**, not a bare path argument.

**Build the solution before running the tests.** `PublicApiSurfaceTests` reads the libraries'
compiled assemblies off disk through `MetadataLoadContext` rather than referencing them, so every
library must have been built in the same configuration first. `dotnet test` at solution level does
this; a CI job that skipped the build step would not.

The build is clean: **0 warnings, 0 errors**, Debug and Release. Keep it that way.

`DemoApp` remains a useful manual check — it exercises all three published components in a browser.

## Packaging and publishing

`GeneratePackageOnBuild` has been **removed** now that CI owns packaging; local builds no longer emit
a `.nupkg`. Use `dotnet pack` explicitly when you want one.

**Packaging is done by GitHub Actions**, in `.github/workflows/`:

- `ci.yml` — build and test on every push and PR to master. Builds with `-warnaserror`, so the
  zero-warning bar is enforced there, and a new NuGet advisory fails the build.
- `publish.yml` — **one workflow for all four packages, deliberately.** A NuGet Trusted Publishing
  policy is bound to a single workflow file, so one file means one policy. Tag `v<version>`
  publishes everything; `diff-v<version>`, `split-v<version>`, `streamsaver-v<version>` or
  `testhost-v<version>` publishes just that package; a manual run defaults to a dry run that packs
  and uploads the `.nupkg` files without publishing. It verifies each tag matches the csproj
  `<Version>`, runs the tests, and pushes with `--skip-duplicate`.

**Publishing uses NuGet Trusted Publishing, not an API key.** The job requests a GitHub OIDC token
(`id-token: write`), and `NuGet/login@v1` exchanges it for an API key valid for one hour. No
publishing secret is stored in the repository — do not reintroduce `NUGET_API_KEY`. The login step
deliberately sits immediately before the push: the key expires, and each OIDC token buys exactly one
key.

The policy on nuget.org names Repository Owner `melihercan`, Repository `Blazorme`, Workflow File
`publish.yml` (file name only, no path), no Environment, and is scoped to the glob `Blazorme.*`.
**Do not split publishing into per-package workflow files** — that would require a policy per file.
The policy binds to the repository *ID*, which is why the `Utilme.*` policy could never cover this
repository.

All four packages set `PackageReadmeFile`, `PackageLicenseExpression`, `PackageProjectUrl` and a
date-based `<Version>` (`26.09.07`, which NuGet normalises to `26.9.7`). **Tag with the csproj
spelling** — `v26.09.07`, not `v26.9.7` — because the workflow compares against the raw csproj text.
`AssemblyVersion` and `FileVersion` are left to derive from `Version`.

None of the libraries set `GenerateDocumentationFile`: their public members are not XML-documented,
so turning it on would emit hundreds of CS1591 warnings and break the zero-warning bar.

## Targeting

Everything targets **`net10.0`**, except `Blazorme.TestHost`, which multi-targets
**`net8.0;net10.0`**. The repo was migrated from `netstandard2.1`/`net5.0`; that history explains
some of the code shape but is no longer a constraint.

TestHost carries net8.0 deliberately. Its 1.0.0 on nuget.org crashes at runtime on anything past
.NET Core 3.1, so a net10.0-only release would leave every project on .NET 5 to .NET 9 resolving
that broken version. The other three libraries do not need this: their 1.0.x releases work fine on
those frameworks, they are merely old.

**Framework packages in TestHost.csproj are declared only inside per-framework `ItemGroup`s.** Never
add an unconditional `PackageReference` for a package that also appears in a conditional group —
that is precisely what broke 1.0.0. `MultiTargetingTests` asserts each shipped asset binds to its
own Components major.

Consequences of the migration, so they are not accidentally reintroduced:

- The vendored `System.Private.Runtime.InteropServices.JavaScript.dll` is **gone**, along with the
  `StreamSaver/Resources/` folder it lived in. It was the .NET 5/6 preview WASM interop, removed in
  .NET 7.
- No `RazorLangVersion`; it follows the SDK default.
- `Nullable` is enabled **everywhere**, including `DemoApp` and the test project.
- Third-party packages are pinned at their pre-migration versions and work fine: `htmldiff.net`
  1.4.0, `Fizzler.Systems.HtmlAgilityPack` 1.2.1, `RichardSzalay.MockHttp` 6.0.0, `Markdig` 0.24.0.
  Framework packages are on 10.0.11, and on 8.0.x for TestHost's net8.0 assets.

## Backward compatibility

All four packages are under an **additive-only guarantee**: real apps depend on them, so no public
member may be renamed, removed, or have its type changed. New capability is added *beside* the old
surface, never in place of it.

`PublicApiSurfaceTests` enforces this against `Blazorme.Tests/PublicApi.approved.txt`, which captures
every public type, member signature, generic arity, enum numeric value, `const` literal and default
parameter value across all five libraries. When a change is intentional, review the diff and copy
`PublicApi.received.txt` from the test output directory over `PublicApi.approved.txt`. **Never
weaken the assertion.**

It reads metadata only, via `MetadataLoadContext`, because two libraries could not be referenced from
a `net10.0` test project before the migration and a third could be referenced but threw on use.

Nullable annotations are metadata-only and produce **no** baseline change; that is the expected
outcome and is how the nullable pass stayed binary compatible.

Exactly one removal has been approved: the `protected override` lifecycle hooks
`Diff.OnInitializedAsync()` and `Split.OnInitialized()`. Deleting them *is* the fix for two defects,
neither is callable by a consumer, and a subclass calling `base.OnInitialized()` still dispatches to
`ComponentBase`.

Two warts are **permanent** and pinned as such — do not "fix" them:

- **`FFmpegerviceExtensions`** — the `S` of "Service" was eaten. It is a public type name. A
  correctly spelled type may be added beside it.
- **The generated `_Imports` classes** are public in every Razor library. Razor offers no knob to
  make them internal.

## Blazorme.Tests

One project covers all five libraries. `bunit` 2.9.0 renders components; note that bUnit v2's base
class is `BunitContext`, and `Bunit.TestContext` collides with `Xunit.TestContext`.
FluentAssertions is pinned to **7.x** — 8.x requires payment for commercial use.

| File | Covers |
|---|---|
| `PublicApiSurfaceTests` | The whole public surface against the approved baseline. |
| `PublicApiDumper` / `TestAssemblies` | The machinery: metadata-only reflection, configuration-aware assembly lookup. |
| `DiffCharacterizationTests` | `DiffApi` over a mocked `IJSRuntime`; the JS argument order. |
| `SplitCharacterizationTests` | Rendering, the split.js options object, cursor derivation, pane sizing. |
| `StreamSaverTests` | Module import, writer creation, the write window, disposal. |
| `TestHostCharacterizationTests` | That TestHost renders, and that its Fizzler selector layer works. |
| `DemoAppTests` | The demo pages, driven through bUnit against a recording stream. |
| `KnownDefectTests` | One test per known defect. |

### The defect-test convention

`KnownDefectTests` holds one test per defect, and the prefix carries the status:

- **`FIXED_`** — was a defect pin, **rewritten in place** when fixed, with a comment naming the pin
  it replaced. Rewrite these, never delete them, so a fix shows in the diff.
- **`LIMITATION_`** — not a bug in this code.
- **`PERMANENT_`** — cannot be fixed without breaking the public API.

A red test elsewhere means behaviour changed; acceptable only if intended, in which case update the
test in the same commit.

### Dispatch demo-page events with the async helpers

`DemoAppTests` uses `ClickAsync` / `ChangeAsync` and awaits every one. The synchronous helpers
return once the handler yields, and `StreamSaverDemo.AppendAsync` ends with `await Task.Delay(1)`,
so a synchronous `Click` lets the next interaction race the tail of the previous one. Written that
way the suite failed about one run in eight.

## Component conventions

**Do not override both `OnInitializedAsync` and `OnParametersSetAsync` with the same work.** Blazor
sets parameters before `OnInitializedAsync` and runs `OnParametersSetAsync` immediately after, so
overriding both makes every mount do the work twice. `Diff` did exactly that.

**Do not assign to your own `[Parameter]` properties.** Blazor owns them, and `OnInitialized` runs
once, so a default computed there goes stale. Derive such values at the point of use — see
`Split.EffectiveCursor`.

**Lowercase with `ToLowerInvariant`** for anything the JS side matches exactly. `ToLower()` is
culture sensitive and Turkish maps `I` to a dotless lowercase form.

## Blazorme.Diff

`DiffApi` splits two ways. `DiffOutputFormat.Inline` goes through **htmldiff.net in process** and
never touches JS; `Row` and `Column` call `Diff.createTwoFilesPatch` then `Diff2Html.html`. The JS
argument order is titles **before** inputs, which is not the C# parameter order.

Inline silently ignores both the titles and `DiffStyle`, because htmldiff.net has no concept of
either. That is pinned as a `LIMITATION_`, not a defect. An out-of-range `DiffOutputFormat` returns
an empty string rather than throwing.

The component renders its result as a `MarkupString`, i.e. **unescaped HTML**. That is the point of
a diff renderer, but it means the inputs are trusted.

Consumers must add the diff2html CSS and the `diff` / `diff2html` scripts themselves; the package
ships no static web assets.

## Blazorme.Split

Wraps split.js. `Split` cascades itself to `SplitPane` children, which register on
`OnInitialized`; a `SplitPane` outside a `Split` throws `InvalidOperationException`.

JS is invoked **once, on first render only**, from `OnAfterRenderAsync`, with `[elements, options]`.
Changing parameters afterwards does not re-invoke it.

Note the casing asymmetry: the `data-direction` attribute carries the enum name as-is
(`Horizontal`), because the CSS selectors in `Split.razor` match that, while the options object
sends lowercase, because split.js expects that. Both are deliberate.

`Options.Sizes` is null when no pane declares a size, which split.js reads as "distribute evenly".
If any pane declares one, the array is taken verbatim — a second pane left at 0 stays 0.

Consumers must add the split.js script themselves; the package ships no static web assets.

## Blazorme.StreamSaver

Interop goes through `wwwroot/StreamSaverJsInterop.js`, imported as an ES module via
`IJSObjectReference`. `StreamSaver.min.js` and `polyfill.min.js` are classic scripts that assign a
global and **cannot** be imported as modules, so the host page still loads them with `<script>` tags
and the interop module wraps the global. That is why there are three JS files.

**Do not reintroduce `System.Private.Runtime.InteropServices.JavaScript`.** It does not exist on
.NET 10.

`byte[]` marshals to a real `Uint8Array` on .NET 6+, which is what defeated the original 2021
attempt (it passed base64).

`WriteAsync(byte[], int, int)` copies exactly `count` bytes from `offset`.
`Stream.WriteAsync(ReadOnlyMemory<byte>)` supplies an **oversized pooled buffer**, so sending
`buffer` wholesale appends trailing garbage to every download. Do not "simplify" that back.

The library is no longer WASM-only — module interop works on Blazor Server — but every chunk crosses
the SignalR circuit there, which is a poor fit for large transfers.

## Blazorme.TestHost

Steve Sanderson's `BlazorUnitTestingPrototype`, packaged. **bUnit grew out of the same prototype and
is the maintained option**; the README and package description say to prefer it for new work.

Version 1.0.0 was broken on every framework it claimed to support. `TestHost.csproj` declared
`Microsoft.AspNetCore.Components` 3.1.10 unconditionally *and* 5.0.0 for `net5.0`; duplicate
`PackageReference` items do not merge, the first wins, and the conditional one is silently discarded
(NU1504). The shipped IL was bound to 3.1, whose `RenderTreeFrame` exposed public **fields** that
became **properties** in .NET 5, so rendering threw `MissingFieldException`.

The repair needed **no source change** — the C# was always written `frame.FrameType`, which binds to
a property as well as to a field. NU1504 was never cosmetic here; it was the bug.

`RenderedComponent.Find` returns `HtmlNode?`; it is `FirstOrDefault` underneath.
`AddComponent(IDictionary<string, object>)` keeps its exact parameter type — widening it to
`object?` would change the IL signature — so it uses `!` at the call site.

## The demo site

`https://melihercan.github.io/` comes from the separate `melihercan/melihercan.github.io`
repository — a GitHub user site must live at that repository's root. Deploy with
**`./deploy-demo.ps1`**, which publishes `DemoApp`, replaces the site contents and leaves the result
staged; it never commits or pushes. There is no CI deployment on purpose, because writing to another
repository from Actions requires a stored credential and this repository keeps none.

`DemoApp/wwwroot` carries `.nojekyll` and `404.html`, and `index.html` carries the
spa-github-pages decoder, so the published output is the complete site. Do not remove any of the
three: without `.nojekyll` GitHub Pages strips `_framework`; without the 404/decoder pair a deep
link dies on refresh. The site repository's `.gitattributes` (`* binary`) is equally load-bearing —
without it git corrupts the `.wasm` files and Blazor's integrity check rejects its own runtime. The
script verifies all of these and refuses to deploy if any is missing.

## Blazorme.FFmpeg

An unfinished stub: `IFFmpeg` has no members and `FFmpeg` is an empty class. It has never been
published and is marked `IsPackable=false` so it cannot be published by accident. It builds, and
that is all. `wwwroot/ffmpeg.min.js` is vendored but unused.
