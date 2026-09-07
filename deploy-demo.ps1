<#
.SYNOPSIS
    Publishes DemoApp into the melihercan.github.io repository, ready for review.

.DESCRIPTION
    https://melihercan.github.io/ is served from a SEPARATE repository, melihercan.github.io,
    because it is a GitHub user site and must live at the root of that repo. This repository
    cannot deploy there from CI without storing a credential for the other repo, so the demo is
    deployed by running this script and pushing the result yourself.

    The script never commits and never pushes. It leaves the site repository dirty so the change
    can be reviewed first.

    Everything the site needs now comes out of the publish: index.html carries the
    spa-github-pages decoder, and 404.html and .nojekyll live in DemoApp/wwwroot. Only the site
    repository's own metadata is preserved.

.PARAMETER SitePath
    The melihercan.github.io working copy. Defaults to a sibling of this repository.

.PARAMETER SkipTests
    Skip the test run. Don't, unless you have just run them.

.EXAMPLE
    ./deploy-demo.ps1
    ./deploy-demo.ps1 -SitePath C:\dev\melihercan.github.io
#>
[CmdletBinding()]
param(
    [string] $SitePath = (Join-Path (Split-Path $PSScriptRoot -Parent) 'melihercan.github.io'),
    [switch] $SkipTests
)

$ErrorActionPreference = 'Stop'

# Files belonging to the site repository rather than to the demo. Everything else at the site
# root is replaced wholesale, so stale output cannot linger — the previous deployment was a
# Blazor 3.x build whose _framework layout shares almost no file names with a .NET 10 one.
$preserve = @('.git', '.gitattributes', 'LICENSE', 'README.md')

function Fail($message) {
    Write-Host "ERROR: $message" -ForegroundColor Red
    exit 1
}

# --- checks ------------------------------------------------------------------------------------

if (-not (Test-Path $SitePath)) {
    Fail "Site repository not found at '$SitePath'. Clone melihercan.github.io, or pass -SitePath."
}
$SitePath = (Resolve-Path $SitePath).Path

if (-not (Test-Path (Join-Path $SitePath '.git'))) {
    Fail "'$SitePath' is not a git repository."
}

# "* binary" stops git rewriting line endings inside .wasm and .dll files. Without it Blazor's
# integrity check rejects its own framework files at runtime, which is documented in that
# repository's README (dotnet/aspnetcore#21560). Losing it breaks the site in a way that is very
# hard to diagnose from the browser.
$attributes = Join-Path $SitePath '.gitattributes'
if (-not (Test-Path $attributes) -or -not ((Get-Content $attributes -Raw) -match '\*\s+binary')) {
    Fail "'$attributes' must exist and contain '* binary', or git will corrupt the .wasm files."
}

$dirty = git -C $SitePath status --porcelain
if ($dirty) {
    Fail "The site repository has uncommitted changes. Commit or discard them first, so the diff this produces contains only the new deployment."
}

# --- build -------------------------------------------------------------------------------------

Push-Location $PSScriptRoot
try {
    if (-not $SkipTests) {
        Write-Host 'Running tests...' -ForegroundColor Cyan
        dotnet test --configuration Release
        if ($LASTEXITCODE -ne 0) { Fail 'Tests failed. Not deploying.' }
    }

    Write-Host 'Publishing DemoApp...' -ForegroundColor Cyan
    $publishRoot = Join-Path $PSScriptRoot 'DemoApp/bin/Release/net10.0/publish'
    if (Test-Path $publishRoot) { Remove-Item $publishRoot -Recurse -Force }

    dotnet publish DemoApp/DemoApp.csproj --configuration Release
    if ($LASTEXITCODE -ne 0) { Fail 'Publish failed.' }
}
finally {
    Pop-Location
}

$wwwroot = Join-Path $publishRoot 'wwwroot'
if (-not (Test-Path $wwwroot)) { Fail "Expected published output at '$wwwroot'." }

# --- sync --------------------------------------------------------------------------------------

Write-Host "Replacing site contents in '$SitePath'..." -ForegroundColor Cyan

Get-ChildItem -LiteralPath $SitePath -Force |
    Where-Object { $preserve -notcontains $_.Name } |
    ForEach-Object { Remove-Item -LiteralPath $_.FullName -Recurse -Force }

Copy-Item -Path (Join-Path $wwwroot '*') -Destination $SitePath -Recurse -Force

# --- verify ------------------------------------------------------------------------------------

# Each of these has broken this site before, or would break it silently.
$required = @{
    '.nojekyll'  = 'GitHub Pages would strip _framework and _content without it'
    '404.html'   = 'deep links such as /streamsaverdemo would 404 on refresh'
    'index.html' = 'the app entry point'
    '_framework' = 'the Blazor runtime'
}

foreach ($name in $required.Keys) {
    if (-not (Test-Path (Join-Path $SitePath $name))) {
        Fail "'$name' is missing from the deployment - $($required[$name])."
    }
}

$index = Get-Content (Join-Path $SitePath 'index.html') -Raw
if ($index -notmatch 'Single Page Apps') {
    Fail 'index.html is missing the spa-github-pages decoder; deep links would not survive a refresh.'
}
if ($index -notmatch '<base href="/"') {
    Fail 'index.html must use <base href="/"> - the demo is served from the root of a user site.'
}

# --- report ------------------------------------------------------------------------------------

Write-Host ''
Write-Host 'Deployment staged. Nothing has been committed or pushed.' -ForegroundColor Green
Write-Host ''
git -C $SitePath status --short
Write-Host ''
Write-Host 'Note: .gitattributes marks everything binary, so git reports "binary files differ"' -ForegroundColor DarkGray
Write-Host 'rather than showing text diffs. That is expected.' -ForegroundColor DarkGray
Write-Host ''
Write-Host 'To publish:' -ForegroundColor Cyan
Write-Host "  git -C `"$SitePath`" add -A"
Write-Host "  git -C `"$SitePath`" commit -m `"Update demo`""
Write-Host "  git -C `"$SitePath`" push"
Write-Host ''
Write-Host 'Then HARD REFRESH (Ctrl+Shift+R) before believing what you see.' -ForegroundColor Yellow
Write-Host 'index.html is cached for ten minutes, and a stale one loading new assets hangs on' -ForegroundColor Yellow
Write-Host '"Loading..." exactly like a broken deploy. Incognito settles it either way.' -ForegroundColor Yellow
