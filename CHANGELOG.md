# Changelog

All notable changes to POGPOVChanger are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.2] - 2026-11-08

### Added
- Added automatic changelog generation for both GitHub and the Thunderstore package.
- Included the changelog file in the generated release ZIP.

### Changed
- Updated the release packaging flow so changelog updates are generated before the zip is built.
- Kept the README changelog section synced with the main changelog file.

### Fixed
- Fixed the changelog ordering so the newest release appears directly below the top header.

## [1.0.1] - 2026-11-08

### Changed
- Simplified the release workflow so the mod is built locally and the finished ZIP is published instead of relying on CI to produce a fake stub build.
- Updated the project docs to explain the required game and MelonLoader paths for local builds.
- Streamlined MelonLoader reference handling and disabled nullable compatibility workarounds where they were no longer needed.
- Normalized manifest formatting and dependency metadata for POG Config.

### Fixed
- Fixed the GitHub release upload flow to publish the actual built package.
- Corrected the config path handling for the ModManager directory.
- Removed stale stub-build support and unused stub files from the release path.
- Updated the README and manifest metadata to match the real assembly-based build process.

## [1.0.0] - 2026-11-05

### Added
- Improved third-person interaction ray handling and camera switching behavior.
- Added nullable compatibility attributes to support the build environment more reliably.
- Added new helper methods and classes to improve object interaction and physics handling.

### Changed
- Updated MelonLoader and project reference handling for better compatibility with the game install layout.
- Clarified the README requirements and third-person interaction notes.
- Bumped the project version to 1.0.0.

## [0.0.3] - 2026-10-27

### Fixed
- Added the Thunderstore package namespace to the package configuration.
- Improved .gitignore handling for editor and generated files.
- Cleaned up stale build outputs and release workflow behavior.
- Added missing checkout and tagging steps in the release workflow.
- Corrected version file rollback and manifest consistency issues.
- Updated the icon and packaging metadata for the mod.

## [0.0.2] - 2026-10-26

### Added
- Initial public release of POGPOVChanger.
- Initial project scaffolding and versioning.
- Initial Thunderstore release workflow and GitHub Actions publishing setup.
- Initial implementation of the third-person camera and interaction features.
- Initial stub-build support and compatibility scaffolding used during early release testing.

### Changed
- Updated the manifest, version, and dependency metadata for the initial package publication.
- Switched the Thunderstore tool installation approach and cleaned up workflow details.

### Fixed
- Corrected manifest formatting and dependency names.
- Tightened the early release workflow and cleaned up initial packaging issues.
