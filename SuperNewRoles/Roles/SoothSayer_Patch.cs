using HarmonyLib;
using Hazel;
using SuperNewRoles.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace SuperNewRoles.Roles
{


    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.UpdateButtons))]
    class SoothSayer_updatepatch
    {
        static void Postfix(MeetingHud __instance)
        {
            if (!PlayerControl.LocalPlayer.isAlive())
            {
                __instance.playerStates.ToList().ForEach(x => { if (x.transform.FindChild("SoothSayerButton") != null) UnityEngine.Object.Destroy(x.transform.FindChild("SoothSayerButton").gameObject); });
            }
        }
    }
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    class SoothSayer_Patch
    {
        private static string namedate;
        static void SoothSayerOnClick(int Index, MeetingHud __instance)
        {
            var Target = ModHelpers.playerById((byte)__instance.playerStates[Index].TargetPlayerId);
            var introdate = Target.getRole();
            if (RoleClass.SoothSayer.DisplayMode)
            {
                if (Target.isImpostor())
                {
                    namedate = "Impostor";
                }
                if (Target.isHauntedWolf())
                {
                    namedate = "Impostor";
                }
                else if (Target.isNeutral())
                {
                    namedate = "Neutral";
                }
                else if (Target.isCrew())
                {
                    namedate = "CrewMate";
                }
            }
            else
            {
                namedate = Intro.IntroDate.GetIntroDate(introdate, Target).NameKey;
            }
            var name = ModTranslation.getString(namedate + "Name");
            FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, string.Format(ModTranslation.getString("SoothSayerGetChat"), Target.nameText.text, name));

            RoleClass.SoothSayer.Count--;
            if (RoleClass.SoothSayer.Count <= 0)
            {
                __instance.playerStates.ToList().ForEach(x => { if (x.transform.FindChild("SoothSayerButton") != null) UnityEngine.Object.Destroy(x.transform.FindChild("SoothSayerButton").gameObject); });
            }
        }
        static void Event(MeetingHud __instance)
        {
            if (PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.SoothSayer) && PlayerControl.LocalPlayer.isAlive() && RoleClass.SoothSayer.Count >= 1)
            {
                for (int i = 0; i < __instance.playerStates.Length; i++)
                {
                    PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                    var player = ModHelpers.playerById((byte)__instance.playerStates[i].TargetPlayerId);
                    if (player.isAlive() && player.PlayerId != CachedPlayer.LocalPlayer.PlayerId)
                    {
                        GameObject template = playerVoteArea.Buttons.transform.Find("CancelButton").gameObject;
                        GameObject targetBox = UnityEngine.Object.Instantiate(template, playerVoteArea.transform);

                        targetBox.name = "SoothSayerButton";
                        targetBox.transform.localPosition = new Vector3(1f, 0.03f, -1f);
                        SpriteRenderer renderer = targetBox.GetComponent<SpriteRenderer>();
                        renderer.sprite = RoleClass.SoothSayer.getButtonSprite();
                        PassiveButton button = targetBox.GetComponent<PassiveButton>();
                        button.OnClick.RemoveAllListeners();
                        int copiedIndex = i;
                        button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => SoothSayerOnClick(copiedIndex, __instance)));
                    }
                }
            }
        }

        static void SpiritOnClick(int Index, MeetingHud __instance)
        {
            var Target = ModHelpers.playerById((byte)__instance.playerStates[Index].TargetPlayerId);
            var introdate = Target.getRole();
            namedate = Intro.IntroDate.GetIntroDate(introdate, Target).NameKey;
            if (RoleClass.SpiritMedium.DisplayMode)
            {
                if (Target.isImpostor())
                {
                    namedate = "Impostor";
                }
                if (Target.isHauntedWolf())
                {
                    namedate = "Impostor";
                }
                else if (Target.isNeutral())
                {
                    namedate = "Neutral";
                }
                else if (Target.isCrew())
                {
                    namedate = "CrewMate";
                }
            }
            else
            {
                namedate = Intro.IntroDate.GetIntroDate(introdate, Target).NameKey;
            }
            var name = ModTranslation.getString(namedate + "Name");
            FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, string.Format(ModTranslation.getString("SoothSayerGetChat"), Target.nameText.text, name));
            RoleClass.SpiritMedium.MaxCount--;
            if (RoleClass.SpiritMedium.MaxCount <= 0)
            {
                __instance.playerStates.ToList().ForEach(x => { if (x.transform.FindChild("SoothSayerButton") != null) UnityEngine.Object.Destroy(x.transform.FindChild("SoothSayerButton").gameObject); });
            }
        }
        static void spiritEvent(MeetingHud __instance)
        {
            if (PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.SpiritMedium) && PlayerControl.LocalPlayer.isAlive() && RoleClass.SpiritMedium.MaxCount >= 1)
            {
                for (int i = 0; i < __instance.playerStates.Length; i++)
                {
                    PlayerVoteArea playerVoteArea = __instance.playerStates[i];

                    var player = ModHelpers.playerById((byte)__instance.playerStates[i].TargetPlayerId);
                    if (!player.Data.Disconnected && player.isDead() && player.PlayerId != CachedPlayer.LocalPlayer.PlayerId)
                    {
                        GameObject template = playerVoteArea.Buttons.transform.Find("CancelButton").gameObject;
                        GameObject targetBox = UnityEngine.Object.Instantiate(template, playerVoteArea.transform);

                        targetBox.name = "SoothSayerButton";
                        targetBox.transform.localPosition = new Vector3(1f, 0.03f, -1f);
                        SpriteRenderer renderer = targetBox.GetComponent<SpriteRenderer>();
                        renderer.sprite = RoleClass.SoothSayer.getButtonSprite();
                        PassiveButton button = targetBox.GetComponent<PassiveButton>();
                        button.OnClick.RemoveAllListeners();
                        int copiedIndex = i;
                        button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => SpiritOnClick(copiedIndex, __instance)));
                    }
                }
            }
        }

        static void Postfix(MeetingHud __instance)
        {
            Event(__instance);
            spiritEvent(__instance);
        }

    }
}