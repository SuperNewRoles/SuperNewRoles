using System.Collections.Generic;
using Il2Col = Il2CppSystem.Collections.Generic;

namespace SuperNewRoles.Helpers {
    public static class ListHelpers {
        public static void DestroyList<T>(Il2Col.List<T> items) where T : UnityEngine.Object
        {
            if (items == null) return;
            foreach (T item in items)
            {
                UnityEngine.Object.Destroy(item);
            }
        }
        public static void DestroyList<T>(List<T> items) where T : UnityEngine.Object
        {
            if (items == null) return;
            foreach (T item in items)
            {
                UnityEngine.Object.Destroy(item);
            }
        }

        public static List<T> ToList<T>(this Il2Col.List<T> list)
        {
            List<T> newList = new();
            foreach (T item in list)
            {
                newList.Add(item);
            }
            return newList;
        }
        public static Il2Col.List<T> ToIl2CppList<T>(this List<T> list)
        {
            Il2Col.List<T> newList = new();
            foreach (T item in list)
            {
                newList.Add(item);
            }
            return newList;
        }

        public static T GetRandom<T>(this List<T> list)
        {
            var indexData = UnityEngine.Random.Range(0, list.Count);
            return list[indexData];
        }
        public static int GetRandomIndex<T>(List<T> list)
        {
            var indexData = UnityEngine.Random.Range(0, list.Count);
            return indexData;
        }
    }
}