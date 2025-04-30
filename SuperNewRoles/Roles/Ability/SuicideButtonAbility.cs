using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;
using UnityEngine;

namespace SuperNewRoles.Roles.Ability;

public class SuicideButtonAbility : CustomButtonBase
{
    private readonly float _cooldown;

    public override float DefaultTimer => _cooldown;
    public override string buttonText => ModTranslation.GetString("SuicideButtonText");
    public override Sprite Sprite => _sprite;
    private readonly Sprite _sprite;
    protected override KeyType keytype => KeyType.Ability1; // 古いコードの KeyCode.F に対応
    public override ShowTextType showTextType => ShowTextType.Show; // ボタンテキストを表示

    public SuicideButtonAbility(float cooldown, Sprite sprite)
    {
        _cooldown = cooldown;
        _sprite = sprite;
    }

    public override bool CheckIsAvailable() => PlayerControl.LocalPlayer.CanMove;

    public override void OnClick()
    {
        ExPlayerControl.LocalPlayer.RpcCustomDeath(CustomDeathType.Suicide);
    }
}