using System;
using AmongUs.GameOptions;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.CrewMate;
using SuperNewRoles.Roles.Neutral;
using UnityEngine;

namespace SuperNewRoles.Roles.Ability;

public record SchrodingersCatData(
    bool HasKillAbility,
    float KillCooldown,
    bool KillVictimSuicide,
    float SuicideTime,
    bool BeCrewOnExile,
    bool CrewOnKillByNonSpecific
);
public enum SchrodingersCatTeam
{
    SchrodingersCat,
    Crewmate,
    Madmate,
    Impostor,
    Friends,
    Jackal,
    Pavlovs,
    PavlovFriends,
}
public class SchrodingersCatAbility : AbilityBase
{
    private SchrodingersCatData Data { get; }
    private CustomKillButtonAbility _customKillButtonAbility;
    private KnowOtherAbility _knowOtherAbility;
    private CustomVentAbility _customVentAbility;
    private CustomSaboAbility _customSaboAbility;
    private ImpostorVisionAbility _impostorVisionAbility;
    private EventListener<MurderEventData> _murderListener;
    private EventListener<NameTextUpdateEventData> _nameTextUpdateListener;
    public SchrodingersCatTeam CurrentTeam { get; private set; } = SchrodingersCatTeam.SchrodingersCat;
    public SchrodingersCatAbility(SchrodingersCatData data)
    {
        Data = data;
        CurrentTeam = SchrodingersCatTeam.SchrodingersCat;
    }

    public override void AttachToAlls()
    {
        _customKillButtonAbility = new CustomKillButtonAbility(
            () => Data.HasKillAbility && CurrentTeam is not SchrodingersCatTeam.SchrodingersCat and not SchrodingersCatTeam.Crewmate,
            () => Data.KillCooldown,
            () => false,
            isTargetable: (player) => player.IsAlive() && CheckTargetable(player)
        );
        _knowOtherAbility = new KnowOtherAbility(
            (player) => CurrentTeam != SchrodingersCatTeam.SchrodingersCat && CanKnow(player),
            () => false
        );
        _customVentAbility = new CustomVentAbility(
            () => CurrentTeam != SchrodingersCatTeam.SchrodingersCat && CouldUseVent()
        );
        _customSaboAbility = new CustomSaboAbility(
            () => CurrentTeam != SchrodingersCatTeam.SchrodingersCat && CouldUseSabo()
        );
        _impostorVisionAbility = new ImpostorVisionAbility(
            () => CurrentTeam != SchrodingersCatTeam.SchrodingersCat && IsImpostorVision()
        );
        Player.AttachAbility(_customKillButtonAbility, new AbilityParentAbility(this));
        Player.AttachAbility(_knowOtherAbility, new AbilityParentAbility(this));
        Player.AttachAbility(_customVentAbility, new AbilityParentAbility(this));
        Player.AttachAbility(_customSaboAbility, new AbilityParentAbility(this));
        Player.AttachAbility(_impostorVisionAbility, new AbilityParentAbility(this));

        _murderListener = MurderEvent.Instance.AddListener(OnMurder);
        _nameTextUpdateListener = NameTextUpdateEvent.Instance.AddListener(OnNameTextUpdate);
    }
    private void OnMurder(MurderEventData data)
    {
        if (Player.IsLovers()) return;
        if (data.target != Player) return;
        if (data.killer == Player) return;
        if (CurrentTeam != SchrodingersCatTeam.SchrodingersCat) return;
        if (data.killer.IsImpostor())
            CurrentTeam = Data.HasKillAbility ? SchrodingersCatTeam.Impostor : SchrodingersCatTeam.Madmate;
        else if (data.killer.IsJackal())
            CurrentTeam = Data.HasKillAbility ? SchrodingersCatTeam.Jackal : SchrodingersCatTeam.Friends;
        else if (data.killer.IsPavlovsTeam())
            CurrentTeam = Data.HasKillAbility ? SchrodingersCatTeam.Pavlovs : SchrodingersCatTeam.PavlovFriends;
        else if (data.killer.IsCrewmate())
            CurrentTeam = SchrodingersCatTeam.Crewmate;
        else if (Data.CrewOnKillByNonSpecific)
            CurrentTeam = SchrodingersCatTeam.Crewmate;
        else
            Player.CustomDeath(CustomDeathType.Suicide);
        if (CurrentTeam != SchrodingersCatTeam.SchrodingersCat)
            Dominate(CurrentTeam);
    }
    private void OnNameTextUpdate(NameTextUpdateEventData data)
    {
        if (CurrentTeam == SchrodingersCatTeam.SchrodingersCat) return;
        if (data.Visible)
        {
            Color32 color = Color.white;
            switch (CurrentTeam)
            {
                case SchrodingersCatTeam.Impostor:
                case SchrodingersCatTeam.Madmate:
                    color = Palette.ImpostorRed; ;
                    break;
                case SchrodingersCatTeam.Jackal:
                case SchrodingersCatTeam.Friends:
                    color = Jackal.Instance.RoleColor;
                    break;
                case SchrodingersCatTeam.Pavlovs:
                case SchrodingersCatTeam.PavlovFriends:
                    color = PavlovsOwner.Instance.RoleColor;
                    break;
                default:
                    return;
            }
            data.Player.PlayerInfoText.text = ModHelpers.Cs(color, data.Player.PlayerInfoText.text);
            if (data.Player.MeetingInfoText)
                data.Player.MeetingInfoText.text = ModHelpers.Cs(color, data.Player.MeetingInfoText.text);
            data.Player.cosmetics.nameText.text = ModHelpers.Cs(color, data.Player.cosmetics.nameText.text);
            if (data.Player.VoteArea != null)
                data.Player.VoteArea.PlayerIcon.cosmetics.nameText.text = ModHelpers.Cs(color, data.Player.VoteArea.PlayerIcon.cosmetics.nameText.text);
        }
    }
    private bool CheckTargetable(ExPlayerControl player)
    {
        switch (CurrentTeam)
        {
            case SchrodingersCatTeam.SchrodingersCat:
                return false;
            case SchrodingersCatTeam.Crewmate:
                return player.IsAlive();
            case SchrodingersCatTeam.Impostor:
                return player.IsAlive() && !player.IsImpostor();
            case SchrodingersCatTeam.Jackal:
                return player.IsAlive() && !player.IsJackal();
            case SchrodingersCatTeam.Pavlovs:
                return player.IsAlive() && !player.IsPavlovsTeam();
            default:
                return false;
        }
    }
    private bool CouldUseVent()
    {
        switch (CurrentTeam)
        {
            case SchrodingersCatTeam.SchrodingersCat:
                return false;
            case SchrodingersCatTeam.Crewmate:
                return false;
            case SchrodingersCatTeam.Madmate:
                return Madmate.MadmateCouldUseVent;
            case SchrodingersCatTeam.Impostor:
                return true;
            case SchrodingersCatTeam.Friends:
                return JackalFriends.JackalFriendsCanUseVent;
            case SchrodingersCatTeam.Jackal:
                return Jackal.JackalCanUseVent;
            case SchrodingersCatTeam.PavlovFriends:
                return PavlovsOwner.PavlovsDogCanUseVent;
            case SchrodingersCatTeam.Pavlovs:
                return PavlovsOwner.PavlovsDogCanUseVent;
            default:
                return false;
        }
    }
    public void Dominate(SchrodingersCatTeam team)
    {
        foreach (var deadBody in GameObject.FindObjectsOfType<DeadBody>())
        {
            if (deadBody.ParentId == Player.PlayerId)
                deadBody.transform.localPosition = new(999, 999);
        }
        new LateTask(() =>
        {
            Player.Player.Revive();
            RoleManager.Instance.SetRole(Player, RoleTypes.Crewmate);
        }, 0.017f);
        new LateTask(() =>
        {
            foreach (var deadBody in GameObject.FindObjectsOfType<DeadBody>())
            {
                if (deadBody.ParentId == Player.PlayerId)
                {
                    GameObject.Destroy(deadBody.gameObject);
                }
            }
        }, 0.5f);
        switch (team)
        {
            case SchrodingersCatTeam.Madmate:
                Player.DetachAbility(_knowOtherAbility.AbilityId);
                Player.DetachAbility(_customVentAbility.AbilityId);
                Player.DetachAbility(_customSaboAbility.AbilityId);
                Player.DetachAbility(_impostorVisionAbility.AbilityId);
                MadmateAbility madmateAbility = new(new MadmateData(
                    hasImpostorVision: Madmate.MadmateHasImpostorVision,
                    couldUseVent: Madmate.MadmateCouldUseVent,
                    couldKnowImpostors: Madmate.MadmateCanKnowImpostors,
                    taskNeeded: Madmate.MadmateNeededTaskCount,
                    specialTasks: Madmate.MadmateSpecialTasks
                ));
                if (Player.AmOwner)
                    madmateAbility.CustomTaskAbility.AssignTasks();
                Player.AttachAbility(madmateAbility, new AbilityParentAbility(this));
                break;
            case SchrodingersCatTeam.Friends:
                Player.DetachAbility(_knowOtherAbility.AbilityId);
                Player.DetachAbility(_customVentAbility.AbilityId);
                Player.DetachAbility(_customSaboAbility.AbilityId);
                Player.DetachAbility(_impostorVisionAbility.AbilityId);
                JFriendAbility jackalFriendsAbility = new(new JFriendData(
                    CanUseVent: JackalFriends.JackalFriendsCanUseVent,
                    IsImpostorVision: JackalFriends.JackalFriendsImpostorVision,
                    CouldKnowJackals: JackalFriends.JackalFriendsCouldKnowJackals,
                    TaskNeeded: JackalFriends.JackalFriendsTaskNeed,
                    SpecialTasks: JackalFriends.JackalFriendsTaskOption
                ));
                if (Player.AmOwner)
                    jackalFriendsAbility.CustomTaskAbility.AssignTasks();
                Player.AttachAbility(jackalFriendsAbility, new AbilityParentAbility(this));
                break;
        }
    }
    private bool CouldUseSabo()
    {
        switch (CurrentTeam)
        {
            case SchrodingersCatTeam.SchrodingersCat:
                return false;
            case SchrodingersCatTeam.Crewmate:
                return false;
            case SchrodingersCatTeam.Madmate:
                return false;
            case SchrodingersCatTeam.Impostor:
                return true;
            case SchrodingersCatTeam.Friends:
                return false;
            case SchrodingersCatTeam.Jackal:
                return false;
            case SchrodingersCatTeam.PavlovFriends:
                return false;
            case SchrodingersCatTeam.Pavlovs:
                return false;
            default:
                return false;
        }
    }
    private bool IsImpostorVision()
    {
        switch (CurrentTeam)
        {
            case SchrodingersCatTeam.SchrodingersCat:
                return false;
            case SchrodingersCatTeam.Crewmate:
                return false;
            case SchrodingersCatTeam.Madmate:
                return Madmate.MadmateHasImpostorVision;
            case SchrodingersCatTeam.Impostor:
                return true;
            case SchrodingersCatTeam.Friends:
                return JackalFriends.JackalFriendsImpostorVision;
            case SchrodingersCatTeam.Jackal:
                return Jackal.JackalImpostorVision;
            case SchrodingersCatTeam.PavlovFriends:
                return PavlovsOwner.PavlovsDogIsImpostorVision;
            case SchrodingersCatTeam.Pavlovs:
                return PavlovsOwner.PavlovsDogIsImpostorVision;
            default:
                return false;
        }
    }

    private bool CanKnow(ExPlayerControl player)
    {
        switch (CurrentTeam)
        {
            case SchrodingersCatTeam.SchrodingersCat:
            case SchrodingersCatTeam.Crewmate:
                return false;
            case SchrodingersCatTeam.Impostor:
                return player.IsImpostor();
            case SchrodingersCatTeam.Jackal:
                return player.IsJackal();
            case SchrodingersCatTeam.Pavlovs:
                return player.IsPavlovsTeam();
            default:
                return false;
        }
    }

    public override void DetachToAlls()
    {
        _murderListener?.RemoveListener();
    }
}