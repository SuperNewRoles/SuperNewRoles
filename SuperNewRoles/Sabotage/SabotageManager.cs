using System.Collections.Generic;
using System.Linq;
using Hazel;
using SuperNewRoles.Helpers;
using SuperNewRoles.MapCustoms;
using SuperNewRoles.WaveCannonObj;

namespace SuperNewRoles.Sabotage;

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
        return Options.SabotageSetting.GetBool() && sabotage switch
        {
            CustomSabotage.CognitiveDeficit => GameManager.Instance.LogicOptions.currentGameOptions.MapId == 4 && Options.CognitiveDeficitSetting.GetBool(),
            _ => false,
        };
    }
    public static bool IsOKMeeting()
    {
        if (RoleHelpers.IsSabotage(false))
            return false;
        if (MapCustomHandler.IsMapCustom(MapCustomHandler.MapCustomId.TheFungle, false) &&
            MapCustom.TheFungleMushroomMixupOption.GetBool() &&
            MapCustom.TheFungleMushroomMixupCantOpenMeeting.GetBool() &&
            PlayerControl.LocalPlayer.IsMushroomMixupActive())
            return false;
        if (thisSabotage != CustomSabotage.None &&
            !(thisSabotage switch
            {
                CustomSabotage.CognitiveDeficit => CognitiveDeficit.Main.IsLocalEnd,
                _ => false,
            }))
            return false;
        return true;
    }
    public static InfectedOverlay InfectedOverlayInstance;
    public const float SabotageMaxTime = 30f;
    public static void SetSabotage(PlayerControl player, CustomSabotage Sabotage, bool Is)
    {
        switch (Sabotage)
        {
            case CustomSabotage.CognitiveDeficit:
                if (Is)
                {
                    CognitiveDeficit.Main.StartSabotage();
                }
                else
                {
                    CognitiveDeficit.Main.EndSabotage(player);
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
            CognitiveDeficit.Main.DefaultDistanceTime = Options.CognitiveDeficitReleaseTimeSetting.GetFloat();
            CognitiveDeficit.Main.DefaultUpdateTime = Options.CognitiveDeficitOutfitUpdateTimeSetting.GetFloat();
            CognitiveDeficit.Main.IsAllEndSabotage = Options.CognitiveDeficitIsAllEndSabotageSetting.GetBool();
        }
    }
    public static void Update()
    {
        if (CustomButtons.Count > 0)
        {
            if (InfectedOverlayInstance != null)
            {
                float specialActive = (InfectedOverlayInstance.doors != null && InfectedOverlayInstance.doors.IsActive) ? 1f : InfectedOverlayInstance.sabSystem.PercentCool;
                foreach (ButtonBehavior button in CustomButtons.AsSpan())
                {
                    button.spriteRenderer.material.SetFloat("_Percent", specialActive);
                }
            }
        }
        switch (thisSabotage)
        {
            case CustomSabotage.CognitiveDeficit:
                CognitiveDeficit.Main.Update();
                break;
        }
    }
    public static void CustomSabotageRPC(PlayerControl p, CustomSabotage type, bool Is)
    {
        MessageWriter writer = RPCHelper.StartRPC(CustomRPC.SetCustomSabotage);
        writer.Write(CachedPlayer.LocalPlayer.PlayerId);
        writer.Write((byte)type);
        writer.Write(Is);
        writer.EndRPC();
        SetSabotage(p, type, Is);
    }
}