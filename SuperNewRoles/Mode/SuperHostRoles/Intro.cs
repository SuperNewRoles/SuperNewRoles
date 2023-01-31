using System;
using SuperNewRoles.Roles;
using UnityEngine;

namespace SuperNewRoles.Mode.SuperHostRoles;

class Intro
{
    public static Il2CppSystem.Collections.Generic.List<PlayerControl> ModeHandler(IntroCutscene __instance)
    {
        Il2CppSystem.Collections.Generic.List<PlayerControl> Teams = new();
        Teams.Add(PlayerControl.LocalPlayer);
        try
        {
            if (PlayerControl.LocalPlayer.IsCrew())
            {
                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                {
                    if (p.PlayerId != CachedPlayer.LocalPlayer.PlayerId && !p.IsBot())
                    {
                        Teams.Add(p);
                    }
                }
            }
            else if (PlayerControl.LocalPlayer.IsImpostor())
            {
                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                {
                    if ((p.IsImpostor() || p.IsRole(RoleId.Spy)) && p.PlayerId != CachedPlayer.LocalPlayer.PlayerId && !p.IsBot())
                    {
                        Teams.Add(p);
                    }
                }
            }
        }
        catch (Exception e) { SuperNewRolesPlugin.Logger.LogInfo("[SHR:Intro] Intro Error:" + e); }
        return Teams;
    }

    public static void RoleTextHandler(IntroCutscene __instance)
    {
        var myrole = PlayerControl.LocalPlayer.GetRole();
        var data = IntroData.GetIntroData(myrole);

        __instance.YouAreText.color = data.color;           //あなたのロールは...を役職の色に変更
        __instance.RoleText.color = data.color;             //役職名の色を変更
        __instance.RoleBlurbText.color = data.color;        //イントロの簡易説明の色を変更

        if ((myrole == RoleId.DefaultRole && !PlayerControl.LocalPlayer.IsImpostor()) || myrole == RoleId.Bestfalsecharge)
        {
            data = IntroData.CrewmateIntro;
            __instance.YouAreText.color = Palette.CrewmateBlue;     //あなたのロールは...を役職の色に変更
            __instance.RoleText.color = Palette.CrewmateBlue;       //役職名の色を変更
            __instance.RoleBlurbText.color = Palette.CrewmateBlue;  //イントロの簡易説明の色を変更
        }
        else if (myrole is RoleId.DefaultRole)
        {
            data = IntroData.ImpostorIntro;
            __instance.YouAreText.color = Palette.ImpostorRed;     //あなたのロールは...を役職の色に変更
            __instance.RoleText.color = Palette.ImpostorRed;       //役職名の色を変更
            __instance.RoleBlurbText.color = Palette.ImpostorRed;  //イントロの簡易説明の色を変更
        }

        __instance.RoleText.text = data.Name;               //役職名を変更
        __instance.RoleBlurbText.text = data.TitleDesc;     //イントロの簡易説明を変更

        if (PlayerControl.LocalPlayer.IsLovers()) __instance.RoleBlurbText.text += "\n" + ModHelpers.Cs(RoleClass.Lovers.color, string.Format(ModTranslation.GetString("LoversIntro"), PlayerControl.LocalPlayer.GetOneSideLovers()?.GetDefaultName() ?? ""));
        if (PlayerControl.LocalPlayer.IsQuarreled()) __instance.RoleBlurbText.text += "\n" + ModHelpers.Cs(RoleClass.Quarreled.color, string.Format(ModTranslation.GetString("QuarreledIntro"), PlayerControl.LocalPlayer.GetOneSideQuarreled()?.Data?.PlayerName ?? ""));
    }
}