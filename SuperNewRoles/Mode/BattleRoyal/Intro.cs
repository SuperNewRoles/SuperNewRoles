using UnityEngine;

namespace SuperNewRoles.Mode.BattleRoyal;

class Intro
{
    public static Il2CppSystem.Collections.Generic.List<PlayerControl> ModeHandler()
    {
        Il2CppSystem.Collections.Generic.List<PlayerControl> Teams = new();
        Teams.Add(PlayerControl.LocalPlayer);
        foreach (PlayerControl p in CachedPlayer.AllPlayers.AsSpan())
        {
            if (p.IsImpostor() && p.PlayerId != CachedPlayer.LocalPlayer.PlayerId)
            {
                Teams.Add(p);
            }
        }
        return Teams;
    }
    public static (string, string, Color) IntroHandler(IntroCutscene __instance)
    {
        return (ModTranslation.GetString("BattleRoyalModeName"), "", new Color32(116, 80, 48, byte.MaxValue));
    }

    public static void YouAreHandle(IntroCutscene __instance)
    {
        var data = IntroData.ImpostorIntro;
        __instance.YouAreText.color = data.color;           //あなたのロールは...を役職の色に変更
        __instance.RoleText.color = data.color;             //役職名の色を変更
        __instance.RoleBlurbText.color = data.color;        //イントロの簡易説明の色を変更
        __instance.YouAreText.color = Palette.ImpostorRed;     //あなたのロールは...を役職の色に変更
        __instance.RoleText.color = Palette.ImpostorRed;       //役職名の色を変更
        __instance.RoleBlurbText.color = Palette.ImpostorRed;  //イントロの簡易説明の色を変更
    }
}