# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.0-beta4] - 2024-05-16

### Fixed

- Length properties referenced by a field or array now correctly generate private readonly properties in all cases.

## [1.0.0-beta3] - 2024-05-15

### Fixed

- Fixed-length fields in packets/structures now assert that the collection is the correct length during serialization.
- Break bytes are handled correctly. An exception will be thrown during code generation if a break byte is not encapsulated by a `chunked` element.
- Length properties referenced by a field or array are now private and no longer writable.

## [1.0.0-beta2] - 2024-05-15

### Added

- Support for doc comments in nuget package

## 1.0.0-beta1 - 2024-05-14

### Added

- Support for EO data structures
    - Client packets
    - Server packets
    - Endless Map Files (EMF)
    - Client pub files (items, npcs, spells, classes)
    - Server pub files (talk, shops, drops, inns, skillmasters)

- Utilities
    - Data reader/writer
    - Number/string encoding
    - Data encryption
    - Packet sequencing

[Unreleased]: http://github.com/ethanmoffat/eolib-dotnet/compare/v1.0.0-beta4...HEAD
[1.0.0-beta4]: http://github.com/ethanmoffat/eolib-dotnet/compare/v1.0.0-beta2...v1.0.0-beta3
[1.0.0-beta3]: http://github.com/ethanmoffat/eolib-dotnet/compare/v1.0.0-beta2...v1.0.0-beta3
[1.0.0-beta2]: http://github.com/ethanmoffat/eolib-dotnet/compare/v1.0.0-beta1...v1.0.0-beta2
