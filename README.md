# eolib-dotnet

Core library for writing Endless Online applications using .NET core.

[![build](https://github.com/ethanmoffat/eolib-dotnet/actions/workflows/build.yml/badge.svg?event=push)](https://github.com/ethanmoffat/eolib-dotnet/actions/workflows/build.yml)
[![Nuget](https://badgen.net/nuget/v/Moffat.EndlessOnline.SDK?icon=nuget)](https://www.nuget.org/packages/Moffat.EndlessOnline.SDK/)

## Features

Read and write the following EO data structures:

- Client packets
- Server packets
- Endless Map Files (EMF)
- Client pub files (items, npcs, spells, classes)
- Server pub files (talk, shops, drops, inns, skillmasters)

Utilities:

- Data reader/writer
- Number/string encoding
- Data encryption
- Packet sequencing

## Requirements

- .NET 8.0.x
- Windows or Linux OS that supports .NET core

## Usage

### Referencing the package

This project is compiled and available as a package on nuget.org with the ID `Moffat.EndlessOnline.SDK`. Execute the following command from within your .NET core project to add a reference to the compiled binaries.

```
dotnet add package Moffat.EndlessOnline.SDK
```

### Sample code

This package is referenced by the [EndlessClient](https://www.github.com/ethanmoffat/EndlessClient) project.

### Building from source

1. Clone the repository, including submodules
    ```
    git clone --recurse-submodules git@github.com:ethanmoffat/eolib-dotnet.git
    ```

2. Restore, build, and test the solution
    ```
    dotnet test
    ```

## Versioning and releases

eolib-dotnet uses [Semantic Versioning](https://semver.org/) (`MAJOR.MINOR.PATCH`, with an optional `-beta.N` or
`-rc.N` suffix). Changes are tracked in [CHANGELOG.md](CHANGELOG.md), following
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

To release a new version:

1. Make sure the `[Unreleased]` section of `CHANGELOG.md` lists the changes.
2. Run `./scripts/prepare-release.sh x.y.z-suffix --tag`. It updates every file that references the version
   (`<Version>` in `Moffat.EndlessOnline.SDK.csproj`, the versions in `Properties/AssemblyInfo.cs`, and the changelog
   section and links), runs `scripts/validate-release.sh`, commits the changes as "Release x.y.z-suffix" and creates
   the tag. Use `--commit` to commit without tagging, `--date` to set the changelog date, or no option to only update
   the files for review.
3. Push master, and wait for CI to pass.
4. Push the tag `vx.y.z-suffix`. The release workflow runs `validate-release.sh` again, which also checks that the
   commit is on `origin/master`, before building anything. It then builds, tests and publishes the NuGet package, and
   publishes a GitHub release. It is marked as a prerelease when the version has a suffix.
