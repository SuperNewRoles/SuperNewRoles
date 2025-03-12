using System;

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
}