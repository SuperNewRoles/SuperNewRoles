using System;
using System.Collections.Generic;

namespace SuperNewRoles.Modules {
    public static class Extensions{
        public static void Shuffle<T>(this IList<T> self, int startAt = 0)
        {
            for (int i = startAt; i < self.Count - 1; i++)
            {
                T value = self[i];
                int index = UnityEngine.Random.Range(i, self.Count);
                self[i] = self[index];
                self[index] = value;
            }
        }

        // Token: 0x060002F4 RID: 756 RVA: 0x00013308 File Offset: 0x00011508
        public static void Shuffle<T>(this Random r, IList<T> self)
        {
            for (int i = 0; i < self.Count; i++)
            {
                T value = self[i];
                int index = r.Next(self.Count);
                self[i] = self[index];
                self[index] = value;
            }
        }
        public static void ForEach<T>(this IList<T> self, Action<T> todo)
        {
            for (int i = 0; i < self.Count; i++)
            {
                todo(self[i]);
            }
        }
        public static T Random<T>(this IList<T> self)
        {
            return self.Count > 0 ? self[UnityEngine.Random.Range(0, self.Count)] : default;
        }
    }
}