using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Map.Agartha.Patch.Task
{
    class FixWiring
    {
        [HarmonyPatch(typeof(Console), nameof(Console.Use))]
        class AutoTaskConsolePatch
        {
            public static void Postfix(AutoTaskConsole __instance)
            {
                //GameObject.Find("WireMinigame(Clone)").transform.FindChild("Background").GetComponent<SpriteRenderer>().sprite = ImageManager.Task_FixWiring_BackGround;
                //GameObject.Find("WireMinigame(Clone)").transform.position += new Vector3(0f,0.25f,0f);
            }
        }
    }
}
