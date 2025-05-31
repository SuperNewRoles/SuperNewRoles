using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AmongUs.GameOptions;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Ability;
using SuperNewRoles.CustomObject;

namespace SuperNewRoles.Roles.Impostor;

class Rocket : RoleBase<Rocket>
{
    public override RoleId Role { get; } = RoleId.Rocket;
    public override Color32 RoleColor { get; } = Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new RocketAbility(RocketInitialGrabCooldown, RocketSubsequentGrabCooldown, RocketLaunchAfterMeeting)
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Shapeshifter; // Based on WaveCannon
    public override short IntroNum { get; } = 1; // Based on WaveCannon

    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Impostor;
    public override TeamTag TeamTag { get; } = TeamTag.Impostor;
    public override RoleTag[] RoleTags { get; } = [RoleTag.SpecialKiller]; // Based on WaveCannon
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Impostor;

    // Custom Options
    [CustomOptionFloat("RocketInitialGrabCooldown", 2.5f, 60f, 2.5f, 15f)]
    public static float RocketInitialGrabCooldown;

    [CustomOptionFloat("RocketSubsequentGrabCooldown", 0f, 60f, 2.5f, 5f)]
    public static float RocketSubsequentGrabCooldown;
    [CustomOptionBool("RocketLaunchAfterMeeting", false)]
    public static bool RocketLaunchAfterMeeting;
}

// --- Main Ability Class (Container) ---
public class RocketAbility : AbilityBase
{
    private readonly float _initialCooldown;
    private readonly float _subsequentCooldown;
    private readonly bool _launchAfterMeeting;
    public RocketGrabAbility GrabAbility { get; private set; }
    public RocketLaunchAbility LaunchAbility { get; private set; }

    public RocketAbility(float initialCooldown, float subsequentCooldown, bool launchAfterMeeting)
    {
        _initialCooldown = initialCooldown;
        _subsequentCooldown = subsequentCooldown;
        _launchAfterMeeting = launchAfterMeeting;
    }

    public override void AttachToAlls()
    {
        base.AttachToAlls();
        GrabAbility = new RocketGrabAbility(_initialCooldown, _subsequentCooldown, _launchAfterMeeting);
        LaunchAbility = new RocketLaunchAbility();
        GrabAbility.SetLaunchAbility(LaunchAbility);
        LaunchAbility.SetGrabAbility(GrabAbility);
        Player.AttachAbility(GrabAbility, new AbilityParentAbility(this));
        Player.AttachAbility(LaunchAbility, new AbilityParentAbility(this));
        Player.AttachAbility(new KillableAbility(() => false), new AbilityParentAbility(this));
    }
}

public class RocketGrabAbility : TargetCustomButtonBase
{
    private float initialCooldown;
    private float subsequentCooldown;
    // Reference is set by the main RocketAbility
    public RocketLaunchAbility launchAbility { get; private set; }

    public List<ExPlayerControl> GrabbedPlayers { get; } = new();
    private bool launchAfterMeeting = false;
    private bool launchAfterMeetingOption = false;

    private EventListener<MeetingStartEventData> _onMeetingStartEvent;
    private EventListener<WrapUpEventData> _onWrapUpEvent;
    private EventListener _fixedUpdateEvent;

    // Use a placeholder sprite path for now
    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("RocketGrabButton.png");
    public override string buttonText => ModTranslation.GetString("RocketGrabButtonText");
    protected override KeyType keytype => KeyType.Ability1;
    public override Color32 OutlineColor => Rocket.Instance.RoleColor;
    // Implement required abstract member
    public override bool OnlyCrewmates => true; // Can grab Crewmates/Neutrals, adjust if Impostors should be grabbable
    public override Func<ExPlayerControl, bool> IsTargetable => (player) => !GrabbedPlayers.Contains(player);

    public override float DefaultTimer => GrabbedPlayers.Count == 0 ? initialCooldown : subsequentCooldown;

    public RocketGrabAbility(float initialCooldown, float subsequentCooldown, bool launchAfterMeeting)
    {
        this.initialCooldown = initialCooldown;
        this.subsequentCooldown = subsequentCooldown;
        this.launchAfterMeetingOption = launchAfterMeeting;
    }

    // Method called by RocketAbility during Attach
    public void SetLaunchAbility(RocketLaunchAbility launchAbility)
    {
        this.launchAbility = launchAbility;
    }

    public override bool CheckIsAvailable()
    {
        // Try accessing PlayerControl via .PlayerControl property
        bool standardConditions = Player != null && Player.IsAlive() && Player.Player.CanMove && MeetingHud.Instance == null;
        bool targetValid = Target != null && Target.IsAlive() && !GrabbedPlayers.Contains(Target);
        return standardConditions && targetValid;
    }

    public override void OnClick()
    {
        if (!CheckIsAvailable()) return;
        // Pass PlayerControl via .PlayerControl property
        RpcAddGrabbedPlayer(Target);
        launchAbility.ResetTimer();
    }

    public override void AttachToAlls()
    {
        base.AttachToAlls();
        _onMeetingStartEvent = MeetingStartEvent.Instance.AddListener(OnMeetingStart);
        _onWrapUpEvent = WrapUpEvent.Instance.AddListener(OnWrapUp);
        _fixedUpdateEvent = FixedUpdateEvent.Instance.AddListener(OnFixedUpdate);
    }

    public override void DetachToAlls()
    {
        base.DetachToAlls();
        _onMeetingStartEvent?.RemoveListener();
        _onWrapUpEvent?.RemoveListener();
        _fixedUpdateEvent?.RemoveListener();
        if (Player != null && Player.AmOwner && GrabbedPlayers.Count > 0)
        {
            RpcClearGrabbedPlayers();
        }
    }

    private void OnFixedUpdate()
    {
        if (Player == null || GrabbedPlayers.Count == 0) return;
        foreach (var grabbedPlayer in GrabbedPlayers.ToList())
        {
            if (Player.AmOwner && (grabbedPlayer == null || grabbedPlayer.IsDead()))
            {
                // Pass PlayerControl via .PlayerControl property
                RpcRemoveGrabbedPlayer(grabbedPlayer.Player);
                continue;
            }
            if (MeetingHud.Instance == null)
            {
                grabbedPlayer.NetTransform.SnapTo(Player.transform.position);
                grabbedPlayer.MyPhysics.body.velocity = Vector2.zero;
                grabbedPlayer.transform.position = Player.transform.position;
            }
        }
    }

    private void OnMeetingStart(MeetingStartEventData data)
    {
        if (Player != null && Player.AmOwner && GrabbedPlayers.Count > 0)
        {
            launchAfterMeeting = true;
        }
        else
        {
            launchAfterMeeting = false;
        }
    }

    private void OnWrapUp(WrapUpEventData data)
    {
        if (Player == null || !Player.AmOwner) return;
        if (launchAfterMeeting && Player.IsAlive() && GrabbedPlayers.Count > 0)
        {
            if (launchAbility != null)
            {
                List<ExPlayerControl> playersToLaunch = GrabbedPlayers;
                if (launchAfterMeetingOption)
                {
                    launchAbility.RpcLaunchPlayers(playersToLaunch);
                }
                else
                {
                    launchAbility.RpcAllExile(playersToLaunch);
                }
            }
        }
        launchAfterMeeting = false;
        if (!Player.IsAlive() || (data.exiled != null && data.exiled.PlayerId == Player.PlayerId))
        {
            RpcClearGrabbedPlayers();
        }
    }

    // --- RPCs ---
    [CustomRPC]
    public void RpcAddGrabbedPlayer(ExPlayerControl targetEx)
    {
        // Get ExPlayerControl using GameData
        if (targetEx != null && !GrabbedPlayers.Contains(targetEx))
        {
            GrabbedPlayers.Add(targetEx);
            if (Player != null && Player.AmOwner && launchAbility != null)
            {
                launchAbility.SetActive(launchAbility.CheckIsAvailable());
            }
        }
    }

    [CustomRPC]
    public void RpcRemoveGrabbedPlayer(PlayerControl targetPlayerControl)
    {
        var playerInfo = GameData.Instance.GetPlayerById(targetPlayerControl.PlayerId);
        var targetEx = playerInfo != null ? (ExPlayerControl)playerInfo.Object : null;

        if (targetEx != null && GrabbedPlayers.Remove(targetEx))
        {
            if (Player != null && Player.AmOwner && launchAbility != null)
            {
                launchAbility.SetActive(launchAbility.CheckIsAvailable());
            }
        }
    }

    [CustomRPC]
    public void RpcClearGrabbedPlayers()
    {
        ClearGrabbedPlayersLocally();
    }

    // Called locally by LaunchAbility after successful launch
    public void ClearGrabbedPlayersLocally()
    {
        GrabbedPlayers.Clear();
        if (Player != null && Player.AmOwner && launchAbility != null)
        {
            launchAbility.SetActive(launchAbility.CheckIsAvailable());
        }
    }
}

public class RocketLaunchAbility : CustomButtonBase
{
    // Reference set by the main RocketAbility
    public RocketGrabAbility grabAbility { get; private set; }

    public override float DefaultTimer => 0.1f; // Minimal cooldown

    // Use a placeholder sprite path for now
    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("RocketLaunchButton.png");
    public override string buttonText => ModTranslation.GetString("RocketLaunchButtonText");
    protected override KeyType keytype => KeyType.Ability2;

    // Method called by RocketAbility during Attach
    public void SetGrabAbility(RocketGrabAbility grabAbility)
    {
        this.grabAbility = grabAbility;
    }

    public override bool CheckIsAvailable()
    {
        // Use .PlayerControl
        bool standardConditions = Player != null && Player.IsAlive() && Player.Player.CanMove && MeetingHud.Instance == null;
        bool hasGrabbed = grabAbility != null && grabAbility.GrabbedPlayers.Count > 0;
        return standardConditions && hasGrabbed;
    }

    public override bool CheckHasButton()
        => base.CheckHasButton() && grabAbility != null && grabAbility.GrabbedPlayers.Count > 0;

    public override void OnClick()
    {
        if (!CheckIsAvailable() || grabAbility == null) return;
        // Convert using .PlayerControl
        List<ExPlayerControl> playersToLaunch = grabAbility.GrabbedPlayers;
        if (playersToLaunch.Count > 0)
        {
            RpcLaunchPlayers(playersToLaunch);
        }
    }


    [CustomRPC]
    public void RpcAllExile(List<ExPlayerControl> playersToLaunch)
    {
        if (grabAbility == null || Player == null) return;
        foreach (var targetControl in playersToLaunch)
        {
            if (targetControl == null || targetControl.IsDead()) continue;
            if (grabAbility.GrabbedPlayers.Contains(targetControl))
            {
                targetControl.CustomDeath(CustomDeathType.LaunchByRocket, source: Player);
            }
        }
        grabAbility.ClearGrabbedPlayersLocally();
    }

    [CustomRPC]
    public void RpcLaunchPlayers(List<ExPlayerControl> targets)
    {
        if (grabAbility == null || Player == null) return;
        Vector3 launchPosition = Player.transform.position;
        int count = 0;

        foreach (var targetControl in targets)
        {
            // Get ExPlayerControl using GameData
            var playerInfo = GameData.Instance.GetPlayerById(targetControl.PlayerId);
            var targetEx = playerInfo != null ? (ExPlayerControl)playerInfo.Object : null;

            if (targetEx != null && targetEx.IsAlive() && grabAbility.GrabbedPlayers.Contains(targetEx))
            {
                targetEx.CustomDeath(CustomDeathType.LaunchByRocket, source: Player);
                // SoundEffectManager.Instance?.PlaySound("RocketLaunch", false, 0.8f, launchPosition);
            }
            new GameObject("RocketDeadbody").AddComponent<RocketDeadbody>().Init(targetEx, count, targets.Count);
            count++;
        }
        grabAbility.ClearGrabbedPlayersLocally();
    }
}