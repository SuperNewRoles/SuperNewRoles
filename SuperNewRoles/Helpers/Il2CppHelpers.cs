using System;
using System.Linq.Expressions;
using System.Reflection;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;

namespace SuperNewRoles
{
    public static class Il2CppHelpers
    {
        private static class CastHelper<T> where T : Il2CppObjectBase
        {
            public static Func<IntPtr, T> Cast;
            static CastHelper()
            {
                var constructor = typeof(T).GetConstructor(new[] { typeof(IntPtr) });
                var ptr = Expression.Parameter(typeof(IntPtr));
                var create = Expression.New(constructor!, ptr);
                var lambda = Expression.Lambda<Func<IntPtr, T>>(create, ptr);
                Cast = lambda.Compile();
            }
        }

        public static T CastFast<T>(this Il2CppObjectBase obj) where T : Il2CppObjectBase
        {
            return obj is T casted ? casted : obj.Pointer.CastFast<T>();
        }

        public static T CastFast<T>(this IntPtr ptr) where T : Il2CppObjectBase
        {
            return CastHelper<T>.Cast(ptr);
        }
    }
}