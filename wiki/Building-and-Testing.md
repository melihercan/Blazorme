# Building and Testing

Requires the **.NET 10 SDK**.

```powershell
dotnet build Blazorme.sln
dotnet test
dotnet run --project DemoApp
dotnet pack Diff/Diff.csproj -c Release
```

The build is expected to be **clean: 0 warnings, 0 errors**, in Debug and Release. CI enforces that
with `-warnaserror`, which also turns a new NuGet advisory (NU1903) into a build failure.

`GeneratePackageOnBuild` was removed once CI took over packaging, so a local build no longer drops a
`.nupkg` into `bin/`. Use `dotnet pack` explicitly when you want one.

## `dotnet test` runs on Microsoft.Testing.Platform

`Blazorme.Tests` uses **xUnit v3**, whose test projects are executables. The .NET 10 SDK refuses to
run those through the legacy VSTest path, so the repository opts into MTP mode via `global.json`:

```json
{
  "test": {
    "runner": "Microsoft.Testing.Platform"
  }
}
```

Deleting that file breaks the suite with *"Testing with VSTest target is no longer supported"*.

Two consequences worth knowing before you fight the CLI:

- **Do not pass `--nologo`** or other VSTest-era flags. MTP forwards unrecognised arguments to the
  test executable, which rejects them with exit code 5 and *"Zero tests ran"* — a failure that looks
  like broken tests but is really a bad command line.
- Target one project with **`--project <path>`**, not a bare path argument:

```powershell
dotnet test --project Blazorme.Tests/Blazorme.Tests.csproj
```

### Build the solution before running the tests

`PublicApiSurfaceTests` reads the libraries' **compiled assemblies off disk** through
`MetadataLoadContext` rather than referencing them, so every library must have been built in the
same configuration first. `dotnet test` at solution level does this for you; a CI job that skips the
build step would not. The lookup is configuration-aware, so a stale `Release` build cannot shadow a
fresh `Debug` one by having a newer timestamp.

### Component tests use bUnit

`bunit` 2.9.0 renders components on .NET 10. Note that bUnit v2's base class is `BunitContext`, and
that `Bunit.TestContext` and `Xunit.TestContext` collide if you reach for the old name.

### FluentAssertions is pinned to 7.x

Version 8 moved to a licence that requires payment for commercial use; 7.x is the last Apache-2.0
release. The pin is deliberate — do not let a tool bump it.

## The test suite

| File | Covers |
|---|---|
| `PublicApiSurfaceTests` | The whole public surface of all five libraries against `PublicApi.approved.txt`. See [Design Notes](Design-Notes#the-api-baseline). |
| `PublicApiDumper` / `TestAssemblies` | The machinery behind it — metadata-only reflection, and locating the built assemblies. |
| `DiffCharacterizationTests` | `DiffApi` over a mocked `IJSRuntime`, and the argument order the JS side expects. |
| `SplitCharacterizationTests` | Rendering, the options object handed to split.js, cursor derivation, pane sizing. |
| `StreamSaverTests` | Module import, writer creation, the write window, disposal. |
| `TestHostCharacterizationTests` | That TestHost renders and that its Fizzler selector layer works. |
| `KnownDefectTests` | One test per defect — see below. |

## The defect-test convention

`KnownDefectTests` holds one test per known defect, and the prefix carries the status:

- **`FIXED_`** — was a defect pin, rewritten in place when the defect was fixed. The comment names
  the pin it replaced. **Rewrite these, never delete them**, so a fix shows up as a diff rather than
  as silence.
- **`LIMITATION_`** — not a bug in this code. `LIMITATION_Inline_output_ignores_both_titles_and_style`
  is htmldiff.net having no concept of titles or word/char granularity.
- **`PERMANENT_`** — cannot be fixed without breaking the public API. Do not "fix" these.

A red test that is not one of these means behaviour changed. That is only acceptable if the change
was intended, in which case update the test in the same commit and say so.
