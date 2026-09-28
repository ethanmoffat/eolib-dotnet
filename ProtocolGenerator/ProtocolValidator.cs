using System;
using System.Collections.Generic;
using System.Linq;
using ProtocolGenerator.Model.Xml;
using ProtocolGenerator.Types;

namespace ProtocolGenerator;

/// <summary>
/// Validates the ordering and attribute rules from the eo-protocol element documentation that would otherwise produce
/// generated code that silently reads or writes the wrong data.
/// </summary>
public class ProtocolValidator
{
    private static readonly HashSet<string> IntegerTypes = new() { "byte", "char", "short", "three", "int" };
    private static readonly HashSet<string> StringTypes = new() { "string", "encoded_string" };

    private readonly TypeMapper _typeMapper;

    public ProtocolValidator(TypeMapper typeMapper)
    {
        _typeMapper = typeMapper;
    }

    /// <summary>
    /// Validates every struct and packet in the spec.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown for the first rule violation, naming the type it's in.</exception>
    public void Validate(ProtocolSpec spec)
    {
        foreach (var s in spec.Structs)
            Validate(s.Name, s.Instructions);

        foreach (var p in spec.Packets)
            Validate($"{p.Family}_{p.Action}", p.Instructions);
    }

    private void Validate(string typeName, List<object> instructions)
    {
        try
        {
            var state = new SequenceState();
            ValidateSequence(instructions, isChunked: false, state);
            ValidateLengthReferences(instructions);
        }
        catch (InvalidOperationException ex)
        {
            throw new InvalidOperationException($"{typeName}: {ex.Message}", ex);
        }
    }

    private void ValidateSequence(IEnumerable<object> instructions, bool isChunked, SequenceState state)
    {
        foreach (var inst in instructions ?? Enumerable.Empty<object>())
        {
            if (state.ReachedDummy)
                throw new InvalidOperationException("<dummy> elements must not be followed by any other elements.");

            if (state.ReachedUnsizedArray && inst is not ProtocolBreakInstruction)
                throw new InvalidOperationException("Non-delimited arrays without a length must be the final element (or the final element in the chunk, if chunked reading is enabled).");

            switch (inst)
            {
                case ProtocolFieldInstruction pfi:
                    CheckOptional(state, pfi.Optional == true, pfi.Name);
                    ValidateField(pfi);
                    state.AddField(pfi.Name);
                    break;
                case ProtocolArrayInstruction pai:
                    CheckOptional(state, pai.Optional == true, pai.Name);
                    ValidateType(pai.Type, pai.Name);
                    if (pai.Delimited == true && !isChunked)
                        throw new InvalidOperationException($"Delimited arrays are only allowed in chunked sections ({pai.Name}).");
                    if (pai.Delimited != true && string.IsNullOrWhiteSpace(pai.Length))
                        state.ReachedUnsizedArray = true;
                    state.AddField(pai.Name);
                    break;
                case ProtocolLengthInstruction pli:
                    CheckOptional(state, pli.Optional == true, pli.Name);
                    ValidateType(pli.Type, pli.Name);
                    state.AddField(pli.Name);
                    break;
                case ProtocolDummyInstruction:
                    state.ReachedDummy = true;
                    break;
                case ProtocolBreakInstruction:
                    state.ResetForBreak();
                    break;
                case ProtocolChunkedInstruction pci:
                    ValidateSequence(pci.Instructions, isChunked: true, state);
                    break;
                case ProtocolSwitchInstruction psi:
                    ValidateSwitch(psi, isChunked, state);
                    break;
            }
        }
    }

    private void ValidateSwitch(ProtocolSwitchInstruction psi, bool isChunked, SequenceState state)
    {
        if (string.IsNullOrWhiteSpace(psi.Field) || !state.HasField(psi.Field))
            throw new InvalidOperationException($"Switch must reference a preceding field ({psi.Field}).");

        var cases = psi.Cases ?? new List<ProtocolCase>();
        var values = new HashSet<string>();
        var merged = state.Clone();
        for (var i = 0; i < cases.Count; i++)
        {
            var c = cases[i];
            if (c.Default && i != cases.Count - 1)
                throw new InvalidOperationException($"Only the last case in a switch on {psi.Field} can be the default case.");
            if (!c.Default && !values.Add(c.Value))
                throw new InvalidOperationException($"Duplicate case value {c.Value} in switch on {psi.Field}.");

            var caseState = state.Clone();
            ValidateSequence(c.Instructions, isChunked, caseState);
            merged.Merge(caseState);
        }

        state.CopyFrom(merged);
    }

    private static void CheckOptional(SequenceState state, bool optional, string name)
    {
        if (state.ReachedOptional && !optional)
            throw new InvalidOperationException($"Optional fields may not be followed by non-optional fields ({name}).");

        state.ReachedOptional |= optional;
    }

    private void ValidateField(ProtocolFieldInstruction pfi)
    {
        var name = pfi.Name ?? pfi.Content;
        ValidateType(pfi.Type, name);

        var isString = StringTypes.Contains(TypeInfo.GetTypeName(pfi.Type));
        if (!string.IsNullOrWhiteSpace(pfi.Length) && !isString)
            throw new InvalidOperationException($"The length attribute is only allowed for string types ({name}).");
        if (pfi.Padded == true && !isString)
            throw new InvalidOperationException($"The padded attribute is only allowed for string types ({name}).");
        if (pfi.Padded == true && string.IsNullOrWhiteSpace(pfi.Length))
            throw new InvalidOperationException($"Padded fields must specify a length ({name}).");
    }

    private void ValidateType(string rawType, string name)
    {
        var underlying = TypeInfo.GetTypeSize(rawType ?? string.Empty);
        if (string.IsNullOrWhiteSpace(underlying))
            return;

        var typeName = TypeInfo.GetTypeName(rawType);
        if (!IntegerTypes.Contains(underlying))
            throw new InvalidOperationException($"Underlying type {underlying} must be an integer type ({name}).");
        if (typeName != "bool" && !IntegerTypes.Contains(typeName) && !_typeMapper.HasEnum(typeName))
            throw new InvalidOperationException($"Only integer, bool and enum types can specify an underlying type ({name}: {rawType}).");
    }

    private static void ValidateLengthReferences(IEnumerable<object> instructions)
    {
        var references = new Dictionary<string, string>();
        foreach (var (length, name) in LengthReferences(instructions))
        {
            if (string.IsNullOrWhiteSpace(length) || int.TryParse(length, out _))
                continue;

            if (references.TryGetValue(length, out var existing))
                throw new InvalidOperationException($"Length field {length} must not be referenced by multiple fields ({existing}, {name}).");

            references.Add(length, name);
        }
    }

    // Case bodies are separate scopes: the same length field can be referenced once per case.
    private static IEnumerable<(string Length, string Name)> LengthReferences(IEnumerable<object> instructions)
    {
        foreach (var inst in instructions ?? Enumerable.Empty<object>())
        {
            if (inst is ProtocolFieldInstruction pfi)
                yield return (pfi.Length, pfi.Name);
            else if (inst is ProtocolArrayInstruction pai)
                yield return (pai.Length, pai.Name);
            else if (inst is ProtocolChunkedInstruction pci)
                foreach (var reference in LengthReferences(pci.Instructions))
                    yield return reference;
            else if (inst is ProtocolSwitchInstruction psi)
                foreach (var c in psi.Cases ?? new List<ProtocolCase>())
                    ValidateLengthReferences(c.Instructions);
        }
    }

    private sealed class SequenceState
    {
        private HashSet<string> _fieldNames = new();

        public bool ReachedOptional { get; set; }

        public bool ReachedDummy { get; set; }

        public bool ReachedUnsizedArray { get; set; }

        public void AddField(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
                _fieldNames.Add(name);
        }

        public bool HasField(string name) => _fieldNames.Contains(name);

        public void ResetForBreak()
        {
            ReachedOptional = false;
            ReachedDummy = false;
            ReachedUnsizedArray = false;
        }

        public SequenceState Clone() => new()
        {
            _fieldNames = new HashSet<string>(_fieldNames),
            ReachedOptional = ReachedOptional,
            ReachedDummy = ReachedDummy,
            ReachedUnsizedArray = ReachedUnsizedArray,
        };

        public void Merge(SequenceState other)
        {
            ReachedOptional |= other.ReachedOptional;
            ReachedDummy |= other.ReachedDummy;
            ReachedUnsizedArray |= other.ReachedUnsizedArray;
        }

        public void CopyFrom(SequenceState other)
        {
            _fieldNames = other._fieldNames;
            ReachedOptional = other.ReachedOptional;
            ReachedDummy = other.ReachedDummy;
            ReachedUnsizedArray = other.ReachedUnsizedArray;
        }
    }
}
