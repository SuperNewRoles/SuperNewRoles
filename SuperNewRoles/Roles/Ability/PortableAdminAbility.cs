using System;
using System.Linq;
using HarmonyLib;
using SuperNewRoles.Modules;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles.Ability.CustomButton;
using UnityEngine;

namespace SuperNewRoles.Roles.Ability;

public record PortableAdminData(Func<bool> CanUseAdmin, Func<bool> canUseAdminDuringComms = null, Func<bool> canMoveWhileUsingAdmin = null, Func<bool> coundRemainingTime = null)
{
    public bool CanUseAdminDuringComms => canUseAdminDuringComms?.Invoke() ?? true;
    public bool CanMoveWhileUsingAdmin => canMoveWhileUsingAdmin?.Invoke() ?? true;
    public bool CountRemainingTime => coundRemainingTime?.Invoke() ?? false;
}
public class PortableAdminAbility : CustomButtonBase
{
    private readonly PortableAdminData Data;
    public PortableAdminAbility(PortableAdminData data)
    {
        this.Data = data;
    }

    public override float DefaultTimer => 0.01f;

    public override string buttonText => FastDestroyableSingleton<TranslationController>.Instance.GetString(StringNames.Admin);

    public override Sprite Sprite => GetAdminButtonSprite();

    public override bool IsFirstCooldownTenSeconds => false;

    protected override KeyType keytype => KeyType.Ability1;

    public override bool CheckHasButton()
    {
        return base.CheckHasButton() && Data.CanUseAdmin();
    }

    public override bool CheckIsAvailable()
    {
        return PlayerControl.LocalPlayer.CanMove && (Data.CanUseAdminDuringComms || !ModHelpers.IsComms());
    }

    public override void OnClick()
    {
        // IsMyAdmin = true;
        if (!Data.CanMoveWhileUsingAdmin)
            ExPlayerControl.LocalPlayer.NetTransform.Halt();

        DevicesPatch.DontCountBecausePortableAdmin = !Data.CountRemainingTime;

        FastDestroyableSingleton<HudManager>.Instance.ToggleMapVisible(new MapOptions()
        {
            Mode = MapOptions.Modes.CountOverlay,
            AllowMovementWhileMapOpen = Data.CanMoveWhileUsingAdmin
        });
    }

    private Sprite GetAdminButtonSprite()
    {
        MapNames mapId = (MapNames)GameOptionsManager.Instance.CurrentGameOptions.MapId;
        UseButtonSettings button = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.PolusAdminButton]; // Polus
        if (mapId is MapNames.Skeld or MapNames.Dleks) button = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.AdminMapButton]; // Skeld || Dleks
        else if (mapId is MapNames.MiraHQ) button = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.MIRAAdminButton]; // Mira HQ
        else if (mapId == MapNames.Airship) button = FastDestroyableSingleton<HudManager>.Instance.UseButton.fastUseSettings[ImageNames.AirshipAdminButton]; // Airship
        return button.Image;
    }
}