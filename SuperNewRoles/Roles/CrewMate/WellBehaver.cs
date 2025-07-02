using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using SuperNewRoles.CustomObject;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Events;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Ability.CustomButton;
using UnityEngine;

namespace SuperNewRoles.Roles.CrewMate;

class WellBehaver : RoleBase<WellBehaver>
{
    public override RoleId Role => RoleId.WellBehaver;
    public override Color32 RoleColor => new(254, 196, 88, byte.MaxValue);
    public override List<Func<AbilityBase>> Abilities => [() => new WellBehaverAbility(new WellBehaverData(FrequencyGarbageDumping, LimitTrashCount, AllPlayerCanSeeGarbage))];
    public override QuoteMod QuoteMod => QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType => RoleTypes.Crewmate;
    public override short IntroNum => 1;
    public override AssignedTeamType AssignedTeam => AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam => WinnerTeamType.Crewmate;
    public override TeamTag TeamTag => TeamTag.Crewmate;
    public override RoleTag[] RoleTags => [RoleTag.Information];
    public override RoleOptionMenuType OptionTeam => RoleOptionMenuType.Crewmate;

    // ゴミの上限数
    [CustomOptionInt("WellBehaverLimitTrashCountSetting", 15, 60, 1, 15)]
    public static int LimitTrashCount;

    // ゴミ投棄頻度
    [CustomOptionFloat("WellBehaverFrequencyGarbageDumpingSetting", 1f, 30f, 1f, 5f)]
    public static float FrequencyGarbageDumping;

    // 全員がゴミを見れる
    [CustomOptionBool("WellBehaverCanAllPlayerSeeGarbageSetting", true)]
    public static bool AllPlayerCanSeeGarbage;
}

public class WellBehaverButtonAbility : CustomButtonBase
{
    public override float DefaultTimer => 0f;
    public override string buttonText => ModTranslation.GetString("WellBehaverPickUpGarbageButtonName");
    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("WellBehaverButton.png");
    protected override KeyType keytype => KeyType.Ability1;

    public override bool CheckIsAvailable()
    {
        if (!PlayerControl.LocalPlayer.CanMove) return false;
        return Garbage.AllGarbage.Any(x => Vector2.Distance(x.GarbageObject.transform.position, PlayerControl.LocalPlayer.GetTruePosition()) <= Garbage.Distance);
    }

    public override void OnClick()
    {
        Garbage garbage = null;
        float min_distance = float.MaxValue;
        Vector2 truepos = PlayerControl.LocalPlayer.GetTruePosition();
        foreach (Garbage data in Garbage.AllGarbage)
        {
            Vector2 pos = data.GarbageObject.transform.position;
            if (PhysicsHelpers.AnythingBetween(truepos, pos, Constants.ShadowMask, false)) continue;
            float distance = Vector2.Distance(truepos, pos);
            if (distance <= Garbage.Distance && distance < min_distance)
            {
                min_distance = distance;
                garbage = data;
            }
        }
        if (garbage == null) return;
        Logger.Info($"{garbage.GarbageObject.name}が拾われた", "Garbage");
        WellBehaverAbility.RpcDestroyGarbage(ExPlayerControl.LocalPlayer, garbage.MadeBy, garbage.Index);
    }
}

public record WellBehaverData(float FrequencyGarbageDumping, int LimitTrashCount, bool AllPlayerCanSeeGarbage);

public class WellBehaverAbility : AbilityBase
{
    private WellBehaverButtonAbility button;
    private CustomTaskAbility customTaskAbility;
    private EventListener<WrapUpEventData> _wrapUpEventListener;
    private EventListener _fixedUpdateEventListener;
    private EventListener<NameTextUpdateEventData> _nameTextUpdateEventListener;

    private PlayerControl garbager;
    private float timer;
    private int index;
    public WellBehaverData Data { get; }
    public int LimitTrashCount => _limitTrashCount;
    private int _limitTrashCount;

    public WellBehaverAbility(WellBehaverData data)
    {
        timer = 0;
        Data = data;
        // 一旦上限数を無限級にしておく
        _limitTrashCount = int.MaxValue;
    }

    public override void AttachToAlls()
    {
        button = new WellBehaverButtonAbility();
        customTaskAbility = new CustomTaskAbility(() => (false, false, 0));

        Player.AttachAbility(button, new AbilityParentAbility(this));
        Player.AttachAbility(customTaskAbility, new AbilityParentAbility(this));

        new LateTask(() => _limitTrashCount = Data.LimitTrashCount * ExPlayerControl.ExPlayerControls.Count(x => x.Role == RoleId.WellBehaver), 1f);
        ReAssignGarbager();
        _nameTextUpdateEventListener = NameTextUpdateEvent.Instance.AddListener(OnNameTextUpdate);
    }

    public override void DetachToAlls()
    {
        _nameTextUpdateEventListener?.RemoveListener();
    }

    public override void AttachToLocalPlayer()
    {
        _wrapUpEventListener = WrapUpEvent.Instance.AddListener(OnWrapUp);
        _fixedUpdateEventListener = FixedUpdateEvent.Instance.AddListener(OnFixedUpdate);
    }

    public override void DetachToLocalPlayer()
    {
        _wrapUpEventListener?.RemoveListener();
        _fixedUpdateEventListener?.RemoveListener();
    }

    private void OnNameTextUpdate(NameTextUpdateEventData data)
    {
        if (data.Player != Player) return;
        if (data.Player.AmOwner || ExPlayerControl.LocalPlayer.IsDead())
            NameText.SetCustomTaskCount(data.Player, Garbage.AllGarbage.Count, _limitTrashCount, true, true);
    }

    private void OnWrapUp(WrapUpEventData data)
    {
        if (ExPlayerControl.LocalPlayer.IsDead()) return;
        ReAssignGarbager();
        _limitTrashCount = Data.LimitTrashCount * ExPlayerControl.ExPlayerControls.Count(x => x.Role == RoleId.WellBehaver);
    }

    private void OnFixedUpdate()
    {
        if (ExPlayerControl.LocalPlayer.IsDead()) return;
        if (MeetingHud.Instance != null || ExileController.Instance != null || HudManager.Instance.IsIntroDisplayed) return;
        if (garbager == null || garbager.Data.IsDead)
            ReAssignGarbager();
        if (garbager == null) return;
        // スポーン待ちの場合はスキップ
        if (garbager.IsWaitingSpawn()) return;
        timer += Time.fixedDeltaTime;

        // ゴミ投棄頻度に達したらゴミを投げる
        if (timer >= Data.FrequencyGarbageDumping)
        {
            timer = 0;
            RpcCreateGarbage(garbager.transform.position, garbager, index, Data.AllPlayerCanSeeGarbage);
            index++;
        }

        // ゴミの上限数を超えたら自殺
        if (Garbage.AllGarbage.Count >= LimitTrashCount)
            ExPlayerControl.LocalPlayer.RpcCustomDeath(CustomDeathType.Suicide);
    }

    //
    private void ReAssignGarbager()
    {
        garbager = ModHelpers.GetRandom(ExPlayerControl.ExPlayerControls.Where(x => !x.AmOwner && !x.Data.IsDead).ToList());
    }

    [CustomRPC]
    public static void RpcCreateGarbage(Vector2 pos, ExPlayerControl madeBy, int index, bool allPlayerCanSeeGarbage)
    {
        new Garbage(pos, madeBy, index, allPlayerCanSeeGarbage);
        if (ExPlayerControl.LocalPlayer.Role == RoleId.WellBehaver || ExPlayerControl.LocalPlayer.IsDead())
            NameText.UpdateNameInfo(ExPlayerControl.LocalPlayer);
    }

    [CustomRPC]
    public static void RpcDestroyGarbage(ExPlayerControl getter, ExPlayerControl madeBy, int index)
    {
        foreach (Garbage garbage in Garbage.AllGarbage)
        {
            if (garbage.GarbageObject.name == $"Garbage {madeBy.PlayerId} {index}")
            {
                garbage.Clear();
                break;
            }
        }
        if (ExPlayerControl.LocalPlayer.Role == RoleId.WellBehaver || ExPlayerControl.LocalPlayer.IsDead())
            NameText.UpdateNameInfo(ExPlayerControl.LocalPlayer);
    }
}