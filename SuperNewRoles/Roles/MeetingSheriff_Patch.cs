using HarmonyLib;
using Hazel;
using SuperNewRoles.CustomOption;
using SuperNewRoles.Intro;
using SuperNewRoles.Mode;
using SuperNewRoles.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace SuperNewRoles.Roles
{
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
    class MeetingUpdatePatch
    {
        public static void Postfix(MeetingHud __instance)
        {
            if (RoleClass.Assassin.TriggerPlayer != null)
            {
                __instance.TitleText.text = ModTranslation.getString("MarineWhois");
            }
            if (!IsFlag) return;
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                MeetingSheriff_Patch.right();
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                MeetingSheriff_Patch.left();
            }
            meetingsheriff_updatepatch.Change(__instance, false);
        }
        public static PassiveButton RightButton;
        public static PassiveButton LeftButton;
        public static bool IsFlag;
        public static bool IsSHRFlag;
        private static Sprite m_Meeting_AreaTabChange;
        public static Sprite Meeting_AreaTabChange
        {
            get
            {
                if (m_Meeting_AreaTabChange == null)
                {
                    m_Meeting_AreaTabChange = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.Meeting_AreaTabChange.png", 110f);
                }
                return m_Meeting_AreaTabChange;
            }
        }
    }
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.UpdateButtons))]
    class meetingsheriff_updatepatch
    {
        static void Postfix(MeetingHud __instance)
        {
            if (PlayerControl.LocalPlayer.isDead())
            {
                __instance.playerStates.ToList().ForEach(x => { if (x.transform.FindChild("ShootButton") != null) UnityEngine.Object.Destroy(x.transform.FindChild("ShootButton").gameObject); });
            }
        }
        public static void Change(MeetingHud __instance, bool right)
        {
            if (!(meetingsheriff_updatepatch.index < (CachedPlayer.AllPlayers.Count / 15) + 1))
            {
                MeetingSheriff_Patch.Right.SetActive(false);
            }
            else
            {
                MeetingSheriff_Patch.Right.SetActive(true);
            }
            if (index <= 1)
            {
                MeetingSheriff_Patch.Left.SetActive(false);
            }
            else
            {
                MeetingSheriff_Patch.Left.SetActive(true);
            }
            int i = 0;
            foreach (PlayerVoteArea area in PlayerVoteAreas)
            {
                try
                {
                    if (!(index * 15 < i && i >= 15 * (index - 1)))
                    {
                        area.transform.localPosition = Positions[i - ((index - 1) * 15)];
                    }
                    else
                    {
                        area.transform.localPosition = new Vector3(100, 100, 100);
                    }
                }
                catch
                {
                    area.transform.localPosition = new Vector3(100, 100, 100);
                }
                i++;
            }
        }
        public static int index;
        public static List<PlayerVoteArea> PlayerVoteAreas;
        public static Vector3[] Positions = new Vector3[] {
            new Vector3(-3.1f, 1.5f, -0.9f), new Vector3(-0.2f, 1.5f, -0.9f), new Vector3(2.7f, 1.5f, -0.9f), new Vector3(-3.1f, 0.74f, -0.91f), new Vector3(-0.2f, 0.74f, -0.91f),
            new Vector3(2.7f, 0.74f, -0.91f), new Vector3(-3.1f, -0.02f, -0.92f), new Vector3(-0.2f, -0.02f, -0.92f), new Vector3(2.7f, -0.02f, -0.92f), new Vector3(-3.1f, -0.78f, -0.93f),
            new Vector3(-0.2f, -0.78f, -0.93f), new Vector3(2.7f, -0.78f, -0.93f), new Vector3(-3.1f, -1.54f, -0.94f), new Vector3(-0.2f, -1.54f, -0.94f), new Vector3(2.7f, -1.54f, -0.94f)
        };
    }
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    class MeetingSheriff_Patch
    {
        public static bool IsMeetingSheriffKill(PlayerControl Target)
        {
            var roledata = CountChanger.GetRoleType(Target);
            if (roledata == TeamRoleType.Impostor) return true;
            if (RoleClass.MadMate.MadMatePlayer.IsCheckListPlayerControl(Target) && RoleClass.MeetingSheriff.MadRoleKill) return true;
            if (RoleClass.MadMate.MadMatePlayer.IsCheckListPlayerControl(Target) && RoleClass.MeetingSheriff.MadRoleKill) return true;
            if (RoleClass.MadJester.MadJesterPlayer.IsCheckListPlayerControl(Target) && RoleClass.MeetingSheriff.MadRoleKill) return true;
            if (Target.isNeutral() && RoleClass.MeetingSheriff.NeutralKill) return true;
            if (RoleClass.MadStuntMan.MadStuntManPlayer.IsCheckListPlayerControl(Target) && RoleClass.MeetingSheriff.MadRoleKill) return true;
            if (RoleClass.MadMayor.MadMayorPlayer.IsCheckListPlayerControl(Target) && RoleClass.MeetingSheriff.MadRoleKill) return true;
            if (RoleClass.MadHawk.MadHawkPlayer.IsCheckListPlayerControl(Target) && RoleClass.MeetingSheriff.MadRoleKill) return true;
            if (RoleClass.MadSeer.MadSeerPlayer.IsCheckListPlayerControl(Target) && RoleClass.MeetingSheriff.MadRoleKill) return true;
            if (RoleClass.JackalFriends.JackalFriendsPlayer.IsCheckListPlayerControl(Target) && RoleClass.MeetingSheriff.MadRoleKill) return true;
            if (RoleClass.SeerFriends.SeerFriendsPlayer.IsCheckListPlayerControl(Target) && RoleClass.MeetingSheriff.MadRoleKill) return true;
            if (RoleClass.HauntedWolf.HauntedWolfPlayer.IsCheckListPlayerControl(Target)) return true;
            return false;
        }
        static void MeetingSheriffOnClick(int Index, MeetingHud __instance)
        {
            var Target = ModHelpers.playerById((byte)__instance.playerStates[Index].TargetPlayerId);
            var misfire = !IsMeetingSheriffKill(Target);
            var TargetID = Target.PlayerId;
            var LocalID = CachedPlayer.LocalPlayer.PlayerId;

            CustomRPC.RPCProcedure.MeetingSheriffKill(LocalID, TargetID, misfire);

            MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.MeetingSheriffKill, Hazel.SendOption.Reliable, -1);
            killWriter.Write(LocalID);
            killWriter.Write(TargetID);
            killWriter.Write(misfire);
            AmongUsClient.Instance.FinishRpcImmediately(killWriter);
            RoleClass.MeetingSheriff.KillMaxCount--;
            if (RoleClass.MeetingSheriff.KillMaxCount <= 0 || !RoleClass.MeetingSheriff.OneMeetingMultiKill || misfire)
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
                    var player = ModHelpers.playerById((byte)__instance.playerStates[i].TargetPlayerId);
                    if (player.isAlive() && player.PlayerId != CachedPlayer.LocalPlayer.PlayerId)
                    {
                        GameObject template = playerVoteArea.Buttons.transform.Find("CancelButton").gameObject;
                        GameObject targetBox = UnityEngine.Object.Instantiate(template, playerVoteArea.transform);
                        targetBox.name = "ShootButton";
                        targetBox.transform.localPosition = new Vector3(1f, 0.03f, -1f);
                        SpriteRenderer renderer = targetBox.GetComponent<SpriteRenderer>();
                        renderer.sprite = RoleClass.MeetingSheriff.getButtonSprite();
                        PassiveButton button = targetBox.GetComponent<PassiveButton>();
                        button.OnClick.RemoveAllListeners();
                        int copiedIndex = i;
                        button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => MeetingSheriffOnClick(copiedIndex, __instance)));
                    }
                }
            }
        }

        static void Postfix(MeetingHud __instance)
        {
            RoleClass.IsMeeting = true;
            if (Mode.ModeHandler.isMode(Mode.ModeId.SuperHostRoles))
            {
                Mode.SuperHostRoles.MorePatch.StartMeeting(__instance);
            }

            MeetingUpdatePatch.IsFlag = false;
            MeetingUpdatePatch.IsSHRFlag = false;
            if (!ModeHandler.isMode(ModeId.SuperHostRoles) && CachedPlayer.AllPlayers.Count > 15)
            {
                MeetingUpdatePatch.IsFlag = true;
                meetingsheriff_updatepatch.PlayerVoteAreas = new List<PlayerVoteArea>();
                List<PlayerVoteArea> deadareas = new List<PlayerVoteArea>();
                foreach (PlayerVoteArea area in __instance.playerStates)
                {
                    if (ModHelpers.playerById(area.TargetPlayerId).isAlive())
                    {
                        meetingsheriff_updatepatch.PlayerVoteAreas.Add(area);
                    }
                    else
                    {
                        deadareas.Add(area);
                    }
                }
                foreach (PlayerVoteArea area in deadareas)
                {
                    meetingsheriff_updatepatch.PlayerVoteAreas.Add(area);
                }
                meetingsheriff_updatepatch.index = 1;
                CreateAreaButton(__instance);
            }
            if (ModeHandler.isMode(ModeId.SuperHostRoles) && BotManager.AllBots.Count != 0)
            {
                List<PlayerVoteArea> newareas = new List<PlayerVoteArea>();
                List<PlayerVoteArea> deadareas = new List<PlayerVoteArea>();
                foreach (PlayerVoteArea area in __instance.playerStates)
                {
                    if (ModHelpers.playerById(area.TargetPlayerId).IsPlayer())
                    {
                        if (ModHelpers.playerById(area.TargetPlayerId).isAlive())
                        {
                            newareas.Add(area);
                        }
                        else
                        {
                            deadareas.Add(area);
                        }
                    }
                    else
                    {
                        area.gameObject.SetActive(false);
                    }
                }
                foreach (PlayerVoteArea area in deadareas)
                {
                    newareas.Add(area);
                }
                int i = 0;
                foreach (PlayerVoteArea area in newareas)
                {
                    area.transform.localPosition = meetingsheriff_updatepatch.Positions[i];
                    i++;
                }
                __instance.playerStates = newareas.ToArray();
            }

            Event(__instance);
        }
        public static GameObject Right;
        public static GameObject Left;
        static void CreateAreaButton(MeetingHud __instance)
        {
            GameObject template = __instance.transform.FindChild("ButtonStuff").FindChild("button_skipVoting").gameObject;
            GameObject targetBox = UnityEngine.Object.Instantiate(template, __instance.transform);
            targetBox.name = "RightButton";
            targetBox.gameObject.SetActive(true);
            targetBox.transform.localPosition = new Vector3(4.8f, 0f, -3f);
            targetBox.transform.localScale = new Vector3(0.075f, 0.075f, 0.075f);
            Right = targetBox;
            GameObject.Destroy(targetBox.transform.FindChild("Text_TMP").gameObject);
            SpriteRenderer renderer = targetBox.GetComponent<SpriteRenderer>();
            renderer.sprite = MeetingUpdatePatch.Meeting_AreaTabChange;
            GameObject.Destroy(targetBox.GetComponent<BoxCollider2D>());
            PassiveButton button = targetBox.GetComponent<PassiveButton>();
            button.Colliders = new List<Collider2D>() { targetBox.AddComponent<PolygonCollider2D>() }.ToArray();
            button.OnClick.RemoveAllListeners();
            button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => right()));
            button.OnMouseOver.AddListener((UnityEngine.Events.UnityAction)(() => renderer.color = Color.green));
            button.OnMouseOut.AddListener((UnityEngine.Events.UnityAction)(() => renderer.color = Color.white));

            GameObject targetBoxl = UnityEngine.Object.Instantiate(template, __instance.transform);
            targetBoxl.name = "LeftButton";
            targetBoxl.gameObject.SetActive(true);
            targetBoxl.transform.localPosition = new Vector3(-4.75f, 0f, -3f);
            targetBoxl.transform.localScale = new Vector3(-0.075f, 0.075f, 0.075f);
            Left = targetBoxl;
            GameObject.Destroy(targetBoxl.transform.FindChild("Text_TMP").gameObject);
            SpriteRenderer rendererl = targetBoxl.GetComponent<SpriteRenderer>();
            rendererl.sprite = MeetingUpdatePatch.Meeting_AreaTabChange;
            GameObject.Destroy(targetBoxl.GetComponent<BoxCollider2D>());
            PassiveButton buttonl = targetBoxl.GetComponent<PassiveButton>();
            buttonl.Colliders = new List<Collider2D>() { targetBoxl.AddComponent<PolygonCollider2D>() }.ToArray();
            buttonl.OnClick.RemoveAllListeners();
            buttonl.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => left()));
            buttonl.OnMouseOver.AddListener((UnityEngine.Events.UnityAction)(() => rendererl.color = Color.green));
            buttonl.OnMouseOut.AddListener((UnityEngine.Events.UnityAction)(() => rendererl.color = Color.white));
        }
        public static void right()
        {
            if (meetingsheriff_updatepatch.index < (CachedPlayer.AllPlayers.Count / 15) + 1)
            {
                meetingsheriff_updatepatch.index++;
            }
        }
        public static void left()
        {
            if (meetingsheriff_updatepatch.index > 1)
            {
                meetingsheriff_updatepatch.index--;
            }
        }
    }
}