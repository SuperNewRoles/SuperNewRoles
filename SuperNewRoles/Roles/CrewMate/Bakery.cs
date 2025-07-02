using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using TMPro;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Crewmate;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Events;
using System.Linq;

namespace SuperNewRoles.Roles.Crewmate;

class Bakery : RoleBase<Bakery>
{
    public override RoleId Role { get; } = RoleId.Bakery;
    public override Color32 RoleColor { get; } = new(0, 255, 0, byte.MaxValue);
    public override List<Func<AbilityBase>> Abilities { get; } = [() => new BakeryAbility()];
    public override QuoteMod QuoteMod { get; } = QuoteMod.ExtremeRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Crewmate;
    public override short IntroNum { get; } = 1;
    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Crewmate;
    public override TeamTag TeamTag { get; } = TeamTag.Crewmate;
    public override RoleTag[] RoleTags { get; } = [];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Crewmate;
}

public class BakeryAbility : AbilityBase
{
    private static TextMeshPro confirmImpostorSecondText;
    private EventListener<ExileControllerEventData> _exileControllerEventListener;
    private string ExileText
    {
        get
        {
            var rand = new System.Random();
            return rand.Next(1, 10) == 1 ? ModTranslation.GetString("BakeryExileText2") : ModTranslation.GetString("BakeryExileText");
        }
    }
    public override void AttachToAlls()
    {
        base.AttachToAlls();
        _exileControllerEventListener = ExileControllerEvent.Instance.AddListener(OnExileControllerEvent);
    }
    public override void DetachToAlls()
    {
        _exileControllerEventListener.RemoveListener();
        base.DetachToAlls();
    }

    private void OnExileControllerEvent(ExileControllerEventData data)
    {
        if (confirmImpostorSecondText != null)
            return;
        confirmImpostorSecondText = GameObject.Instantiate(data.instance.ImpostorText, data.instance.Text.transform);
        StringBuilder changeStringBuilder = new();
        bool isUseConfirmImpostorSecondText = false;

        bool isBakeryAlive = ExPlayerControl.ExPlayerControls.Any(x => x.Role == RoleId.Bakery && x.IsAlive());

        if (isBakeryAlive)
        {
            Logger.Info("パン屋がパンを焼きました", "ConfirmImpostorSecondText");
            isUseConfirmImpostorSecondText = true;
            changeStringBuilder.AppendLine(ExileText);
        }

        if (isUseConfirmImpostorSecondText)
        {
            if (GameManager.Instance.LogicOptions.currentGameOptions.GetBool(BoolOptionNames.ConfirmImpostor))
                confirmImpostorSecondText.transform.localPosition += new Vector3(0f, -0.4f, 0f);
            else
                confirmImpostorSecondText.transform.localPosition += new Vector3(0f, -0.2f, 0f);

            confirmImpostorSecondText.text = changeStringBuilder.ToString();
            confirmImpostorSecondText.gameObject.SetActive(true);
        }
    }
}