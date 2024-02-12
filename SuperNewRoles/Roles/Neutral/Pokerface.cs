using System.Collections.Generic;
using System.Linq;
using Hazel;
using SuperNewRoles.Helpers;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral;

public static class Pokerface
{
    public static class CustomOptionData
    {
        private static int optionId = 303500;
        public static CustomRoleOption Option;
        public static CustomOption PlayerCount;
        public static CustomOption CanUseVent;
        public static CustomOption WinnerOnlyAlive;

        public static void SetupCustomOptions()
        {
            Option = CustomOption.SetupCustomRoleOption(optionId, true, RoleId.Pokerface); optionId++;
            PlayerCount = CustomOption.Create(optionId, true, CustomOptionType.Neutral, "SettingTeamCountName", CustomOptionHolder.CrewPlayers[0], CustomOptionHolder.CrewPlayers[1], CustomOptionHolder.CrewPlayers[2], CustomOptionHolder.CrewPlayers[3], Option); optionId++;
            CanUseVent = CustomOption.Create(optionId, true, CustomOptionType.Neutral, "JesterIsVentSetting", false, Option); optionId++;
            WinnerOnlyAlive = CustomOption.Create(optionId, true, CustomOptionType.Neutral, "PokerfaceWinnerOnlyAlive", false, Option); optionId++;
        }
    }
    public static void OnAssigned(List<(RoleId, PlayerControl)> Assigned)
    {
        if (Assigned.Count != 3)
            return;
        PlayerControl[] players = new PlayerControl[3];
        for (int i = 0; i < 3; i++)
        {
            players[i] = Assigned[i].Item2;
        }
        RpcSetPokerfaceTeam(players);
    }
    public static void RpcSetPokerfaceTeam(PlayerControl[] players)
    {
        if (players.Length >= 3)
        {
            MessageWriter writer = RPCHelper.StartRPC(CustomRPC.SetPokerfaceTeam);
            writer.Write(players[0]?.PlayerId ?? 255);
            writer.Write(players[1]?.PlayerId ?? 255);
            writer.Write(players[2]?.PlayerId ?? 255);
            writer.EndRPC();
            RPCProcedure.SetPokerfaceTeam(players[0]?.PlayerId ?? 255,
                players[1]?.PlayerId ?? 255, players[2]?.PlayerId ?? 255);
        }
    }

    public class PokerfaceTeam
    {
        public readonly PlayerControl[] TeamPlayers = new PlayerControl[3];
        public readonly byte[] TeamPlayerIds = new byte[3];
        public PokerfaceTeam(PlayerControl player1,PlayerControl player2, PlayerControl player3)
        {
            TeamPlayers[0] = player1;
            TeamPlayers[1] = player2;
            TeamPlayers[2] = player3;
            TeamPlayerIds[0] = player1.PlayerId;
            TeamPlayerIds[1] = player2.PlayerId;
            TeamPlayerIds[2] = player3.PlayerId;
            foreach (byte id in TeamPlayerIds)
            {
                //キャッシュ
                RoleData._cachedPokerfaceTeams.TryAdd(id, this);
            }
        }
        public bool CanWin()
        {
            bool AlivePlayerOnShip = false;
            foreach (PlayerControl player in TeamPlayers)
            {
                //プレイヤーが存在しないならパス
                if (player == null)
                    continue;
                //プレイヤーがポーカーフェイスじゃないならパス
                if (!player.IsRole(RoleId.Pokerface))
                    continue;
                //もう生存者が居るか確認
                if (AlivePlayerOnShip && player.IsAlive())
                    return false;
                //これまでのforeachループ内に生存者がいなかった場合、生存者が居ると設定
                else if (player.IsAlive())
                    AlivePlayerOnShip = true;
            }
            return AlivePlayerOnShip;
        }
    }
    public static PokerfaceTeam GetPokerfaceTeam(byte playerid)
    {
        //キャッシュを参照する
        if (RoleData._cachedPokerfaceTeams.TryGetValue(playerid, out PokerfaceTeam team))
            return team;
        return null;
    }
    public static PokerfaceTeam GetPokerfaceTeam(PlayerControl player)
    {
        if (player == null)
            return null;
        return GetPokerfaceTeam(player.PlayerId);
    }
    internal static class RoleData
    {
        public static List<PlayerControl> Player;
        public static List<PokerfaceTeam> PokerfaceTeams;
        public static Dictionary<byte, PokerfaceTeam> _cachedPokerfaceTeams;
        public static Color32 color = new(114, 209, 107, byte.MaxValue);

        public static void ClearAndReload()
        {
            Player = new();
            PokerfaceTeams = new();
            _cachedPokerfaceTeams = new();
        }
    }


    // ここにコードを書きこんでください
}
