# eolib-dotnet

Core library for writing Endless Online applications using .NET core.

[![build](https://github.com/ethanmoffat/eolib-dotnet/actions/workflows/build.yml/badge.svg?event=push)](https://github.com/ethanmoffat/eolib-dotnet/actions/workflows/build.yml)

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

- .NET 6.0.421+
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
