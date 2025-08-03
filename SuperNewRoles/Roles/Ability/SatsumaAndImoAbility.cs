using SuperNewRoles.Events;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.CrewMate;
using SuperNewRoles.Roles.Ability;

namespace SuperNewRoles.Roles.Ability;
public class SatsumaAndImoAbility : AbilityBase
{
    // チーム状態を管理
    private enum SatsumaTeam { Crewmate, Madmate }
    private SatsumaTeam _teamState = SatsumaTeam.Crewmate;
    public bool IsMadTeam => _teamState == SatsumaTeam.Madmate;
    private EventListener<WrapUpEventData> _wrapUpListener;
    private EventListener<NameTextUpdateEventData> _nameTextListener;

    public override void AttachToAlls()
    {
        _nameTextListener = NameTextUpdateEvent.Instance.AddListener(OnNameTextUpdate);
        _wrapUpListener = WrapUpEvent.Instance.AddListener(OnWrapUp);
    }

    public override void DetachToAlls()
    {
        _nameTextListener?.RemoveListener();
        _wrapUpListener?.RemoveListener();
    }

    private void OnWrapUp(WrapUpEventData data)
    {
        new LateTask(() =>
        {
            // WrapUp ごとにチームを切り替え
            _teamState = _teamState == SatsumaTeam.Crewmate ? SatsumaTeam.Madmate : SatsumaTeam.Crewmate;
            // 名前更新
            NameText.UpdateAllNameInfo();
        }, 0.6f, "SatsumaAndImoAbility");
    }

    private void OnNameTextUpdate(NameTextUpdateEventData data)
    {
        if (data.Player != Player) return;
        if (!data.Visible) return;
        // 現在のチーム状態に応じてサフィックスを追加
        string suffix = _teamState == SatsumaTeam.Madmate ? ModHelpers.Cs(Madmate.Instance.RoleColor, " (M)") : ModHelpers.Cs(Palette.CrewmateBlue, " (C)");
        data.Player.PlayerInfoText.text += suffix;
        if (data.Player.MeetingInfoText != null)
            data.Player.MeetingInfoText.text += suffix;
    }
}