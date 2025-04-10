using System;
using SuperNewRoles.Events;
using SuperNewRoles.Modules.Events.Bases;

namespace SuperNewRoles.Roles.Ability;

class AdditionalVoteAbility : AbilityBase
{
    public Func<int> AdditionalVote { get; }
    private EventListener<MeetingHudCalculateVotesOnPlayerOnlyHostEventData> OnMeetingHudCalculateVotesOnPlayerOnlyHostEventListener;
    public AdditionalVoteAbility(Func<int> getAdditionalVote) => AdditionalVote = getAdditionalVote;
    public override void AttachToAlls()
    {
        base.AttachToAlls();
        OnMeetingHudCalculateVotesOnPlayerOnlyHostEventListener = MeetingHudCalculateVotesOnPlayerOnlyHostEvent.Instance.AddListener(OnMeetingHudCalculateVotesOnPlayerOnlyHost);
    }
    public override void DetachToAlls()
    {
        base.DetachToAlls();
        OnMeetingHudCalculateVotesOnPlayerOnlyHostEventListener?.RemoveListener();
    }
    private void OnMeetingHudCalculateVotesOnPlayerOnlyHost(MeetingHudCalculateVotesOnPlayerOnlyHostEventData data)
    {
        if (data.Source.PlayerId == Player.PlayerId)
            data.VoteCount += AdditionalVote();
    }
}