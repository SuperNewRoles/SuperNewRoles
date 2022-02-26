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



    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    class MeetingSheriff_Patch
    {
        public static bool IsMeetingSheriffKill(PlayerControl Target) {
            if (Target.Data.Role.IsImpostor) return true;
            if (RoleClass.MadMate.MadMatePlayer.IsCheckListPlayerControl(Target) && RoleClass.MeetingSheriff.MadMateKill) return true;
            return false;
        }
        static void MeetingSheriffOnClick(int Index, MeetingHud __instance)
        {
                var Target = ModHelpers.playerById((byte)__instance.playerStates[Index].TargetPlayerId);
                var misfire = !IsMeetingSheriffKill(Target);
                var TargetID = Target.PlayerId;
                var LocalID = PlayerControl.LocalPlayer.PlayerId;

                CustomRPC.RPCProcedure.MeetingSheriffKill(LocalID, TargetID, misfire);

                MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.MeetingSheriffKill, Hazel.SendOption.Reliable, -1);
                killWriter.Write(LocalID);
                killWriter.Write(TargetID);
                killWriter.Write(misfire);
                AmongUsClient.Instance.FinishRpcImmediately(killWriter);
            RoleClass.MeetingSheriff.KillMaxCount--;
            if (RoleClass.MeetingSheriff.KillMaxCount <= 0 || !RoleClass.MeetingSheriff.OneMeetingMultiKill)
            {
                __instance.playerStates.ToList().ForEach(x => { if (x.transform.FindChild("ShootButton") != null) UnityEngine.Object.Destroy(x.transform.FindChild("SoothSayerButton").gameObject); });
            }

        }
        static void Event(MeetingHud __instance)
        {
            if (PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.MeetingSheriff) && PlayerControl.LocalPlayer.isAlive() && RoleClass.MeetingSheriff.KillMaxCount >= 1)
            {
                for (int i = 0; i < __instance.playerStates.Length; i++)
                {
                    PlayerVoteArea playerVoteArea = __instance.playerStates[i];

                    GameObject template = playerVoteArea.Buttons.transform.Find("CancelButton").gameObject;
                    GameObject targetBox = UnityEngine.Object.Instantiate(template, playerVoteArea.transform);
                    targetBox.name = "ShootButton";
                    targetBox.transform.localPosition = new Vector3(1f, 0.03f, -1f);
                    SpriteRenderer renderer = targetBox.GetComponent<SpriteRenderer>();
                    renderer.sprite = RoleClass.MeetingSheriff.getButtonSprite();
                    PassiveButton button = targetBox.GetComponent<PassiveButton>();
                    button.OnClick.RemoveAllListeners();
                    int copiedIndex = i;
                    button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => MeetingSheriffOnClick(copiedIndex,__instance)));
                }
            }
        }
        
        static void Postfix(MeetingHud __instance)
        {
                Event(__instance);
         }
        
    }
}
