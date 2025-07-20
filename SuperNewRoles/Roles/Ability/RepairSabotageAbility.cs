using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;
using UnityEngine;

namespace SuperNewRoles.Roles.Ability;

public class RepairSabotageAbility : CustomButtonBase
{
    private readonly float _coolTime;
    private bool _ignoreDead;
    private bool _ignoreMushroomMixup;

    public RepairSabotageAbility(float coolTime, bool ignoreDead = false, bool ignoreMushroomMixup = false)
    {
        _coolTime = coolTime;
        _ignoreDead = ignoreDead;
        _ignoreMushroomMixup = ignoreMushroomMixup;
    }

    public override float DefaultTimer => _coolTime;
    public override Sprite Sprite { get; } = AssetManager.GetAsset<Sprite>("RepairButton.png");
    public override string buttonText => ModTranslation.GetString("RepairButtonText");
    protected override KeyType keytype => KeyType.Ability1;

    public override bool CheckIsAvailable()
    {
        return ModHelpers.IsSabotageAvailable(!_ignoreMushroomMixup);
    }

    public override bool CheckHasButton()
    {
        return Player.AmOwner && (_ignoreDead || Player.IsAlive());
    }

    public override void OnClick()
    {
        this.UseAbilityCount();
        ModHelpers.RpcFixingSabotage();
    }
}