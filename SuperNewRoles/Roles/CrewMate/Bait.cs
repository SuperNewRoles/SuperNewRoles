using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Hazel;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.SuperTrophies;
using UnityEngine;

namespace SuperNewRoles.Roles.CrewMate;

class Bait : RoleBase<Bait>
{
    public override RoleId Role { get; } = RoleId.Bait;
    //使いまわすことがないならNameKeyは必要なさそう
    //public override string NameKey { get; } = RoleId.Bait.ToString();

    public override Color32 RoleColor { get; } = new(222, 184, 135, byte.MaxValue);
    public override List<Func<AbilityBase>> Abilities { get; } = [() => new BaitAbility()];

    public override QuoteMod QuoteMod { get; } = QuoteMod.TheOtherRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    //TODO:Intro文章の指定だけどこれなんで複数あるの？
    public override short IntroNum { get; } = 1;

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    //これはむしろPlayer毎に設定するべきでは？
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Crewmate;

    public override TeamTag TeamTag { get; } = TeamTag.Crewmate;

    //TODO:要検討
    public override RoleTag[] RoleTags { get; } = [RoleTag.PowerPlayResistance];

    // 通報を遅延させる
    [CustomOptionFloat("BaitReportTime", 0.5f, 10f, 0.1f, 0.5f)]
    public static float BaitReportTime;
    // キラーに警告する
    [CustomOptionBool("BaitWarnKiller", true)]
    public static bool BaitWarnKiller;

    // 通報をランダムに遅延させる
    [CustomOptionBool("BaitRandomDelay", false)]
    public static bool BaitRandomDelay;

    // 遅延のブレ幅
    [CustomOptionInt("BaitDelayVariation", 0, 30, 1, 2, parentFieldName: nameof(BaitRandomDelay))]
    public static int BaitDelayVariation;

    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Crewmate;
}

class BaitAbility : AbilityBase
{
    //何らかの要因で能力を失う時に使うのでListenerは保持しておく
    public EventListener<MurderEventData> killedEventListener;
    private bool _isDetached;

    internal static bool ShouldSkipEventProcessing()
    {
        return ExPlayerControl.LocalPlayer == null
            || ExPlayerControl.LocalPlayer.IsAlive()
            || MeetingHud.Instance != null
            || ExileController.Instance != null;
    }

    public override void AttachToLocalPlayer()
    {
        _isDetached = false;
        //ここでEventListenerと紐付ける
        killedEventListener = MurderEvent.Instance.AddListener(OnKilled);
    }

    public override void DetachToLocalPlayer()
    {
        _isDetached = true;
        base.DetachToLocalPlayer();
        if (killedEventListener != null)
        {
            MurderEvent.Instance.RemoveListener(killedEventListener);
            killedEventListener = null;
        }
    }

    public void OnKilled(MurderEventData data)
    {
        if (_isDetached || ShouldSkipEventProcessing() || data == null || data.target == null || PlayerControl.LocalPlayer == null)
            return;

        if (data.target.PlayerId == PlayerControl.LocalPlayer.PlayerId)
        {
            // キラーに警告する（画面を青く光らせる）
            if (Bait.BaitWarnKiller && data.killer?.Player != null)
            {
                FlashHandler.RpcShowFlash(data.killer.Player, Color.cyan, 0.2f);
            }

            //Reportの遅延呼び出しを行う(多分Coroutineがよいのでは？)
            PlayerControl.LocalPlayer.StartCoroutine(DelayedReport(data).WrapToIl2Cpp());
        }
    }

    IEnumerator DelayedReport(MurderEventData data)
    {
        if (data == null)
            yield break;

        // 最低限の遅延（キラーへの警告が見えるように）
        yield return new WaitForSeconds(0.5f);

        if (_isDetached || ShouldSkipEventProcessing())
            yield break;

        // ランダム遅延が有効な場合
        if (Bait.BaitRandomDelay)
        {
            float minDelay = Mathf.Max(0f, Bait.BaitReportTime - Bait.BaitDelayVariation);
            float maxDelay = Bait.BaitReportTime + Bait.BaitDelayVariation;
            float delay = UnityEngine.Random.Range(minDelay, maxDelay);

            if (delay > 0f)
                yield return new WaitForSeconds(delay);
        }
        // 固定遅延の場合
        else if (Bait.BaitReportTime > 0)
        {
            yield return new WaitForSeconds(Bait.BaitReportTime);
        }

        if (_isDetached
            || ShouldSkipEventProcessing()
            || data.killer?.Player == null
            || data.target == null
            || data.target.Data == null)
            yield break;

        data.killer.RpcCustomReportDeadBody(data.target.Data);
    }
}
