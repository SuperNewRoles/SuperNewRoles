using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using System;
using System.Linq;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnhollowerBaseLib;
using Hazel;
using Il2CppSystem.Collections.Generic;
using Il2CppSystem.Linq;
using Il2CppSystem;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.EndGame;
using SuperNewRoles.CustomOption;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles;

namespace SuperNewRoles
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public static class Zoom
    {
        public static void Postfix(HudManager __instance)
        {
            if (ModeHandler.isMode(ModeId.Default) && MapOptions.MapOption.MouseZoom && PlayerControl.LocalPlayer.Data.IsDead)
            {
                if (Input.GetAxis("Mouse ScrollWheel") > 0)
                {
                    if (Camera.main.orthographicSize > 1.0f)
                    {
                        Camera.main.orthographicSize /= 1.5f;
                        __instance.transform.localScale /= 1.5f;
                        __instance.UICamera.orthographicSize /= 1.5f;
                        HudManager.Instance.TaskStuff.SetActive(false);
                    }

                    else if (Camera.main.orthographicSize > 3.0f)
                    {
                        Camera.main.orthographicSize /= 1.5f;
                        __instance.transform.localScale /= 1.5f;
                        __instance.UICamera.orthographicSize /= 1.5f;
                    }
                }
                if (Input.GetAxis("Mouse ScrollWheel") < 0)
                {
                    if (PlayerControl.LocalPlayer.Data.IsDead)
                    {
                        if (Camera.main.orthographicSize < 18.0f)
                        {
                            Camera.main.orthographicSize *= 1.5f;
                            __instance.transform.localScale *= 1.5f;
                            __instance.UICamera.orthographicSize *= 1.5f;
                        }
                    }
                }
                if (ModeHandler.isMode(ModeId.Default))
                {
                    if (Camera.main.orthographicSize != 3.0f)
                    {
                        HudManager.Instance.TaskStuff.SetActive(false);
                        ModManager.Instance.ModStamp.gameObject.SetActive(false);
                        if (!PlayerControl.LocalPlayer.Data.IsDead) __instance.ShadowQuad.gameObject.SetActive(false);
                    }
                    else
                    {
                        HudManager.Instance.TaskStuff.SetActive(true);
                        ModManager.Instance.ModStamp.gameObject.SetActive(true);
                        if (!PlayerControl.LocalPlayer.Data.IsDead) __instance.ShadowQuad.gameObject.SetActive(true);
                    }
                }
                CreateFlag.NewFlag("Zoom");
            }
            else
            {
                CreateFlag.Run(() =>
                {
                    Camera.main.orthographicSize = 3.0f;
                    HudManager.Instance.UICamera.orthographicSize = 3.0f;
                    HudManager.Instance.transform.localScale = Vector3.one;
                    if (MeetingHud.Instance != null) MeetingHud.Instance.transform.localScale = Vector3.one;
                    HudManager.Instance.Chat.transform.localScale = Vector3.one;
                    if (!PlayerControl.LocalPlayer.Data.IsDead) __instance.ShadowQuad.gameObject.SetActive(true);
                }, "Zoom");
            }
        }
    }
}
