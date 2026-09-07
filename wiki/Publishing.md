# Publishing

Packaging is done by GitHub Actions, in `.github/workflows/`.

- **`ci.yml`** — build and test on every push and PR to master. Builds with `-warnaserror`, so the
  repository's zero-warning bar is enforced there, and a new NuGet advisory fails the build.
- **`publish.yml`** — **one workflow for all four packages, deliberately.**

## One workflow, on purpose

A NuGet Trusted Publishing policy is bound to a **single workflow file**, so one file means one
policy covers every package, every future version, and any future `Blazorme.X` package.
**Do not split publishing into per-package workflow files** — that would require a policy per file.

## Ways in

| Trigger | Publishes |
|---|---|
| tag `v<version>` | every package |
| tag `diff-v<version>` | `Blazorme.Diff` only |
| tag `split-v<version>` | `Blazorme.Split` only |
| tag `streamsaver-v<version>` | `Blazorme.StreamSaver` only |
| tag `testhost-v<version>` | `Blazorme.TestHost` only |
| manual run | defaults to a **dry run** that packs and uploads the `.nupkg` files as artifacts without publishing |

`Blazorme.FFmpeg` is absent by design: unfinished, never published, and marked `IsPackable=false`
so it cannot be published by accident.

The workflow verifies the tag matches each project's `<Version>`, runs the tests, and pushes with
`--skip-duplicate` so a re-run is harmless.

## Tag with the csproj spelling

The projects declare `<Version>26.09.07</Version>` and NuGet normalises that to `26.9.7`. The
verification step compares against the **raw csproj text**, so the tag is:

```
v26.09.07
```

not `v26.9.7`. Tagging the normalised form fails the version check.

## Trusted Publishing, not an API key

nuget.org discourages API keys for automated publishing. The job requests a GitHub OIDC token
(`id-token: write`) and `NuGet/login@v1` exchanges it for an API key valid for one hour. **No
publishing secret is stored in the repository — do not reintroduce `NUGET_API_KEY`.**

The login step sits immediately before the push on purpose: the key expires, and each OIDC token
buys exactly one key.

### The policy

Created on nuget.org under Account → Trusted Publishing:

| Field | Value |
|---|---|
| Package Owner | `melihercan` |
| CI/CD Provider | GitHub Actions |
| Repository Owner | `melihercan` |
| Repository | `Blazorme` |
| Workflow File | `publish.yml` (file name only, no path) |
| Environment | *(none)* |
| Scopes | Push new packages and package versions |
| Glob | `Blazorme.*` |

Note the policy binds to the repository **ID**, not just its name, which is why the `Utilme.*`
policy could never have covered this repository even with a wider glob.

## Versioning

Date-based: `26.09.07`, normalised by NuGet to `26.9.7`, matching the convention used in Utilme.
Bump `<Version>` in each `.csproj` together with its `<PackageReleaseNotes>`.

`AssemblyVersion` and `FileVersion` are left to derive from `Version` rather than being set
separately, so they cannot drift.

Because `26.9.7` sorts above the old `1.0.x`, it becomes the version consumers resolve by default —
including those still on `netstandard2.1` or `net5.0`, for whom it will not restore. That is
inherent to the .NET 10 migration, and each package's release notes lead with it.

## The demo site

`https://melihercan.github.io/` is served from the separate **`melihercan/melihercan.github.io`**
repository, deployed by hand. It was last pushed in June 2020 — before `Blazorme.StreamSaver`
existed — so it does not reflect the current `DemoApp`. This repository has no GitHub Pages site and
no Pages workflow.
