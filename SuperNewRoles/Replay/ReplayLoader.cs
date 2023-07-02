using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AmongUs.GameOptions;
using SuperNewRoles.Replay.ReplayActions;
using UnityEngine;
using UnityEngine.Events;
using static GameData;

namespace SuperNewRoles.Replay
{
    public static class ReplayLoader
    {
        public static void SpawnBots() {
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
        public static void UpdateLocalPlayerFirst() {
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
                Logger.Info(p.Data.PlayerName+":"+p.PlayerId.ToString());
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
            CurrentTurn++;
            posindex = 0;
            postime = 0;
            actiontime = 0;
            actionindex = 0;
            GetPosAndActionsThisTurn();
            if (ReplayTurns[CurrentTurn].Actions.Count > actionindex)
                actiontime = ReplayTurns[CurrentTurn].Actions[actionindex].ActionTime;
        }
        public static void CoStartGame() {
            if (ReplayManager.IsReplayMode)
            {
                SetOptions();
                UpdateLocalPlayerFirst();
                SpawnBots();
                UpdateLocalPlayerEnd();
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
            GetPosAndActionsThisTurn();
            if (ReplayTurns[CurrentTurn].Actions.Count > actionindex)
                actiontime = ReplayTurns[CurrentTurn].Actions[actionindex].ActionTime;
            Logger.Info(ReplayTurns[CurrentTurn].Actions[actionindex].ActionTime.ToString()+":"+ReplayManager.CurrentReplay.RecordRate.ToString());
            //postime -= ReplayManager.CurrentReplay.RecordRate;
            PlayerControl.LocalPlayer.Exiled();
            PlayerControl.LocalPlayer.SetTasks(new());
            PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.transform.parent.gameObject.SetActive(false);
            PlayerControl.LocalPlayer.cosmetics.gameObject.SetActive(false);
            PlayerControl.LocalPlayer.cosmetics.nameText.transform.parent.gameObject.SetActive(false);
            CreateGUI();
        }
        public static void CreateGUI() {
            GameObject back = new();
            GUIObject = back;
            back.name = "ReplayGUI";
            back.transform.parent = FastDestroyableSingleton<HudManager>.Instance.transform;
            back.layer = 5;
            back.transform.localPosition = new(0, -1.5f, -20f);
            back.transform.localScale = new(0.6f, 0.9f, 0.75f);
            SpriteRenderer backrender = back.AddComponent<SpriteRenderer>();
            backrender.sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Replay.ReplayGUIBack.png", 110f);
            CreateItem("Play", 0);
            CreateItem("", 1, "Faster");

            GUIObject.gameObject.SetActive(false);
        }
        public static SpriteRenderer CreateItem(string Id, int index, string name= "")
        {
            GameObject item = new();
            item.name = Id == "" ? name : Id;
            item.transform.parent = GUIObject.transform;
            item.layer = 5;
            item.transform.localPosition = new(-3.4f + 1.75f * index, 0, -20);
            item.transform.localScale = new(0.275f, 0.175f, 0.175f);
            SpriteRenderer itemrender = item.AddComponent<SpriteRenderer>();
            PassiveButton btn = item.AddComponent<PassiveButton>();
            BoxCollider2D collider = item.AddComponent<BoxCollider2D>();
            collider.size = new(1.75f, 1.75f);
            btn.Colliders = new[] { collider };
            btn.OnClick = new();
            btn.OnClick.AddListener((UnityAction)(() => {
                Logger.Info("おけ");
            }));
            itemrender.sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Replay.ReplayGUI"+Id+".png", 110f);
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
                    if (p.PlayerId != PlayerControl.LocalPlayer.PlayerId)
                    {
                        ReplayPlayer rp = ReplayManager.CurrentReplay.ReplayPlayers.FirstOrDefault(x => x.PlayerId == p.PlayerId);
                        if (rp != null)
                            role = rp.RoleType;
                        else
                            Logger.Info(p.PlayerId+"の役職が参照できませんでした。");
                    }
                    p.SetRole(role);
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
            Logger.Info(ReplayManager.CurrentReplay.GameOptions.GetFloat(FloatOptionNames.PlayerSpeedMod).ToString(),"CurrentReplay");
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
                Logger.Info(poscount.ToString(),"poscount");
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
                    Logger.Info(i2.ToString(),"ab");
                }
            }
            int actioncount = reader.ReadInt32();
            Logger.Info("アクション数:"+actioncount.ToString());
            for (int i = 0; i < actioncount; i++)
            {
                ReplayActionId replayActionId = (ReplayActionId)reader.ReadByte();
                Logger.Info(i.ToString()+":"+actioncount.ToString(),"今の数");
                if (replayActionId != ReplayActionId.None)
                {
                    Logger.Info(replayActionId + "追加:"+reader.BaseStream.Position.ToString());
                    ReplayAction action = ReplayAction.CreateReplayAction(replayActionId);
                    action.ReadReplayFile(reader);
                    turn.Actions.Add(action);
                    Logger.Info(replayActionId + "終わり:" + reader.BaseStream.Position.ToString());
                }
                else
                {
                    Logger.Info(replayActionId + "だったのでパス");
                }
            }
            Logger.Info(reader.ReadBoolean().ToString(),"終了？");
            ReplayTurns.Add(turn);
        }
        public static void ClearAndReloads()
        {
            ReplayTurns = new();
            postime = 99999;
            IsStarted = false;
        }
        public static List<ReplayTurn> ReplayTurns;
        static float postime;
        static float actiontime;
        static int CurrentTurn;
        static int posindex;
        static int actionindex;
        public static void HudUpdate() {
            if (!IsStarted) return;
            postime -= Time.deltaTime;
            if (actiontime != -999)
                actiontime -= Time.deltaTime;
            if (postime <= 0)
            {
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (player.PlayerId == PlayerControl.LocalPlayer.PlayerId) continue;
                    try
                    {
                        if (ReplayTurns[CurrentTurn].Positions[player.PlayerId].Count <= posindex) continue;
                        //Logger.Info(ReplayTurns[CurrentTurn].Positions[player.PlayerId][posindex].ToString());
                        //player.StopAllCoroutines();
                        //player.NetTransform.RpcSnapTo(ReplayTurns[CurrentTurn].Positions[player.PlayerId][posindex]);
                        //if (ReplayTurns[CurrentTurn].Positions[player.PlayerId].Count > posindex + 1)
                        if (ReplayTurns[CurrentTurn].Positions[player.PlayerId].Count > posindex + 1)
                        {
                            player.NetTransform.SnapTo(ReplayTurns[CurrentTurn].Positions[player.PlayerId][posindex + 1]);
                        }
                        player.transform.position = new(ReplayTurns[CurrentTurn].Positions[player.PlayerId][posindex].x,
                            ReplayTurns[CurrentTurn].Positions[player.PlayerId][posindex].y,
                            0f);/*
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
                posindex++;
                Logger.Info(posindex.ToString(),"POSINDEXXXXX");
                postime = ReplayManager.CurrentReplay.RecordRate;
            }
            //Logger.Info("actiontime:"+actiontime.ToString());
            while (actiontime <= 0 && actiontime != -999)
            {
                if (ReplayTurns[CurrentTurn].Actions.Count > actionindex)
                {
                    Logger.Info("アクション！:"+ ReplayTurns[CurrentTurn].Actions[actionindex].GetActionId());
                    ReplayTurns[CurrentTurn].Actions[actionindex].OnAction();
                    actionindex++;
                    if (ReplayTurns[CurrentTurn].Actions.Count > actionindex) actiontime = ReplayTurns[CurrentTurn].Actions[actionindex].ActionTime;
                    else actiontime = -999;
                }
            }
        }
    }
}