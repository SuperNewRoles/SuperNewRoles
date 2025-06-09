using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Impostor;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Events;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.CustomCosmetics.CosmeticsPlayer;

namespace SuperNewRoles.Roles.Ability;

public class CamouflagerAbility : AbilityBase
{
    public float CoolTime;
    public float DurationTime;
    public int CamouflageColor;
    public int ChangeColorType;

    private CamouflageButtonAbility _camouflageButtonAbility;
    private Dictionary<byte, PlayerOutfitData> _originalOutfits = new();
    public bool _isCamouflaged { get; private set; }

    private EventListener<MeetingStartEventData> _meetingStartListener;

    public CamouflagerAbility(CamouflagerAbilityOption option)
    {
        CoolTime = option.CoolTime;
        DurationTime = option.DurationTime;
        CamouflageColor = option.CamouflageColor;
        ChangeColorType = option.ChangeColorType;
    }

    public override void AttachToAlls()
    {
        base.AttachToAlls();

        _camouflageButtonAbility = new CamouflageButtonAbility(CoolTime, DurationTime, this);
        _meetingStartListener = MeetingStartEvent.Instance.AddListener(OnMeetingStart);
        Player.AttachAbility(_camouflageButtonAbility, new AbilityParentAbility(this));
    }

    public void OnMeetingStart(MeetingStartEventData data)
    {
        EndCamouflage();
    }

    public override void DetachToAlls()
    {
        base.DetachToAlls();
        _meetingStartListener?.RemoveListener();
    }

    [CustomRPC]
    public void RpcStartCamouflage()
    {
        StartCamouflage();
    }

    [CustomRPC]
    public void RpcEndCamouflage()
    {
        EndCamouflage();
    }

    private void StartCamouflage()
    {
        if (_isCamouflaged) return;

        _isCamouflaged = true;
        _originalOutfits.Clear();

        // 全プレイヤーの元の外見を保存
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player == null || player.Data.Disconnected) continue;

            _originalOutfits[player.PlayerId] = new PlayerOutfitData
            {
                PlayerName = player.Data.PlayerName,
                ColorId = player.Data.DefaultOutfit.ColorId,
                SkinId = player.Data.DefaultOutfit.SkinId,
                HatId = player.Data.DefaultOutfit.HatId,
                VisorId = player.Data.DefaultOutfit.VisorId,
                PetId = player.Data.DefaultOutfit.PetId
            };
        }

        // カモフラージュを適用
        ApplyCamouflage();
    }

    private void ApplyCamouflage()
    {
        var camouflageOutfit = new NetworkedPlayerInfo.PlayerOutfit
        {
            PlayerName = "　",
            ColorId = GetCamouflageColor(),
            SkinId = "",
            HatId = "",
            VisorId = "",
            PetId = ""
        };

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player == null || player.Data.Disconnected) continue;

            // ランダムカラーの場合は個別に色を設定
            if (ChangeColorType == 2) // Random
            {
                camouflageOutfit.ColorId = GetRandomColorForPlayer(player.PlayerId);
            }

            player.setOutfit(camouflageOutfit);

            CustomCosmeticsLayer layer = CustomCosmeticsLayers.ExistsOrInitialize(player.cosmetics);
            layer.hat2.gameObject.SetActive(false);
            layer.visor2.gameObject.SetActive(false);
        }
    }

    private int GetCamouflageColor()
    {
        return ChangeColorType switch
        {
            0 => 15, // Fixed - Gray
            1 => CamouflageColor, // Select
            2 => 15, // Random - Default Gray (個別に設定される)
            _ => 15
        };
    }

    private int GetRandomColorForPlayer(byte playerId)
    {
        if (!_originalOutfits.ContainsKey(playerId)) return 15;

        var allColors = _originalOutfits.Values.Select(o => o.ColorId).ToList();
        var playerOriginalColor = _originalOutfits[playerId].ColorId;

        // 自分の色以外からランダム選択
        var availableColors = allColors.Where(c => c != playerOriginalColor).ToList();
        if (availableColors.Count == 0) return 15;

        return ModHelpers.GetRandom(availableColors);
    }

    private void EndCamouflage()
    {
        if (!_isCamouflaged) return;

        _isCamouflaged = false;

        // 他のカモフラがいればそのまま
        if (ExPlayerControl.ExPlayerControls.Any(x => x.TryGetAbility<CamouflagerAbility>(out var ability) && ability._isCamouflaged)) return;

        // 元の外見に戻す
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player == null || player.Data.Disconnected) continue;
            if (!_originalOutfits.ContainsKey(player.PlayerId)) continue;

            var originalOutfit = _originalOutfits[player.PlayerId];
            var outfit = new NetworkedPlayerInfo.PlayerOutfit
            {
                PlayerName = originalOutfit.PlayerName,
                ColorId = originalOutfit.ColorId,
                SkinId = originalOutfit.SkinId,
                HatId = originalOutfit.HatId,
                VisorId = originalOutfit.VisorId,
                PetId = originalOutfit.PetId
            };

            player.setOutfit(outfit);

            CustomCosmeticsLayer layer = CustomCosmeticsLayers.ExistsOrInitialize(player.cosmetics);
            layer.hat2.gameObject.SetActive(true);
            layer.visor2.gameObject.SetActive(true);
        }

        _originalOutfits.Clear();
    }

    public class CamouflagerAbilityOption
    {
        public float CoolTime;
        public float DurationTime;
        public int CamouflageColor;
        public int ChangeColorType;

        public CamouflagerAbilityOption(float coolTime, float durationTime, int camouflageColor, int changeColorType)
        {
            CoolTime = coolTime;
            DurationTime = durationTime;
            CamouflageColor = camouflageColor;
            ChangeColorType = changeColorType;
        }
    }

    private class PlayerOutfitData
    {
        public string PlayerName { get; set; }
        public int ColorId { get; set; }
        public string SkinId { get; set; }
        public string HatId { get; set; }
        public string VisorId { get; set; }
        public string PetId { get; set; }
    }
}

public class CamouflageButtonAbility : CustomButtonBase, IButtonEffect
{
    private readonly float _coolTime;
    private readonly float _durationTime;
    private readonly CamouflagerAbility _camouflagerAbility;

    public CamouflageButtonAbility(float coolTime, float durationTime, CamouflagerAbility camouflagerAbility)
    {
        _coolTime = coolTime;
        _durationTime = durationTime;
        _camouflagerAbility = camouflagerAbility;
    }

    public override float DefaultTimer => _coolTime;
    public override string buttonText => ModTranslation.GetString("CamouflagerButtonText");
    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("CamouflagerButton.png");
    protected override KeyType keytype => KeyType.Ability1;

    public bool isEffectActive { get; set; }
    public bool effectCancellable => false;
    public float EffectDuration => _durationTime;
    public float EffectTimer { get; set; }
    public Action OnEffectEnds => () =>
    {
        _camouflagerAbility.RpcEndCamouflage();
    };

    public override bool CheckIsAvailable()
    {
        return PlayerControl.LocalPlayer.CanMove && !isEffectActive;
    }

    public override void OnClick()
    {
        _camouflagerAbility.RpcStartCamouflage();
    }

    public bool IsEffectAvailable()
    {
        return true;
    }
}