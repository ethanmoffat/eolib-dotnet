using System;
using System.Collections.Generic;
using System.Linq;
using ProtocolGenerator.Extensions;
using ProtocolGenerator.Model.Xml;
using ProtocolGenerator.Types;

namespace ProtocolGenerator.Model.Protocol;

/// <summary>
/// Creates the <see cref="SwitchFactory"/> methods for the switches in a packet or struct.
/// </summary>
/// <remarks>
/// A factory is created for each value of a switch field's enum, and for each numeric or default case with data.
/// Nested switches are flattened: the outer case gets a factory for each value of the inner switch instead.
/// </remarks>
public static class SwitchFactoryBuilder
{
    private const string FactoryPrefix = "For";

    /// <summary>
    /// Creates the factories for the switches in the instructions of a packet or struct.
    /// </summary>
    public static List<SwitchFactory> Build(IReadOnlyList<object> instructions, TypeMapper typeMapper)
    {
        var factories = new List<SwitchFactory>();
        var scope = instructions.FlattenChunked();
        var psi = FindSwitch(scope);
        if (psi != null)
            AddFactories(factories, psi, scope, new List<SwitchFactoryStep>(), string.Empty, string.Empty, typeMapper);

        return factories;
    }

    private static void AddFactories(
        List<SwitchFactory> factories,
        ProtocolSwitchInstruction psi,
        IReadOnlyList<object> scope,
        IReadOnlyList<SwitchFactoryStep> parentSteps,
        string namePrefix,
        string typePrefix,
        TypeMapper typeMapper)
    {
        var field = scope.OfType<ProtocolFieldInstruction>().First(x => x.Name == psi.Field);
        var codeTypeInfo = new TypeInfo(typeMapper, field.Type);
        var codeType = codeTypeInfo.PropertyType;
        var cases = psi.Cases ?? new List<ProtocolCase>();
        var hasDefault = cases.Any(x => x.Default);

        var enumValues = codeTypeInfo.IsEnum
            ? typeMapper.GetEnumValues(codeTypeInfo.ProtocolTypeName)
            : new List<ProtocolEnumValue>();

        foreach (var value in enumValues)
        {
            var valueName = value.Name.Trim();
            var c = cases.FirstOrDefault(x => !x.Default && x.Value == valueName);

            // Values without a case are handled by the default case factory, when there is one
            if (c == null && hasDefault)
                continue;

            var step = CreateStep(psi, c, SwitchInstruction.GetCaseValueExpression(codeType, valueName), $"<see cref=\"{codeType}.{valueName}\"/>", typePrefix, typeMapper);
            var name = namePrefix + IdentifierConverter.SnakeCaseToPascalCase(valueName);
            AddCaseFactories(factories, c, step, parentSteps, name, typeMapper);
        }

        foreach (var c in cases.Where(x => !x.Default && int.TryParse(x.Value, out _)))
        {
            if (Instructions(c).Count == 0)
                continue;

            var step = CreateStep(psi, c, SwitchInstruction.GetCaseValueExpression(codeType, c.Value), c.Value, typePrefix, typeMapper);
            var dataTypeName = SwitchInstruction.GetSwitchCaseName(psi.Field, c.Value, isDefault: false);
            factories.Add(new SwitchFactory(FactoryPrefix + dataTypeName, parentSteps.Append(step).ToList()));
        }

        var defaultCase = cases.FirstOrDefault(x => x.Default);
        if (defaultCase == null)
            return;

        var excludedCodes = cases
            .Where(x => !x.Default)
            .Select(x => SwitchInstruction.GetCaseValueExpression(codeType, x.Value))
            .ToList();
        var defaultStep = CreateStep(psi, defaultCase, string.Empty, "<paramref name=\"code\"/>", typePrefix, typeMapper);
        var defaultName = $"{FactoryPrefix}{IdentifierConverter.SnakeCaseToPascalCase(psi.Field)}Default";
        factories.Add(new SwitchFactory(defaultName, new List<SwitchFactoryStep> { defaultStep }, codeType, excludedCodes));
    }

    private static void AddCaseFactories(
        List<SwitchFactory> factories,
        ProtocolCase c,
        SwitchFactoryStep step,
        IReadOnlyList<SwitchFactoryStep> parentSteps,
        string name,
        TypeMapper typeMapper)
    {
        var steps = parentSteps.Append(step).ToList();

        var scope = c == null ? new List<object>() : Instructions(c).FlattenChunked();
        var nestedSwitch = FindSwitch(scope);
        if (nestedSwitch == null)
        {
            factories.Add(new SwitchFactory(FactoryPrefix + name, steps));
            return;
        }

        // The outer case only exists to hold the nested switch, so its data is created by the factory
        AddFactories(factories, nestedSwitch, scope, steps, name, $"{step.DataTypeName}.", typeMapper);
    }

    private static SwitchFactoryStep CreateStep(ProtocolSwitchInstruction psi, ProtocolCase c, string codeExpression, string codeDocumentation, string typePrefix, TypeMapper typeMapper)
    {
        var fieldName = IdentifierConverter.SnakeCaseToPascalCase(psi.Field);
        var dataPropertyName = SwitchInstruction.GetSwitchInterfaceMemberName(psi.Field);
        var hasData = c != null && Instructions(c).Count > 0;
        var dataTypeName = hasData
            ? typePrefix + SwitchInstruction.GetSwitchCaseName(psi.Field, c.Value, c.Default)
            : string.Empty;

        return new SwitchFactoryStep(
            typePrefix + fieldName,
            fieldName,
            dataPropertyName,
            typePrefix + dataPropertyName,
            codeExpression,
            codeDocumentation,
            dataTypeName,
            hasData && Instructions(c).Select(x => ProtocolInstructionFactory.Transform(typeMapper, x)).Any(IsSettable));
    }

    // ProtocolValidator ensures there is at most one switch per scope
    private static ProtocolSwitchInstruction FindSwitch(IReadOnlyList<object> scope) => scope.OfType<ProtocolSwitchInstruction>().FirstOrDefault();

    // Hardcoded fields and lengths of other fields have no public setter, so a case with only those is created by the factory
    private static bool IsSettable(IProtocolInstruction instruction)
    {
        return instruction is ChunkedInstruction
            ? instruction.Instructions.Any(IsSettable)
            : instruction.HasProperty && !instruction.IsReadOnly;
    }

    private static IReadOnlyList<object> Instructions(ProtocolCase c) => c.Instructions ?? new List<object>();
}
