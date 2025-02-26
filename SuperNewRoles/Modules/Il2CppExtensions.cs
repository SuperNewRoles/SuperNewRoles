using System;
using System.Linq;
using BepInEx.Unity.IL2CPP.Utils.Collections;

namespace SuperNewRoles.Modules;
public static class Il2CppExtensions
{
    public static int Count<T>(this Il2CppSystem.Collections.Generic.List<T> list)
    {
        return list.Count;
    }
    public static int Count<T>(this Il2CppSystem.Collections.Generic.List<T> list, Func<T, bool> predicate)
    {
        int count = 0;
        foreach (T item in list)
        {
            if (predicate(item))
                count++;
        }
        return count;
    }
    public static Il2CppSystem.Collections.Generic.List<T> ToIl2CppList<T>(this System.Collections.Generic.List<T> list)
    {
        return new Il2CppSystem.Collections.Generic.List<T>(list.WrapToIl2Cpp().TryCast<Il2CppSystem.Collections.Generic.IEnumerable<T>>());
    }
    public static System.Collections.Generic.List<T> ToSystemList<T>(this Il2CppSystem.Collections.Generic.List<T> list)
    {
        return [.. CollectionExtensions.WrapToManaged(list.TryCast<Il2CppSystem.Collections.IEnumerable>()).Cast<T>()];
    }
    public static System.Collections.Generic.List<T> ToSystemList<T>(this Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppArrayBase<T> array)
    {
        return [.. array.Cast<T>()];
    }
}