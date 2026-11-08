param(
    [string]$ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path,
    [string]$Repo,
    [string]$TagName,
    [switch]$SkipBuild,
    [switch]$SkipUpload
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Get-GitHubRepoFromRemote {
    param([string]$RemoteUrl)

    if ([string]::IsNullOrWhiteSpace($RemoteUrl)) {
        return $null
    }

    if ($RemoteUrl -match 'github\.com[:/](?<owner>[^/]+)/(?<repo>[^/]+?)(?:\.git)?$') {
        return "$($matches.owner)/$($matches.repo)"
    }

    return $null
}

$repoRoot = (Resolve-Path $ProjectRoot).Path
$version = (Get-Content (Join-Path $repoRoot "VERSION") -Raw).Trim()
$tag = if ($TagName) { $TagName } else { "v$version" }

if (-not $Repo) {
    $remoteUrl = git -C $repoRoot remote get-url origin 2>$null
    if ($LASTEXITCODE -eq 0 -and -not [string]::IsNullOrWhiteSpace($remoteUrl)) {
        $Repo = Get-GitHubRepoFromRemote -RemoteUrl $remoteUrl.Trim()
    }
}

if (-not $Repo) {
    throw "Unable to determine the GitHub repository. Pass -Repo 'Owner/Repo' or define a GitHub origin remote."
}

if (-not $SkipBuild) {
    & powershell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $repoRoot "tools\GenerateChangelog.ps1") -ProjectRoot $repoRoot
    if ($LASTEXITCODE -ne 0) { throw "Changelog generation failed." }

    $csproj = Get-ChildItem -Path $repoRoot -Filter *.csproj | Select-Object -First 1
    if (-not $csproj) {
        throw "No .csproj file was found in $repoRoot"
    }

    & dotnet build $csproj.FullName -c Release --nologo
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet build failed."
    }
}

$zip = Get-ChildItem -Path (Join-Path $repoRoot "bin\Release\net6.0") -Filter *.zip -ErrorAction Stop |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1

if (-not $zip) {
    throw "No release zip was found in bin\Release\net6.0."
}

Write-Host "Release package: $($zip.FullName)"
Write-Host "Target repo: $Repo"
Write-Host "Target tag: $tag"

if (-not $SkipUpload) {
    gh release view $tag --repo $Repo 2>$null
    if ($LASTEXITCODE -ne 0) {
        gh release create $tag --repo $Repo --generate-notes --title $tag
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to create GitHub release $tag"
        }
    }

    gh release upload $tag $zip.FullName --repo $Repo --clobber
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to upload the release zip to GitHub."
    }

    Write-Host "Upload complete: ${Repo}:$tag"
} else {
    Write-Host "Upload skipped. You can publish the zip manually with: gh release upload $tag $($zip.FullName) --repo $Repo --clobber"
}
