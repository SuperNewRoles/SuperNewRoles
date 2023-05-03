using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;
using System.Collections;
using BepInEx.IL2CPP.Utils;
using System.Linq;
using SuperNewRoles.Replay.ReplayActions;

namespace SuperNewRoles.Replay
{
    public static class Recorder
    {
        public static float RecordRate;
        public static bool IsPosFloat;
        public static void ClearAndReloads() {
            RecordRate = 0.5f;
            currenttime = 0;
            FirstOutfits = new();
            FirstRoles = new();
            FirstIsImpostor = new();
            writer = null;
            filePath = "";
            PlayerPositions = new();
            ReplayActions = new();
            ReplayActionTime = 0;
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
                bool IsImpostor = false;
                if (player.Object != null)
                    role = player.Object.GetRole();
                if (player.Role != null)
                    IsImpostor = player.Role.IsImpostor;
                FirstRoles.Add(player.PlayerId, role);
                FirstIsImpostor.Add(player.PlayerId, IsImpostor);
            }
            WriteReplayDataFirst();
            PlayerPositions = new();
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                PlayerPositions.Add(player.PlayerId, new());
            }
        }
        static float currenttime;
        public static Dictionary<byte, GameData.PlayerOutfit> FirstOutfits;
        public static Dictionary<byte, RoleId> FirstRoles;
        public static Dictionary<byte, bool> FirstIsImpostor;
        static Dictionary<byte, List<Vector2>> PlayerPositions;
        static List<(byte, byte, float)> MeetingVoteData;
        public static List<ReplayAction> ReplayActions;
        public static float ReplayActionTime;
        public static void HudUpdate() {
            currenttime += Time.deltaTime;
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
            AmongUsClient.Instance.StartCoroutine(SavePositions());
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
        }
        public static void OnEndGame() {

        }
        static BinaryWriter writer;
        static string filePath;
        public static void WriteReplayDataFirst() {
            (writer, filePath) = ReplayFileWriter.CreateWriter();
            ReplayFileWriter.WriteSNRData(writer);
            ReplayFileWriter.WriteCheckSam(writer);
            ReplayFileWriter.WriteReplayData(writer, RecordRate, IsPosFloat);
            ReplayFileWriter.WriteGameData(writer);
            ReplayFileWriter.WriteGameOptionData(writer);
            ReplayFileWriter.WriteCustomOptionData(writer);
            ReplayFileWriter.WritePlayerData(writer, FirstOutfits, FirstRoles, FirstIsImpostor);
        }
    }
}
