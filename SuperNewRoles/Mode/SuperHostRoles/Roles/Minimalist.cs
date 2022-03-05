using HarmonyLib;
using SuperNewRoles.Roles;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Mode.SuperHostRoles.Roles
{
    class Minimalist
    {
		[HarmonyPatch(typeof(HudManager), nameof(HudManager.SetHudActive))]
		class SetHudActivePatch
		{
			public static void Postfix(HudManager __instance, [HarmonyArgument(0)] bool isActive)
			{
				if (PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.Minimalist))
				{
					__instance.ReportButton.ToggleVisible(visible: RoleClass.Minimalist.UseReport);
					__instance.SabotageButton.ToggleVisible(visible: RoleClass.Minimalist.UseSabo);
					__instance.ImpostorVentButton.ToggleVisible(visible: RoleClass.Minimalist.UseVent);
				}
			}
		}

	}
}
