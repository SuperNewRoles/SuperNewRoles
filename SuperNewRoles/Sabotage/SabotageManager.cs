using Hazel;
using SuperNewRoles.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Sabotage
{
    public static class SabotageManager
    {
        public static CustomSabotage thisSabotage;
        public static List<ButtonBehavior> CustomButtons;
        public enum CustomSabotage
        {
            None,
            CognitiveDeficit,
            Blizzard,
        }
        public static bool IsOK(CustomSabotage sabotage)
        {
            if (!Options.SabotageSetting.getBool()) return false;
            switch (sabotage)
            {
                case CustomSabotage.CognitiveDeficit:
                    if (PlayerControl.GameOptions.MapId != 4) return false;
                    else return Options.CognitiveDeficitSetting.getBool();
                case CustomSabotage.Blizzard:
                    if (false) return false;
                    else return Options.BlizzardSetting.getBool();
            }
            return false;
        }
        public static bool IsOKMeeting()
        {
            if (RoleHelpers.IsSabotage()) return false;
            if (thisSabotage == CustomSabotage.None) return true;
            switch (thisSabotage)
            {
                case CustomSabotage.CognitiveDeficit:
                    return CognitiveDeficit.main.IsLocalEnd;
                case CustomSabotage.Blizzard:
                    return Blizzard.main.IsLocalEnd;
            }
            return false;
        }
        public static InfectedOverlay InfectedOverlayInstance;
        public const float SabotageMaxTime = 30f;
        public static void SetSabotage(PlayerControl player,CustomSabotage Sabotage,bool Is)
        {
            switch (Sabotage)
            {
                case CustomSabotage.CognitiveDeficit:
                    if (Is)
                    {
                        CognitiveDeficit.main.StartSabotage();
                    } else
                    {
                        CognitiveDeficit.main.EndSabotage(player);
                    }
                    break;
                case CustomSabotage.Blizzard:
                    if (Is)
                    {
                        Blizzard.main.StartSabotage();
                    }
                    else
                    {
                        Blizzard.main.EndSabotage(player);
                    }
                    break;
            }
        }
        public static void ClearAndReloads()
        {
            SuperNewRolesPlugin.Logger.LogInfo("クリアアンドリロード");
            InfectedOverlayInstance = null;
            thisSabotage = CustomSabotage.None;
            CustomButtons = new List<ButtonBehavior>();
            if (IsOK(CustomSabotage.CognitiveDeficit))
            {
                CognitiveDeficit.main.DefaultDistanceTime = Options.CognitiveDeficitReleaseTimeSetting.getFloat();
                CognitiveDeficit.main.DefaultUpdateTime = Options.CognitiveDeficitOutfitUpdateTimeSetting.getFloat();
                CognitiveDeficit.main.IsAllEndSabotage = Options.CognitiveDeficitIsAllEndSabotageSetting.getBool();
            }
            if (IsOK(CustomSabotage.Blizzard))
            {
                Blizzard.main.BlizzardSlowSpeedmagnification = Options.BlizzardSlowSpeedmagnificationSetting.getFloat();
                if (PlayerControl.GameOptions.MapId == 0)
                {
                    Blizzard.main.BlizzardDuration = Options.BlizzardskeldDurationSetting.getFloat();
                }
                if (PlayerControl.GameOptions.MapId == 1)
                {
                    Blizzard.main.BlizzardDuration = Options.BlizzardmiraDurationSetting.getFloat();
                }
                if (PlayerControl.GameOptions.MapId == 2)
                {
                    Blizzard.main.BlizzardDuration = Options.BlizzardpolusDurationSetting.getFloat();
                }
                if (PlayerControl.GameOptions.MapId == 4)
                {
                    Blizzard.main.BlizzardDuration = Options.BlizzardairshipDurationSetting.getFloat();
                }
                if (PlayerControl.GameOptions.MapId == 5)
                {
                    Blizzard.main.BlizzardDuration = Options.BlizzardagarthaDurationSetting.getFloat();
                }
                Blizzard.main.Timer = 0;
                Blizzard.main.OverlayTimer = DateTime.Now;
                Blizzard.main.ReacTimer = DateTime.Now;
                Blizzard.main.IsOverlay = false;
            }
        }
        public static void Update()
        {
            if (InfectedOverlayInstance != null) {
                float specialActive = ((InfectedOverlayInstance.doors != null && InfectedOverlayInstance.doors.IsActive) ? 1f : InfectedOverlayInstance.SabSystem.PercentCool);
                foreach (ButtonBehavior button in CustomButtons) {
                    button.spriteRenderer.material.SetFloat("_Percent", specialActive);
                }
            }
            switch (thisSabotage)
            {
                case CustomSabotage.CognitiveDeficit:
                    CognitiveDeficit.main.Update();
                    break;
                case CustomSabotage.Blizzard:
                    Blizzard.main.Update();
                    break;
            }
        }
        public static void CustomSabotageRPC(PlayerControl p,CustomSabotage type,bool Is)
        {
            MessageWriter writer = RPCHelper.StartRPC(CustomRPC.CustomRPC.SetCustomSabotage);
            writer.Write(PlayerControl.LocalPlayer.PlayerId);
            writer.Write((byte)type);
            writer.Write(Is);
            writer.EndRPC();
            SetSabotage(p,type,Is);
        }
    }
}
