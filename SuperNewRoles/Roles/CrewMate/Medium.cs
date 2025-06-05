using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Modules;
using SuperNewRoles.Events;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.CustomCosmetics;

namespace SuperNewRoles.Roles.Crewmate;

class Medium : RoleBase<Medium>
{
    public override RoleId Role { get; } = RoleId.Medium;
    public override Color32 RoleColor { get; } = new(79, 255, 227, 255);
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new MediumSpiritVisionAbility(new MediumSpiritVisionData(
            MediumSpiritVisionCooldown,
            MediumSpiritVisionDuration
        )),
        () => new MediumSpiritTalkAbility(new MediumSpiritTalkData(
            MediumSpiritTalkMaxUses
        ))
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Crewmate;
    public override TeamTag TeamTag { get; } = TeamTag.Crewmate;
    public override RoleTag[] RoleTags { get; } = [RoleTag.Information];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Crewmate;

    [CustomOptionFloat("MediumSpiritVisionCooldown", 2.5f, 60f, 2.5f, 20f)]
    public static float MediumSpiritVisionCooldown;

    [CustomOptionInt("MediumSpiritVisionDuration", 1, 30, 1, 5)]
    public static int MediumSpiritVisionDuration;

    [CustomOptionInt("MediumSpiritTalkMaxUses", 1, 20, 1, 1)]
    public static int MediumSpiritTalkMaxUses;
}

public record MediumSpiritVisionData(float Cooldown, int Duration);
public record MediumSpiritTalkData(int MaxUses);

public class MediumSpiritVisionAbility : CustomButtonBase, IButtonEffect
{
    public MediumSpiritVisionData Data { get; }

    public override float DefaultTimer => Data.Cooldown;
    public override string buttonText => ModTranslation.GetString("MediumSpiritVisionButtonText");
    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("MediumSpiritVisionButton.png") ?? HudManager.Instance.UseButton.graphic.sprite;
    protected override KeyType keytype => KeyType.Ability1;

    public bool isEffectActive { get; set; }
    public Action OnEffectEnds => () => SetSpiritVision(false);
    public float EffectDuration => Data.Duration;
    public float EffectTimer { get; set; }

    private EventListener<MeetingStartEventData> _meetingStartListener;
    private VisibleGhostAbility _visibleGhostAbility;

    private CustomMessage message;

    public MediumSpiritVisionAbility(MediumSpiritVisionData data)
    {
        Data = data;
    }

    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        _meetingStartListener = MeetingStartEvent.Instance.AddListener(OnMeetingStart);
        _visibleGhostAbility = new VisibleGhostAbility(() => isEffectActive);
        Player.AttachAbility(_visibleGhostAbility, new AbilityParentAbility(this));
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        _meetingStartListener?.RemoveListener();
    }

    public override bool CheckIsAvailable()
    {
        return !isEffectActive && ExPlayerControl.LocalPlayer.Player.CanMove;
    }

    public override void OnClick()
    {
        SetSpiritVision(true);
    }

    private void OnMeetingStart(MeetingStartEventData data)
    {
        isEffectActive = false;
    }

    public void SetSpiritVision(bool isActive)
    {
        isEffectActive = isActive;
        if (Player.AmOwner)
        {
            if (isActive)
                message = new CustomMessage(ModTranslation.GetString("MediumSpiritVisionActiveMessage"), EffectDuration);
            else if (message?.text?.gameObject != null)
                GameObject.Destroy(message.text.gameObject);
        }
    }
    public void FinishSpiritForce()
        => EffectTimer = 0.0001f;
}

public class MediumSpiritTalkAbility : TargetCustomButtonBase, IAbilityCount
{
    public MediumSpiritTalkData Data { get; }

    public override Color32 OutlineColor => Medium.Instance.RoleColor;
    public override bool OnlyCrewmates => false;
    public override Func<bool> IsDeadPlayerOnly => () => true;
    public override Func<ExPlayerControl, bool> IsTargetable => (player) => true;
    public override bool CheckIsAvailable() => Target != null && ExPlayerControl.LocalPlayer.Player.CanMove && IsSpiritVisionActive();
    protected override KeyType keytype => KeyType.Ability2;

    public override float DefaultTimer => 0f; // クールダウンなし
    public override bool IsFirstCooldownTenSeconds => false;
    public override string buttonText => ModTranslation.GetString("MediumSpiritTalkButtonText");
    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("MediumSpiritTalkButton.png") ?? HudManager.Instance.UseButton.graphic.sprite;

    public override ShowTextType showTextType => ShowTextType.ShowWithCount;

    public MediumSpiritTalkAbility(MediumSpiritTalkData data)
    {
        Data = data;
        Count = Data.MaxUses;
    }

    public override bool CheckHasButton()
        => base.CheckHasButton() && HasCount && IsSpiritVisionActive();

    private bool IsSpiritVisionActive()
    {
        var spiritVisionAbility = Player.GetAbility<MediumSpiritVisionAbility>();
        return spiritVisionAbility?.isEffectActive == true;
    }

    public override void OnClick()
    {
        this.UseAbilityCount();

        // ランダムで情報を選択（1-4）
        int infoType = UnityEngine.Random.Range(1, 5);
        string message = "";

        // 追放で死亡した場合は特別メッセージ
        if (Target.Data.Disconnected || Target.FinalStatus == FinalStatus.Exiled || !MurderDataManager.TryGetMurderData(Target, out var murderData))
        {
            message = ModTranslation.GetString("MediumSpiritTalkExiledMessage", Target.Data.PlayerName);
        }
        else
        {

            switch (infoType)
            {
                case 1: // キルした役職
                    message = ModTranslation.GetString("MediumSpiritTalkKillerRoleMessage", Target.Data.PlayerName, ModTranslation.GetString(murderData.Killer?.Role.ToString() ?? "no role"));
                    break;

                case 2: // キルした人の色の明暗
                    string colorType = CustomColors.IsLighter(murderData.Killer) ? ModTranslation.GetString("LightColor") : ModTranslation.GetString("DarkColor");
                    message = ModTranslation.GetString("MediumSpiritTalkKillerColorMessage", Target.Data.PlayerName, colorType);
                    break;

                case 3: // 死亡場所
                    string deathLocation = GetDeathLocation(murderData.DeathPosition);
                    message = ModTranslation.GetString("MediumSpiritTalkDeathLocationMessage", Target.Data.PlayerName, deathLocation);
                    break;

                case 4: // 役職
                    string targetRoleName = Target.roleBase != null ? ModTranslation.GetString(Target.roleBase.RoleName) : ModTranslation.GetString(Target.Role.ToString());
                    message = ModTranslation.GetString("MediumSpiritTalkRoleMessage", Target.Data.PlayerName, targetRoleName);
                    break;
            }
        }

        // チャットに送信
        if (!string.IsNullOrEmpty(message))
        {
            HudManager.Instance.Chat.AddChat(Player, message);
        }

        // 霊視を終了
        var spiritVisionAbility = Player.GetAbility<MediumSpiritVisionAbility>();
        spiritVisionAbility?.FinishSpiritForce();
    }

    private static string GetDeathLocation(Vector3 position)
    {
        foreach (var room in ShipStatus.Instance.AllRooms)
        {
            if (ModHelpers.CheckCollision(room.roomArea, position))
                return FastDestroyableSingleton<TranslationController>.Instance.GetString(room.RoomId);
        }
        return ModTranslation.GetString("NoCollision");
    }
}