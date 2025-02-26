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
        var il2cppList = new Il2CppSystem.Collections.Generic.List<T>();
        foreach (var item in list)
        {
            il2cppList.Add(item);
        }
        return il2cppList;
    }
    public static System.Collections.Generic.List<T> ToSystemList<T>(this Il2CppSystem.Collections.Generic.List<T> list)
    {
        var systemList = new System.Collections.Generic.List<T>();
        foreach (var item in list)
        {
            systemList.Add(item);
        }
        return systemList;
    }
    public static System.Collections.Generic.List<T> ToSystemList<T>(this Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppArrayBase<T> array)
    {
        return [.. array.Cast<T>()];
    }
}