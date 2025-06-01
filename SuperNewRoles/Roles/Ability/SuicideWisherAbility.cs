using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;
using UnityEngine;

namespace SuperNewRoles.Roles.Ability;

public class SuicideWisherAbility : CustomButtonBase
{
    public float CoolTime;
    public SuicideWisherAbility(float coolTime)
    {
        CoolTime = coolTime;
    }
    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("SuicideWisherButton.png");
    public override string buttonText => ModTranslation.GetString("SuicideWisherButtonText");
    protected override KeyType keytype => KeyType.Ability1;
    public override float DefaultTimer => CoolTime;

    public override bool CheckIsAvailable()
    {
        return PlayerControl.LocalPlayer.CanMove;
    }

    public override void OnClick()
    {
        // 自殺処理の実行
        RpcSuicide(ExPlayerControl.LocalPlayer);
    }

    [CustomRPC]
    public static void RpcSuicide(ExPlayerControl player)
    {
        // 自殺処理のRPC
        player.CustomDeath(CustomDeathType.Suicide);
    }
}