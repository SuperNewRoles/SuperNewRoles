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
class Meetingsheriff_updatepatch
{
    internal static void UpdateButtonsPostfix(MeetingHud __instance)
    {
        if (PlayerControl.LocalPlayer.IsDead())
        {
            __instance.playerStates.ForEach(x => { if (x.transform.FindChild("ShootButton") != null) Object.Destroy(x.transform.FindChild("ShootButton").gameObject); });
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
        (var killResult, var suicideResult) = Sheriff.SheriffKillResult(CachedPlayer.LocalPlayer, Target);

        var TargetID = Target.PlayerId;
        var LocalID = CachedPlayer.LocalPlayer.PlayerId;

        RPCProcedure.MeetingSheriffKill(LocalID, TargetID, killResult.Item1, suicideResult.Item1);
        MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.MeetingSheriffKill, SendOption.Reliable, -1);
        killWriter.Write(LocalID);
        killWriter.Write(TargetID);
        killWriter.Write(killResult.Item1);
        killWriter.Write(suicideResult.Item1);
        AmongUsClient.Instance.FinishRpcImmediately(killWriter);

        if (killResult.Item1) FinalStatusClass.RpcSetFinalStatus(Target, killResult.Item2);
        if (suicideResult.Item1) FinalStatusClass.RpcSetFinalStatus(CachedPlayer.LocalPlayer, suicideResult.Item2);

        RoleClass.MeetingSheriff.KillMaxCount--;
        if (RoleClass.MeetingSheriff.KillMaxCount <= 0 || !RoleClass.MeetingSheriff.OneMeetingMultiKill || suicideResult.Item1)
        {
            __instance.playerStates.ForEach(x => { if (x.transform.FindChild("ShootButton") != null) Object.Destroy(x.transform.FindChild("ShootButton").gameObject); });
        }
    }
    static void Event(MeetingHud __instance)
    {
        if (!PlayerControl.LocalPlayer.IsRole(RoleId.MeetingSheriff) || PlayerControl.LocalPlayer.IsAlive() && RoleClass.MeetingSheriff.KillMaxCount >= 1)
            return;
        for (int i = 0; i < __instance.playerStates.Length; i++)
        {
            PlayerVoteArea playerVoteArea = __instance.playerStates[i];
            var player = ModHelpers.PlayerById(__instance.playerStates[i].TargetPlayerId);
            if (player.IsDead() || player.PlayerId == CachedPlayer.LocalPlayer.PlayerId)
                continue;
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

    static void Postfix(MeetingHud __instance)
    {
        new LateTask(() => PlayerAnimation.PlayerAnimations.Values.All(x => { x.HandleAnim(RpcAnimationType.Stop); return false; }), 0.5f);
        LadderDead.Reset();
        if (ModeHandler.IsMode(ModeId.SuperHostRoles))
        {
            Mode.SuperHostRoles.MorePatch.StartMeeting();
        }

        MeetingUpdatePatch.IsFlag = false;
        MeetingUpdatePatch.IsSHRFlag = false;
        if (!ModeHandler.IsMode(ModeId.SuperHostRoles, ModeId.BattleRoyal) && PlayerControl.AllPlayerControls.Count > 15)
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

    public static void MeetingSheriffKillChatAnnounce(PlayerControl sheriff, PlayerControl target, bool isTargetKill, bool isSuicide)
    {
        Sheriff.SheriffRoleExecutionData.ExecutionMode mode = (Sheriff.SheriffRoleExecutionData.ExecutionMode)CustomOptionHolder.MeetingSheriffExecutionMode.GetSelection();
        var targetText = string.Format(ModTranslation.GetString("MeetingSheriffkillChat_KillTarget"), sheriff.name, target.name);

        string mainText = null;
        switch (mode)
        {
            case Sheriff.SheriffRoleExecutionData.ExecutionMode.Default:
                if (isTargetKill)
                    mainText = string.Format(ModTranslation.GetString("MeetingSheriffkillChat_Success"), sheriff.name);
                else
                    mainText = string.Format(ModTranslation.GetString("MeetingSheriffkillChat_MissFire"), sheriff.name);
                break;
            case Sheriff.SheriffRoleExecutionData.ExecutionMode.AlwaysSuicide:
                if (isTargetKill) // 「結果に関わらず 自殺する」設定で、対象が死亡している場合は シェリフも死亡している
                    mainText = string.Format(ModTranslation.GetString("MeetingSheriffkillChat_AlwaysSuicideSuccess"), target.name, sheriff.name);
                else // 対象が死亡していない場合は シェリフの通常キル(誤射) と同じ扱い。
                    mainText = string.Format(ModTranslation.GetString("MeetingSheriffkillChat_MissFire"), sheriff.name);
                break;
            case Sheriff.SheriffRoleExecutionData.ExecutionMode.AlwaysKill:
                if (!isSuicide) // 「誤射時も 対象を殺し自殺する」設定で、シェリフが死んでいない時は シェリフの通常キル(成功) と同じ扱い。
                    mainText = string.Format(ModTranslation.GetString("MeetingSheriffkillChat_Success"), sheriff.name);
                else // 自殺している場合は、誤射の為 対象も死亡している。
                    mainText = string.Format(ModTranslation.GetString("MeetingSheriffkillChat_AlwaysKillMissFire"), target.name, sheriff.name);
                break;
        }

        var originalName = sheriff.Data.PlayerName;

        const string line = "|--------------------------------------------------------|";
        var titelName = ModHelpers.Cs(RoleClass.SheriffYellow, $"<size=90%>{line}\n{ModTranslation.GetString("MeetingSheriffName")} {ModTranslation.GetString("KillName")} {ModTranslation.GetString("InformationName")}\n{line}</size>");

        const string blank = "<color=#00000000>.</color>\n";
        var contents = $"{blank}{blank}{targetText}{(mainText != null ? $"\n\n{mainText}" : "")}{blank}";

        sheriff.SetName(titelName);
        FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(sheriff, contents, false);
        sheriff.SetName(originalName);
    }
}