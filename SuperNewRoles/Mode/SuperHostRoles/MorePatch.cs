using HarmonyLib;

using SuperNewRoles.Roles;

namespace SuperNewRoles.Mode.SuperHostRoles
{
    class MorePatch
    {
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CompleteTask))]
        class PlayerControlCompleteTaskPatch
        {
            public static void Postfix(PlayerControl __instance)
            {
                FixedUpdate.SetRoleName(__instance);
            }
        }

        public static bool RepairSystem
        (ShipStatus __instance,
                SystemTypes systemType,
                PlayerControl player,
                byte amount)
        {
            if (systemType == SystemTypes.Sabotage && AmongUsClient.Instance.GameMode != GameModes.FreePlay)
            {
                if ((player.IsRole(RoleId.Jackal) && !RoleClass.Jackal.IsUseSabo) || player.IsRole(RoleId.Demon, RoleId.Arsonist, RoleId.RemoteSheriff, RoleId.Sheriff,
                    RoleId.truelover, RoleId.FalseCharges, RoleId.MadMaker, RoleId.ToiletFan, RoleId.NiceButtoner)
                    || (!RoleClass.Minimalist.UseSabo && player.IsRole(RoleId.Minimalist))
                    || (!RoleClass.Samurai.UseSabo && player.IsRole(RoleId.Samurai))
                    || (!RoleClass.Egoist.UseSabo && player.IsRole(RoleId.Egoist))
                    || (!RoleClass.JackalSeer.IsUseSabo && player.IsRole(RoleId.JackalSeer))) return false;
            }
            if (PlayerControl.LocalPlayer.IsUseVent() && RoleHelpers.IsComms())
            {
                if (BattleRoyal.Main.VentData.ContainsKey(CachedPlayer.LocalPlayer.PlayerId))
                {
                    var data = BattleRoyal.Main.VentData[CachedPlayer.LocalPlayer.PlayerId];
                    if (data != null)
                    {
                        PlayerControl.LocalPlayer.MyPhysics.RpcExitVent((int)data);
                    }
                }
            }
            return true;
        }
        public static void StartMeeting()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            FixedUpdate.SetRoleNames(true);
            new LateTask(() =>
            {
                FixedUpdate.SetDefaultNames();
            }, 5f, "SetMeetingName");
        }
        public static void MeetingEnd()
        {
        }
    }
}