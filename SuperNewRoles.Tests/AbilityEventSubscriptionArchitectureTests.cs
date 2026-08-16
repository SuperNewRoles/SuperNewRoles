using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Impostor;
using Xunit;

namespace SuperNewRoles.Tests;

public class AbilityEventSubscriptionArchitectureTests
{
    private class TestEvent : EventTargetBase<TestEvent> { }

    private class DirectSubscriptionFixtureAbility : AbilityBase
    {
        public void SubscribeDirectly()
        {
            TestEvent.Instance.AddListener(() => { });
        }
    }

    [Fact]
    public void Scanner_DetectsDirectEventTargetSubscription()
    {
        var violations = FindDirectEventSubscriptions([typeof(DirectSubscriptionFixtureAbility)]);

        Assert.Single(violations);
        Assert.Contains(nameof(DirectSubscriptionFixtureAbility.SubscribeDirectly), violations[0]);
    }

    [Fact]
    public void ProductionAbilities_UseSubscribeWithAbilityInsteadOfAddListener()
    {
        var abilityAssembly = typeof(AbilityBase).Assembly;
        var abilityOwnedTypes = GetTypesOrThrow(abilityAssembly).Where(IsAbilityOwnedType);
        var violations = FindDirectEventSubscriptions(abilityOwnedTypes);

        Assert.True(
            violations.Count == 0,
            "Ability から EventTargetBase.AddListener を直接呼ばないでください。" +
            " AbilityBase.SubscribeWithAbility を使うと Detach 時に自動解除されます。\n" +
            string.Join("\n", violations));
    }

    private static List<string> FindDirectEventSubscriptions(IEnumerable<Type> types)
    {
        var violations = new List<string>();
        foreach (var type in types)
        {
            foreach (var method in GetDeclaredMethods(type))
            {
                foreach (var calledMethod in ReadCalledMethods(method))
                {
                    if (!IsEventTargetAddListener(calledMethod))
                        continue;
                    if (IsAllowedDirectSubscription(type, method))
                        continue;

                    violations.Add($"{type.FullName}.{method.Name} -> {calledMethod.DeclaringType?.FullName}.{calledMethod.Name}");
                }
            }
        }

        return violations
            .Distinct(StringComparer.Ordinal)
            .OrderBy(violation => violation, StringComparer.Ordinal)
            .ToList();
    }

    private static IEnumerable<MethodBase> GetDeclaredMethods(Type type)
    {
        const BindingFlags flags = BindingFlags.DeclaredOnly |
                                   BindingFlags.Instance |
                                   BindingFlags.Static |
                                   BindingFlags.Public |
                                   BindingFlags.NonPublic;

        foreach (var method in type.GetMethods(flags))
            yield return method;
        foreach (var constructor in type.GetConstructors(flags))
            yield return constructor;
        if (type.TypeInitializer != null)
            yield return type.TypeInitializer;
    }

    private static bool IsAbilityOwnedType(Type type)
    {
        for (var current = type; current != null; current = current.DeclaringType)
        {
            if (typeof(AbilityBase).IsAssignableFrom(current))
                return true;
        }
        return false;
    }

    private static bool IsEventTargetAddListener(MethodBase method)
    {
        if (method.Name != "AddListener")
            return false;

        for (var current = method.DeclaringType; current != null; current = current.BaseType)
        {
            if (!current.IsGenericType)
                continue;

            var genericType = current.GetGenericTypeDefinition();
            if (genericType == typeof(EventTargetBase<>) || genericType == typeof(EventTargetBase<,>))
                return true;
        }
        return false;
    }

    private static bool IsAllowedDirectSubscription(Type type, MethodBase method)
    {
        if (type == typeof(AbilityBase))
        {
            return IsSameMethod(method, SubscribeWithAbilityMethod) ||
                   IsSameMethod(method, SubscribeWithAbilityWithDataMethod);
        }

        // オルフェウスのリスナーは複数の Ability で共有し、最後の1つが Detach するまで残す必要がある。
        // 単一 Ability のライフサイクルに結び付く SubscribeWithAbility では、その寿命を表現できない。
        return type == typeof(OrpheusMainAbility) && IsSameMethod(method, OrpheusSharedSubscriptionMethod);
    }

    private static bool IsSameMethod(MethodBase method, MethodInfo expectedMethod)
        => method.Module == expectedMethod.Module && method.MetadataToken == expectedMethod.MetadataToken;

    private static IEnumerable<MethodBase> ReadCalledMethods(MethodBase sourceMethod)
    {
        var body = sourceMethod.GetMethodBody();
        var il = body?.GetILAsByteArray();
        if (il == null)
            yield break;

        var position = 0;
        while (position < il.Length)
        {
            var opcode = ReadOpcode(il, ref position);
            if (opcode.OperandType == OperandType.InlineMethod)
            {
                var token = BitConverter.ToInt32(il, position);
                if (opcode == OpCodes.Call || opcode == OpCodes.Callvirt)
                {
                    var calledMethod = sourceMethod.Module.ResolveMethod(
                        token,
                        sourceMethod.DeclaringType?.GetGenericArguments(),
                        sourceMethod.IsGenericMethod ? sourceMethod.GetGenericArguments() : null);

                    if (calledMethod != null)
                        yield return calledMethod;
                }
            }

            position += GetOperandSize(opcode.OperandType, il, position);
        }
    }

    private static OpCode ReadOpcode(byte[] il, ref int position)
    {
        var first = il[position++];
        if (first != 0xfe)
            return OneByteOpCodes[first];

        return TwoByteOpCodes[il[position++]];
    }

    private static int GetOperandSize(OperandType operandType, byte[] il, int operandPosition)
    {
        return operandType switch
        {
            OperandType.InlineNone => 0,
            OperandType.ShortInlineBrTarget or OperandType.ShortInlineI or OperandType.ShortInlineVar => 1,
            OperandType.InlineVar => 2,
            OperandType.InlineBrTarget or
            OperandType.InlineField or
            OperandType.InlineI or
            OperandType.InlineMethod or
            OperandType.InlineSig or
            OperandType.InlineString or
            OperandType.InlineTok or
            OperandType.InlineType or
            OperandType.ShortInlineR => 4,
            OperandType.InlineI8 or OperandType.InlineR => 8,
            OperandType.InlineSwitch => 4 + BitConverter.ToInt32(il, operandPosition) * 4,
            _ => throw new InvalidOperationException($"Unsupported IL operand type: {operandType}")
        };
    }

    private static Type[] GetTypesOrThrow(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            var loaderExceptionDetails = string.Join(
                Environment.NewLine,
                ex.LoaderExceptions
                    .Where(loaderException => loaderException != null)
                    .Select((loaderException, index) => $"[{index}] {loaderException}"));

            if (string.IsNullOrEmpty(loaderExceptionDetails))
                loaderExceptionDetails = "(no LoaderExceptions were provided)";

            throw new InvalidOperationException(
                $"Assembly '{assembly.FullName}' could not be fully loaded. " +
                $"Ability subscription validation cannot continue safely.{Environment.NewLine}" +
                loaderExceptionDetails,
                ex);
        }
    }

    private static MethodInfo GetAbilityBaseSubscriptionMethod(int genericArgumentCount)
    {
        const BindingFlags flags = BindingFlags.DeclaredOnly |
                                   BindingFlags.Instance |
                                   BindingFlags.NonPublic;

        return typeof(AbilityBase)
            .GetMethods(flags)
            .Single(method =>
                method.Name == "SubscribeWithAbility" &&
                method.IsGenericMethodDefinition &&
                method.GetGenericArguments().Length == genericArgumentCount);
    }

    private static readonly MethodInfo SubscribeWithAbilityMethod = GetAbilityBaseSubscriptionMethod(1);
    private static readonly MethodInfo SubscribeWithAbilityWithDataMethod = GetAbilityBaseSubscriptionMethod(2);
    private static readonly MethodInfo OrpheusSharedSubscriptionMethod =
        typeof(OrpheusMainAbility).GetMethod(
            "RegisterSharedListeners",
            BindingFlags.Static | BindingFlags.NonPublic,
            binder: null,
            types: Type.EmptyTypes,
            modifiers: null)
        ?? throw new InvalidOperationException("Orpheus shared subscription method was not found.");

    private static readonly OpCode[] OneByteOpCodes = BuildOpcodeLookup(twoByte: false);
    private static readonly OpCode[] TwoByteOpCodes = BuildOpcodeLookup(twoByte: true);

    private static OpCode[] BuildOpcodeLookup(bool twoByte)
    {
        var result = new OpCode[256];
        foreach (var field in typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if (field.GetValue(null) is not OpCode opcode)
                continue;

            var value = unchecked((ushort)opcode.Value);
            if ((value > byte.MaxValue) != twoByte)
                continue;

            result[value & byte.MaxValue] = opcode;
        }
        return result;
    }
}
