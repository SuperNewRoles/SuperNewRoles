using AmongUs.GameOptions;
using SuperNewRoles.Roles.RoleBases.Interfaces;

namespace SuperNewRoles.Buttons;

public class HauntButtonControl
{
    /// <summary>
    /// 憑依ボタンの有効 / 無効を制御する
    /// </summary>
    /// <param name="haveNotHauntAbility">判定する対象</param>
    public static void HauntButtonSwitch(IHaveHauntAbility haveNotHauntAbility)
    {
        if (CanNotUseHauntAbility(haveNotHauntAbility)) DisableHauntButton(); // 憑依ボタンを非表示にする
        else EnabledHauntButton(); // 条件から外れたら再表示する
    }

    /// <summary>
    /// 憑依ボタンが有効か判定する
    /// </summary>
    /// <param name="haveNotHauntAbility">判定する対象</param>
    /// <returns>true : 有効 / false : 無効</returns>
    private static bool CanNotUseHauntAbility(IHaveHauntAbility haveNotHauntAbility) =>
        haveNotHauntAbility != null &&
        PlayerControl.LocalPlayer.IsDead() &&
        !haveNotHauntAbility.CanUseHauntAbility && // 憑依能力を有さない役職で
        PlayerControl.LocalPlayer.Data.Role.Role is RoleTypes.CrewmateGhost or RoleTypes.ImpostorGhost; // ただのゴーストの場合

    /// <summary>
    /// 憑依ボタンの表示を有効にする。
    /// </summary>
    public static void EnabledHauntButton()
    {
        var hm = FastDestroyableSingleton<HudManager>.Instance;
        if (hm != null && !hm.AbilityButton.gameObject.active) hm.AbilityButton.gameObject.SetActive(true);
    }

    /// <summary>
    /// 憑依ボタンを非表示にする。
    /// </summary>
    public static void DisableHauntButton()
    {
        var hm = FastDestroyableSingleton<HudManager>.Instance;
        if (hm != null && hm.AbilityButton.gameObject.active) hm.AbilityButton.gameObject.SetActive(false);
    }
}