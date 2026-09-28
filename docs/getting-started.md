# Getting started

This guide covers the main parts of the eolib-dotnet API.

## Namespaces

| Namespace | Contents |
|---|---|
| `Moffat.EndlessOnline.SDK.Data` | `EoReader`, `EoWriter`, number/string encoding, packet encryption and server verification |
| `Moffat.EndlessOnline.SDK.Packet` | Packet sequencing |
| `Moffat.EndlessOnline.SDK.Protocol` | `ISerializable`, `PacketResolver`, and the types shared by the rest of the protocol |
| `Moffat.EndlessOnline.SDK.Protocol.Net` | `IPacket`, `PacketFamily`, `PacketAction`, and the types shared by client and server packets |
| `Moffat.EndlessOnline.SDK.Protocol.Net.Client`, `Moffat.EndlessOnline.SDK.Protocol.Net.Server` | Client or server packets |
| `Moffat.EndlessOnline.SDK.Protocol.Pub`, `Moffat.EndlessOnline.SDK.Protocol.Pub.Server` | Client and server pub files |
| `Moffat.EndlessOnline.SDK.Protocol.Map` | Map files |

## Naming conventions

- Namespaces follow the protocol XML layout.
- Types, properties and enum values are PascalCase versions of the protocol XML names. Packets are named
  `<Family><Action><Client|Server>Packet`, for example `WalkPlayerClientPacket`.

## Protocol types

Generated types implement `ISerializable`, which provides `Serialize(EoWriter)`, `Deserialize(EoReader)` and
`ByteSize`. They also override `ToString()`, `Equals()` and `GetHashCode()`.

| Protocol type | .NET type |
|---|---|
| `byte`, `char`, `short`, `three`, `int` | `int` |
| `bool` | `bool` |
| `string`, `encoded_string` | `string` (Windows-1252) |
| `blob` | `byte[]` |
| enum | `enum`. Unrecognized values are preserved. |
| array | `List<T>` |
| optional field | Nullable type (`int?`, `string?`, ...) |
| switch | An interface named `I<Field>Data`, implemented by a nested class for each case |

## Packets

Every packet implements `IPacket`, which adds the `Family` and `Action` properties.

```csharp
using Moffat.EndlessOnline.SDK.Data;
using Moffat.EndlessOnline.SDK.Protocol;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;

var walk = new WalkPlayerClientPacket
{
    WalkAction = new WalkAction
    {
        Direction = Direction.Up,
        Coords = new Coords { X = 5, Y = 7 },
    },
};

var writer = new EoWriter();
walk.Serialize(writer);
byte[] payload = writer.ToByteArray();
```

`PacketResolver` creates packets from their family and action. It throws `InvalidOperationException` for unknown
packets:

```csharp
var resolver = new PacketResolver("Moffat.EndlessOnline.SDK.Protocol.Net.Client");

IPacket packet = resolver.Create(PacketFamily.Walk, PacketAction.Player);
packet.Deserialize(new EoReader(payload));

if (packet is WalkPlayerClientPacket received)
{
    // use received.WalkAction
}
```

Switch fields are properties named `<Field>Data`. Set a case class that matches the switch field:

```csharp
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

var init = new InitInitServerPacket
{
    ReplyCode = InitReply.Ok,
    ReplyCodeData = new InitInitServerPacket.ReplyCodeDataOk { PlayerId = 1 },
};

if (init.ReplyCodeData is InitInitServerPacket.ReplyCodeDataOk ok)
{
    // use ok.PlayerId
}
```

## Pub and map files

```csharp
using Moffat.EndlessOnline.SDK.Protocol.Pub;

var eif = new Eif();
eif.Deserialize(new EoReader(File.ReadAllBytes("dat001.eif")));

foreach (var item in eif.Items)
{
    Console.WriteLine(item.Name);
}
```

`Enf`, `Esf` and `Ecf`, the `Protocol.Pub.Server` files and `Protocol.Map.Emf` work the same way.

## Encryption and sequencing

`DataEncrypter` transforms the packet bytes after the 2-byte length prefix, and returns the result:

```csharp
// Encrypt
bytes = DataEncrypter.SwapMultiples(bytes, 6);
bytes = DataEncrypter.Interleave(bytes);
bytes = DataEncrypter.FlipMSB(bytes);

// Decrypt
bytes = DataEncrypter.FlipMSB(bytes);
bytes = DataEncrypter.Deinterleave(bytes);
bytes = DataEncrypter.SwapMultiples(bytes, 6);
```

A server generates a sequence start for the init handshake and tracks the sequence for each connection:

```csharp
using Moffat.EndlessOnline.SDK.Packet;

var start = InitSequenceStart.Generate(new Random());
var sequencer = new PacketSequencer(start);

// Send start.Seq1 and start.Seq2 in the init reply, then check each client packet's sequence number
int expected = sequencer.NextSequence();
```

`PacketSequencer.WithSequenceStart` returns a sequencer for a new start, such as the `PingSequenceStart` a server
sends with connection pings.

## Reading and writing data

`EoReader` and `EoWriter` read and write EO numbers and strings directly, for data that isn't generated from the
protocol:

```csharp
var writer = new EoWriter();
writer.AddShort(1234);
writer.AddString("hello");

var reader = new EoReader(writer.ToByteArray());
int number = reader.GetShort();
string text = reader.GetString();
```

Reading past the end of the data returns `0` or an empty value instead of throwing, which matches the official
client. `EoReader.ChunkedReadingMode` and `NextChunk()` read data split into chunks by `0xFF` break bytes.
