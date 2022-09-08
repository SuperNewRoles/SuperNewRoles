using System;
using System.Linq.Expressions;
using System.Reflection;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;

namespace SuperNewRoles
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class Il2CppRegisterAttribute : Attribute
    {

        public static void Registration(Assembly dll)
        {
            foreach (Type type in dll.GetTypes())
            {
                Il2CppRegisterAttribute attribute =
                    CustomAttributeExtensions.GetCustomAttribute<Il2CppRegisterAttribute>(type);
                if (attribute != null)
                {
                    registrationForTarget(type);
                }
            }
        }

        private static void registrationForTarget(Type targetType)
        {
            Type targetBase = targetType.BaseType;

            Il2CppRegisterAttribute baseAttribute =
                (targetType == null) ?
                null :
                CustomAttributeExtensions.GetCustomAttribute<Il2CppRegisterAttribute>(targetBase);

            if (baseAttribute != null)
            {
                registrationForTarget(targetType);
            }

            if (ClassInjector.IsTypeRegisteredInIl2Cpp(targetType)) { return; }

            try
            {
                ClassInjector.RegisterTypeInIl2Cpp(targetType);
            }
            catch (Exception e)
            {

            }
        }
    }
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