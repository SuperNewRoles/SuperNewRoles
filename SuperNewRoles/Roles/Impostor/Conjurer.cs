using System;
using System.Collections.Generic;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Events;
using Hazel;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.CustomObject;
using SuperNewRoles.Ability;

namespace SuperNewRoles.Roles.Impostor;

class Conjurer : RoleBase<Conjurer>
{
    public override RoleId Role => RoleId.Conjurer;
    public override Color32 RoleColor => Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities => [
        () => new ConjurerAbility(new(ConjurerCanAddLength, ConjurerCanKillImpostor, ConjurerShowFlash, ConjurerBeaconCooldown)),
        () => new KillableAbility(() => false)
    ];

    public override QuoteMod QuoteMod => QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType => RoleTypes.Impostor;
    public override short IntroNum => 1;

    public override AssignedTeamType AssignedTeam => AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam => WinnerTeamType.Impostor;
    public override TeamTag TeamTag => TeamTag.Impostor;
    public override RoleTag[] RoleTags => [RoleTag.SpecialKiller, RoleTag.Killer, RoleTag.CustomObject];
    public override RoleOptionMenuType OptionTeam => RoleOptionMenuType.Impostor;

    [CustomOptionFloat("ConjurerCanAddLengthOption", 1f, 40f, 1f, 10f)]
    public static float ConjurerCanAddLength;

    [CustomOptionFloat("ConjurerBeaconCooldownOption", 5f, 60f, 1f, 15f, translationName: "CoolTime")]
    public static float ConjurerBeaconCooldown;

    [CustomOptionBool("ConjurerCanKillImpostorOption", false)]
    public static bool ConjurerCanKillImpostor;

    [CustomOptionBool("ConjurerShowFlashOption", false)]
    public static bool ConjurerShowFlash;
}

public class ConjurerAbility : AbilityBase
{
    public ConjurerAbilityData Data { get; }
    public Vector2[] Positions { get; }
    private ConjurerBeaconButton beaconButton;
    private ConjurerStartButton startButton;
    private EventListener<WrapUpEventData> wrapUpListener;

    public ConjurerAbility(ConjurerAbilityData data)
    {
        Data = data;
        Count = 0;
        Positions = [new(), new(), new()];
    }

    public override void AttachToAlls()
    {
        beaconButton = new ConjurerBeaconButton(this);
        startButton = new ConjurerStartButton(this);

        Player.AttachAbility(beaconButton, new AbilityParentAbility(this));
        Player.AttachAbility(startButton, new AbilityParentAbility(this));

        wrapUpListener = WrapUpEvent.Instance.AddListener(OnWrapUp);
    }

    public override void DetachToLocalPlayer()
    {
        wrapUpListener?.RemoveListener();
        wrapUpListener = null;
    }

    private void OnWrapUp(WrapUpEventData data)
    {
        ConjurerBeacon.ClearBeacons();
        Count = 0;
    }

    public void AddBeacon(Vector3 position)
    {
        Logger.Info($"Now:{Count}", "Conjurer Add");
        new ConjurerBeacon(Player, position);
        Positions[Count] = position;
        Count++;
        Logger.Info($"Now:{Count}", "Conjurer Added");
        beaconButton.ResetTimer();
    }

    public void StartTriangle()
    {
        foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
        {
            // プレイヤーがPositionsで形成された三角形の中にいる
            if (!PointInPolygon(pc.transform.position, Positions))
                continue;
            if (pc.Data.IsDead)
                continue;
            if (!Data.CanKillImpostor && pc.Data.Role.IsImpostor)
                continue;

            // シールダーの場合は特殊対応（保護される）が必要だが、シールダーは現在のコードベースでは存在しないためコメントアウト
            /*
            if (((ExPlayerControl)pc).Role == RoleId.Shielder && RoleClass.Shielder.IsShield.ContainsKey(pc.PlayerId) && RoleClass.Shielder.IsShield[pc.PlayerId])
            {
                MessageWriter msgwriter = RPCHelper.StartRPC(CustomRPC.ShielderProtect);
                msgwriter.Write(Player.PlayerId);
                msgwriter.Write(pc.PlayerId);
                msgwriter.Write(0);
                msgwriter.EndRPC();
                RPCProcedure.ShielderProtect(Player.PlayerId, pc.PlayerId, 0);
                continue;
            }
            */

            ((ExPlayerControl)pc).RpcCustomDeath(CustomDeathType.Suicide);
        }
        if (Data.ShowFlash)
        {
            FlashHandler.RpcShowFlashAll(Palette.Blue, 1f);
        }
        ConjurerBeacon.ClearBeacons();
        startButton.ResetTimer();
        Count = 0;
    }

    /// <summary>
    /// pがpolyから形成された多角形の中にあるか
    /// </summary>
    /// <param name="p">調べたい点</param>
    /// <param name="poly">多角形の頂点</param>
    /// <returns>多角形の中にある</returns>
    private static bool PointInPolygon(Vector2 p, Vector2[] poly)
    {
        Vector2 p1, p2;
        bool inside = false;
        Vector2 oldPoint = poly[^1];
        for (int i = 0; i < poly.Length; i++)
        {
            Vector2 newPoint = poly[i];
            if (newPoint.x > oldPoint.x) { p1 = oldPoint; p2 = newPoint; }
            else { p1 = newPoint; p2 = oldPoint; }
            if ((p1.x < p.x) == (p.x <= p2.x) && (p.y - p1.y) * (p2.x - p1.x) < (p2.y - p1.y) * (p.x - p1.x))
            {
                inside = !inside;
            }
            oldPoint = newPoint;
        }
        return inside;
    }
}

public record ConjurerAbilityData(float CanAddLength, bool CanKillImpostor, bool ShowFlash, float ConjurerBeaconCooldown);

// ビーコンボタン
public class ConjurerBeaconButton : CustomButtonBase
{
    private readonly ConjurerAbility ability;

    public ConjurerBeaconButton(ConjurerAbility ability)
    {
        this.ability = ability;
    }

    public override float DefaultTimer => ability.Data.ConjurerBeaconCooldown;
    public override string buttonText => ModTranslation.GetString("ConjurerBeaconName");
    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("ConjurerBeaconButton.png");
    protected override KeyType keytype => KeyType.Ability2;

    public override bool CheckIsAvailable() => CanAddBeacon();

    private bool CanAddBeacon()
    {
        if (ability.Count <= 0)
            return true;
        if (ability.Count >= 3)
            return false;
        return
            Vector2.Distance(
                PlayerControl.LocalPlayer.transform.position,
                ability.Positions[ability.Count - 1]
            ) < ability.Data.CanAddLength;
    }

    public override void OnClick()
    {
        ability.AddBeacon(PlayerControl.LocalPlayer.transform.position);
    }
}

// 起動ボタン
public class ConjurerStartButton : CustomButtonBase
{
    private readonly ConjurerAbility ability;

    public ConjurerStartButton(ConjurerAbility ability)
    {
        this.ability = ability;
    }

    public override float DefaultTimer => 0;
    public override string buttonText => ModTranslation.GetString("ConjurerAddName");
    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("ConjurerStartButton.png");
    protected override KeyType keytype => KeyType.Ability1;

    public override bool CheckIsAvailable() => ability.Count >= 3;

    public override void OnClick()
    {
        ability.StartTriangle();
    }
}

// ビーコンオブジェクト
public class ConjurerBeacon
{
    private static readonly List<ConjurerBeacon> Beacons = new();
    private static CustomAnimationObject BeaconPrefab;
    private CustomAnimationObject beaconObject;
    private SpriteRenderer spriteRenderer;

    ConjurerBeacon()
    {
        var obj = new GameObject("ConjurerBeacon");
        obj.layer = 5;
        SpriteRenderer spriteRenderer = obj.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = AssetManager.GetAsset<Sprite>("ConjurerBeacon.png");
        BeaconPrefab = obj.AddComponent<CustomAnimationObject>();
        obj.SetActive(false);
    }

    public ConjurerBeacon(ExPlayerControl source, Vector3 position)
    {
        if (BeaconPrefab == null)
            new ConjurerBeacon();

        beaconObject = GameObject.Instantiate(BeaconPrefab);
        beaconObject.gameObject.SetActive(true);
        beaconObject.transform.position = position;
        spriteRenderer = beaconObject.GetComponent<SpriteRenderer>();
        spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
        beaconObject.Init(new CustomAnimationObjectOption(
            CustomPlayerAnimationSimple.GetSprites("Conjurer_Beacon_00{0}.png", 1, 60, 1),
            true,
            30
        ), spriteRenderer);
        Beacons.Add(this);
    }

    public static void ClearBeacons()
    {
        foreach (var beacon in Beacons)
        {
            if (beacon.beaconObject != null)
                GameObject.Destroy(beacon.beaconObject);
        }
        Beacons.Clear();
    }
}