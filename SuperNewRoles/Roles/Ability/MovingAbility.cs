using System;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;
using UnityEngine;

namespace SuperNewRoles.Roles.Ability;

public record MovingAbilityData(float cooldown);

internal class MovingAbility : CustomButtonBase
{
    public override float DefaultTimer => Data.cooldown;
    public override string buttonText => ModTranslation.GetString("MovingAbilityButtonText");
    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("MovingLocationSetButton.png");
    protected override KeyType keytype => KeyType.Ability1;

    public MovingAbilityData Data { get; }
    private Vector3? setPosition = null;
    private bool isPositionSet = false;

    public MovingAbility(MovingAbilityData data)
    {
        Data = data;
    }

    public override bool CheckIsAvailable()
    {
        return PlayerControl.LocalPlayer.CanMove;
    }

    public override void OnClick()
    {
        if (!isPositionSet)
        {
            // 位置を設定
            setPosition = PlayerControl.LocalPlayer.transform.position;
            isPositionSet = true;

            // メッセージを表示
            new CustomMessage(ModTranslation.GetString("MovingPositionSetText"), 2f, true);
        }
        else
        {
            // テレポート実行
            if (setPosition.HasValue)
            {
                RpcTeleportToPosition(setPosition.Value);
                // 状態をリセット
                isPositionSet = false;
                setPosition = null;
            }
        }
    }

    [CustomRPC]
    public void RpcTeleportToPosition(Vector3 position)
    {
        Player.NetTransform.SnapTo(position);
        Player.RpcCustomSnapTo(position);
    }
}