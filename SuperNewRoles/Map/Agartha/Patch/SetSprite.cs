using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Map.Agartha.Patch
{
    class SetSpritePosition
    {

		[HarmonyPatch(typeof(UseButton), nameof(UseButton.SetTarget))]
		public static class UseButtonTargetPatch
		{
			[HarmonyPostfix]
			public static void Postfix(UseButton __instance, IUsable target)
			{
				if (target != null)
				{
					switch (target.UseIcon)
					{
						case ImageNames.AdminMapButton:
						case ImageNames.MIRAAdminButton:
							__instance.graphic.sprite = ImageManager.Button_Admin;
							break;
					}
				}
			}
		}
	}
}
