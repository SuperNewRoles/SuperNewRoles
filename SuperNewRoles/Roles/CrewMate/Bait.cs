using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Hazel;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles.Ability;
using UnityEngine;
using UnityEngine.Networking.Types;

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

    [CustomOptionFloat("BaitReportTime", 0f, 10f, 0.1f, 0f)]
    public static float BaitReportTime;

    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Crewmate;
}

class BaitAbility : AbilityBase
{

    //何らかの要因で能力を失う時に使うのでListenerは保持しておく
    public EventListener<MurderEventData> killedEventListener;

    public override void AttachToLocalPlayer()
    {
        //ここでEventListenerと紐付ける
        killedEventListener = MurderEvent.Instance.AddListener(OnKilled);
    }

    public override void Detach()
    {
        base.Detach();
        if (killedEventListener != null)
        {
            MurderEvent.Instance.RemoveListener(killedEventListener);
            killedEventListener = null;
        }
    }

    public void OnKilled(MurderEventData data)
    {
        if (data.target == PlayerControl.LocalPlayer)
        {
            //Reportの遅延呼び出しを行う(多分Coroutineがよいのでは？)
            PlayerControl.LocalPlayer.StartCoroutine(DelayedReport().WrapToIl2Cpp());
        }

        IEnumerator DelayedReport()
        {
            if (Bait.BaitReportTime > 0)
                yield return new WaitForSeconds(Bait.BaitReportTime);

            data.killer.RpcCustomReportDeadBody(data.target.Data);
        }
    }
}
