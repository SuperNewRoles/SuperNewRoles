using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Events;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.Ability;
using UnityEngine;

namespace SuperNewRoles.Roles.Crewmate;

// エラーの原因となっていたクラス。プロパティを確実に定義します。
public class ClinicalLaboratoryTechnicianData
{
    public float CoolTime { get; set; }
    public int MaxUses { get; set; }
    public bool JudgeMadByWinner { get; set; }
    public bool JudgeNeutralAsSame { get; set; }

    public ClinicalLaboratoryTechnicianData(float coolTime, int maxUses, bool judgeMad, bool judgeNeutral)
    {
        CoolTime = coolTime;
        MaxUses = maxUses;
        JudgeMadByWinner = judgeMad;
        JudgeNeutralAsSame = judgeNeutral;
    }
}

class ClinicalLaboratoryTechnician : RoleBase<ClinicalLaboratoryTechnician>
{
    public override RoleId Role { get; } = RoleId.ClinicalLaboratoryTechnician;
    public override Color32 RoleColor { get; } = new(41, 158, 149, 255); // #299E95

    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new ClinicalLaboratoryTechnicianAbility(new ClinicalLaboratoryTechnicianData(
            ClinicalLaboratoryTechnicianCoolTime,
            ClinicalLaboratoryTechnicianMaxUses,
            ClinicalLaboratoryTechnicianJudgeMadByWinner,
            ClinicalLaboratoryTechnicianJudgeNeutralAsSame
        ))
    ];

    public override QuoteMod QuoteMod => QuoteMod.SuperNewRoles;
    public override AssignedTeamType AssignedTeam => AssignedTeamType.Crewmate;
    public override WinnerTeamType WinnerTeam => WinnerTeamType.Crewmate;
    public override TeamTag TeamTag => TeamTag.Crewmate;
    public override RoleTag[] RoleTags => [RoleTag.Information];
    public override short IntroNum => 1;
    public override RoleTypes IntroSoundType => RoleTypes.Crewmate;
    public override RoleOptionMenuType OptionTeam => RoleOptionMenuType.Crewmate;

    [CustomOptionFloat("ClinicalLab_CoolTime", 0f, 180f, 2.5f, 20f, translationName: "ClinicalLab_CoolTime")]
    public static float ClinicalLaboratoryTechnicianCoolTime;

    [CustomOptionInt("ClinicalLab_MaxUses", 1, 10, 1, 3, translationName: "ClinicalLab_MaxUses")]
    public static int ClinicalLaboratoryTechnicianMaxUses;

    [CustomOptionBool("ClinicalLab_JudgeMadByWinner", true, translationName: "ClinicalLab_JudgeMadByWinner")]
    public static bool ClinicalLaboratoryTechnicianJudgeMadByWinner;

    [CustomOptionBool("ClinicalLab_JudgeNeutralAsSame", false, translationName: "ClinicalLab_JudgeNeutralAsSame")]
    public static bool ClinicalLaboratoryTechnicianJudgeNeutralAsSame;
}

public class ClinicalLaboratoryTechnicianAbility : AbilityBase
{
    public ClinicalLaboratoryTechnicianData Data { get; }
    public byte Sample1 { get; set; } = 255;
    public byte Sample2 { get; set; } = 255;
    public int RemainingUses { get; private set; }
    public readonly List<byte> _markedPlayers = new();

    private GetSampleAbility _getSampleAbility;
    private EventListener<MeetingStartEventData> _meetingStartListener;

    public ClinicalLaboratoryTechnicianAbility(ClinicalLaboratoryTechnicianData data)
    {
        Data = data;
        RemainingUses = data.MaxUses;
    }

    public void SetSamples(byte s1, byte s2)
    {
        Sample1 = s1;
        Sample2 = s2;
        RemainingUses--;
    }

    public void UpdateMark(byte playerId)
    {
        if (playerId == 255) return;
        if (!_markedPlayers.Contains(playerId)) _markedPlayers.Add(playerId);

        if (Player.AmOwner)
        {
            var target = ExPlayerControl.ExPlayerControls.FirstOrDefault(x => x.PlayerId == playerId);
            if (target != null)
            {
                ApplyMark(target);
            }
        }
    }

    // 名前を書き換える本体
    private void ApplyMark(ExPlayerControl target)
    {
        string markTag = " <color=#62110D>Ⓒ</color>";
        if (!target.Data.PlayerName.Contains(markTag))
        {
            target.Data.PlayerName += markTag;

            // cosmetics.nameText.text を書き換えることで即時反映
            if (target.Player?.cosmetics?.nameText != null)
            {
                target.Player.cosmetics.nameText.text = target.Data.PlayerName;
            }
            NameText.UpdateNameInfo(target);
        }
    }

    public override void AttachToAlls()
    {
        base.AttachToAlls();
        _getSampleAbility = new GetSampleAbility(
            Data.CoolTime,
            ModTranslation.GetString("ClinicalLab_Btn")
        );
        Player.AttachAbility(_getSampleAbility, new AbilityParentAbility(this));
    }

    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        _meetingStartListener = MeetingStartEvent.Instance.AddListener(OnMeetingStart);
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        _meetingStartListener?.RemoveListener();
    }

    private void OnMeetingStart(MeetingStartEventData data)
    {
        if (Sample1 == 255 || Sample2 == 255)
        {
            Sample1 = 255;
            Sample2 = 255;
            ClearAllMarks();
            return;
        }

        // 会議開始時にマークが消えるのを防ぐため再適用
        foreach (var id in _markedPlayers)
        {
            var target = ExPlayerControl.ById(id);
            if (target != null) ApplyMark(target);
        }

        new LateTask(SendAnalysisResult, 0.5f, "ClinicalLabTask");
    }

    private void ClearAllMarks()
    {
        if (!Player.AmOwner) return;

        string markTag = " <color=#62110D>Ⓒ</color>";
        foreach (var id in _markedPlayers.ToList())
        {
            var target = ExPlayerControl.ById(id);
            if (target != null && target.Data.PlayerName.Contains(markTag))
            {
                target.Data.PlayerName = target.Data.PlayerName.Replace(markTag, "");
                if (target.Player?.cosmetics?.nameText != null)
                {
                    target.Player.cosmetics.nameText.text = target.Data.PlayerName;
                }
                NameText.UpdateNameInfo(target);
            }
        }
        _markedPlayers.Clear();
    }

    private void SendAnalysisResult()
    {
        if (!Player.AmOwner || HudManager.Instance?.Chat == null) return;

        var p1 = ExPlayerControl.ExPlayerControls.FirstOrDefault(x => x.PlayerId == Sample1);
        var p2 = ExPlayerControl.ExPlayerControls.FirstOrDefault(x => x.PlayerId == Sample2);

        if (p1 != null && p2 != null)
        {
            bool isSame = CheckIsSameTeam(p1, p2);

            string madText = ModTranslation.GetString(Data.JudgeMadByWinner ? "ClinicalLab_MadWinner" : "ClinicalLab_MadAssign");
            string neutralText = ModTranslation.GetString(Data.JudgeNeutralAsSame ? "ClinicalLab_NeutralSame" : "ClinicalLab_NeutralDiff");

            string roleLabel = ModTranslation.GetString("ClinicalLab_RoleName", madText, neutralText);
            string name1 = p1.Data.PlayerName.Replace(" <color=#62110D>Ⓒ</color>", "");
            string name2 = p2.Data.PlayerName.Replace(" <color=#62110D>Ⓒ</color>", "");

            string targetBlock = ModTranslation.GetString("ClinicalLab_Target", name1, name2);
            string resultBlock = ModTranslation.GetString(isSame ? "ClinicalLab_ResSame" : "ClinicalLab_ResDiff");

            string finalReport = $"{roleLabel}\n\n{targetBlock}\n\n{resultBlock}";
            HudManager.Instance.Chat.AddChat(Player.Player, finalReport);
        }

        ClearAllMarks();
        Sample1 = 255;
        Sample2 = 255;
    }

    private bool CheckIsSameTeam(ExPlayerControl p1, ExPlayerControl p2)
    {
        WinnerTeamType team1 = GetTeam(p1);
        WinnerTeamType team2 = GetTeam(p2);
        if (team1 != team2) return false;

        if (team1 == WinnerTeamType.Neutral && !Data.JudgeNeutralAsSame)
        {
            return p1.roleBase.Role == p2.roleBase.Role || p1.roleBase.TeamTag == p2.roleBase.TeamTag;
        }
        return true;
    }

    private WinnerTeamType GetTeam(ExPlayerControl p)
    {
        return Data.JudgeMadByWinner ? p.roleBase.WinnerTeam : (WinnerTeamType)p.roleBase.AssignedTeam;
    }
}