---
_layout: landing
---

# eolib-dotnet

Core .NET library for writing Endless Online applications.

The protocol code is generated from the [eo-protocol](https://github.com/cirras/eo-protocol) XML specification by a
source generator. The source code is available on [GitHub](https://github.com/ethanmoffat/eolib-dotnet). Libraries
with the same API are available for [C++](https://ethanmoffat.github.io/eolib-cpp/) and
[Go](https://pkg.go.dev/github.com/ethanmoffat/eolib-go/v3).

## Features

Read and write the following EO data structures:

- Client and server packets ([Protocol.Net.Client](xref:Moffat.EndlessOnline.SDK.Protocol.Net.Client),
  [Protocol.Net.Server](xref:Moffat.EndlessOnline.SDK.Protocol.Net.Server))
- Endless Map Files (EMF) ([Protocol.Map](xref:Moffat.EndlessOnline.SDK.Protocol.Map))
- Client pub files: items, NPCs, spells and classes ([Protocol.Pub](xref:Moffat.EndlessOnline.SDK.Protocol.Pub))
- Server pub files: talk, shops, drops, inns and skill masters
  ([Protocol.Pub.Server](xref:Moffat.EndlessOnline.SDK.Protocol.Pub.Server))

Utilities:

- Data reader and writer ([EoReader](xref:Moffat.EndlessOnline.SDK.Data.EoReader),
  [EoWriter](xref:Moffat.EndlessOnline.SDK.Data.EoWriter))
- Number and string encoding ([NumberEncoder](xref:Moffat.EndlessOnline.SDK.Data.NumberEncoder),
  [StringEncoder](xref:Moffat.EndlessOnline.SDK.Data.StringEncoder))
- Data encryption and server verification ([DataEncrypter](xref:Moffat.EndlessOnline.SDK.Data.DataEncrypter),
  [ServerVerifier](xref:Moffat.EndlessOnline.SDK.Data.ServerVerifier))
- Packet sequencing ([Packet](xref:Moffat.EndlessOnline.SDK.Packet))

## Installation

The package is available on [nuget.org](https://www.nuget.org/packages/Moffat.EndlessOnline.SDK/) with the ID
`Moffat.EndlessOnline.SDK`:

```
dotnet add package Moffat.EndlessOnline.SDK --version @EOLIB_DOCS_VERSION@
```

## Where to start

- [Getting started](getting-started.md) walks through the main parts of the API.
- The [API reference](xref:Moffat.EndlessOnline.SDK.Data) lists every namespace and type, including the generated
  packets.
- The [changelog](https://github.com/ethanmoffat/eolib-dotnet/blob/master/CHANGELOG.md) lists the changes in each
  release.
