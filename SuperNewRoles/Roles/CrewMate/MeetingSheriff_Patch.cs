using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hazel;
using SuperNewRoles.CustomObject;
using SuperNewRoles.Mode;
using SuperNewRoles.Patches;
using UnityEngine;

namespace SuperNewRoles.Roles;

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
class MeetingUpdatePatch
{
    public static void Postfix(MeetingHud __instance)
    {
        if (RoleClass.Assassin.TriggerPlayer != null)
        {
            __instance.TitleText.text = ModTranslation.GetString("MarlinWhois");
        }
        if (!IsFlag) return;
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            MeetingSheriff_Patch.Rights();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            MeetingSheriff_Patch.Lefts();
        }
        Meetingsheriff_updatepatch.Change();
    }
    public static PassiveButton RightButton = null;
    public static PassiveButton LeftButton = null;
    public static bool IsFlag;
    public static bool IsSHRFlag;
    private static Sprite m_Meeting_AreaTabChange;
    public static Sprite Meeting_AreaTabChange
    {
        get
        {
            if (m_Meeting_AreaTabChange == null)
            {
                m_Meeting_AreaTabChange = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Meeting_AreaTabChange.png", 110f);
            }
            return m_Meeting_AreaTabChange;
        }
    }
}
[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.UpdateButtons))]
class Meetingsheriff_updatepatch
{
    static void Postfix(MeetingHud __instance)
    {
        if (PlayerControl.LocalPlayer.IsRole(RoleId.MeetingSheriff) && PlayerControl.LocalPlayer.IsDead())
        {
            __instance.playerStates.ToList().ForEach(x => { if (x.transform.FindChild("ShootButton") != null) Object.Destroy(x.transform.FindChild("ShootButton").gameObject); });
        }
    }
    public static void Change()
    {
        if (!(index < (CachedPlayer.AllPlayers.Count / 15) + 1))
            MeetingSheriff_Patch.Right.SetActive(false);
        else
            MeetingSheriff_Patch.Right.SetActive(true);
        if (index <= 1)
            MeetingSheriff_Patch.Left.SetActive(false);
        else
            MeetingSheriff_Patch.Left.SetActive(true);
        int i = 0;
        foreach (PlayerVoteArea area in PlayerVoteAreas)
        {
            try
            {
                area.transform.localPosition = !(index * 15 < i && i >= 15 * (index - 1)) ? Positions[i - ((index - 1) * 15)] : new Vector3(100, 100, 100);
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
    static void MeetingSheriffOnClick(int Index, MeetingHud __instance)
    {
        var Target = ModHelpers.PlayerById(__instance.playerStates[Index].TargetPlayerId);
        var misfire = !Sheriff.IsSheriffRolesKill(CachedPlayer.LocalPlayer, Target);
        var alwaysKill = !Sheriff.IsSheriffRolesKill(CachedPlayer.LocalPlayer, Target) && CustomOptionHolder.MeetingSheriffAlwaysKills.GetBool();
        var TargetID = Target.PlayerId;
        var LocalID = CachedPlayer.LocalPlayer.PlayerId;

        RPCProcedure.MeetingSheriffKill(LocalID, TargetID, misfire, alwaysKill);

        MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.MeetingSheriffKill, SendOption.Reliable, -1);
        killWriter.Write(LocalID);
        killWriter.Write(TargetID);
        killWriter.Write(misfire);
        killWriter.Write(alwaysKill);
        AmongUsClient.Instance.FinishRpcImmediately(killWriter);
        FinalStatusClass.RpcSetFinalStatus(misfire ? CachedPlayer.LocalPlayer : Target, misfire ? FinalStatus.MeetingSheriffMisFire : (Target.IsRole(RoleId.HauntedWolf) ? FinalStatus.MeetingSheriffHauntedWolfKill : FinalStatus.MeetingSheriffKill));
        if (alwaysKill) FinalStatusClass.RpcSetFinalStatus(Target, FinalStatus.SheriffInvolvedOutburst);
        RoleClass.MeetingSheriff.KillMaxCount--;
        if (RoleClass.MeetingSheriff.KillMaxCount <= 0 || !RoleClass.MeetingSheriff.OneMeetingMultiKill || misfire)
        {
            __instance.playerStates.ToList().ForEach(x => { if (x.transform.FindChild("ShootButton") != null) Object.Destroy(x.transform.FindChild("ShootButton").gameObject); });
        }

    }
    static void Event(MeetingHud __instance)
    {
        if (PlayerControl.LocalPlayer.IsRole(RoleId.MeetingSheriff) && PlayerControl.LocalPlayer.IsAlive() && RoleClass.MeetingSheriff.KillMaxCount >= 1)
        {
            for (int i = 0; i < __instance.playerStates.Length; i++)
            {
                PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                var player = ModHelpers.PlayerById(__instance.playerStates[i].TargetPlayerId);
                if (player.IsAlive() && player.PlayerId != CachedPlayer.LocalPlayer.PlayerId)
                {
                    GameObject template = playerVoteArea.Buttons.transform.Find("CancelButton").gameObject;
                    GameObject targetBox = Object.Instantiate(template, playerVoteArea.transform);
                    targetBox.name = "ShootButton";
                    targetBox.transform.localPosition = new Vector3(1f, 0.03f, -1f);
                    SpriteRenderer renderer = targetBox.GetComponent<SpriteRenderer>();
                    renderer.sprite = RoleClass.MeetingSheriff.GetButtonSprite();
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
        if (AmongUsClient.Instance.AmHost)
        {
            PlayerAnimation.PlayerAnimations.All(x => { x.RpcAnimation(RpcAnimationType.Stop); return false; });
        }
        LadderDead.Reset();
        if (ModeHandler.IsMode(ModeId.SuperHostRoles))
        {
            Mode.SuperHostRoles.MorePatch.StartMeeting();
        }

        MeetingUpdatePatch.IsFlag = false;
        MeetingUpdatePatch.IsSHRFlag = false;
        if (!ModeHandler.IsMode(ModeId.SuperHostRoles) && PlayerControl.AllPlayerControls.Count > 15)
        {
            MeetingUpdatePatch.IsFlag = true;
            Meetingsheriff_updatepatch.PlayerVoteAreas = new List<PlayerVoteArea>();
            List<PlayerVoteArea> deadareas = new();
            foreach (PlayerVoteArea area in __instance.playerStates)
            {
                if (ModHelpers.PlayerById(area.TargetPlayerId).IsAlive())
                {
                    Meetingsheriff_updatepatch.PlayerVoteAreas.Add(area);
                }
                else
                {
                    deadareas.Add(area);
                }
            }
            foreach (PlayerVoteArea area in deadareas)
            {
                Meetingsheriff_updatepatch.PlayerVoteAreas.Add(area);
            }
            Meetingsheriff_updatepatch.index = 1;
            CreateAreaButton(__instance);
        }
        if (ModeHandler.IsMode(ModeId.Default) && RoleClass.GM.gm != null)
        {
            List<PlayerVoteArea> newareas = new();
            List<PlayerVoteArea> deadareas = new();
            foreach (PlayerVoteArea area in __instance.playerStates)
            {
                if (!ModHelpers.PlayerById(area.TargetPlayerId).IsRole(RoleId.GM))
                {
                    if (ModHelpers.PlayerById(area.TargetPlayerId).IsAlive())
                        newareas.Add(area);
                    else
                        deadareas.Add(area);
                }
                else
                    area.gameObject.SetActive(false);
            }
            foreach (PlayerVoteArea area in deadareas)
            {
                newareas.Add(area);
            }
            int i = 0;
            foreach (PlayerVoteArea area in newareas)
            {
                area.transform.localPosition = Meetingsheriff_updatepatch.Positions[i];
                i++;
            }
            __instance.playerStates = newareas.ToArray();
        }
        if (ModeHandler.IsMode(ModeId.SuperHostRoles) && BotManager.AllBots.Count > 0)
        {
            List<PlayerVoteArea> newareas = new();
            List<PlayerVoteArea> deadareas = new();
            foreach (PlayerVoteArea area in __instance.playerStates)
            {
                if (!ModHelpers.PlayerById(area.TargetPlayerId).IsBot())
                {
                    if (ModHelpers.PlayerById(area.TargetPlayerId).IsAlive())
                        newareas.Add(area);
                    else
                        deadareas.Add(area);
                }
                else
                    area.gameObject.SetActive(false);
            }
            foreach (PlayerVoteArea area in deadareas)
            {
                newareas.Add(area);
            }
            int i = 0;
            foreach (PlayerVoteArea area in newareas)
            {
                area.transform.localPosition = Meetingsheriff_updatepatch.Positions[i];
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
        GameObject template = __instance.SkipVoteButton.gameObject;
        GameObject targetBox = Object.Instantiate(template, __instance.transform);
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
        button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => Rights()));
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
        buttonl.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => Lefts()));
        buttonl.OnMouseOver.AddListener((UnityEngine.Events.UnityAction)(() => rendererl.color = Color.green));
        buttonl.OnMouseOut.AddListener((UnityEngine.Events.UnityAction)(() => rendererl.color = Color.white));
    }
    public static void Rights()
    {
        if (Meetingsheriff_updatepatch.index < (CachedPlayer.AllPlayers.Count / 15) + 1)
        {
            Meetingsheriff_updatepatch.index++;
        }
    }
    public static void Lefts()
    {
        if (Meetingsheriff_updatepatch.index > 1)
        {
            Meetingsheriff_updatepatch.index--;
        }
    }
}