using HarmonyLib;
using Hazel;
using SuperNewRoles.Mode;
using SuperNewRoles.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using SuperNewRoles.CustomRPC;
using System.Runtime.InteropServices;

namespace SuperNewRoles.Roles
{
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
    class Guesser
    {
        /*public static void Postfix(MeetingHud __instance)
        {
            if (!IsFlag) return;
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                Guesser_Patch.right();
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                Guesser_Patch.left();
            }
            GuesserUpdatePatch.Change(__instance, false);
        }
        public static PassiveButton RightButton;
        public static PassiveButton LeftButton;*/
        //public static bool IsFlag;
        //public static bool IsSHRFlag;
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
    class GuesserUpdatePatch
    {
        public static int RoleSelectPage = 1;
        public static List<Transform> buttons = new List<Transform>();
        static void Postfix(MeetingHud __instance)
        {
            if (PlayerControl.LocalPlayer.isDead())
            {
                __instance.playerStates.ToList().ForEach(x => { if (x.transform.FindChild("ShootButton") != null) UnityEngine.Object.Destroy(x.transform.FindChild("ShootButton").gameObject); });
                if (__instance.transform.Find("GuesserShootHud").gameObject != null) UnityEngine.Object.Destroy(__instance.transform.Find("GuesserShootHud").gameObject);
                if (Guesser_Patch.Left != null) UnityEngine.Object.Destroy(Guesser_Patch.Left);
                if (Guesser_Patch.Right != null) UnityEngine.Object.Destroy(Guesser_Patch.Right);
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                MeetingSheriff_Patch.right();
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                MeetingSheriff_Patch.left();
            }
            // meetingsheriff_updatepatch.Change(__instance, false);
            int i = 1;
            foreach (Transform RoleSelect in buttons)
            {
                if (RoleSelect != null)
                {
                   // SuperNewRolesPlugin.Logger.LogInfo("ページ切り替え");
                    if (i <= RoleSelectPage * 45 && i >= 45 * (RoleSelectPage - 1))
                    {
                        RoleSelect.gameObject.SetActive(true);
                       // SuperNewRolesPlugin.Logger.LogInfo("ページ切り替え(true)");
                    }
                    else
                    {
                        RoleSelect.gameObject.SetActive(false);
                       // SuperNewRolesPlugin.Logger.LogInfo("ページ切り替え(false)");
                    }
                    i++;
                }
            }
        }
        /*public static void Change(MeetingHud __instance, bool right)
        {
            int i = 0;
            foreach (Transform RoleSelect in buttons)
            {
                if (i >= /*RoleSelectPage*///45)
                /*{
                    RoleSelect.gameObject.SetActive(true);
                }
                else
                {
                    RoleSelect.gameObject.SetActive(false);
                }
                i++;*/
                /*try
                {
                    if (!(RoleSelectPage * 45 < i && i >= 45 * (RoleSelectPage - 1)))
                    {
                        RoleSelect.gameObject.SetActive(true);
                    }
                    else
                    {
                        RoleSelect.gameObject.SetActive(false);
                    }
                }
                catch
                {
                    RoleSelect.gameObject.SetActive(false);
                }
                i++;*/
           /* }
        }*/
        public static int index;
        //public static List<GameObject> RoleSelectAreas;
        /*public static Vector3[] Positions = new Vector3[] {
            new Vector3(-3.1f, 1.5f, -0.9f), new Vector3(-0.2f, 1.5f, -0.9f), new Vector3(2.7f, 1.5f, -0.9f), new Vector3(-3.1f, 0.74f, -0.91f), new Vector3(-0.2f, 0.74f, -0.91f),
            new Vector3(2.7f, 0.74f, -0.91f), new Vector3(-3.1f, -0.02f, -0.92f), new Vector3(-0.2f, -0.02f, -0.92f), new Vector3(2.7f, -0.02f, -0.92f), new Vector3(-3.1f, -0.78f, -0.93f),
            new Vector3(-0.2f, -0.78f, -0.93f), new Vector3(2.7f, -0.78f, -0.93f), new Vector3(-3.1f, -1.54f, -0.94f), new Vector3(-0.2f, -1.54f, -0.94f), new Vector3(2.7f, -1.54f, -0.94f)
        };*/
    }
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    class Guesser_Patch
    {
        public static bool IsGuesserKill(PlayerControl Target,RoleId Role)
        {
            SuperNewRolesPlugin.Logger.LogInfo(Target.getRole() + $"{Role}");
            if ((int)Target.getRole() == (int)Role) return true;
            return false;
        }
        static void GuesserOnClick(int Index,MeetingHud __instance)
        {
            if (GameObject.Find("GuesserShootHud") != null) return;
            int p = 0;
            var buttonTemplate = __instance.playerStates[0].transform.FindChild("votePlayerBase");
            var maskTemplate = __instance.playerStates[0].transform.FindChild("MaskArea");
            var smallButtonTemplate = __instance.playerStates[0].Buttons.transform.Find("CancelButton");
            var textTemplate = __instance.playerStates[0].NameText;
            RoleId Role = 0;
            CreateAreaButton(__instance);
            Transform template = MeetingHud.Instance.transform.FindChild("PhoneUI");
            Transform GuesserShootHud = UnityEngine.Object.Instantiate(template, __instance.transform);
            GuesserShootHud.localScale = new Vector3(0.975f, 0.975f, 10);
            GuesserShootHud.localPosition = new Vector3(0, 0, -5f);
            GuesserShootHud.gameObject.layer = 5;
            GuesserShootHud.name = "GuesserShootHud";
            GuesserUpdatePatch.buttons = new List<Transform>();
            Transform exitButtonParent = (new GameObject()).transform;
            exitButtonParent.SetParent(GuesserShootHud);
            Transform exitButton = UnityEngine.Object.Instantiate(buttonTemplate.transform, exitButtonParent);
            Transform exitButtonMask = UnityEngine.Object.Instantiate(maskTemplate, exitButtonParent);
            exitButton.gameObject.GetComponent<SpriteRenderer>().sprite = smallButtonTemplate.GetComponent<SpriteRenderer>().sprite;
            exitButtonParent.transform.localPosition = new Vector3(3.88f, 2.1f, -15);
            exitButtonParent.transform.localScale = new Vector3(0.5f, 0.5f, 1);
            exitButton.GetComponent<PassiveButton>().OnClick.RemoveAllListeners();
            exitButton.GetComponent<PassiveButton>().OnClick.AddListener((System.Action)(() =>
            {
                __instance.playerStates.ToList().ForEach(x => x.gameObject.SetActive(true));
                UnityEngine.Object.Destroy(GuesserShootHud.gameObject);
                UnityEngine.Object.Destroy(Guesser_Patch.Left);
                UnityEngine.Object.Destroy(Guesser_Patch.Right);
            }));
            int Count = 0;
            Count = System.Enum.GetValues(typeof(RoleId)).Length - 1;
            for (int i = 1; i <= Count; i++)
            {
                Role++;
                if (!(Role == RoleId.AllCleaner || Role == RoleId.Hunter || Role == RoleId.Speeder || Role == RoleId.Freezer || Role == RoleId.Tasker || Role == RoleId.EvilLighter || Role == RoleId.Sealdor || Role == RoleId.Vulture || Role == RoleId.NiceGambler || Role == RoleId.Neta))
                {
                    if (p == 45) p = 0;
                    p++;
                    RoleId buttonRole;
                    buttonRole = Role;
                    Transform buttonParent = (new GameObject()).transform;
                    buttonParent.SetParent(GuesserShootHud);
                    Transform button = UnityEngine.Object.Instantiate(buttonTemplate, buttonParent);
                    Transform buttonMask = UnityEngine.Object.Instantiate(maskTemplate, buttonParent);
                    TMPro.TextMeshPro label = UnityEngine.Object.Instantiate(textTemplate, button);
                    button.GetComponent<SpriteRenderer>().sprite = DestroyableSingleton<HatManager>.Instance.GetNamePlateById("nameplate_NoPlate")?.viewData?.viewData?.Image;
                    GuesserUpdatePatch.buttons.Add(button);
                    int row = p / 5, col = p % 5;
                    buttonParent.localPosition = new Vector3(-3.47f + 1.75f * col, 1.5f - 0.45f * row, -5);
                    buttonParent.localScale = new Vector3(0.55f, 0.55f, 1f);
                    label.alignment = TMPro.TextAlignmentOptions.Center;
                    label.transform.localPosition = new Vector3(0, 0, label.transform.localPosition.z);
                    label.transform.localScale *= 1.7f;
                    label.text = ModTranslation.getString(Role.ToString() + "Name");
                    button.GetComponent<PassiveButton>().OnClick.RemoveAllListeners();
                    button.GetComponent<PassiveButton>().OnClick.AddListener((System.Action)(() =>
                    {
                        var Target = ModHelpers.playerById((byte)__instance.playerStates[Index].TargetPlayerId);
                        var misfire = !IsGuesserKill(Target, buttonRole);
                        var TargetID = Target.PlayerId;
                        var LocalID = PlayerControl.LocalPlayer.PlayerId;

                        CustomRPC.RPCProcedure.GuesserKill(LocalID, TargetID, misfire);

                        MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.GuesserKill, Hazel.SendOption.Reliable, -1);
                        killWriter.Write(LocalID);
                        killWriter.Write(TargetID);
                        killWriter.Write(misfire);
                        AmongUsClient.Instance.FinishRpcImmediately(killWriter);
                    }));
                }
            }

        }
        static void Event(MeetingHud __instance)
        {
            if (PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.EvilGuesser) || PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.Guesser) && PlayerControl.LocalPlayer.isAlive() && RoleClass.Guesser.KillMaxCount >= 1)
            {
                for (int i = 0; i < __instance.playerStates.Length; i++)
                {
                    PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                    var player = ModHelpers.playerById((byte)__instance.playerStates[i].TargetPlayerId);
                    if (player.isAlive() && player.PlayerId != PlayerControl.LocalPlayer.PlayerId)
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
                        button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => GuesserOnClick(copiedIndex, __instance)));
                    }
                }
            }
        }

        static void Postfix(MeetingHud __instance)
        {
            /*RoleClass.IsMeeting = true;
            if (Mode.ModeHandler.isMode(Mode.ModeId.SuperHostRoles))
            {
                Mode.SuperHostRoles.MorePatch.StartMeeting(__instance);
            }

            MeetingUpdatePatch.IsFlag = false;
            MeetingUpdatePatch.IsSHRFlag = false;
            if (!ModeHandler.isMode(ModeId.SuperHostRoles) && PlayerControl.AllPlayerControls.Count > 15)
            {
                MeetingUpdatePatch.IsFlag = true;
                GuesserUpdatePatch.RoleSelectAreas = new List<GameObject>();
                List<GameObject> deadareas = new List<GameObject>();
                /*foreach (Transform RoleSelectbutton in deadareas)
                {
                    GuesserUpdatePatch.PlayerVoteAreas.Add(area);
                }
                GuesserUpdatePatch.index = 1;
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
                __instance.playerStates = newareas.ToArray();
            }*/

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
            targetBoxl.transform.localScale = new Vector3(-0.05f, 0.05f, 0.075f);
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
            //if (GuesserUpdatePatch.index < (PlayerControl.AllPlayerControls.Count / 15) + 1)
            //{
            GuesserUpdatePatch.RoleSelectPage++;
            //}
        }
        public static void left()
        {
            //if (GuesserUpdatePatch.index > 1)
            //{
            GuesserUpdatePatch.RoleSelectPage--;
            //}
        }
    }
}
