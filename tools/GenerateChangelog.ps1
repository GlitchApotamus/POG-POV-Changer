param(
    [string]$ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Get-ReleaseSection {
    param(
        [string]$Version,
        [string[]]$CommitMessages
    )

    $added = New-Object System.Collections.Generic.List[string]
    $changed = New-Object System.Collections.Generic.List[string]
    $fixed = New-Object System.Collections.Generic.List[string]
    $docs = New-Object System.Collections.Generic.List[string]

    foreach ($line in $CommitMessages) {
        if ([string]::IsNullOrWhiteSpace($line)) { continue }

        $trimmed = $line.Trim()
        if ($trimmed -match '^(feat|feature)\s*:\s*(.+)$') {
            $added.Add("- $($matches[2])")
            continue
        }
        if ($trimmed -match '^(fix|bugfix)\s*:\s*(.+)$') {
            $fixed.Add("- $($matches[2])")
            continue
        }
        if ($trimmed -match '^(docs?|documentation)\s*:\s*(.+)$') {
            $docs.Add("- $($matches[2])")
            continue
        }
        if ($trimmed -match '^(refactor|chore|perf|style)\s*:\s*(.+)$') {
            $changed.Add("- $($matches[2])")
            continue
        }

        $changed.Add("- $trimmed")
    }

    $sections = @(
        "## [$Version] - $(Get-Date -Format 'yyyy-MM-dd')"
    )

    foreach ($category in @(
        @{ Name = "### Added"; Items = $added },
        @{ Name = "### Changed"; Items = $changed },
        @{ Name = "### Fixed"; Items = $fixed },
        @{ Name = "### Documentation"; Items = $docs }
    )) {
        if ($category.Items.Count -gt 0) {
            $sections += $category.Name
            $sections += $category.Items
        }
    }

    if ($sections.Count -eq 1) {
        $sections += "### Changed"
        $sections += "- No user-facing changes were recorded for this release."
    }

    return ($sections -join [Environment]::NewLine)
}

$projectRoot = (Resolve-Path $ProjectRoot).Path
$version = (Get-Content (Join-Path $projectRoot "VERSION") -Raw).Trim()
$changelogPath = Join-Path $projectRoot "CHANGELOG.md"

Push-Location $projectRoot
try {
    if (Test-Path ".git") {
        $tags = @(git tag --sort=-version:refname)
        $currentTag = "v$version"
        $previousTag = $null

        if ($tags.Count -gt 0) {
            for ($i = 0; $i -lt $tags.Count; $i++) {
                if ($tags[$i] -eq $currentTag) {
                    if ($i + 1 -lt $tags.Count) { $previousTag = $tags[$i + 1] }
                    break
                }
            }

            if (-not $previousTag) {
                $previousTag = $tags[0]
            }
        }

        $range = if ($previousTag) { "$previousTag..HEAD" } else { "HEAD" }
        $commitMessages = @(git log --pretty=format:'%s' $range 2>$null)
    } else {
        $commitMessages = @("Project release notes generated without git history.")
    }
} finally {
    Pop-Location
}

$section = Get-ReleaseSection -Version $version -CommitMessages $commitMessages

if (Test-Path $changelogPath) {
    $existing = [System.IO.File]::ReadAllText($changelogPath)
    if ($existing -notmatch "## \[$([regex]::Escape($version))\]") {
        $firstVersionIndex = $existing.IndexOf("## [", [System.StringComparison]::Ordinal)
        if ($firstVersionIndex -ge 0) {
            $prefix = $existing.Substring(0, $firstVersionIndex).TrimEnd()
            $suffix = $existing.Substring($firstVersionIndex).TrimStart()
            $updated = $prefix + [Environment]::NewLine + [Environment]::NewLine + $section + [Environment]::NewLine + [Environment]::NewLine + $suffix
        } else {
            $updated = $existing.TrimEnd() + [Environment]::NewLine + [Environment]::NewLine + $section
        }

        [System.IO.File]::WriteAllText($changelogPath, $updated)
    }
} else {
    $header = @(
        "# Changelog",
        "",
        "All notable changes to POGPOVChanger are documented in this file.",
        "",
        "The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).",
        ""
    )

    $updated = ($header + $section) -join [Environment]::NewLine
    [System.IO.File]::WriteAllText($changelogPath, $updated + [Environment]::NewLine)
}
