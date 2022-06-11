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
            CognitiveDeficit
        }
        public static bool IsOK(CustomSabotage sabotage)
        {
            if (!Options.SabotageSetting.getBool()) return false;
            switch (sabotage)
            {
                case CustomSabotage.CognitiveDeficit:
                    if (PlayerControl.GameOptions.MapId != 4) return false;
                    else return Options.CognitiveDeficitSetting.getBool();
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
            }
        }
        public static void ClearAndReloads()
        {
            InfectedOverlayInstance = null;
            thisSabotage = CustomSabotage.None;
            CustomButtons = new List<ButtonBehavior>();
            if (IsOK(CustomSabotage.CognitiveDeficit))
            {
                CognitiveDeficit.main.DefaultDistanceTime = Options.CognitiveDeficitReleaseTimeSetting.getFloat();
                CognitiveDeficit.main.DefaultUpdateTime = Options.CognitiveDeficitOutfitUpdateTimeSetting.getFloat();
                CognitiveDeficit.main.IsAllEndSabotage = Options.CognitiveDeficitIsAllEndSabotageSetting.getBool();
            }
        }
        public static void Update()
        {
            if (CustomButtons.Count > 0)
            {
                if (InfectedOverlayInstance != null)
                {
                    float specialActive = ((InfectedOverlayInstance.doors != null && InfectedOverlayInstance.doors.IsActive) ? 1f : InfectedOverlayInstance.SabSystem.PercentCool);
                    foreach (ButtonBehavior button in CustomButtons)
                    {
                        button.spriteRenderer.material.SetFloat("_Percent", specialActive);
                    }
                }
                switch (thisSabotage)
                {
                    case CustomSabotage.CognitiveDeficit:
                        CognitiveDeficit.main.Update();
                        break;
                }
            }
        }
        public static void CustomSabotageRPC(PlayerControl p,CustomSabotage type,bool Is)
        {
            MessageWriter writer = RPCHelper.StartRPC(CustomRPC.CustomRPC.SetCustomSabotage);
            writer.Write(CachedPlayer.LocalPlayer.PlayerId);
            writer.Write((byte)type);
            writer.Write(Is);
            writer.EndRPC();
            SetSabotage(p,type,Is);
        }
    }
}
