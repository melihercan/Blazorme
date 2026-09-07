# Design Notes

Why the repository looks the way it does, and the invariants any change must preserve.

## The governing constraint

All four packages are on nuget.org and real applications depend on them, so the modernization has
been **additive only**: no public member renamed, removed, or retyped. New capability is added
*beside* the old surface, never in place of it.

## The API baseline

`PublicApiSurfaceTests` enforces that mechanically against `Blazorme.Tests/PublicApi.approved.txt`,
which captures every public type, member signature, generic arity, enum numeric value, `const`
literal and default parameter value across all five libraries.

It reads **metadata only**, through `MetadataLoadContext` over the built assemblies, rather than
referencing the projects. That was not a stylistic choice:

- `Blazorme.StreamSaver` and `Blazorme.FFmpeg` targeted `net5.0` and could not be referenced from a
  `net10.0` test project at all.
- `Blazorme.TestHost` could be referenced but **threw on use**, so anything that had to load and run
  it would have been useless as a baseline.

When a change is intentional, review the diff and copy `PublicApi.received.txt` from the test output
directory over `PublicApi.approved.txt`, in the same commit. Never weaken the assertion.

**Nullable annotations produce no baseline change.** They are metadata-only and the IL signatures
are unchanged, which is why enabling `Nullable` across all five libraries was binary compatible.

### The one approved removal

Two lines were removed deliberately, both `protected override` lifecycle hooks:
`Diff.OnInitializedAsync()` and `Split.OnInitialized()`. Deleting them *is* the fix for two defects
(see below). Neither is callable by a consumer, and a subclass calling `base.OnInitialized()` still
compiles and still dispatches — to `ComponentBase`.

## Defects that are staying

Two are pinned `PERMANENT_` because fixing them would break the public API:

- **`FFmpegerviceExtensions`** — the `S` of "Service" was eaten. It is a public type name. A
  correctly spelled type may be added *beside* it; this one stays.
- **The generated `_Imports` classes** — each `_Imports.razor` compiles to a public class. They are
  meaningless to consumers, but they are public types in shipped assemblies. Razor does not offer a
  knob to make them internal.

## Blazorme.StreamSaver

The original implementation used `System.Private.Runtime.InteropServices.JavaScript` —
`Runtime.GetGlobalObject`, `JSObject.Invoke`, `Uint8Array.From` — the .NET 5/6 **preview** WASM
interop, removed in .NET 7. A copy of the private assembly was checked into the repository to make
it compile. That code could not be carried to .NET 10 under any circumstances, so the migration
could not complete without replacing it.

It now uses ordinary Blazor JS interop against `wwwroot/StreamSaverJsInterop.js`, imported as an ES
module through `IJSObjectReference` — the path the original author sketched in `TODO` comments and
abandoned in 2021. Consequences:

- **The public surface is unchanged.** `IStreamSaver`, `StreamSaver` and `AddStreamSaver` have
  identical signatures; only `internal` code moved.
- **It is no longer WASM-only.** It runs on Blazor Server too, though every chunk crosses the
  SignalR circuit there, which is a poor fit for the large transfers the package exists for.
- **`byte[]` marshals to a real `Uint8Array`** since .NET 6, which is precisely what defeated the
  2021 attempt (its commit messages record passing base64 and StreamSaver expecting `Uint8Array`).

`StreamSaver.min.js` and `polyfill.min.js` are classic scripts that assign a global, so they cannot
be imported as modules and are still loaded by the host page with `<script>` tags. The interop
module wraps the global. That is why there are three JS files rather than one.

### The write window

`WriteAsync(byte[], int, int)` copies exactly `count` bytes from `offset`. The original sent the
whole array. `Stream.WriteAsync(ReadOnlyMemory<byte>)` — the overload `DemoApp` uses — supplies an
**oversized pooled buffer**, so downloads had the pool's trailing bytes appended. Do not
"simplify" this back to passing `buffer`.

## Blazorme.TestHost

Version 1.0.0 did not work on .NET 5 or later, despite advertising that it did.

`TestHost.csproj` declared `Microsoft.AspNetCore.Components` 3.1.10 unconditionally **and** 5.0.0
for `net5.0`. Duplicate `PackageReference` items do not merge — the first wins and the conditional
one is silently discarded, which is all NU1504 was telling anyone. So the `net5.0` build shipped IL
bound to 3.1, whose `RenderTreeFrame` exposed public **fields** that became **properties** in
.NET 5. Rendering a component threw `MissingFieldException`.

The fix required **no source change at all**. The C# was always written `frame.FrameType`, which
binds to a property exactly as well as to a field; only the stale compiled reference was wrong.
Removing the duplicate and retargeting to `net10.0` was the whole repair.

The lesson worth keeping: NU1504 was never cosmetic here. It was the bug.

## Component conventions

**Do not override both `OnInitializedAsync` and `OnParametersSetAsync` with the same work.** Blazor
sets parameters before `OnInitializedAsync` and runs `OnParametersSetAsync` immediately after it, so
doing both means every mount does the work twice. `Diff` did, which meant two JS round trips per
mount for the Row and Column formats.

**Do not assign to your own `[Parameter]` properties.** Blazor owns them. `Split.OnInitialized`
assigned a default `Cursor`, and because `OnInitialized` runs once, changing `Direction` afterwards
left the cursor describing the direction the component happened to mount with. Derive such values at
the point of use instead.

**Lowercase with `ToLowerInvariant`.** The strings sent to split.js are matched exactly on the JS
side. `ToLower()` is culture sensitive and Turkish maps `I` to `ı`. This was latent rather than live
— no member of `SplitDirection` or `SplitGutterAlign` contains a capital `I` — but adding one would
have broken it silently.

## No XML documentation file

`GenerateDocumentationFile` is deliberately **off**. The public members carry no XML docs, so
turning it on would emit hundreds of CS1591 warnings and break the zero-warning bar. Documenting the
full surface first would be a prerequisite.
