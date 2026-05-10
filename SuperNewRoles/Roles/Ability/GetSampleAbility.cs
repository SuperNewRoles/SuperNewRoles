using System;
using System.Linq;
using SuperNewRoles.Events;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Roles.Crewmate;
using UnityEngine;

namespace SuperNewRoles.Roles.Ability;

public class GetSampleAbility : TargetCustomButtonBase
{
    public float CoolTime { get; }
    private string _btnText;

    public override float DefaultTimer => CoolTime;
    public override string buttonText => _btnText;

    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("CupidButton.png");

    protected override KeyType keytype => KeyType.Ability1;
    public override Color32 OutlineColor => new(41, 158, 149, 255);
    public override bool OnlyCrewmates => false;

    private ExPlayerControl _firstTarget;
    private bool _isComplete = false;
    private EventListener<MeetingStartEventData> _meetingStartListener;

    public GetSampleAbility(float coolTime, string text)
    {
        CoolTime = coolTime;
        _btnText = text;
    }

    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        _meetingStartListener = MeetingStartEvent.Instance.AddListener((_) => {
            _firstTarget = null;
            _isComplete = false;
        });
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        _meetingStartListener?.RemoveListener();
    }

    public override bool CheckIsAvailable()
    {
        var labAbility = Player.GetAbility<ClinicalLaboratoryTechnicianAbility>();
        if (labAbility == null || labAbility.RemainingUses <= 0 || _isComplete) return false;
        return Player.IsAlive() && Target != null && !Target.IsDead() && Target.PlayerId != Player.PlayerId;
    }

    public override ShowTextType showTextType => ShowTextType.Show;

    public override string showText
    {
        get
        {
            var lab = Player.GetAbility<ClinicalLaboratoryTechnicianAbility>();
            if (lab == null) return "";

            string markColor = "#62110D";
            string marks = "";
            if (_isComplete)
                marks = $"<color={markColor}>ⒸⒸ</color>";
            else if (_firstTarget != null)
                marks = $"<color={markColor}>Ⓒ</color>";

            return $"<align=center>{ModTranslation.GetString("ClinicalLab_Remaining", lab.RemainingUses)}\n" +
                   $"{ModTranslation.GetString("ClinicalLab_Status", marks)}</align>";
        }
    }

    public override void OnClick()
    {
        if (_firstTarget == null)
        {
            RpcSetFirstSample(Target.PlayerId);
            Timer = 0f;
        }
        else if (_firstTarget.PlayerId != Target.PlayerId)
        {
            RpcGetSample(_firstTarget.PlayerId, Target.PlayerId);
            Timer = 0f;
        }
    }

    [CustomRPC]
    public void RpcSetFirstSample(byte p1)
    {
        _firstTarget = ExPlayerControl.ById(p1);
        var labAbility = Player.GetAbility<ClinicalLaboratoryTechnicianAbility>();
        if (labAbility != null)
        {
            labAbility.Sample1 = p1;
            labAbility.UpdateMark(p1);
        }
    }

    [CustomRPC]
    public void RpcGetSample(byte p1, byte p2)
    {
        var labAbility = Player.GetAbility<ClinicalLaboratoryTechnicianAbility>();
        if (labAbility != null)
        {
            labAbility.SetSamples(p1, p2);
            labAbility.UpdateMark(p2);
        }
        _isComplete = true;
    }
}