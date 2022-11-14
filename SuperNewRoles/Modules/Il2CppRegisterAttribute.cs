using System;
using System.Reflection;
using HarmonyLib;
using UnhollowerRuntimeLib;

namespace SuperNewRoles
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class Il2CppRegisterAttribute : Attribute
    {
		public Type[] Interfaces { get; private set; }

		public Il2CppRegisterAttribute()
		{
			this.Interfaces = Type.EmptyTypes;
		}

		public Il2CppRegisterAttribute(params Type[] interfaces)
		{
			this.Interfaces = interfaces;
		}

		public static void Registration(Assembly dll)
        {
            Logger.Info(
				"---------- Il2CppRegister: Start Registration ----------");

			foreach (Type type in dll.GetTypes())
			{
				Il2CppRegisterAttribute attribute = 
					CustomAttributeExtensions.GetCustomAttribute<Il2CppRegisterAttribute>(type);
				if (attribute != null)
				{
					registrationForTarget(type, attribute.Interfaces);
				}
			}

            Logger.Info(
				"---------- Il2CppRegister: Complete Registration ----------");

		}

		private static void registrationForTarget(
			Type targetType, Type[] interfaces)
        {
			Type targetBase = targetType.BaseType;

			Il2CppRegisterAttribute baseAttribute = 
				(targetType == null) ? 
				null :
				CustomAttributeExtensions.GetCustomAttribute<Il2CppRegisterAttribute>(targetBase);
			
			if (baseAttribute != null)
            {
				registrationForTarget(targetBase, baseAttribute.Interfaces);
            }

            Logger.Info(
				$"Il2CppRegister:  Register {targetType}");

			if (ClassInjector.IsTypeRegisteredInIl2Cpp(targetType)) { return; }

			try
            {
				ClassInjector.RegisterTypeInIl2Cpp(
					targetType, new RegisterTypeOptions
					{
						Interfaces = interfaces,
						LogSuccess = true
					}
				);
			}
			catch (Exception e)
            {

				string excStr = GeneralExtensions.FullDescription(targetType);
                Logger.Error(
					$"Registion Fail!!    Target:{excStr}   Il2CppError:{e}","");
            }
		}
	}
}
