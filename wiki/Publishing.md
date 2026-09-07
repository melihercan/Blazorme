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
repository. It has to live there: a GitHub *user* site must sit at the root of that repository, and
deploying from here instead would move the demo to `melihercan.github.io/Blazorme/`.

Deploy it with **`./deploy-demo.ps1`**, which publishes `DemoApp`, replaces the site contents, and
leaves the change staged for you to review and push. It never commits and never pushes.

There is deliberately no CI deployment. Writing to another repository from Actions needs a stored
credential — a PAT or a deploy key — and this repository otherwise stores none, having moved
publishing to OIDC precisely to avoid that.

Four things the site needs that are easy to lose, all of which the script verifies:

| | Why |
|---|---|
| `.gitattributes` with `* binary` | Git would otherwise rewrite line endings inside `.wasm` and `.dll` files, and Blazor's integrity check then rejects its own runtime ([aspnetcore#21560](https://github.com/dotnet/aspnetcore/issues/21560)). The script refuses to run without it. |
| `.nojekyll` | Pages runs Jekyll, which strips `_`-prefixed paths — `_framework` and `_content` would vanish. |
| `404.html` | Pages serves it for unknown paths. It converts `/streamsaverdemo` into `/?p=/streamsaverdemo`, which is how a deep link survives a hard refresh. |
| The decoder in `index.html` | The other half: turns that query back into a route. Inert without a `?p=`. |

`.nojekyll` and `404.html` now live in `DemoApp/wwwroot` and the decoder is in the app's own
`index.html`, so the published output is the whole site. Only `.gitattributes`, `LICENSE` and
`README.md` belong to the site repository and are preserved across a deployment.

`404.html` sets `segmentCount = 0` because the demo is served from the root of a user site. A
project site under `/repo-name/` would need `1`, and a matching `<base href>`.

Expect a `404` in the browser console on any deep link. That is the mechanism, not a fault:
GitHub Pages genuinely returns HTTP 404 for `/streamsaverdemo`, with `404.html` as the body, and
the redirect happens from there. A normal load of the site root logs nothing.

### After deploying, hard refresh before believing anything

`index.html` is served with `Cache-Control: max-age=600`, so your own browser may keep the previous
one for up to ten minutes while loading the new assets around it. That combination hangs on
*"Loading..."* and looks exactly like a broken deployment.

Reach for **Ctrl+Shift+R**, or an incognito window, before debugging. If it still hangs there, it is
real.

This was at its worst going from the 2020 Blazor 3.x build to .NET 10, because the whole
`_framework` layout changed and a cached `index.html` was asking for a runtime that no longer
existed. Later deployments are safer: .NET 10 fingerprints asset file names
(`dotnet.native.nxw7lo0lh5.wasm`), so a stale `index.html` is about the only thing cache can still
get wrong.
