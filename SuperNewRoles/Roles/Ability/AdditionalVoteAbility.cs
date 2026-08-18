using System;
using SuperNewRoles.Events;

namespace SuperNewRoles.Roles.Ability;

class AdditionalVoteAbility : AbilityBase
{
    public Func<int> AdditionalVote { get; }
    public AdditionalVoteAbility(Func<int> getAdditionalVote) => AdditionalVote = getAdditionalVote;
    public override void AttachToAlls()
    {
        base.AttachToAlls();
        SubscribeWithAbility(MeetingHudCalculateVotesOnPlayerOnlyHostEvent.Instance, OnMeetingHudCalculateVotesOnPlayerOnlyHost);
    }
    private void OnMeetingHudCalculateVotesOnPlayerOnlyHost(MeetingHudCalculateVotesOnPlayerOnlyHostEventData data)
    {
        if (data.Source.PlayerId == Player.PlayerId)
            data.VoteCount += AdditionalVote();
    }
}
