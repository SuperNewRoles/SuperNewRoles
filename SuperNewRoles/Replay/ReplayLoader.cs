using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AmongUs.GameOptions;
using SuperNewRoles.Replay.ReplayActions;
using UnityEngine;
using static GameData;

namespace SuperNewRoles.Replay
{
    public static class ReplayLoader
    {
        public static void SpawnBots() {
            if (ReplayManager.CurrentReplay == null) return;
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
                Bot.Data.Object.SetTasks(Bot.Data.Tasks);
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
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                if (p.PlayerId > id)
                {
                    id = p.PlayerId;
                }
            }
            id++;
            PlayerControl.LocalPlayer.PlayerId = id;
            PlayerControl.LocalPlayer.Data.PlayerId = id;
        }
        public static void CoStartGame() {
            if (ReplayManager.IsReplayMode)
            {
                UpdateLocalPlayerFirst();
                SpawnBots();
                UpdateLocalPlayerEnd();
            }
        }
        public static void CoIntroStart()
        {
            postime = 0;
            posindex = 0;
            actiontime = 0;
            CurrentTurn = 0;
            actionindex = 0;
            IsStarted = true;
            GetPosAndActionsThisTurn();
            if (ReplayTurns[CurrentTurn].Actions.Count > actionindex)
                actiontime = ReplayTurns[CurrentTurn].Actions[actionindex].ActionTime - 0.8f;
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (p.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                    p.SetRole(RoleTypes.CrewmateGhost);
            }
        }
        static bool IsStarted;
        public static void AllRoleSet()
        {
            if (ReplayManager.IsReplayMode)
            {
                foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                {
                    RoleTypes role = RoleTypes.CrewmateGhost;
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
            }
        }
        public static void GetPosAndActionsThisTurn()
        {
            ReplayTurn turn = new()
            {
                Positions = new(),
                Actions = new()
            };
            var reader = ReplayReader.reader;
            bool IsPosFloat = ReplayManager.CurrentReplay.IsPosFloat;
            int playercount = reader.ReadInt32();
            for (int i = 0; i < playercount; i++)
            {
                byte playerId = reader.ReadByte();
                turn.Positions.Add(playerId, new());
                int poscount = reader.ReadInt32();
                for (int i2 = 0; i2 < poscount; i2++)
                {
                    if (IsPosFloat)
                    {
                        turn.Positions[playerId].Add(new(reader.ReadSingle(),
                            reader.ReadSingle()));
                    }
                    else
                    {
                        turn.Positions[playerId].Add(new(reader.ReadInt16() / 10.0f,
                            reader.ReadInt16() / 10.0f));
                    }
                }
            }
            int actioncount = reader.ReadInt32();
            for (int i = 0; i < actioncount; i++)
            {
                ReplayActionId replayActionId = (ReplayActionId)reader.ReadByte();
                Logger.Info(replayActionId+"追加");
                ReplayAction action = ReplayAction.CreateReplayAction(replayActionId);
                action.ReadReplayFile(reader);
                turn.Actions.Add(action);
            }
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
            actiontime -= Time.deltaTime;
            if (postime <= 0)
            {
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (player.PlayerId == PlayerControl.LocalPlayer.PlayerId) continue;
                    try
                    {
                        if (ReplayTurns[CurrentTurn].Positions[player.PlayerId].Count <= posindex) continue;
                        Logger.Info(ReplayTurns[CurrentTurn].Positions[player.PlayerId][posindex].ToString());
                        //player.StopAllCoroutines();
                        //player.NetTransform.RpcSnapTo(ReplayTurns[CurrentTurn].Positions[player.PlayerId][posindex]);
                        //if (ReplayTurns[CurrentTurn].Positions[player.PlayerId].Count > posindex + 1)
                        if (ReplayTurns[CurrentTurn].Positions[player.PlayerId].Count > posindex + 1)
                        {
                            player.NetTransform.SnapTo(ReplayTurns[CurrentTurn].Positions[player.PlayerId][posindex + 1]);
                        }
                        player.transform.position = new(ReplayTurns[CurrentTurn].Positions[player.PlayerId][posindex].x,
                            ReplayTurns[CurrentTurn].Positions[player.PlayerId][posindex].y,
                            0f);
                        Logger.Info($"{ReplayTurns[CurrentTurn].Positions[player.PlayerId][posindex + 1]} => {new Vector3(ReplayTurns[CurrentTurn].Positions[player.PlayerId][posindex + 1].x,
                            ReplayTurns[CurrentTurn].Positions[player.PlayerId][posindex + 1].y,
                            0f)}");
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
                postime = ReplayManager.CurrentReplay.RecordRate;
            }
            Logger.Info("actiontime:"+actiontime.ToString());
            if (actiontime <= 0)
            {
                if (ReplayTurns[CurrentTurn].Actions.Count > actionindex)
                {
                    ReplayTurns[CurrentTurn].Actions[actionindex].OnAction();
                    actionindex++;
                    if (ReplayTurns[CurrentTurn].Actions.Count > actionindex)  actiontime = ReplayTurns[CurrentTurn].Actions[actionindex].ActionTime;
                }
            }
        }
    }
}