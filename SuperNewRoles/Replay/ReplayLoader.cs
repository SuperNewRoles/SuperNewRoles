using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using Il2CppSystem.Dynamic.Utils;
using SuperNewRoles.CustomObject;
using SuperNewRoles.Replay.ReplayActions;
using SuperNewRoles.Roles.Impostor;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using static GameData;
using static UnityEngine.GraphicsBuffer;

namespace SuperNewRoles.Replay;
public static class ReplayLoader
{
    public static void SpawnBots()
    {
        if (!ReplayManager.IsReplayMode) return;
        foreach (ReplayPlayer player in ReplayManager.CurrentReplay.ReplayPlayers)
        {
            PlayerControl Bot = BotManager.SpawnBot(player.PlayerId);

            Bot.RpcSetName(player.PlayerName);
            Bot.RpcSetColor((byte)player.ColorId);
            Bot.RpcSetHat(player.HatId);
            Bot.RpcSetPet(player.PetId);
            Bot.RpcSetVisor(player.VisorId);
            Bot.RpcSetNamePlate(player.NamePlateId);
            Bot.RpcSetSkin(player.SkinId);
            if (player.IsBot)
                BotManager.SetBot(Bot);
            Bot.Data.Tasks = new Il2CppSystem.Collections.Generic.List<TaskInfo>(player.Tasks.Count);
            for (int i = 0; i < player.Tasks.Count; i++)
            {
                Bot.Data.Tasks.Add(new TaskInfo(player.Tasks[i].Item2, player.Tasks[i].Item1));
                Bot.Data.Tasks[i].Id = player.Tasks[i].Item1;
            }
            Bot.SetTasks(Bot.Data.Tasks);
        }
    }
    public static bool IsInited;
    public static void UpdateLocalPlayerFirst()
    {
        byte id = 254;
        PlayerControl.LocalPlayer.PlayerId = id;
        PlayerControl.LocalPlayer.Data.PlayerId = id;
        PlayerControl.LocalPlayer.SetName("　");
    }
    public static void UpdateLocalPlayerEnd()
    {
        byte id = 0;
        foreach (PlayerControl p in PlayerControl.AllPlayerControls)
        {
            if (p == PlayerControl.LocalPlayer) continue;
            Logger.Info(p.Data.PlayerName + ":" + p.PlayerId.ToString());
            if (p.PlayerId > id)
            {
                id = p.PlayerId;
            }
        }
        id++;
        PlayerControl.LocalPlayer.PlayerId = id;
        PlayerControl.LocalPlayer.Data.PlayerId = id;
    }
    public static void StartMeeting()
    {
        if (!ReplayManager.IsReplayMode) return;
        posindex = 0;
        postime = 0;
        actiontime = 0;
        CurrentTurn++;
        actionindex = 0;
        IsStarted = true;
        //もう読み込み済みか判定
        if (CurrentTurn >= ReplayTurns.Count)
            GetPosAndActionsThisTurn();
        if (ReplayTurns[CurrentTurn].Actions.Count > actionindex)
            actiontime = ReplayTurns[CurrentTurn].Actions[actionindex].ActionTime;
    }
    public static void CoStartGame()
    {
        if (ReplayManager.IsReplayMode)
        {
            SetOptions();
            UpdateLocalPlayerFirst();
            SpawnBots();
            UpdateLocalPlayerEnd();
            IsInited = true;
        }
    }
    public static void CoIntroDestory()
    {
        if (ReplayManager.CurrentReplay.IsFirstLoaded)
            return;
        ReplayManager.CurrentReplay.IsFirstLoaded = true;
        posindex = 0;
        postime = 0;
        actiontime = 0;
        CurrentTurn = 0;
        actionindex = 0;
        IsStarted = true;
        ElectricalDoors electrical = ShipStatus.Instance?.transform?.Find("Electrical")?.GetComponent<ElectricalDoors>();
        if (electrical == null)
        {
            Logger.Info("ドアたちがいない...");
        }
        else
        {
            int index = 0;
            foreach (StaticDoor door in electrical.Doors)
            {
                if (ReplayManager.CurrentReplay.DoorTrues.Count > index)
                {
                    door.SetOpen(ReplayManager.CurrentReplay.DoorTrues[index]);
                    index++;
                }
            }
        }
        ReplayAction.CoIntroDestory();
        GetPosAndActionsThisTurn();
        if (ReplayTurns[CurrentTurn].Actions.Count > actionindex)
            actiontime = ReplayTurns[CurrentTurn].Actions[actionindex].ActionTime;
        //postime -= ReplayManager.CurrentReplay.RecordRate;
        PlayerControl.LocalPlayer.Exiled();
        PlayerControl.LocalPlayer.SetTasks(new());
        PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.transform.parent.gameObject.SetActive(false);
        PlayerControl.LocalPlayer.cosmetics.gameObject.SetActive(false);
        PlayerControl.LocalPlayer.cosmetics.nameText.transform.parent.gameObject.SetActive(false);
        CreateGUI();
        UpdateUIByState();
    }
    public static void CreateGUI()
    {
        GameObject back = new();
        GUIObject = back;
        back.name = "ReplayGUI";
        back.transform.parent = FastDestroyableSingleton<HudManager>.Instance.transform;
        back.layer = 5;
        back.transform.localPosition = new(0, -1.5f, -20f);
        back.transform.localScale = new(0.6f, 0.9f, 0.75f);
        SpriteRenderer backrender = back.AddComponent<SpriteRenderer>();
        backrender.sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Replay.ReplayGUIBack.png", 110f);
        PauseButtonRenderer = CreateItem("Pause", 0, (UnityAction)PlayOrPause, scale: new(3.25f, 4.7f, 4.7f));
        //ぬるぽ対策で残しとく
        //ｶﾞｯ!
        FastPlayButtonRenderer = CreateItem("FastPlay_seeyou", 9, (UnityAction)FastPlay, "FastPlayer_seeyou", size: new(6, 6));
        PlayRewindButtonRenderer = CreateItem("Play", 1, (UnityAction)PlayRewind, "PlayRewind", new(-3.25f, 4.7f, 4.7f));
        SpriteRenderer MTNM = CreateItem("Play", 2, (UnityAction)MoveToNextMeeting, "MoveToNextMeeting");
        MTNM.transform.localScale = new(1.95f, 4.7f, 4.7f);
        MTNM.transform.localPosition = new(1.5f, 0, 0);
        SpriteRenderer SubMTNMRender = GameObject.Instantiate(MTNM, MTNM.transform.parent);
        SubMTNMRender.transform.localScale = new(0.6f, 1.2f, 1);
        SubMTNMRender.transform.localPosition = new(-1.5f, 0, 0);
        SubMTNMRender.sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Replay.ReplayGUIButton.png", 110f);
        CreateItem("Exit", 3, (UnityAction)ReplayExit, scale: new(3.25f, 4.7f, 4.7f));

        GUIObject.gameObject.SetActive(true);
        UpdateButton();
    }
    public static SpriteRenderer PauseButtonRenderer;
    public static SpriteRenderer PlayRewindButtonRenderer;
    public static SpriteRenderer FastPlayButtonRenderer;
    public static void SetReplayStatus(ReplayState state)
    {
        MovingPlatformBehaviour mpb;
        //変更前の処理だから間違えないように
        switch (state)
        {
            case ReplayState.Pause:
                if (ReplayManager.CurrentReplay.CurrentPlayState != ReplayState.Pause)
                {
                    mpb = GameObject.FindObjectOfType<MovingPlatformBehaviour>();
                    if (mpb != null && mpb.Target != null)
                    {
                        mpb.Target.MyPhysics.body.velocity = Vector2.zero;
                        mpb.StopAllCoroutines();
                    }
                    foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                    {
                        if (player.onLadder)
                        {
                            player.MyPhysics.body.velocity = Vector2.zero;
                            player.MyPhysics.StopAllCoroutines();
                        }
                    }
                }
                break;
            case ReplayState.PlayRewind:
            case ReplayState.FastPlay:
            case ReplayState.Play:
                if (ReplayManager.CurrentReplay.CurrentPlayState == ReplayState.Pause)
                {
                    mpb = GameObject.FindObjectOfType<MovingPlatformBehaviour>();
                    if (mpb != null && mpb.Target != null)
                    {
                        mpb.IsLeft = !mpb.IsLeft;
                        mpb.StartCoroutine(ReplayActionMovingPlatform.UseMovingPlatform(mpb, mpb.Target, ReplayActionMovingPlatform.currentAction).WrapToIl2Cpp());
                    }
                    foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                    {
                        if (player.onLadder)
                        {
                            player.MyPhysics.StartCoroutine(ReplayActionClimbLadder.CoClimbLadderCustom(player.MyPhysics, ReplayManager.CurrentReplay.CurrentLadder.FirstOrDefault(x => x.Key == player.PlayerId).Value, player.MyPhysics.lastClimbLadderSid).WrapToIl2Cpp());
                        }
                    }
                }
                break;
        }
        //巻き戻しになるか巻き戻しから戻るか
        if ((ReplayManager.CurrentReplay.CurrentPlayState == ReplayState.PlayRewind && state != ReplayState.PlayRewind) ||
            (ReplayManager.CurrentReplay.CurrentPlayState != ReplayState.PlayRewind && state == ReplayState.PlayRewind))
        {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                player.transform.localScale = new(-player.transform.localScale.x, player.transform.localScale.y, player.transform.localScale.z);
            }
            if (ReplayManager.CurrentReplay.CurrentPlayState != ReplayState.PlayRewind && state == ReplayState.PlayRewind)
            {
                mpb = GameObject.FindObjectOfType<MovingPlatformBehaviour>();
                if (mpb != null && mpb.Target != null)
                {
                    mpb.Target.MyPhysics.body.velocity = Vector2.zero;
                    ReplayManager.CurrentReplay.MovingPlatformFrameCount = ((int)(mpb.Target.MyPhysics.Speed * 60)) - ReplayManager.CurrentReplay.MovingPlatformFrameCount;
                    mpb.StopAllCoroutines();
                    //mpb.IsLeft = !mpb.IsLeft;
                    mpb.StartCoroutine(ReplayActionMovingPlatform.UseMovingPlatform(mpb, mpb.Target).WrapToIl2Cpp());
                }

                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (player.onLadder)
                    {
                        player.MyPhysics.body.velocity = Vector2.one;
                        player.MyPhysics.StopAllCoroutines();
                        player.MyPhysics.StartCoroutine(ReplayActionClimbLadder.CoClimbLadderRewind(player.MyPhysics, ReplayManager.CurrentReplay.CurrentLadder.FirstOrDefault(x => x.Key == player.PlayerId).Value, player.MyPhysics.lastClimbLadderSid).WrapToIl2Cpp());
                    }
                }
                //アニメーションの巻き戻し処理
                foreach (var anim in CustomAnimation.CustomAnimations)
                    anim.OnPlayRewind();
                foreach (var anim in PlayerAnimation.PlayerAnimations)
                    anim.Value.OnPlayRewind();
            }
            else
            {
                mpb = GameObject.FindObjectOfType<MovingPlatformBehaviour>();
                if (mpb != null && mpb.Target != null)
                {
                    ReplayManager.CurrentReplay.MovingPlatformFrameCount = ((int)(mpb.Target.MyPhysics.Speed * 60)) - ReplayManager.CurrentReplay.MovingPlatformFrameCount;
                    mpb.Target.MyPhysics.body.velocity = Vector2.zero;
                    mpb.StopAllCoroutines();
                    //mpb.IsLeft = !mpb.IsLeft;
                    mpb.StartCoroutine(ReplayActionMovingPlatform.UseMovingPlatform(mpb, mpb.Target).WrapToIl2Cpp());
                }
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (player.onLadder)
                    {
                        player.MyPhysics.body.velocity = Vector2.one;
                        player.MyPhysics.StopAllCoroutines();
                        player.MyPhysics.StartCoroutine(ReplayActionClimbLadder.CoClimbLadderCustom(player.MyPhysics, ReplayManager.CurrentReplay.CurrentLadder.FirstOrDefault(x => x.Key == player.PlayerId).Value, player.MyPhysics.lastClimbLadderSid).WrapToIl2Cpp());
                    }
                }
                //アニメーションのプレイ処理
                foreach (var anim in CustomAnimation.CustomAnimations)
                    anim.Play();
                foreach (var anim in PlayerAnimation.PlayerAnimations)
                    anim.Value.Play();
            }
        }
        if (state == ReplayState.Pause)
            foreach (KeyValuePair<byte, PlayerAnimation> anim in PlayerAnimation.PlayerAnimations)
                anim.Value.Pause();
        ReplayManager.CurrentReplay.CurrentPlayState = state;
    }
    public static void PlayOrPause()
    {
        if (ReplayManager.CurrentReplay != null)
        {
            SetReplayStatus(ReplayManager.CurrentReplay.CurrentPlayState == ReplayState.Play ? ReplayState.Pause : ReplayState.Play);
        }
        UpdateButton();
    }
    public static void FastPlay()
    {
        SetReplayStatus(ReplayManager.CurrentReplay.CurrentPlayState == ReplayState.FastPlay ? ReplayState.Pause : ReplayState.FastPlay);
        UpdateButton();
    }
    public static void PlayRewind()
    {
        SetReplayStatus(ReplayManager.CurrentReplay.CurrentPlayState == ReplayState.PlayRewind ? ReplayState.Pause : ReplayState.PlayRewind);
        UpdateButton();
    }
    public static void MoveToNextMeeting()
    {
        if (ReplayTurns[CurrentTurn].IsGameEnd)
        {
            GameManager.Instance.RpcEndGame(ReplayTurns[CurrentTurn].CurrentEndGameData.OverReason, false);
            return;
        }
        else
        {
            //会議のReplayActionを実行
            foreach (ReplayAction action in ReplayTurns[CurrentTurn].Actions)
            {
                if (action.GetActionId() == ReplayActionId.ReportDeadBody)
                {
                    action.OnAction();
                    break;
                }
            }
        }
        UpdateButton();
    }
    public static void ReplayExit()
    {
        AmongUsClient.Instance.ExitGame(DisconnectReasons.ExitGame);
    }
    public static void UpdateButton()
    {
        if (ReplayManager.CurrentReplay != null)
        {
            if (PauseButtonRenderer != null)
            {
                if (ReplayManager.CurrentReplay.CurrentPlayState is ReplayState.Pause)
                {
                    PauseButtonRenderer.sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Replay.ReplayGUIPause.png", 110f);
                }
                else
                {
                    PauseButtonRenderer.sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Replay.ReplayGUIPlay.png", 110f);
                }
                PauseButtonRenderer.transform.localScale = new(3.25f, 4.7f, 4.7f);
            }
            if (PlayRewindButtonRenderer != null)
            {
                if (ReplayManager.CurrentReplay.CurrentPlayState is ReplayState.PlayRewind)
                {
                    PlayRewindButtonRenderer.sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Replay.ReplayGUIPlayRewind.png", 110f);
                    PlayRewindButtonRenderer.transform.localScale = new(3.25f, 4.7f, 4.7f);
                }
                else
                {
                    PlayRewindButtonRenderer.sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Replay.ReplayGUIPlay.png", 110f);
                    PlayRewindButtonRenderer.transform.localScale = new(-3.25f, 4.7f, 4.7f);
                }
            }
            if (FastPlayButtonRenderer != null)
            {
                if (ReplayManager.CurrentReplay.CurrentPlayState is ReplayState.FastPlay)
                {
                    FastPlayButtonRenderer.sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Replay.ReplayGUIButtonFastPlaying.png", 110f);
                }
                else
                {
                    FastPlayButtonRenderer.sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Replay.ReplayGUIButtonFastPlay.png", 110f);
                }
                FastPlayButtonRenderer.transform.localScale = new(3.25f, 4.7f, 4.7f);
            }
        }
    }
    public static SpriteRenderer CreateItem(string Id, int index, UnityAction action, string name = "", Vector3? scale = null, Vector2? size = null)
    {
        GameObject item = new()
        {
            name = Id == "" ? name : Id
        };
        item.transform.parent = GUIObject.transform;
        item.layer = 5;
        item.transform.localPosition = new(-3.2f + 2.15f * index, 0, -20);
        item.transform.localScale = new(0.275f, 0.175f, 0.175f);
        GameObject renderobj = new("Renderer")
        {
            layer = 5
        };
        renderobj.transform.parent = item.transform;
        renderobj.transform.localPosition = new();
        renderobj.transform.localScale = new(1, 1, 1);
        if (scale != null)
        {
            renderobj.transform.localScale = scale.Value;
        }
        SpriteRenderer itemrender = renderobj.AddComponent<SpriteRenderer>();
        PassiveButton btn = item.AddComponent<PassiveButton>();
        BoxCollider2D collider = item.AddComponent<BoxCollider2D>();
        collider.size = new(4, 4);
        if (size != null)
        {
            collider.size = size.Value;
        }
        btn.Colliders = new[] { collider };
        btn.OnClick = new();
        btn.OnClick.AddListener((UnityAction)(() =>
        {
        }));
        btn.OnClick.AddListener(action);
        btn.OnMouseOut = new();
        btn.OnMouseOver = new();
        itemrender.sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Replay.ReplayGUI" + Id + ".png", 110f);
        return itemrender;
    }
    public static GameObject GUIObject;
    static bool IsStarted;
    public static void AllRoleSet()
    {
        if (ReplayManager.IsReplayMode)
        {
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                RoleTypes role = RoleTypes.Crewmate;
                RoleId roleid = RoleId.DefaultRole;
                if (p.PlayerId != PlayerControl.LocalPlayer.PlayerId)
                {
                    ReplayPlayer rp = ReplayManager.CurrentReplay.ReplayPlayers.FirstOrDefault(x => x.PlayerId == p.PlayerId);
                    if (rp != null)
                    {
                        role = rp.RoleType;
                        roleid = rp.RoleId;
                    }
                    else
                        Logger.Info(p.PlayerId + "の役職が参照できませんでした。");
                }
                p.SetRole(role);
                p.SetRole(roleid);
            }
            //ShowIntro();
        }
    }
    public static void ShowIntro()
    {
        PlayerControl.AllPlayerControls.ForEach((Il2CppSystem.Action<PlayerControl>)((PlayerControl pc) =>
        {
            PlayerNameColor.Set(pc);
        }));
        ((MonoBehaviour)PlayerControl.LocalPlayer).StopAllCoroutines();
        ((MonoBehaviour)DestroyableSingleton<HudManager>.Instance).StartCoroutine(DestroyableSingleton<HudManager>.Instance.CoShowIntro());
        FastDestroyableSingleton<HudManager>.Instance.HideGameLoader();
    }
    public static void SetOptions()
    {
        GameOptionsManager.Instance.CurrentGameOptions = ReplayManager.CurrentReplay.GameOptions;
        GameManager.Instance.LogicOptions.SetGameOptions(ReplayManager.CurrentReplay.GameOptions);
        Logger.Info(ReplayManager.CurrentReplay.GameOptions.GetFloat(FloatOptionNames.PlayerSpeedMod).ToString(), "CurrentReplay");
        Logger.Info(GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.PlayerSpeedMod).ToString(), "CurrentReplay");
        Logger.Info(GameManager.Instance.LogicOptions.currentGameOptions.GetFloat(FloatOptionNames.PlayerSpeedMod).ToString(), "CurrentReplay");
        ReplayManager.CurrentReplay.UpdateCustomOptionByData();
    }
    public static void GetPosAndActionsThisTurn()
    {
        var caller = new System.Diagnostics.StackFrame(1, false);
        var callerMethod = caller.GetMethod();
        string callerMethodName = callerMethod.Name;
        string callerClassName = callerMethod.DeclaringType.FullName;
        SuperNewRolesPlugin.Logger.LogInfo("[Replay:FixedUpdate]" + callerClassName + "." + callerMethodName + " Called.");
        ReplayTurn turn = new()
        {
            Positions = new(),
            Actions = new()
        };
        var reader = ReplayManager.CurrentReplay.binaryReader;
        Logger.Info(reader.BaseStream.Length.ToString());
        Logger.Info(reader.BaseStream.Position.ToString());
        bool IsPosFloat = ReplayManager.CurrentReplay.IsPosFloat;
        int playercount = reader.ReadInt32();
        for (int i = 0; i < playercount; i++)
        {
            byte playerId = reader.ReadByte();
            turn.Positions.Add(playerId, new());
            int poscount = reader.ReadInt32();
            Logger.Info(poscount.ToString(), "poscount");
            for (int i2 = 0; i2 < poscount; i2++)
            {
                if (IsPosFloat)
                {
                    turn.Positions[playerId].Add(new(reader.ReadSingle(),
                        reader.ReadSingle()));
                }
                else
                {
                    turn.Positions[playerId].Add(new((reader.ReadInt16() / 10.0f),
                        (reader.ReadInt16() / 10.0f)));
                }
            }
        }
        int actioncount = reader.ReadInt32();
        Logger.Info("アクション数:" + actioncount.ToString());
        for (int i = 0; i < actioncount; i++)
        {
            ReplayActionId replayActionId = (ReplayActionId)reader.ReadByte();
            Logger.Info(i.ToString() + ":" + actioncount.ToString(), "今の数");
            if (replayActionId != ReplayActionId.None)
            {
                Logger.Info(replayActionId + "追加:" + reader.BaseStream.Position.ToString());
                ReplayAction action = ReplayAction.CreateReplayAction(replayActionId);
                action.Init();
                action.ReadReplayFile(reader);
                turn.Actions.Add(action);
                Logger.Info(replayActionId + "終わり:" + reader.BaseStream.Position.ToString());
            }
            else
            {
                Logger.Info(replayActionId + "だったのでパス");
            }
        }
        turn.IsGameEnd = reader.ReadBoolean();
        if (turn.IsGameEnd)
        {
            turn.CurrentEndGameData = new(reader);
        }
        ReplayTurns.Add(turn);
    }
    public static void ClearAndReloads()
    {
        ReplayTurns = new();
        postime = 99999;
        IsStarted = false;
        IsInited = false;
    }
    [HarmonyPatch(typeof(HauntMenuMinigame), nameof(HauntMenuMinigame.Start))]
    public static class HauntMenuMinigameStartPatch
    {
        public static void Postfix()
        {
            if (GUIObject != null)
                GUIObject.transform.localPosition = new(0, -0.6f, -20);
        }
    }
    [HarmonyPatch(typeof(HauntMenuMinigame), nameof(HauntMenuMinigame.Close))]
    public static class HauntMenuMinigameClosePatch
    {
        public static void Postfix()
        {
            if (GUIObject != null)
                GUIObject.transform.localPosition = new(0, -1.5f, -20);
        }
    }
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
    public class MeetingHudUpdatePatch
    {
        public static bool Prefix() => SaboPrefix();
    }
    [HarmonyPatch(typeof(LifeSuppSystemType), nameof(LifeSuppSystemType.Detoriorate))]
    public class LifeSuppSystemTypeDetorioratePatch
    {
        public static bool Prefix() => SaboPrefix();
    }
    [HarmonyPatch(typeof(HeliSabotageSystem), nameof(HeliSabotageSystem.Detoriorate))]
    public class HeliSabotageSystemDetorioratePatch
    {
        public static bool Prefix() => SaboPrefix();
    }
    [HarmonyPatch(typeof(ReactorSystemType), nameof(ReactorSystemType.Detoriorate))]
    public class ReactorSystemTypeDetorioratePatch
    {
        public static bool Prefix() => SaboPrefix();
    }
    public static bool SaboPrefix()
    {
        if (ReplayManager.IsReplayMode && ReplayManager.CurrentReplay != null)
        {
            if (ReplayManager.CurrentReplay.CurrentPlayState == ReplayState.Pause)
            {
                return false;
            }
        }
        return true;
    }
    public static List<ReplayTurn> ReplayTurns;
    static float postime;
    static float actiontime;
    public static int CurrentTurn;
    public static int posindex;
    static int actionindex;
    static int currentwrapupposindex;
    public static void OnWrapUp()
    {
        currentwrapupposindex = posindex;
    }
    public static void ShowMeetingRewindPopup()
    {
        HudManager.Instance.transform.FindChild("GenericDialogue/Background/FullScreen").GetComponent<BoxCollider2D>().enabled = false;
        HudManager.Instance.transform.FindChild("GenericDialogue/CloseButton").gameObject.SetActive(false);
        HudManager.Instance.transform.FindChild("GenericDialogue").GetComponent<DialogueBox>().enabled = false;
        static void Createbtn(string name, string text, Vector3 pos, Action action)
        {
            GameObject btntemplate = DiscordManager.Instance.discordPopup.transform.FindChild("ExitGame").gameObject;
            GameObject RewindTurnBtn = GameObject.Instantiate(btntemplate, HudManager.Instance.transform.FindChild("GenericDialogue"));
            TextMeshPro RewindTurnTex = RewindTurnBtn.GetComponentInChildren<TextMeshPro>();
            GameObject.Destroy(RewindTurnTex.GetComponent<TextTranslatorTMP>());
            RewindTurnTex.text = text;
            RewindTurnBtn.transform.localPosition = pos;
            RewindTurnBtn.name = name;
            PassiveButton psbtn = RewindTurnBtn.GetComponent<PassiveButton>();
            psbtn.OnClick = new();
            psbtn.OnClick.AddListener(action);
        }
        static void Reset()
        {
            GameObject.Destroy(HudManager.Instance.transform.FindChild("GenericDialogue/ThisPosStartButton").gameObject);
            GameObject.Destroy(HudManager.Instance.transform.FindChild("GenericDialogue/RewindTurnButton").gameObject);
            HudManager.Instance.transform.FindChild("GenericDialogue/Background/FullScreen").GetComponent<BoxCollider2D>().enabled = true;
            HudManager.Instance.transform.FindChild("GenericDialogue/CloseButton").gameObject.SetActive(true);
            HudManager.Instance.transform.FindChild("GenericDialogue").GetComponent<DialogueBox>().enabled = true;
            HudManager.Instance.transform.FindChild("GenericDialogue").gameObject.SetActive(false);
        }
        Createbtn("RewindTurnButton", "前のターンの終わりから巻き戻す", new(-0.875f, -0.6f, -0.5f), () =>
        {
            CurrentTurn--;
            Logger.Info(ReplayTurns[CurrentTurn].Positions.Values.FirstOrDefault().Count.ToString(), "Count");
            posindex = ReplayTurns[CurrentTurn].Positions.Values.FirstOrDefault().Count - 1;
            actionindex = ReplayTurns[CurrentTurn].Actions.Count - 1;
            postime = ReplayManager.CurrentReplay.RecordRate;
            actiontime = ReplayTurns[CurrentTurn].Actions[ReplayTurns[CurrentTurn].Actions.Count - 1].ActionTime;
            SetReplayStatus(ReplayState.PlayRewind);
            UpdateButton();
            if (MeetingHud.Instance != null)
            {
                GameObject.Destroy(MeetingHud.Instance.gameObject);
                MeetingHud.Instance = null;
            }
            Reset();
            ConsoleJoystick.SetMode_Task();
            ControllerManager.Instance.CloseAndResetAll();
            PlayerControl.LocalPlayer.moveable = true;
            Camera.main.GetComponent<FollowerCamera>().Locked = false;
            currentwrapupposindex = 999999;
            Logger.Info("Click Rewind Turn Button");
        });
        Createbtn("ThisPosStartButton", "ここからはじめる", new(0.875f, -0.6f, -0.5f), () =>
        {
            SetReplayStatus(ReplayState.Play);
            UpdateButton();
            Reset();
        });
        HudManager.Instance.ShowPopUp("会議を跨いでの巻き戻しはできません。");
        ReplayManager.CurrentReplay.CurrentUIState = UIState.HideAllUI;
        UpdateUIByState();
        Logger.Info("Commed UI");
    }
    public static void HudUpdate()
    {
        if (!IsStarted) return;
        if (ReplayManager.CurrentReplay.CurrentPlayState == ReplayState.Pause) return;
        if (ReplayManager.CurrentReplay.CurrentPlayState == ReplayState.PlayRewind)
        {
            if ((MeetingHud.Instance != null && MeetingHud.Instance.discussionTimer <= 0) ||
                posindex == currentwrapupposindex)
            {
                ShowMeetingRewindPopup();
                SetReplayStatus(ReplayState.Pause);
                return;
            }
            postime += Time.deltaTime;
            if (actiontime != -999)
                actiontime += Time.deltaTime;
            if (MeetingHud.Instance != null)
            {
                MeetingHud.Instance.discussionTimer -= (Time.deltaTime * 2);
            }
        }
        else
        {
            postime -= Time.deltaTime;
            if (actiontime != -999)
                actiontime -= Time.deltaTime;
        }
        if (ReplayManager.CurrentReplay.CurrentPlayState == ReplayState.PlayRewind ? ReplayManager.CurrentReplay.RecordRate <= postime : postime <= 0)
        {
            int targetindex = posindex + (ReplayManager.CurrentReplay.CurrentPlayState == ReplayState.Play ? 1 : -1);
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.PlayerId == PlayerControl.LocalPlayer.PlayerId) continue;
                try
                {
                    if (ReplayTurns[CurrentTurn].Positions[player.PlayerId].Count <= posindex || posindex < 0) continue;
                    //Logger.Info(ReplayTurns[CurrentTurn].Positions[player.PlayerId][posindex].ToString());
                    //player.StopAllCoroutines();
                    //player.NetTransform.RpcSnapTo(ReplayTurns[CurrentTurn].Positions[player.PlayerId][posindex]);
                    //if (ReplayTurns[CurrentTurn].Positions[player.PlayerId].Count > posindex + 1)
                    if (!player.onLadder && !player.inMovingPlat)
                    {
                        if (ReplayTurns[CurrentTurn].Positions[player.PlayerId].Count > targetindex)
                        {
                            player.NetTransform.SnapTo(ReplayTurns[CurrentTurn].Positions[player.PlayerId][targetindex]);
                        }
                        player.transform.position = new(ReplayTurns[CurrentTurn].Positions[player.PlayerId][posindex].x,
                            ReplayTurns[CurrentTurn].Positions[player.PlayerId][posindex].y,
                            0f);
                    }/*
                        Logger.Info(ReplayManager.CurrentReplay.GameOptions.GetFloat(FloatOptionNames.PlayerSpeedMod).ToString(), "CurrentReplay");
                        Logger.Info(GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.PlayerSpeedMod).ToString(), "CurrentReplay");
                        Logger.Info(GameManager.Instance.LogicOptions.currentGameOptions.GetFloat(FloatOptionNames.PlayerSpeedMod).ToString(), "CurrentReplay");*/
                    //Logger.Info($"{ReplayTurns[CurrentTurn].Positions[player.PlayerId][posindex + 1]} => {new Vector3(ReplayTurns[CurrentTurn].Positions[player.PlayerId][posindex + 1].x,
                    //    ReplayTurns[CurrentTurn].Positions[player.PlayerId][posindex + 1].y,
                    //    0f)}");
                    //player.MyPhysics.Animations.PlayRunAnimation();
                    //AmongUsClient.Instance.StartCoroutine(player.MyPhysics.WalkPlayerTo(ReplayTurns[CurrentTurn].Positions[player.PlayerId][posindex]));
                    //AmongUsClient.Instance.StartCoroutine(player.MyPhysics.WalkPlayerTo(ReplayTurns[CurrentTurn].Positions[player.PlayerId][posindex]));

                }
                catch (Exception e)
                {
                    Logger.Info(e.ToString());
                }
            }
            if (targetindex >= 0)
            {
                posindex = targetindex;
                if (ReplayTurns[CurrentTurn].IsGameEnd && ReplayTurns[CurrentTurn].Positions[0].Count <= posindex)
                {
                    GameManager.Instance.RpcEndGame(ReplayTurns[CurrentTurn].CurrentEndGameData.OverReason, false);
                    return;
                }
            }
            if (ReplayManager.CurrentReplay.CurrentPlayState == ReplayState.PlayRewind)
            {
                ReplayAction moving = ReplayTurns[CurrentTurn].Actions.FirstOrDefault(x => (x as ReplayActionMovingPlatform != null) && (x as ReplayActionMovingPlatform).endposindex == posindex && (x as ReplayActionMovingPlatform).CurrentTurn == CurrentTurn);
                if (moving != null)
                {
                    ReplayActionMovingPlatform ramp = moving as ReplayActionMovingPlatform;
                    MovingPlatformBehaviour mpb = GameObject.FindObjectOfType<MovingPlatformBehaviour>();
                    PlayerControl movingtarget = ModHelpers.PlayerById(ramp.sourcePlayer);
                    movingtarget.MyPhysics.body.velocity = Vector2.zero;
                    mpb.StopAllCoroutines();
                    //mpb.IsLeft = !mpb.IsLeft;
                    mpb.StartCoroutine(ReplayActionMovingPlatform.UseMovingPlatform(mpb, movingtarget).WrapToIl2Cpp());
                }
            }
            Logger.Info(posindex.ToString(), "POSINDEXXXXX");
            postime = ReplayManager.CurrentReplay.CurrentPlayState == ReplayState.PlayRewind ? 0 : ReplayManager.CurrentReplay.RecordRate;
        }
        //Logger.Info("actiontime:"+actiontime.ToString());
        if (ReplayManager.CurrentReplay.CurrentPlayState == ReplayState.PlayRewind)
        {
            while (ReplayTurns[CurrentTurn].Actions.Count > 0 && actionindex >= 0 && actiontime >= ReplayTurns[CurrentTurn].Actions[actionindex].ActionTime && actiontime != -999)
            {
                if (ReplayTurns[CurrentTurn].Actions.Count > actionindex)
                {
                    Logger.Info("アクション！:" + ReplayTurns[CurrentTurn].Actions[actionindex - 1].GetActionId());
                    ReplayTurns[CurrentTurn].Actions[actionindex - 1].OnReplay();
                    actionindex--;
                    if (ReplayTurns[CurrentTurn].Actions.Count > actionindex && actionindex >= 0) actiontime = 0;//ReplayTurns[CurrentTurn].Actions[actionindex].ActionTime;
                }
            }
        }
        else
        {
            while (ReplayTurns[CurrentTurn].Actions.Count > 0 && actiontime <= 0 && actiontime != -999)
            {
                if (ReplayTurns[CurrentTurn].Actions.Count > actionindex)
                {
                    Logger.Info("アクション！:" + ReplayTurns[CurrentTurn].Actions[actionindex].GetActionId());
                    ReplayTurns[CurrentTurn].Actions[actionindex].OnAction();
                    actionindex++;
                    if (ReplayTurns[CurrentTurn].Actions.Count > actionindex) actiontime = ReplayTurns[CurrentTurn].Actions[actionindex].ActionTime;
                    else actiontime = -999;
                }
            }
        }
        //キーボード判定
        if (Input.GetKeyDown(ChangeUIKeyCode))
        {
            switch (ReplayManager.CurrentReplay.CurrentUIState)
            {
                case UIState.Default:
                    ReplayManager.CurrentReplay.CurrentUIState = UIState.HideAllUI;
                    break;
                case UIState.HideAllUI:
                    ReplayManager.CurrentReplay.CurrentUIState = UIState.ShowOperationUI;
                    break;
                case UIState.ShowOperationUI:
                    ReplayManager.CurrentReplay.CurrentUIState = UIState.Default;
                    break;
            }
            UpdateUIByState();
        }
        else if (Input.GetKeyDown(PauseKeyCode))
            PlayOrPause();
        else if (Input.GetKeyDown(RewindKeyCode))
            PlayRewind();
        else if (Input.GetKeyDown(FastPlayKeyCode))
            FastPlay();
    }
    public static void UpdateUIByState()
    {
        bool GUIObjectActive = true;
        bool DefaultUIActive = true;
        switch (ReplayManager.CurrentReplay.CurrentUIState)
        {
            case UIState.Default:
                GUIObjectActive = false;
                break;
            case UIState.HideAllUI:
                GUIObjectActive = false;
                DefaultUIActive = false;
                break;
            default:
                break;
        }
        GUIObject.SetActive(GUIObjectActive);
        HudManager.Instance.transform.FindChild("Buttons").gameObject.SetActive(DefaultUIActive);
        HudManager.Instance.transform.FindChild("SimpleTextLabels").gameObject.SetActive(DefaultUIActive);
        HudManager.Instance.transform.FindChild("TaskDisplay").gameObject.SetActive(DefaultUIActive);
    }
    //各キーのKeyCode
    public const KeyCode ChangeUIKeyCode = KeyCode.F6;
    public const KeyCode PauseKeyCode = KeyCode.K;
    public const KeyCode RewindKeyCode = KeyCode.J;
    public const KeyCode FastPlayKeyCode = KeyCode.L;
}

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.PopulateButtons))]
class MeetingHudPopulateButtons
{
    public static void Postfix(MeetingHud __instance, byte reporter)
    {
        Logger.Info("Commed pop");
        if (ReplayManager.IsReplayMode)
        {
            Logger.Info("Commed popRep:" + __instance.playerStates.Count.ToString());
            var states = __instance.playerStates.ToList();
            GameObject.Destroy(states.FirstOrDefault(x => x.TargetPlayerId == PlayerControl.LocalPlayer.PlayerId)?.gameObject);
            states.RemoveAll(x => x.TargetPlayerId == PlayerControl.LocalPlayer.PlayerId);
            __instance.playerStates = states.ToArray();
            Logger.Info("Commed popRepens:" + __instance.playerStates.Count.ToString());
            __instance.SortButtons();
        }
    }
}