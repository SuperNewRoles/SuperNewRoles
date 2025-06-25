using System;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;
using UnityEngine;

namespace SuperNewRoles.Roles.Ability;

public record MagazinerAbilityData(float setKillTime);

public class MagazinerAbility : CustomButtonBase, IAbilityCount
{
    public MagazinerAbilityData Data { get; }
    private bool isConsumeMode = false;

    public override float DefaultTimer => 0f;

    public override string buttonText => isConsumeMode
        ? ModTranslation.GetString("MagazinerGetButtonName")
        : ModTranslation.GetString("MagazinerAddButtonName");

    public override Sprite Sprite => isConsumeMode
        ? AssetManager.GetAsset<Sprite>("MagazinerGetButton.png")
        : AssetManager.GetAsset<Sprite>("MagazinerAddButton.png");

    protected override KeyType keytype => KeyType.Ability1;

    public override ShowTextType showTextType => ShowTextType.ShowWithCount;

    public override string showText => isConsumeMode ? ModTranslation.GetString("RemainingText", Count) : "";

    public MagazinerAbility(MagazinerAbilityData data)
    {
        Data = data;
    }

    public override bool CheckIsAvailable()
    {
        if (isConsumeMode)
        {
            // 弾薬が1以上あり、キルボタンがクールダウン中の場合のみ使用可能
            return Count >= 1 && PlayerControl.LocalPlayer.killTimer >= 0f;
        }
        else
        {
            // キルボタンがクールダウン中でない場合のみ使用可能
            return PlayerControl.LocalPlayer.killTimer <= 0f;
        }
    }

    public override void OnUpdate()
    {
        // モードの切り替え判定
        bool shouldBeConsumeMode = Count >= 1 && PlayerControl.LocalPlayer.killTimer > 0f;
        if (shouldBeConsumeMode != isConsumeMode)
            isConsumeMode = shouldBeConsumeMode;
        base.OnUpdate();
    }

    public override void OnClick()
    {
        if (isConsumeMode)
        {
            UseAmmunition();
        }
        else
        {
            AddAmmunition();
        }
    }

    public void UseAmmunition()
    {
        if (Count > 0)
        {
            this.UseAbilityCount();

            // クールダウンを短縮
            PlayerControl.LocalPlayer.SetKillTimer(Data.setKillTime);
        }
    }

    public void AddAmmunition()
    {
        RpcAddCount();
        // キルクールダウンをリセット
        Player.ResetKillCooldown();
    }

    [CustomRPC]
    public void RpcAddCount()
    {
        Count++;
    }
}