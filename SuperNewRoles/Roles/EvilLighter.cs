using System.Net;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using HarmonyLib;
using Hazel;
using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using UnityEngine;
using SuperNewRoles.Patches;
using System.Reflection;


namespace SuperNewRoles.Roles
{
	class EvilLighter
	{
		private static Sprite lightOutButtonSprite;
		public static Sprite getLightsOutButtonSprite()
			{
				if (lightOutButtonSprite) return lightOutButtonSprite;
				lightOutButtonSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.LightsOutButton.png", 115f);
				return lightOutButtonSprite;
			}

	
	}
	
}


