using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace SuperNewRoles.Modules;

/// <summary>
/// コルーチンのMoveNextメソッドに対してパッチを適用するためのAttribute
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class HarmonyCoroutinePatchAttribute : Attribute
{
    public Type TargetType { get; }
    public string CoroutineMethodName { get; }

    /// <summary>
    /// コルーチンパッチのAttributeを初期化します
    /// </summary>
    /// <param name="targetType">パッチ対象のクラス</param>
    /// <param name="coroutineMethodName">コルーチンメソッドの名前</param>
    public HarmonyCoroutinePatchAttribute(Type targetType, string coroutineMethodName)
    {
        TargetType = targetType;
        CoroutineMethodName = coroutineMethodName;
    }
}

/// <summary>
/// HarmonyCoroutinePatchAttributeを処理するクラス
/// </summary>
public static class HarmonyCoroutinePatchProcessor
{
    // スレッドセーフなキャッシュ
    private static readonly ConcurrentDictionary<(Type, Type), PropertyInfo> _parentInstanceFieldCache = new();
    private static readonly ConcurrentDictionary<(Type, string), Type> _coroutineTypeCache = new();
    private static readonly ConcurrentDictionary<Type, MethodInfo> _moveNextMethodCache = new();
    private static readonly ConcurrentDictionary<Type, (MethodInfo prefix, MethodInfo postfix)> _patchMethodCache = new();

    // メモリリーク防止のためWeakReferenceを使用
    private static readonly ConditionalWeakTable<object, object> _coroutineStateMachineInstanceCache = new();

    // プロパティ名のキャッシュ（頻繁に使用される文字列の最適化）
    private static readonly string[] CommonThisPropertyNames = { "<>4__this", "__this", "4__this" };

    /// <summary>
    /// コルーチンステートマシンインスタンスから親クラスのインスタンスを試行取得します。
    /// </summary>
    /// <typeparam name="TParent">親クラスの型</typeparam>
    /// <param name="coroutineStateMachine">コルーチンステートマシンのインスタンス</param>
    /// <param name="parentInstance">取得された親クラスのインスタンス</param>
    /// <returns>取得に成功した場合はtrue、それ以外はfalse</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryGetParentInstance<TParent>(object coroutineStateMachine, out TParent parentInstance) where TParent : class
    {
        parentInstance = null;
        if (coroutineStateMachine == null) return false;

        var stateMachineType = coroutineStateMachine.GetType();
        var parentType = typeof(TParent);
        var cacheKey = (stateMachineType, parentType);

        // キャッシュから取得を試行
        if (_parentInstanceFieldCache.TryGetValue(cacheKey, out var cachedProperty))
        {
            if (cachedProperty != null)
            {
                try
                {
                    parentInstance = cachedProperty.GetValue(coroutineStateMachine) as TParent;
                    return parentInstance != null;
                }
                catch
                {
                    // プロパティアクセスエラーの場合はキャッシュから削除
                    _parentInstanceFieldCache.TryRemove(cacheKey, out _);
                    return false;
                }
            }
            return false;
        }

        // より効率的なプロパティ検索
        var property = FindThisProperty(stateMachineType, parentType);

        // キャッシュに追加（nullも含む）
        _parentInstanceFieldCache.TryAdd(cacheKey, property);

        if (property != null)
        {
            try
            {
                parentInstance = property.GetValue(coroutineStateMachine) as TParent;
                return parentInstance != null;
            }
            catch
            {
                return false;
            }
        }

        SuperNewRolesPlugin.Logger.LogWarning($"親インスタンスフィールド ({parentType.Name}) がコルーチンステートマシン ({stateMachineType.Name}) 内に見つかりませんでした。");
        return false;
    }

    /// <summary>
    /// __thisプロパティを効率的に検索します
    /// </summary>
    /// <param name="stateMachineType">ステートマシンの型</param>
    /// <param name="parentType">親の型</param>
    /// <returns>見つかったプロパティ、またはnull</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static PropertyInfo FindThisProperty(Type stateMachineType, Type parentType)
    {
        // 最も一般的なプロパティ名を優先的に検索
        foreach (var propertyName in CommonThisPropertyNames)
        {
            var property = stateMachineType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (property != null && IsCompatibleType(property.PropertyType, parentType))
            {
                return property;
            }
        }

        // その他のパターンを検索（最適化済み）
        var properties = stateMachineType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        for (int i = 0; i < properties.Length; i++)
        {
            var prop = properties[i];
            var name = prop.Name;

            // 文字列操作を最小限に抑制
            if (name.Length > 6 && // "__this".Length
                (name.IndexOf("__this", StringComparison.Ordinal) != -1 ||
                 name.IndexOf("4__this", StringComparison.Ordinal) != -1) &&
                IsCompatibleType(prop.PropertyType, parentType))
            {
                return prop;
            }
        }

        return null;
    }

    /// <summary>
    /// 型の互換性をチェックします
    /// </summary>
    /// <param name="propertyType">プロパティの型</param>
    /// <param name="parentType">親の型</param>
    /// <returns>互換性がある場合true</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsCompatibleType(Type propertyType, Type parentType)
    {
        return propertyType == parentType || parentType.IsAssignableFrom(propertyType);
    }

    /// <summary>
    /// コルーチンステートマシンインスタンスから親クラスのインスタンスを取得します。
    /// 取得できない場合はnullを返します。
    /// </summary>
    /// <typeparam name="TParent">親クラスの型</typeparam>
    /// <param name="coroutineStateMachineInstance">コルーチンステートマシンのインスタンス</param>
    /// <returns>親クラスのインスタンス、またはnull</returns>
    public static TParent GetParentFromCoroutine<TParent>(object coroutineStateMachineInstance) where TParent : class
    {
        if (coroutineStateMachineInstance == null) return null;

        // WeakReferenceキャッシュから取得を試行
        if (_coroutineStateMachineInstanceCache.TryGetValue(coroutineStateMachineInstance, out var cachedResult))
        {
            return cachedResult as TParent;
        }

        if (TryGetParentInstance<TParent>(coroutineStateMachineInstance, out var parent))
        {
            // キャッシュに保存
            _coroutineStateMachineInstanceCache.Add(coroutineStateMachineInstance, parent);
            return parent;
        }

        SuperNewRolesPlugin.Logger.LogError($"親インスタンス ({typeof(TParent).Name}) をコルーチンステートマシン ({coroutineStateMachineInstance.GetType().Name}) から取得できませんでした。");
        return null;
    }

    /// <summary>
    /// 指定されたアセンブリ内のHarmonyCoroutinePatchAttributeを持つクラスを処理します
    /// </summary>
    /// <param name="harmony">Harmonyインスタンス</param>
    /// <param name="assembly">処理対象のアセンブリ</param>
    public static void ProcessCoroutinePatches(Harmony harmony, Assembly assembly)
    {
        try
        {
            // より効率的な型検索
            var typesWithCoroutinePatch = GetTypesWithCoroutinePatches(assembly);

            foreach (var (patchClass, attribute) in typesWithCoroutinePatch)
            {
                try
                {
                    ProcessSingleCoroutinePatch(harmony, patchClass, attribute);
                }
                catch (Exception ex)
                {
                    SuperNewRolesPlugin.Logger.LogError($"コルーチンパッチの適用に失敗しました: {patchClass.Name} - {ex}");
                }
            }
        }
        catch (Exception ex)
        {
            SuperNewRolesPlugin.Logger.LogError($"コルーチンパッチ処理中にエラーが発生しました: {ex}");
        }
    }

    /// <summary>
    /// 単一のコルーチンパッチを処理します
    /// </summary>
    /// <param name="harmony">Harmonyインスタンス</param>
    /// <param name="patchClass">パッチクラス</param>
    /// <param name="attribute">パッチ属性</param>
    private static void ProcessSingleCoroutinePatch(Harmony harmony, Type patchClass, HarmonyCoroutinePatchAttribute attribute)
    {
        // コルーチンの内部クラスを検索（キャッシュ付き）
        var coroutineType = FindCoroutineTypeWithCache(attribute.TargetType, attribute.CoroutineMethodName);
        if (coroutineType == null)
        {
            SuperNewRolesPlugin.Logger.LogWarning($"コルーチンタイプが見つかりません: {attribute.TargetType.Name}.{attribute.CoroutineMethodName}");
            return;
        }

        // MoveNextメソッドを取得（キャッシュ付き）
        var moveNextMethod = GetMoveNextMethodWithCache(coroutineType);
        if (moveNextMethod == null)
        {
            SuperNewRolesPlugin.Logger.LogWarning($"MoveNextメソッドが見つかりません: {coroutineType.Name}");
            return;
        }

        // パッチメソッドを検索（キャッシュ付き）
        var (prefixMethod, postfixMethod) = GetPatchMethodsWithCache(patchClass);

        if (prefixMethod == null && postfixMethod == null)
        {
            SuperNewRolesPlugin.Logger.LogWarning($"PrefixまたはPostfixメソッドが見つかりません: {patchClass.Name}");
            return;
        }

        // パッチを適用
        var prefix = prefixMethod != null ? new HarmonyMethod(prefixMethod) : null;
        var postfix = postfixMethod != null ? new HarmonyMethod(postfixMethod) : null;

        harmony.Patch(moveNextMethod, prefix, postfix);

        SuperNewRolesPlugin.Logger.LogInfo($"コルーチンパッチを適用しました: {attribute.TargetType.Name}.{attribute.CoroutineMethodName} -> {coroutineType.Name}.MoveNext");
    }

    /// <summary>
    /// アセンブリからコルーチンパッチ属性を持つクラスを効率的に取得します
    /// </summary>
    /// <param name="assembly">処理対象のアセンブリ</param>
    /// <returns>パッチクラスと属性のペア</returns>
    private static IEnumerable<(Type patchClass, HarmonyCoroutinePatchAttribute attribute)> GetTypesWithCoroutinePatches(Assembly assembly)
    {
        // GetTypes()の呼び出しを一回に削減し、LINQを使用せずループで処理
        Type[] types;
        try
        {
            types = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            // 部分的な読み込み失敗の場合は、読み込めた型のみを使用
            types = ex.Types.Where(t => t != null).ToArray();
            SuperNewRolesPlugin.Logger.LogWarning($"一部の型の読み込みに失敗しました: {assembly.FullName}");
        }

        for (int i = 0; i < types.Length; i++)
        {
            var type = types[i];
            HarmonyCoroutinePatchAttribute attribute = null;

            try
            {
                attribute = type.GetCustomAttribute<HarmonyCoroutinePatchAttribute>();
            }
            catch (Exception ex)
            {
                SuperNewRolesPlugin.Logger.LogWarning($"型 {type?.Name} の属性取得に失敗しました: {ex.Message}");
                continue;
            }

            if (attribute != null)
            {
                yield return (type, attribute);
            }
        }
    }

    /// <summary>
    /// キャッシュ付きでコルーチン型を検索します
    /// </summary>
    /// <param name="targetType">検索対象のクラス</param>
    /// <param name="coroutineMethodName">コルーチンメソッドの名前</param>
    /// <returns>見つかった内部クラス、見つからない場合はnull</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Type FindCoroutineTypeWithCache(Type targetType, string coroutineMethodName)
    {
        var cacheKey = (targetType, coroutineMethodName);

        if (_coroutineTypeCache.TryGetValue(cacheKey, out var cachedType))
        {
            return cachedType;
        }

        var coroutineType = FindCoroutineType(targetType, coroutineMethodName);
        _coroutineTypeCache.TryAdd(cacheKey, coroutineType);
        return coroutineType;
    }

    /// <summary>
    /// キャッシュ付きでMoveNextメソッドを取得します
    /// </summary>
    /// <param name="coroutineType">コルーチン型</param>
    /// <returns>MoveNextメソッド</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static MethodInfo GetMoveNextMethodWithCache(Type coroutineType)
    {
        if (_moveNextMethodCache.TryGetValue(coroutineType, out var cachedMethod))
        {
            return cachedMethod;
        }

        var moveNextMethod = coroutineType.GetMethod("MoveNext", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (moveNextMethod != null)
        {
            _moveNextMethodCache.TryAdd(coroutineType, moveNextMethod);
        }
        return moveNextMethod;
    }

    /// <summary>
    /// キャッシュ付きでパッチメソッドを取得します
    /// </summary>
    /// <param name="patchClass">パッチクラス</param>
    /// <returns>PrefixとPostfixメソッドのタプル</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static (MethodInfo prefix, MethodInfo postfix) GetPatchMethodsWithCache(Type patchClass)
    {
        if (_patchMethodCache.TryGetValue(patchClass, out var cachedMethods))
        {
            return cachedMethods;
        }

        var prefixMethod = patchClass.GetMethod("Prefix", BindingFlags.Public | BindingFlags.Static);
        var postfixMethod = patchClass.GetMethod("Postfix", BindingFlags.Public | BindingFlags.Static);

        var methods = (prefixMethod, postfixMethod);
        _patchMethodCache.TryAdd(patchClass, methods);
        return methods;
    }

    /// <summary>
    /// 指定されたクラス内でコルーチンメソッドに対応する内部クラスを検索します
    /// </summary>
    /// <param name="targetType">検索対象のクラス</param>
    /// <param name="coroutineMethodName">コルーチンメソッドの名前</param>
    /// <returns>見つかった内部クラス、見つからない場合はnull</returns>
    private static Type FindCoroutineType(Type targetType, string coroutineMethodName)
    {
        try
        {
            // ネストされた型を取得
            var nestedTypes = targetType.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic);

            // 最も一般的なパターンを事前計算
            var pattern1 = $"<{coroutineMethodName}>d__";
            var pattern2 = $"_{coroutineMethodName}_d__";

            // 高速パターンマッチング（最適化）
            for (int i = 0; i < nestedTypes.Length; i++)
            {
                var nestedType = nestedTypes[i];
                var typeName = nestedType.Name;

                if (typeName.StartsWith(pattern1, StringComparison.Ordinal) ||
                    typeName.StartsWith(pattern2, StringComparison.Ordinal))
                {
                    return nestedType;
                }
            }

            // フォールバック：より寛容なマッチング
            for (int i = 0; i < nestedTypes.Length; i++)
            {
                var nestedType = nestedTypes[i];
                if (IsCoroutineTypeFallback(nestedType, coroutineMethodName))
                {
                    return nestedType;
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            SuperNewRolesPlugin.Logger.LogError($"コルーチン型の検索中にエラーが発生しました: {targetType.Name}.{coroutineMethodName} - {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// より寛容なコルーチン型チェック（フォールバック用）
    /// </summary>
    /// <param name="type">判定対象の型</param>
    /// <param name="coroutineMethodName">コルーチンメソッドの名前</param>
    /// <returns>コルーチンの内部クラスの場合true</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsCoroutineTypeFallback(Type type, string coroutineMethodName)
    {
        var typeName = type.Name;

        // d__が含まれていない場合は確実に違う（高速チェック）
        if (typeName.IndexOf("d__", StringComparison.Ordinal) == -1)
        {
            return false;
        }

        // メソッド名が含まれているかチェック
        return typeName.IndexOf(coroutineMethodName, StringComparison.Ordinal) != -1;
    }
}

// For example:
// [HarmonyCoroutinePatch(typeof(IntroCutscene), nameof(IntroCutscene.CoBegin))]
// public static class IntroCutscenePatch
// {
//     public static void Postfix(object __instance)
//     {
//         IntroCutscene introCutscene = HarmonyCoroutinePatchProcessor.GetParentFromCoroutine<IntroCutscene>(__instance);
//         if (introCutscene == null) return;
//         // use introCutscene
//     }
// }

// after or before patch all
// HarmonyCoroutinePatchProcessor.ProcessCoroutinePatches(harmony, assembly);