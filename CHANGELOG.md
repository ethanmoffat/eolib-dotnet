# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Changed
- Remarks generated from comments on dummies and other instructions without a property now start with a description of the instruction, e.g. "The dummy byte (always 255): …", so they make sense on the containing type.

### Removed
- Comments on unnamed hardcoded fields are no longer added to the remarks of the containing type, since those values aren't visible to consumers.

## [1.3.0] - 2026-09-30

### Added
- Named hardcoded fields now generate a `Default{Name}` constant with the spec value, e.g.
  `InitInitClientPacket.DefaultProtocolVersion`.
- XML comments in the protocol files are now generated as doc comments. Comments on unnamed fields, dummies and other
  instructions without a property are added to the `<remarks>` of the containing type, and comments in empty switch
  cases are added to the switch data property.

### Changed
- Named hardcoded fields now store the value that was deserialized instead of discarding it, and have a private setter.
  New objects still serialize the default value.

### Updated
- Pulled in changes for eo-protocol, with impact to generated code:
    - [Name hardcoded client fields verified by the official server](https://github.com/cirras/eo-protocol/commit/8ccc442c1ea448b68caa5dbf561873d226def184)
- Pulled in changes for eo-captured-packets for test parity with protocol changes.

## [1.2.0] - 2026-09-28

### Added
- An API reference site at https://ethanmoffat.github.io/eolib-dotnet/, built with DocFX, with a summary page, a getting
  started guide, namespace descriptions and a version picker. It has the newest release of each minor version. Each
  release also includes the docs as `Moffat.EndlessOnline.SDK-<version>-docs.tar.gz`.
- `scripts/serve-docs.sh`, which builds the docs site for the working tree and serves it on localhost.
- The protocol generator now validates the protocol XML and reports error `EO0004` for spec rule violations: misplaced
  unsized arrays, dummies or default cases, required fields after optional fields, delimited arrays outside chunked
  sections, invalid switch fields and duplicate case values, invalid `length`/`padded` usage, invalid underlying types,
  and length fields referenced more than once.

### Updated
- Pulled in changes for eo-protocol, with impact to generated code:
    - [Change SpellTargetOther caster_direction -> target_type](https://github.com/cirras/eo-protocol/commit/0c0ea73c6a30833541ff4c53a4ebe87432ef359d)
    - [Change LoginMessageCode.Yes 2->250](https://github.com/cirras/eo-protocol/commit/aaba85ffd101fdb848e28b90878f685f2b3c5450)
- Pulled in fixes for eo-captured-packets for test parity with protocol changes.

### Fixed
- Generated server code can now reference types declared in the `Net.Client` namespace.
- The protocol generator no longer reports `EO0003` duplicate type warnings, or generates code from stale protocol
  types, when the compiler server or IDE runs the generator more than once in the same process.

## [1.1.0] - 2026-03-15

### Updated
- Updated to .Net 8.0 for pipelines and test project SDK version.
- Pulled in changes for eo-protocol, with impact to generated code:
    - [StatSkill data type fix](https://github.com/cirras/eo-protocol/commit/dee092d4a7f2bb379a71949cf3e59e352c4b1726)
    - [Fix chunking in WELCOME_REPLY](https://github.com/cirras/eo-protocol/commit/9183c91a217f1316d80be4ef568702a89525d155)
    - [Rename npc_index->behavior_id in QUEST_ACCEPT](https://github.com/cirras/eo-protocol/commit/3e09f6b02029a433d9758e237dbbff1493dd4a35)
    - [Add Bard emote](https://github.com/cirras/eo-protocol/commit/6959edeb153e8874bf7783bbedfccc5c2217a518)
- Pulled in fixes for eo-captured-packets for test parity with protocol changes.

## [1.0.2] - 2025-07-01

### Fixed
- Updated Equals method generation to properly compare objects (thanks @do4k for PR #1)

## [1.0.1] - 2025-01-28

### Fixed
- Updated deserialization of optional fields to check against the expected data size of the field when followed by a dummy instruction.
- Updated serialization of dummy fields to only write the dummy value if the packet is otherwise empty.

## [1.0.0] - 2024-10-15

### Fixed
- Updated AssertLength exception string to reference length property so the message makes more sense for string types with fixed or maximum lengths.

### Updated
- Pulled in minor fixes for eo-protocol (no impact to generated code):
    - [TradeItemData data modeling update](https://github.com/Cirras/eo-protocol/commit/d2bf358503c4eeae24128ae205e9a50f2b86efe9)
        - Including: [follow-up fix](https://github.com/Cirras/eo-protocol/commit/0e58893fd3102ec1bc4bdc61ae7d92c926c30cde)
    - [Struct ordering in net/server/protocol.xml](https://github.com/Cirras/eo-protocol/commit/d59a8077d17d504bf1e71fe085fec1c3ba8e65d4)

## [1.0.0-rc3] - 2024-08-21

### Added
- `PacketResolver` class, which constructs an empty packet object type from a given family or action. Specified namespace determines whether packets are for "client" or "server" context.
- Test coverage for generated code using `eo-captured-packets` submodule. See commit d81213d.

### Fixed
- Padded fields no longer assert an exact string length; instead, they assert maximum size.
- Optional fields backed by reference types are no longer marked nullable.
- Equals method overrides no longer result in `NullReferenceException` for `null` fields.
- Array loops calculate element size in most cases, allowing readers to automatically ignore improperly-sized trailing data.

## [1.0.0-rc2] - 2024-05-28

### Fixed

- Generated ToString() implementations correctly stringify structures.
- Arrays that are deserialized based on the value of reader.Remaining now properly store the initial "remaining" value. Fixes a bug where only half of an array in a given chunk would be read.

## [1.0.0-rc1] - 2024-05-23

### Fixed

- Doc comments for structs/packets are now properly parsed and generated from the source XML.
- Nested structures are now properly initialized to their default value; resolves crash bug during attempts to deserialize default-initialized packets.

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

## [1.0.0-beta1] - 2024-05-14

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

[Unreleased]: https://github.com/ethanmoffat/eolib-dotnet/compare/v1.3.0...HEAD
[1.3.0]: https://github.com/ethanmoffat/eolib-dotnet/releases/tag/v1.3.0
[1.2.0]: https://github.com/ethanmoffat/eolib-dotnet/releases/tag/v1.2.0
[1.1.0]: https://github.com/ethanmoffat/eolib-dotnet/releases/tag/v1.1.0
[1.0.2]: https://github.com/ethanmoffat/eolib-dotnet/releases/tag/v1.0.2
[1.0.1]: https://github.com/ethanmoffat/eolib-dotnet/releases/tag/v1.0.1
[1.0.0]: https://github.com/ethanmoffat/eolib-dotnet/releases/tag/v1.0.0
[1.0.0-rc3]: https://github.com/ethanmoffat/eolib-dotnet/releases/tag/v1.0.0-rc3
[1.0.0-rc2]: https://github.com/ethanmoffat/eolib-dotnet/releases/tag/v1.0.0-rc2
[1.0.0-rc1]: https://github.com/ethanmoffat/eolib-dotnet/releases/tag/v1.0.0-rc1
[1.0.0-beta4]: https://github.com/ethanmoffat/eolib-dotnet/releases/tag/v1.0.0-beta4
[1.0.0-beta3]: https://github.com/ethanmoffat/eolib-dotnet/releases/tag/v1.0.0-beta3
[1.0.0-beta2]: https://github.com/ethanmoffat/eolib-dotnet/releases/tag/v1.0.0-beta2
[1.0.0-beta1]: https://github.com/ethanmoffat/eolib-dotnet/releases/tag/v1.0.0-beta1
