using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SuperNewRoles.Replay
{
    public static class Recorder
    {
        public static float RecordRate;
        public static void ClearAndReloads() {
            RecordRate = 0.5f;
            currenttime = 0;
            FirstOutfits = new();
            FirstRoles = new();
            FirstIsImpostor = new();
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
        }
        static float currenttime;
        public static Dictionary<byte, GameData.PlayerOutfit> FirstOutfits;
        public static Dictionary<byte, RoleId> FirstRoles;
        public static Dictionary<byte, bool> FirstIsImpostor;
        public static void HudUpdate() {
            if (currenttime <= 0) {
            }
        }
        public static void OnEndGame() {
            WriteReplayData();
        }
        public static void WriteReplayData() {
            (BinaryWriter writer, string filePath) = ReplayFileWriter.CreateWriter();
            ReplayFileWriter.WriteSNRData(writer);
            ReplayFileWriter.WriteGameData(writer);
            ReplayFileWriter.WriteGameOptionData(writer);
            ReplayFileWriter.WriteCustomOptionData(writer);
            ReplayFileWriter.WritePlayerData(writer, FirstOutfits, FirstRoles, FirstIsImpostor);
            writer.Close();
        }
    }
}
