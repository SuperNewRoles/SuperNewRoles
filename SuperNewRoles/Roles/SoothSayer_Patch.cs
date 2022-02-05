using HarmonyLib;
using Hazel;
using SuperNewRoles.Patches;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace SuperNewRoles.Roles
{



    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    class SoothSayer_Patch
    {
        private static string namedate;
        private static List<PassiveButton> allbutton;
        static void SoothSayerOnClick(int Index, MeetingHud __instance)
        {
            {
                var Target = ModHelpers.playerById((byte)__instance.playerStates[Index].TargetPlayerId);
                var introdate = Target.getRole();
                if (introdate == CustomRPC.RoleId.DefaultRole)
                {
                    namedate = Intro.IntroDate.GetIntroDate(introdate).NameKey;
                } else
                {
                    if (Target.Data.Role.IsImpostor)
                    {
                        namedate = "Impostor";
                    } else
                    {
                        namedate = "CrewMate";
                    }
                }
                var name = ModTranslation.getString(namedate + "Name");
                DestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, string.Format(ModTranslation.getString("SoothSayerGetChat"),Target.nameText.text,name));
                
            }
        }
        static void Event(MeetingHud __instance)
        {
            if (Roles.RoleClass.SoothSayer.SoothSayerPlayer.IsCheckListPlayerControl(PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.isAlive())
            {
                for (int i = 0; i < __instance.playerStates.Length; i++)
                {
                    PlayerVoteArea playerVoteArea = __instance.playerStates[i];

                    if (ModHelpers.playerById((byte)__instance.playerStates[i].TargetPlayerId).isAlive())
                    {

                        GameObject template = playerVoteArea.Buttons.transform.Find("CancelButton").gameObject;
                        GameObject targetBox = UnityEngine.Object.Instantiate(template, playerVoteArea.transform);
                        
                        targetBox.name = "SoothSayerButton";
                        targetBox.transform.localPosition = new Vector3(-0.95f, 0.03f, -1f);
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

        static void Postfix(MeetingHud __instance)
        {
            Event(__instance);
        }

    }
}
