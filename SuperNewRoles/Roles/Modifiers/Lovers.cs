using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;

namespace SuperNewRoles.Roles
{
    [HarmonyPatch]
    public class Lovers : ModifierBase<Lovers>
    {
        public static List<List<PlayerControl>> LoversPlayer;
        public static bool SameDie;
        public static bool AliveTaskCount;
        public static bool IsSingleTeam;
        public static Color32 color = new(255, 105, 180, byte.MaxValue);

        public Lovers()
        {
            ModType = modId = ModifierType.Lovers;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate() { }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason)
        {
            if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started)
            {
                if (player.IsLovers())
                {
                    LoversPlayer.RemoveAll(x => x.TrueForAll(x2 => x2.PlayerId == player.PlayerId));
                    ChacheManager.ResetLoversChache();
                }
                if (player.IsQuarreled() && player.IsAlive())
                {
                    RoleClass.Quarreled.QuarreledPlayer.RemoveAll(x => x.TrueForAll(x2 => x2.PlayerId == player.PlayerId));
                    ChacheManager.ResetQuarreledChache();
                }
                if (ModeHandler.IsMode(ModeId.Default))
                {
                    if (player.IsRole(RoleId.SideKiller))
                    {
                        var sideplayer = RoleClass.SideKiller.GetSidePlayer(PlayerControl.LocalPlayer);
                        if (sideplayer != null)
                        {
                            if (!RoleClass.SideKiller.IsUpMadKiller)
                            {
                                sideplayer.RPCSetRoleUnchecked(RoleTypes.Impostor);
                                RoleClass.SideKiller.IsUpMadKiller = true;
                            }
                        }
                    }
                    else if (player.IsRole(RoleId.MadKiller))
                    {
                        var sideplayer = RoleClass.SideKiller.GetSidePlayer(PlayerControl.LocalPlayer);
                        if (sideplayer != null)
                        {
                            player.RPCSetRoleUnchecked(RoleTypes.Impostor);
                        }
                    }
                }
            }
        }

        public override string ModifyNameText(string nameText)
        {
            return nameText + ModHelpers.Cs(color, " ♥");
        }

        public override void ClearAndReload()
        {
            players = new();

            LoversPlayer = new List<List<PlayerControl>>();
            SameDie = CustomOptionHolder.LoversSameDie.GetBool();
            AliveTaskCount = CustomOptionHolder.LoversAliveTaskCount.GetBool();
            IsSingleTeam = CustomOptionHolder.LoversSingleTeam.GetBool();
        }
    }
}
