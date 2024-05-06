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

    public override void GenerateToString(GeneratorState state)
    {
        state.Text($"$\"{{nameof({Name})}}=[{{string.Join(\",\", {Name}.Select(x => x.ToString()))}}]\"", indented: false);
    }
}
