using System;
using AmongUs.GameOptions;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.CrewMate;
using SuperNewRoles.Roles.Neutral;
using TMPro;
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
    private EventListener<TryKillEventData> _murderListener;
    private EventListener<NameTextUpdateEventData> _nameTextUpdateListener;
    private EventListener<WrapUpEventData> _wrapUpListener;
    private EventListener _fixedUpdateListener;

    private float suicideTimer;
    private ExPlayerControl _suicideTarget;
    private TextMeshPro _suicideText;

    public SchrodingersCatTeam CurrentTeam { get; private set; } = SchrodingersCatTeam.SchrodingersCat;
    public SchrodingersCatAbility(SchrodingersCatData data)
    {
        Data = data;
        CurrentTeam = SchrodingersCatTeam.SchrodingersCat;
    }

    public override void AttachToAlls()
    {
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
        Player.AttachAbility(_knowOtherAbility, new AbilityParentAbility(this));
        Player.AttachAbility(_customVentAbility, new AbilityParentAbility(this));
        Player.AttachAbility(_customSaboAbility, new AbilityParentAbility(this));
        Player.AttachAbility(_impostorVisionAbility, new AbilityParentAbility(this));

        _murderListener = TryKillEvent.Instance.AddListener(TryMurder);
        _nameTextUpdateListener = NameTextUpdateEvent.Instance.AddListener(OnNameTextUpdate);
        _wrapUpListener = WrapUpEvent.Instance.AddListener(OnWrapUp);
        _fixedUpdateListener = FixedUpdateEvent.Instance.AddListener(OnFixedUpdate);
    }
    public override void DetachToAlls()
    {
        _murderListener?.RemoveListener();
        _nameTextUpdateListener?.RemoveListener();
        _wrapUpListener?.RemoveListener();
        _fixedUpdateListener?.RemoveListener();
    }
    private void OnFixedUpdate()
    {
        if (suicideTimer > 0 && _suicideTarget != null)
        {
            suicideTimer -= Time.fixedDeltaTime;
            if (suicideTimer <= 0 || MeetingHud.Instance != null)
            {
                _suicideTarget.CustomDeath(CustomDeathType.Suicide);
                if (_suicideText != null)
                {
                    _suicideText.text = "";
                    GameObject.Destroy(_suicideText.gameObject);
                    _suicideText = null;
                }
                _suicideTarget = null;
            }
        }
    }
    private void OnWrapUp(WrapUpEventData data)
    {
        if (ModHelpers.Not(data.exiled?.PlayerId == Player.PlayerId && Data.BeCrewOnExile && CurrentTeam == SchrodingersCatTeam.SchrodingersCat))
            return;
        CurrentTeam = SchrodingersCatTeam.Crewmate;
        Dominate(CurrentTeam);
    }
    private void TryMurder(TryKillEventData data)
    {
        if (Player.IsLovers()) return;
        if (data.RefTarget != Player) return;
        if (data.Killer == Player) return;
        if (CurrentTeam != SchrodingersCatTeam.SchrodingersCat) return;
        bool showKillAnimation = true;
        if (data.Killer.IsImpostor())
            CurrentTeam = Data.HasKillAbility ? SchrodingersCatTeam.Impostor : SchrodingersCatTeam.Madmate;
        else if (data.Killer.IsJackal())
            CurrentTeam = Data.HasKillAbility ? SchrodingersCatTeam.Jackal : SchrodingersCatTeam.Friends;
        else if (data.Killer.IsPavlovsTeam())
            CurrentTeam = Data.HasKillAbility ? SchrodingersCatTeam.Pavlovs : SchrodingersCatTeam.PavlovFriends;
        else if (data.Killer.IsCrewmate())
            CurrentTeam = SchrodingersCatTeam.Crewmate;
        else if (Data.CrewOnKillByNonSpecific)
        {
            CurrentTeam = SchrodingersCatTeam.Crewmate;
            Player.CustomDeath(CustomDeathType.Suicide);
            showKillAnimation = false;
        }

        if (CurrentTeam != SchrodingersCatTeam.SchrodingersCat)
        {
            Dominate(CurrentTeam);
            if (Data.KillVictimSuicide && Data.HasKillAbility)
            {
                suicideTimer = Data.SuicideTime;
                _suicideTarget = data.Killer;
                if (_suicideTarget.AmOwner)
                {
                    _suicideText = GameObject.Instantiate(HudManager.Instance.roomTracker.text, HudManager.Instance.roomTracker.transform);
                    _suicideText.transform.localPosition = HudManager.Instance.roomTracker.transform.localPosition + new Vector3(0, 0.4f, 0f);
                    _suicideText.alignment = TextAlignmentOptions.Center;
                    _suicideText.color = SchrodingersCat.Instance.RoleColor;
                    _suicideText.text = ModTranslation.GetString("SchrodingersCatSuicideText");
                }
            }
            if (data.RefTarget.AmOwner)
                NameText.UpdateAllNameInfo();
            else
                NameText.UpdateNameInfo(data.RefTarget);
            data.RefSuccess = false;
            if (showKillAnimation && data.RefTarget.AmOwner)
                DestroyableSingleton<HudManager>.Instance.KillOverlay.ShowKillAnimation(data.Killer.Data, data.RefTarget.Data);
        }
    }
    /// <summary>
    /// playerが現在のチームに所属しているかどうかを返す
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    private bool CheckTeam(ExPlayerControl player)
    {
        switch (CurrentTeam)
        {
            case SchrodingersCatTeam.SchrodingersCat:
                return false;
            case SchrodingersCatTeam.Crewmate:
                return player.IsCrewmate();
            case SchrodingersCatTeam.Impostor:
            case SchrodingersCatTeam.Madmate:
                return player.IsImpostor();
            case SchrodingersCatTeam.Jackal:
            case SchrodingersCatTeam.Friends:
                return player.IsJackal();
            case SchrodingersCatTeam.Pavlovs:
            case SchrodingersCatTeam.PavlovFriends:
                return player.IsPavlovsTeam();
            default:
                return false;
        }
    }
    private void OnNameTextUpdate(NameTextUpdateEventData data)
    {
        if (CurrentTeam == SchrodingersCatTeam.SchrodingersCat) return;
        if (data.Player != Player)
        {
            if (Player.AmOwner)
                NameUpdateOwner(data);
            return;
        }
        if (CurrentTeam == SchrodingersCatTeam.Crewmate)
        {
            if (!data.Visible)
                return;
        }
        else if (ModHelpers.Not(data.Visible || Player.AmOwner || CheckTeam(ExPlayerControl.LocalPlayer))) return;

        Color32 color = Color.white;
        switch (CurrentTeam)
        {
            case SchrodingersCatTeam.Impostor:
            case SchrodingersCatTeam.Madmate:
                color = Palette.ImpostorRed;
                break;
            case SchrodingersCatTeam.Jackal:
            case SchrodingersCatTeam.Friends:
                color = Jackal.Instance.RoleColor;
                break;
            case SchrodingersCatTeam.Crewmate:
                color = Palette.CrewmateBlue;
                break;
            case SchrodingersCatTeam.Pavlovs:
            case SchrodingersCatTeam.PavlovFriends:
                color = PavlovsDog.Instance.RoleColor;
                break;
            default:
                return;
        }
        data.Player.PlayerInfoText.text = ModHelpers.Cs(color, data.Player.PlayerInfoText.text);
        data.Player.PlayerInfoText.color = color;
        if (data.Player.MeetingInfoText)
        {
            data.Player.MeetingInfoText.text = ModHelpers.Cs(color, data.Player.MeetingInfoText.text);
            data.Player.MeetingInfoText.color = color;
        }
        data.Player.cosmetics.nameText.text = ModHelpers.Cs(color, data.Player.cosmetics.nameText.text);
        if (data.Player.VoteArea != null)
            data.Player.VoteArea.NameText.text = ModHelpers.Cs(color, data.Player.VoteArea.NameText.text);
    }
    private void NameUpdateOwner(NameTextUpdateEventData data)
    {
        switch (CurrentTeam)
        {
            case SchrodingersCatTeam.Crewmate:
                break;
            case SchrodingersCatTeam.Impostor:
            case SchrodingersCatTeam.Madmate:
                if (data.Player.IsImpostor())
                    data.Player.PlayerInfoText.text = ModHelpers.Cs(Palette.ImpostorRed, data.Player.PlayerInfoText.text);
                break;
            case SchrodingersCatTeam.Jackal:
            case SchrodingersCatTeam.Friends:
                if (data.Player.IsJackal())
                    data.Player.PlayerInfoText.text = ModHelpers.Cs(Jackal.Instance.RoleColor, data.Player.PlayerInfoText.text);
                break;
            case SchrodingersCatTeam.Pavlovs:
            case SchrodingersCatTeam.PavlovFriends:
                if (data.Player.IsPavlovsTeam())
                    data.Player.PlayerInfoText.text = ModHelpers.Cs(PavlovsOwner.Instance.RoleColor, data.Player.PlayerInfoText.text);
                break;
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
        if (Data.HasKillAbility && CurrentTeam != SchrodingersCatTeam.SchrodingersCat && CurrentTeam != SchrodingersCatTeam.Crewmate)
        {
            _customKillButtonAbility = new CustomKillButtonAbility(
                () => Data.HasKillAbility && CurrentTeam is not SchrodingersCatTeam.SchrodingersCat and not SchrodingersCatTeam.Crewmate,
                () => Data.KillCooldown,
                () => false,
                isTargetable: (player) => player.IsAlive() && CheckTargetable(player)
            );
            Player.AttachAbility(_customKillButtonAbility, new AbilityParentAbility(this));
        }
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
                Player.AttachAbility(madmateAbility, new AbilityParentAbility(this));
                if (Player.AmOwner)
                    madmateAbility.CustomTaskAbility.AssignTasks();
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
                Player.AttachAbility(jackalFriendsAbility, new AbilityParentAbility(this));
                if (Player.AmOwner)
                    jackalFriendsAbility.CustomTaskAbility.AssignTasks();
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
}