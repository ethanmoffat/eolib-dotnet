using System;
using System.Collections.Generic;
using ProtocolGenerator.Types;

namespace ProtocolGenerator.Model.Protocol;

public class ArrayInstruction : BaseInstruction
{
    private readonly Xml.ProtocolArrayInstruction _xmlArrayInstruction;

    public ArrayInstruction(Xml.ProtocolArrayInstruction xmlArrayInstruction)
    {
        _xmlArrayInstruction = xmlArrayInstruction;

        var optional = _xmlArrayInstruction.Optional.HasValue && _xmlArrayInstruction.Optional.Value;
        TypeInfo = new TypeInfo(_xmlArrayInstruction.Type, isArray: true, optional: optional);

        Name = IdentifierConverter.SnakeCaseToPascalCase(_xmlArrayInstruction.Name);
        Comment = _xmlArrayInstruction.Comment;
    }

    public override void GenerateProperty(GeneratorState state)
    {
        base.GenerateProperty(state);
        state.Text(" = new();", indented: false);
    }

    public override void GenerateSerialize(GeneratorState state, IReadOnlyList<IProtocolInstruction> outerInstructions)
    {
        var delimited = _xmlArrayInstruction.Delimited.HasValue && _xmlArrayInstruction.Delimited.Value;
        var trailingDelimiter = !_xmlArrayInstruction.TrailingDelimiter.HasValue || _xmlArrayInstruction.TrailingDelimiter.Value;

        AssertLength(state, _xmlArrayInstruction.Length);

        if (!string.IsNullOrWhiteSpace(_xmlArrayInstruction.Length))
        {
            var lenExpr = GetLengthExpression(_xmlArrayInstruction.Length, outerInstructions);
            state.For("int ndx = 0", $"ndx < {lenExpr}", "ndx++");
        }
        else
        {
            state.For("int ndx = 0", $"ndx < {Name}.Count", "ndx++");
        }
        state.BeginBlock();

        if (delimited && !trailingDelimiter)
        {
            state.Text("if (ndx > 0)", indented: true);
            state.NewLine();
            state.BeginBlock();
            addDelimiterByte(state);
            state.EndBlock();
        }

        base.GenerateSerialize(state, outerInstructions);

        if (delimited && trailingDelimiter)
        {
            addDelimiterByte(state);
        }

        state.EndBlock();

        static void addDelimiterByte(GeneratorState s)
        {
            s.Text("writer", indented: true);
            s.MethodInvocation("AddByte", "0xFF");
            s.Text(";", indented: false);
            s.NewLine();
        }
    }

    public override void GenerateDeserialize(GeneratorState state, IReadOnlyList<IProtocolInstruction> outerInstructions)
    {
        var delimited = _xmlArrayInstruction.Delimited.HasValue && _xmlArrayInstruction.Delimited.Value;

        var storeRemaining = false;
        string loopCondition;
        if (!string.IsNullOrWhiteSpace(_xmlArrayInstruction.Length))
        {
            var lenExpr = GetLengthExpression(_xmlArrayInstruction.Length, outerInstructions);
            loopCondition = $"ndx < {lenExpr}";
        }
        else
        {
            try
            {
                var typeSize = TypeInfo.CalculateByteSize();
                storeRemaining = typeSize > 1;
                loopCondition = storeRemaining
                    ? $"ndx < remainingFor{Name} / {typeSize}"
                    : "reader.Remaining > 0";
            }
            catch
            {
                loopCondition = "reader.Remaining > 0";
            }
        }

        if (storeRemaining)
        {
            state.Text($"var remainingFor{Name} = reader.Remaining;", indented: true);
            state.NewLine();
        }

        state.For("int ndx = 0", loopCondition, "ndx++");
        state.BeginBlock();

        base.GenerateDeserialize(state, outerInstructions);

        if (delimited && _xmlArrayInstruction.IsChunked)
        {
            var trailingDelimiter = !_xmlArrayInstruction.TrailingDelimiter.HasValue || _xmlArrayInstruction.TrailingDelimiter.Value;
            if (!trailingDelimiter)
            {
                if (string.IsNullOrWhiteSpace(_xmlArrayInstruction.Length))
                    throw new InvalidOperationException($"delimited arrays with trailing-delimiter=false must have a length (array {Name})");

                var lenExpr = GetLengthExpression(_xmlArrayInstruction.Length, outerInstructions);
                state.Text($"if (ndx + 1 < {lenExpr})", indented: true);
                state.NewLine();
                state.BeginBlock();
            }

            state.Text("reader", indented: true);
            state.MethodInvocation("NextChunk");
            state.Text(";", indented: false);
            state.NewLine();

            if (!trailingDelimiter)
            {
                state.EndBlock();
            }
        }

        state.EndBlock();
    }

    public override void GenerateToString(GeneratorState state)
    {
        state.Text($"$\"{{nameof({Name})}}=[{{string.Join(\",\", {Name}.Select(x => x.ToString()))}}]\"", indented: false);
    }
}
