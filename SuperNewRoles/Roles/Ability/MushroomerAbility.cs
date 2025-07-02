using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.CustomObject;
using SuperNewRoles.Modules;
using SuperNewRoles.Events;
using SuperNewRoles.Modules.Events.Bases;
using HarmonyLib;

namespace SuperNewRoles.Roles.Ability;

public class MushroomerAbility : AbilityBase
{
    public List<Mushroom> CreatedMushrooms { get; private set; } = new();
    public List<Mushroom> NextturnActivateMushrooms { get; private set; } = new();

    private readonly float plantCoolTime;
    private readonly int plantCount;
    private readonly float explosionCoolTime;
    private readonly int explosionCount;
    private readonly float explosionRange;
    private readonly float explosionDurationTime;
    private readonly bool hasGasMask;

    // 有効化後、発動するまで見えない
    private readonly bool activeUsedMushroom;

    private PlantButton plantButton;
    private ExplosionButton explosionButton;
    private EventListener<WrapUpEventData> wrapUpEvent;

    public MushroomerAbility(float plantCoolTime, int plantCount, float explosionCoolTime, int explosionCount,
                             float explosionRange, float explosionDurationTime, bool hasGasMask, bool activeUsedMushroom)
    {
        this.plantCoolTime = plantCoolTime;
        this.plantCount = plantCount;
        this.explosionCoolTime = explosionCoolTime;
        this.explosionCount = explosionCount;
        this.explosionRange = explosionRange;
        this.explosionDurationTime = explosionDurationTime;
        this.hasGasMask = hasGasMask;
        this.activeUsedMushroom = activeUsedMushroom;
    }

    public override void AttachToAlls()
    {
        base.AttachToAlls();
        plantButton = new PlantButton(this);
        explosionButton = new ExplosionButton(this);
        Player.AttachAbility(plantButton, new AbilityParentAbility(this));
        Player.AttachAbility(explosionButton, new AbilityParentAbility(this));
        // 事前読み込み
        if (!ModHelpers.IsMap(MapNames.Fungle))
            MapLoader.LoadMap(MapNames.Fungle, (map) => { });
        wrapUpEvent = WrapUpEvent.Instance.AddListener(OnWrapUp);
    }

    public override void DetachToAlls()
    {
        base.DetachToAlls();
        WrapUpEvent.Instance.RemoveListener(wrapUpEvent);
    }

    private void OnWrapUp(WrapUpEventData data)
    {
        foreach (Mushroom mushRoom in NextturnActivateMushrooms)
        {
            SetActiveMushroom(mushRoom, true);
        }
        NextturnActivateMushrooms.Clear();
    }

    // このactiveは実際に使用できるかで。falseの場合は薄く見えるが使用できない。
    private void SetActiveMushroom(Mushroom mushroom, bool active)
    {
        mushroom.gameObject.SetActive(true);
        mushroom.enabled = active;
        mushroom.GetComponent<SpriteRenderer>().color = new(1, 1, 1, active ? 1 : 0.6f);
    }

    [CustomRPC]
    public void RpcPlantMushroom(Vector2 position)
    {
        CustomSpores.AddMushroom(position, (mushRoom) =>
        {
            mushRoom.gameObject.SetActive(false);
            mushRoom.mushroom.enabled = !activeUsedMushroom || Player.AmOwner;
            CreatedMushrooms.Add(mushRoom);
            NextturnActivateMushrooms.Add(mushRoom);

            // インポスターには非有効状態でも見えるように設定
            if (ExPlayerControl.LocalPlayer.IsImpostor())
                SetActiveMushroom(mushRoom, false);
            else
                mushRoom.gameObject.SetActive(false);
        });
    }

    [CustomRPC]
    public void RpcExplodeMushrooms()
    {
        if (ModHelpers.IsMap(MapNames.Fungle))
        {
            var fungleStatus = ShipStatus.Instance.TryCast<FungleShipStatus>();
            if (fungleStatus != null)
            {
                foreach (var mushrooms in fungleStatus.sporeMushrooms)
                {
                    if (mushrooms.Value.isActiveAndEnabled)
                        CustomTriggerSpores(mushrooms.Value);
                }
            }
        }
        else
        {
            foreach (var mushrooms in CustomSpores.mushRooms)
            {
                if (mushrooms.Value.isActiveAndEnabled)
                    CustomTriggerSpores(mushrooms.Value);
            }
        }
    }

    private void CustomTriggerSpores(Mushroom mushroom)
    {
        mushroom.secondsSporeIsActive = explosionDurationTime;
        mushroom.transform.localScale = Vector3.one * explosionRange;
        mushroom.TriggerSpores();
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
    public static class AmongUsClient_CoStartGame_Patch
    {
        public static void Postfix()
        {
            CustomSpores.ClearAndReloads();
        }
    }

    [HarmonyPatch(typeof(Mushroom), nameof(Mushroom.ResetState))]
    public static class MushroomResetStatePatch
    {
        public static void Postfix(Mushroom __instance)
        {
            //デフォルトに戻す
            __instance.transform.localScale = Vector3.one;
            __instance.secondsSporeIsActive = 5f;
            __instance.sporeMask.transform.localScale = new(2.4f, 2.4f, 1.2f);
        }
    }

    public class PlantButton : CustomButtonBase, IAbilityCount
    {
        private readonly MushroomerAbility ability;

        public PlantButton(MushroomerAbility ability)
        {
            this.ability = ability;
            Timer = DefaultTimer;
            Count = ability.plantCount;
        }

        public override float DefaultTimer => ability.plantCoolTime;
        public override string buttonText => ModTranslation.GetString("MushroomerPlantButtonName");
        public override Sprite Sprite => AssetManager.GetAsset<Sprite>("MushroomerPlanetButton.png");
        protected override KeyType keytype => KeyType.Ability1;

        public override bool CheckHasButton()
        {
            return base.CheckHasButton() && (HasCount || ability.NextturnActivateMushrooms.Count > 0);
        }

        public override bool CheckIsAvailable()
        {
            return PlayerControl.LocalPlayer.CanMove && HasCount;
        }

        public override void OnClick()
        {
            if (Count <= 0) return;

            this.UseAbilityCount();

            Vector2 position = PlayerControl.LocalPlayer.transform.position;
            ability.RpcPlantMushroom(position);
        }

        public override ShowTextType showTextType => ShowTextType.ShowWithCount;
    }

    public class ExplosionButton : CustomButtonBase, IAbilityCount
    {
        private readonly MushroomerAbility ability;

        public ExplosionButton(MushroomerAbility ability)
        {
            this.ability = ability;
            Timer = DefaultTimer;
            Count = ability.explosionCount;
        }

        public override float DefaultTimer => ability.explosionCoolTime;
        public override string buttonText => ModTranslation.GetString("MushroomerExplosionButtonName");
        public override Sprite Sprite => AssetManager.GetAsset<Sprite>("MushroomerExplosionButton.png");
        protected override KeyType keytype => KeyType.Ability1;

        public override bool CheckHasButton()
        {
            return base.CheckHasButton() && ability.plantButton.Count <= 0 && ability.NextturnActivateMushrooms.Count <= 0 && HasCount;
        }

        public override bool CheckIsAvailable()
        {
            return PlayerControl.LocalPlayer.CanMove;
        }

        public override void OnClick()
        {
            if (Count <= 0) return;

            ability.RpcExplodeMushrooms();
            this.UseAbilityCount();
        }

        public override ShowTextType showTextType => ShowTextType.ShowWithCount;
    }
}