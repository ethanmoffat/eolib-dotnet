using System;

namespace ProtocolGenerator;

public sealed class ProtocolGeneratorOptions : IEquatable<ProtocolGeneratorOptions>
{
    public const string InputDirectoryOption = "build_property.ProtocolGenerator_InputDirectory";

    public static ProtocolGeneratorOptions Empty => new(string.Empty, string.Empty);

    public string ProjectRoot { get; }
    public string InputDirectory { get; }

    public ProtocolGeneratorOptions(string projectRoot, string inputDirectory)
    {
        ProjectRoot = projectRoot;
        InputDirectory = inputDirectory;
    }

    public override bool Equals(object obj)
    {
        return obj is ProtocolGeneratorOptions options &&
            Equals(options);
    }

    public override int GetHashCode()
    {
        var hash = 27;
        hash = (13 * hash) + ProjectRoot.GetHashCode();
        hash = (13 * hash) + InputDirectory.GetHashCode();
        return hash;
    }

    public bool Equals(ProtocolGeneratorOptions other)
    {
        return other.ProjectRoot.Equals(ProjectRoot) &&
               other.InputDirectory.Equals(InputDirectory);
    }
}
