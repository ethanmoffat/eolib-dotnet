using Moffat.EndlessOnline.SDK.Data;

namespace Moffat.EndlessOnline.SDK.Protocol;

/// <summary>
/// Represents a protocol object that can be serialized/deserialized.
/// </summary>
public interface ISerializable
{
    /// <summary>
    /// Serialize this object to an EoWriter in the EO binary data representation.
    /// </summary>
    /// <param name="writer">The writer to which this object should be written.</param>
    void Serialize(EoWriter writer);

    /// <summary>
    /// Deserialize the EO binary data representation of this object from an EoReader.
    /// </summary>
    /// <param name="reader">The reader from which this object should be read.</param>
    void Deserialize(EoReader reader);
}
