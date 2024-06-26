using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using SuperNewRoles.Patches;
using SuperNewRoles.Replay.ReplayActions;
using UnityEngine;

namespace SuperNewRoles.Replay
{
    public static class Recorder
    {
        public static float RecordRate;
        public static bool IsPosFloat;
        public static void ClearAndReloads()
        {
            RecordRate = ConfigRoles.ReplayQualityTime.Value;
            IsPosFloat = ConfigRoles.ReplayQualityTime.Value >= 0.75f;
            currenttime = 99999;
            FirstOutfits = new();
            FirstRoles = new();
            writer = null;
            filePath = "";
            PlayerPositions = new();
            ReplayActions = new();
            ReplayActionTime = 0;
            ReplayReader.ClearAndReloads();
            if (!ReplayManager.IsReplayMode)
                ReplayManager.IsRecording = ConfigRoles.ReplayEnable.Value;
        }

        static NetworkedPlayerInfo.PlayerOutfit CopyOutfit(NetworkedPlayerInfo.PlayerOutfit outfit)
        {
            NetworkedPlayerInfo.PlayerOutfit result = new()
            {
                PlayerName = string.Copy(outfit.PlayerName),
                ColorId = outfit.ColorId,
                HatId = outfit.HatId,
                PetId = outfit.PetId,
                SkinId = outfit.SkinId,
                VisorId = outfit.VisorId,
                NamePlateId = outfit.NamePlateId
            };
            return result;
        }

        public static void CoIntroDestroy()
        {
            foreach (NetworkedPlayerInfo player in GameData.Instance.AllPlayers)
            {
                FirstOutfits.Add(player.PlayerId, CopyOutfit(player.DefaultOutfit));
                RoleId role = RoleId.DefaultRole;
                if (player.Object != null)
                    role = player.Object.GetRole();
                FirstRoles.Add(player.PlayerId, role);
            }
            WriteReplayDataFirst();
            ReplayActions = new();
            PlayerPositions = new();
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                PlayerPositions.TryAdd(player.PlayerId, new());
            }
            currenttime = 0;
            ReplayActionTime = 0;
        }
        static float currenttime;
        public static Dictionary<byte, NetworkedPlayerInfo.PlayerOutfit> FirstOutfits;
        public static Dictionary<byte, RoleId> FirstRoles;
        static Dictionary<byte, List<Vector2>> PlayerPositions;
        static List<(byte, byte, float)> MeetingVoteData;
        public static List<ReplayAction> ReplayActions;
        public static float ReplayActionTime;
        public static void HudUpdate()
        {
            currenttime -= Time.deltaTime;
            ReplayActionTime += Time.deltaTime;
            if (currenttime <= 0)
            {
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    PlayerPositions[player.PlayerId].Add(player.transform.localPosition);
                }
                currenttime = RecordRate;
            }
        }
        public static void OnVoteChat(NetworkedPlayerInfo srcPlayer)
        {
        }
        public static void StartMeeting()
        {
            if (ReplayManager.IsReplayMode || !ReplayManager.IsRecording) return;
            AmongUsClient.Instance.StartCoroutine(SavePositions().WrapToIl2Cpp());
        }
        public static IEnumerator SavePositions()
        {
            if (writer == null)
            {
                Logger.Info("writerがnullです", "Recorder:SavePositions");
                yield break;
            }
            if (PlayerPositions == null)
            {
                Logger.Info("PlayerPositionsがnullです", "Recorder:SavePositions");
                yield break;
            }
            writer.Write(PlayerPositions.Count);
            Dictionary<byte, List<Vector2>> playerpositions = new(PlayerPositions);
            PlayerPositions = new();
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                PlayerPositions.TryAdd(player.PlayerId, new());
            }
            foreach (var data in playerpositions)
            {
                int count = 0;
                writer.Write(data.Key);
                writer.Write(data.Value.Count);
                foreach (Vector3 pos in data.Value)
                {
                    if (count > 240)
                    {
                        yield return null;
                    }
                    if (IsPosFloat)
                    {
                        writer.Write(pos.x);
                        writer.Write(pos.y);
                    }
                    else
                    {
                        writer.Write((short)(pos.x * 10));
                        writer.Write((short)(pos.y * 10));
                    }
                    count++;
                }
                yield return null;
            }
            writer.Write(ReplayActions.Count);
            List<ReplayAction> replayactions = new(ReplayActions);
            ReplayActions = new();
            foreach (ReplayAction action in replayactions)
            {
                writer.Write((byte)action.GetActionId());
                action.WriteReplayFile(writer);
            }
            ReplayActionTime = 0f;
            //ゲーム終了かのフラグ
            writer.Write(false);
        }
        public static void OnEndGame(GameOverReason reason)
        {
            Logger.Info("Start-Save-");
            if (ReplayManager.IsReplayMode || !ReplayManager.IsRecording) return;
            Logger.Info("Start-Save-2");
            Logger.Info(writer.BaseStream.Length.ToString());
            Logger.Info(writer.BaseStream.Position.ToString());
            writer.Write(PlayerPositions.Count);
            Dictionary<byte, List<Vector2>> playerpositions = new(PlayerPositions);
            PlayerPositions = new();
            foreach (var data in playerpositions)
            {
                writer.Write(data.Key);
                writer.Write(data.Value.Count);
                foreach (Vector3 pos in data.Value)
                {
                    if (IsPosFloat)
                    {
                        writer.Write(pos.x);
                        writer.Write(pos.y);
                    }
                    else
                    {
                        writer.Write((short)(pos.x * 10));
                        writer.Write((short)(pos.y * 10));
                    }
                }
            }
            Logger.Info(writer.BaseStream.Length.ToString());
            Logger.Info(writer.BaseStream.Position.ToString());
            writer.Write(ReplayActions.Count);
            int index = 0;
            List<ReplayAction> replayactions = new(ReplayActions);
            ReplayActions = new();
            foreach (ReplayAction action in replayactions)
            {
                writer.Write((byte)action.GetActionId());
                Logger.Info(index.ToString() + "開始:" + action.GetActionId().ToString() + ":" + writer.BaseStream.Position.ToString());
                action.WriteReplayFile(writer);
                Logger.Info(index.ToString() + "終了:" + action.GetActionId().ToString() + ":" + writer.BaseStream.Position.ToString());
                index++;
            }
            Logger.Info(writer.BaseStream.Length.ToString());
            Logger.Info(writer.BaseStream.Position.ToString());
            ReplayActions = new();
            ReplayActionTime = 0f;
            //ゲーム終了かのフラグ
            writer.Write(true);
            List<NetworkedPlayerInfo> winners = new();
            foreach (CachedPlayerData winningPlayer in EndGameResult.CachedWinners)
            {
                NetworkedPlayerInfo target = GameData.Instance.AllPlayers.Find((Il2CppSystem.Predicate<NetworkedPlayerInfo>)(x => x.PlayerName == winningPlayer.PlayerName));
                if (target == null) continue;
                winners.Add(target);
            }
            writer.Write(winners.Count);
            foreach (NetworkedPlayerInfo player in winners)
            {
                writer.Write(player.PlayerId);
            }
            writer.Write((byte)reason);
            writer.Write((byte)AdditionalTempData.winCondition);
            Logger.Info(writer.BaseStream.Length.ToString());
            Logger.Info(writer.BaseStream.Position.ToString());
            writer.Close();
            Logger.Info("End-Save-");
        }
        static BinaryWriter writer;
        static string filePath;
        public static void WriteReplayDataFirst()
        {
            (writer, filePath) = ReplayFileWriter.CreateWriter();
            ReplayManager.LastSavedName = filePath;
            ReplayFileWriter.WriteSNRData(writer);
            ReplayFileWriter.WriteGameData(writer);
            ReplayFileWriter.WriteCheckSam(writer);
            ReplayFileWriter.WriteReplayData(writer, RecordRate, IsPosFloat);
            ReplayFileWriter.WriteGameOptionData(writer);
            ReplayFileWriter.WriteCustomOptionData(writer);
            ReplayFileWriter.WritePlayerData(writer, FirstOutfits, FirstRoles);
            ReplayFileWriter.WriteDoorData(writer);
        }
    }
}