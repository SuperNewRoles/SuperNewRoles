using System;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Roles.Neutral;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules.Events.Bases;
using UnityEngine;
using Hazel;

namespace SuperNewRoles.Roles.Ability;

public class ShermansServantAbility : AbilityBase
{
    public ShermansServantData Data { get; }

    private TransformButton _transformButton;
    private SuicideServantButton _suicideButton;
    private DeadBodyArrowsAbility _deadBodyArrowsAbility;
    private PlayerArrowsAbility _playerArrowsAbility;
    private EventListener _fixedUpdateListener;
    private EventListener<WrapUpEventData> _wrapUpListener;

    // ShermansServant specific variables
    private bool _isTransformed = false;
    private OrientalShamanAbility _orientalShamanAbility = null;

    public ShermansServantAbility(ShermansServantData data)
    {
        Data = data;
    }

    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        _fixedUpdateListener = FixedUpdateEvent.Instance.AddListener(OnFixedUpdate);
    }

    public override void AttachToAlls()
    {
        base.AttachToAlls();
        // 変身ボタンの初期化
        _transformButton = new TransformButton(this);
        // 自殺ボタンの初期化
        _suicideButton = new SuicideServantButton(this);
        // 死体の矢印の初期化
        _deadBodyArrowsAbility = new DeadBodyArrowsAbility(() => true, new Color32(165, 42, 42, 255));
        // プレイヤーの矢印の初期化
        _playerArrowsAbility = new PlayerArrowsAbility(() => _orientalShamanAbility?.Player != null ? [_orientalShamanAbility.Player] : [], (player) => OrientalShaman.Instance.RoleColor);

        Player.AttachAbility(_transformButton, new AbilityParentAbility(this));
        Player.AttachAbility(_suicideButton, new AbilityParentAbility(this));
        Player.AttachAbility(_deadBodyArrowsAbility, new AbilityParentAbility(this));
        Player.AttachAbility(_playerArrowsAbility, new AbilityParentAbility(this));

        _wrapUpListener = WrapUpEvent.Instance.AddListener(OnWrapUp);
    }

    public override void DetachToAlls()
    {
        base.DetachToAlls();
        _wrapUpListener?.RemoveListener();
    }

    private void OnWrapUp(WrapUpEventData data)
    {
        if (_isTransformed)
            EndTransform();
    }
    public override void DetachToLocalPlayer()
    {
        _fixedUpdateListener?.RemoveListener();
    }

    public void SetParent(OrientalShamanAbility orientalShamanAbility)
    {
        _orientalShamanAbility = orientalShamanAbility;
    }

    private void OnFixedUpdate()
    {
        // OrientalShamanが死んだ場合、ShermansServantも死ぬ
        if (Player.IsDead()) return;
        if (_orientalShamanAbility != null && _orientalShamanAbility.Player != null && _orientalShamanAbility.Player.IsAlive()) return;
        Player.RpcCustomDeath(CustomDeathType.Suicide);
    }

    public void StartTransform()
    {
        if (_orientalShamanAbility != null && !_isTransformed)
        {
            _isTransformed = true;
            RpcTransform(true);
        }
    }

    public void EndTransform()
    {
        if (_isTransformed)
        {
            _isTransformed = false;
            RpcTransform(false);
        }
    }

    public void Suicide()
    {
        Player.RpcCustomDeath(CustomDeathType.Suicide);
    }

    [CustomRPC]
    public void RpcTransform(bool transform)
    {
        // 変身処理の実装
        if (transform && _orientalShamanAbility != null)
        {
            // OrientalShamanの外見に変身
            Player.Player.Shapeshift(_orientalShamanAbility.Player, false);
            Logger.Info($"ShermansServant transforms to OrientalShaman");
        }
        else
        {
            // 元の外見に戻る
            Player.Player.Shapeshift(Player.Player, false);
            Logger.Info($"ShermansServant returns to original form");
        }
    }

    // カスタムボタンクラス
    private class TransformButton : CustomButtonBase
    {
        private readonly ShermansServantAbility _ability;

        public TransformButton(ShermansServantAbility ability)
        {
            _ability = ability;
        }

        public override Sprite Sprite => AssetManager.GetAsset<Sprite>("ShermansServantTransformButton.png");
        public override string buttonText => ModTranslation.GetString("ShermansServantTransformButton");
        protected override KeyType keytype => KeyType.Ability1;
        public override float DefaultTimer => _ability.Data.transformCooldown;
        public float FillUpTime => 0f;

        public override void OnClick()
        {
            if (!_ability._isTransformed)
                _ability.StartTransform();
            else
                _ability.EndTransform();
        }

        public override bool CheckIsAvailable()
        {
            return Player.IsAlive() && _ability._orientalShamanAbility != null && _ability._orientalShamanAbility.Player.IsAlive();
        }

        public override bool CheckHasButton()
        {
            return Player.IsAlive();
        }
    }

    private class SuicideServantButton : CustomButtonBase
    {
        private readonly ShermansServantAbility _ability;

        public SuicideServantButton(ShermansServantAbility ability)
        {
            _ability = ability;
        }

        public override Sprite Sprite => AssetManager.GetAsset<Sprite>("ShermansServantSuicideButton.png");
        public override string buttonText => ModTranslation.GetString("ShermansServantSuicideButton");
        protected override KeyType keytype => KeyType.Ability2;
        public override float DefaultTimer => _ability.Data.suicideCooldown;

        public override void OnClick()
        {
            _ability.Suicide();
        }

        public override bool CheckIsAvailable()
        {
            return Player.IsAlive();
        }

        public override bool CheckHasButton()
        {
            return Player.IsAlive();
        }
    }
}