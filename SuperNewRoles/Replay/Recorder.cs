using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;
using System.Collections;
using System.Linq;
using SuperNewRoles.Replay.ReplayActions;
using System.Collections.Concurrent;
using BepInEx.Unity.IL2CPP.Utils.Collections;

namespace SuperNewRoles.Replay
{
    public static class Recorder
    {
        public static float RecordRate;
        public static bool IsPosFloat;
        public static void ClearAndReloads() {
            RecordRate = 0.25f;
            currenttime = 99999;
            FirstOutfits = new();
            FirstRoles = new();
            writer = null;
            filePath = "";
            PlayerPositions = new();
            ReplayActions = new();
            ReplayActionTime = 0;
            ReplayReader.ClearAndReloads();
        }
        static GameData.PlayerOutfit CopyOutfit(GameData.PlayerOutfit outfit) {
            GameData.PlayerOutfit result = new()
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
        public static void CoIntroStart() {
            foreach (GameData.PlayerInfo player in GameData.Instance.AllPlayers) {
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
        public static Dictionary<byte, GameData.PlayerOutfit> FirstOutfits;
        public static Dictionary<byte, RoleId> FirstRoles;
        static ConcurrentDictionary<byte, ConcurrentBag<Vector2>> PlayerPositions;
        static List<(byte, byte, float)> MeetingVoteData;
        public static ConcurrentBag<ReplayAction> ReplayActions;
        public static float ReplayActionTime;
        public static void HudUpdate() {
            currenttime -= Time.deltaTime;
            ReplayActionTime += Time.deltaTime;
            if (currenttime <= 0) {
                foreach (PlayerControl player in PlayerControl.AllPlayerControls) {
                    PlayerPositions[player.PlayerId].Add(player.transform.localPosition);
                }
                currenttime = RecordRate;
            }
        }
        public static void OnVoteChat(GameData.PlayerInfo srcPlayer) {
        }
        public static void StartMeeting()
        {
            if (ReplayManager.IsReplayMode) return;
            AmongUsClient.Instance.StartCoroutine(SavePositions().WrapToIl2Cpp());
        }
        public static IEnumerator SavePositions() {
            if (writer == null) {
                Logger.Info("writerがnullです","Recorder:SavePositions");
                yield break;
            }
            if (PlayerPositions == null)
            {
                Logger.Info("PlayerPositionsがnullです", "Recorder:SavePositions");
                yield break;
            }
            writer.Write(PlayerPositions.Count);
            foreach (var data in PlayerPositions) {
                int count = 0;
                writer.Write(data.Key);
                writer.Write(data.Value.Count);
                foreach (Vector3 pos in data.Value) {
                    if (count > 240) {
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
            foreach (ReplayAction action in ReplayActions)
            {
                writer.Write((byte)action.GetActionId());
                action.WriteReplayFile(writer);
            }
            ReplayActions = new();
            PlayerPositions = new();
            ReplayActionTime = 0f;
            //ゲーム終了かのフラグ
            writer.Write(false);
        }
        public static void OnEndGame() {
            if (ReplayManager.IsReplayMode) return;
            writer.Write(PlayerPositions.Count);
            foreach (var data in PlayerPositions)
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
            writer.Write(ReplayActions.Count);
            int index = 0;
            foreach (ReplayAction action in ReplayActions)
            {
                Logger.Info(index.ToString()+"開始:"+action.GetActionId().ToString()+":"+writer.BaseStream.Position.ToString());
                writer.Write((byte)action.GetActionId());
                action.WriteReplayFile(writer);
                Logger.Info(index.ToString()+"終了:" + action.GetActionId().ToString() + ":" + writer.BaseStream.Position.ToString());
                index++;
            }
            ReplayActions = new();
            ReplayActionTime = 0f;
            //ゲーム終了かのフラグ
            writer.Write(true);
            List<GameData.PlayerInfo> winners = new();
            foreach (WinningPlayerData winningPlayer in TempData.winners) {
                GameData.PlayerInfo target = GameData.Instance.AllPlayers.Find((Il2CppSystem.Predicate<GameData.PlayerInfo>)(x => x.PlayerName == winningPlayer.PlayerName));
                if (target == null) continue;
                winners.Add(target);
            }
            writer.Write(winners.Count);
            foreach (GameData.PlayerInfo player in winners) {
                writer.Write(player.PlayerId);
            }
            writer.Close();
        }
        static BinaryWriter writer;
        static string filePath;
        public static void WriteReplayDataFirst() {
            (writer, filePath) = ReplayFileWriter.CreateWriter();
            ReplayFileWriter.WriteSNRData(writer);
            ReplayFileWriter.WriteGameData(writer);
            ReplayFileWriter.WriteCheckSam(writer);
            ReplayFileWriter.WriteReplayData(writer, RecordRate, IsPosFloat);
            ReplayFileWriter.WriteGameOptionData(writer);
            ReplayFileWriter.WriteCustomOptionData(writer);
            ReplayFileWriter.WritePlayerData(writer, FirstOutfits, FirstRoles);
        }
    }
}
