using HarmonyLib;
using SuperNewRoles.Mode;
using UnityEngine;

//TOHより!
namespace SuperNewRoles.Patches
{
    [HarmonyPatch(typeof(GameOptionsData), nameof(GameOptionsData.SetRecommendations))]
    public static class ChangeRecommendedSettingPatch
    {
        public static bool Prefix(GameOptionsData __instance, int numPlayers, GameModes modes)
        {
            //通常モードとSHRモード
            numPlayers = Mathf.Clamp(numPlayers, 4, 15);
            __instance.PlayerSpeedMod = __instance.MapId == 4 ? 1.25f : 1f; //AirShipなら1.25、それ以外は1
            __instance.CrewLightMod = 0.5f;
            __instance.ImpostorLightMod = 1.75f;
            __instance.KillCooldown = 30;
            __instance.NumCommonTasks = 2;
            __instance.NumLongTasks = 3;
            __instance.NumShortTasks = 5;
            __instance.NumEmergencyMeetings = 1;
            __instance.TaskBarMode = TaskBarMode.Invisible;
            if (modes != GameModes.OnlineGame)
                __instance.NumImpostors = GameOptionsData.RecommendedImpostors[numPlayers];
            __instance.KillDistance = 0;
            __instance.DiscussionTime = 0;
            __instance.VotingTime = 150;
            __instance.isDefaults = true;
            __instance.ConfirmImpostor = false;
            __instance.VisualTasks = false;
            __instance.EmergencyCooldown = (int)__instance.killCooldown - 15; //キルクールより15秒短く
            __instance.RoleOptions.ShapeshifterCooldown = 10f;
            __instance.RoleOptions.ShapeshifterDuration = 30f;
            __instance.RoleOptions.ShapeshifterLeaveSkin = false;
            __instance.RoleOptions.ImpostorsCanSeeProtect = false;
            __instance.RoleOptions.ScientistCooldown = 15f;
            __instance.RoleOptions.ScientistBatteryCharge = 5f;
            __instance.RoleOptions.GuardianAngelCooldown = 60f;
            __instance.RoleOptions.ProtectionDurationSeconds = 10f;
            __instance.RoleOptions.EngineerCooldown = 30f;
            __instance.RoleOptions.EngineerInVentMaxTime = 15f;
            switch (ModeHandler.GetMode(false))
            {
                //ハイドアンドシーク
                case ModeId.HideAndSeek:
                    __instance.PlayerSpeedMod = 1.75f;
                    __instance.CrewLightMod = 5f;
                    __instance.ImpostorLightMod = 0.25f;
                    __instance.NumImpostors = 1;
                    __instance.NumCommonTasks = 0;
                    __instance.NumLongTasks = 0;
                    __instance.NumShortTasks = 6;
                    __instance.KillCooldown = 10f;
                    __instance.TaskBarMode = 0;
                    break;
                //バトルロイヤル
                case ModeId.BattleRoyal:
                    __instance.PlayerSpeedMod = 1.75f;
                    __instance.ImpostorLightMod = 2f;
                    __instance.KillCooldown = 1f;
                    __instance.TaskBarMode = TaskBarMode.Invisible;
                    break;
                //ゾンビモード
                case ModeId.Zombie:
                    __instance.PlayerSpeedMod = 1.5f;
                    __instance.CrewLightMod = 1.5f;
                    __instance.ImpostorLightMod = 0.25f;
                    __instance.NumImpostors = 1;
                    __instance.NumCommonTasks = 0;
                    __instance.NumLongTasks = 0;
                    __instance.NumShortTasks = 6;
                    __instance.TaskBarMode = 0;
                    break;
                //ケイドロモード
                case ModeId.CopsRobbers:
                    __instance.PlayerSpeedMod = 1.5f;
                    __instance.CrewLightMod = 2.0f;
                    __instance.ImpostorLightMod = 1.5f;
                    __instance.NumCommonTasks = 0;
                    __instance.NumLongTasks = 0;
                    __instance.NumShortTasks = 6;
                    __instance.TaskBarMode = 0;
                    break;
            }
            return false;
        }
    }
}