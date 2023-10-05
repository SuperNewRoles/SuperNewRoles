using AmongUs.GameOptions;
using HarmonyLib;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomObject;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Replay;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Crewmate;
using SuperNewRoles.Roles.Impostor;
using SuperNewRoles.Roles.Neutral;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Sabotage;
using UnityEngine;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.StartGame))]
public class StartGame
{
    public static void Postfix()
    {
        MapOption.RandomMap.Prefix();
        FixedUpdate.IsProDown = ConfigRoles.CustomProcessDown.Value;
    }
}
[HarmonyPatch(typeof(AbilityButton), nameof(AbilityButton.Update))]
public class AbilityUpdate
{
    public static void Postfix(AbilityButton __instance)
    {
        if (CachedPlayer.LocalPlayer.Data.Role.IsSimpleRole && __instance.commsDown.active)
        {
            __instance.commsDown.SetActive(false);
        }
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
public class FixedUpdate
{
    static void SetBasePlayerOutlines()
    {
        foreach (PlayerControl target in CachedPlayer.AllPlayers)
        {
            var rend = target.MyRend();
            if (target == null || rend == null) continue;
            if (rend.material.GetFloat("_Outline") == 0f) continue;
            rend.material.SetFloat("_Outline", 0f);
        }
    }

    static void ReduceKillCooldown(PlayerControl __instance)
    {
        if (PlayerControl.LocalPlayer.IsRole(RoleId.Tasker) && CustomOptionHolder.TaskerIsKillCoolTaskNow.GetBool())
        {
            if (!__instance.Data.IsDead && !__instance.CanMove && Minigame.Instance != null && Minigame.Instance.MyNormTask != null && Minigame.Instance.MyNormTask.Owner.AmOwner)
                __instance.SetKillTimer(__instance.killTimer - Time.fixedDeltaTime);
        }
    }
    public static bool IsProDown;

    public static void Postfix(PlayerControl __instance)
    {
        if (!PlayerAnimation.IsCreatedAnim(__instance.PlayerId))
            new PlayerAnimation(__instance);
        if (ReplayManager.IsReplayMode && !ReplayLoader.IsInited) return;
        if (__instance != PlayerControl.LocalPlayer) return;
        PlayerAnimation.FixedAllUpdate();
        PVCreator.FixedUpdate();

        VentAndSabo.VentButtonVisibilityPatch.Postfix(__instance);
        OldModeButtons.OldModeUpdate();

        // -- 以下ゲーム中のみ --
        if (AmongUsClient.Instance.GameState != AmongUsClient.GameStates.Started)
        {
            return;
        }

        SetBasePlayerOutlines();
        LadderDead.FixedUpdate();
        switch (ModeHandler.GetMode())
        {
            case ModeId.Default:
                SabotageManager.Update();
                SetNameUpdate.Postfix(__instance);
                NiceMechanic.FixedUpdate();
                Jackal.JackalFixedPatch.Postfix(__instance, PlayerControl.LocalPlayer.GetRole());
                JackalSeer.JackalSeerFixedPatch.Postfix(__instance, PlayerControl.LocalPlayer.GetRole());
                WaveCannonJackal.WaveCannonJackalFixedPatch.Postfix(__instance, PlayerControl.LocalPlayer.GetRole());
                Psychometrist.FixedUpdate();
                Matryoshka.FixedUpdate();
                PartTimer.FixedUpdate();
                WiseMan.FixedUpdate();
                Vampire.FixedUpdate.AllClient();
                ReduceKillCooldown(__instance);
                Penguin.FixedUpdate();
                Squid.FixedUpdate();
                OrientalShaman.FixedUpdate();
                TheThreeLittlePigs.FixedUpdate();
                CustomRoles.FixedUpdate(__instance);
                Balancer.Update();
                Pteranodon.FixedUpdateAll();
                BlackHatHacker.FixedUpdate();
                JumpDancer.FixedUpdate();
                Rocket.FixedUpdate();
                WellBehaver.FixedUpdate();
                if (PlayerControl.LocalPlayer.IsAlive())
                {
                    if (PlayerControl.LocalPlayer.IsImpostor()) { SetTarget.ImpostorSetTarget(); }
                    NormalButtonDestroy.SetActiveState();
                    switch (PlayerControl.LocalPlayer.GetRole())
                    {
                        case RoleId.Pursuer:
                            Pursuer.PursureUpdate.Postfix();
                            break;
                        case RoleId.Levelinger:
                            if (RoleClass.Levelinger.IsPower(RoleClass.Levelinger.LevelPowerTypes.Pursuer))
                            {
                                if (!RoleClass.Pursuer.arrow.arrow.active)
                                {
                                    RoleClass.Pursuer.arrow.arrow.SetActive(true);
                                }
                                Pursuer.PursureUpdate.Postfix();
                            }
                            else
                            {
                                if (RoleClass.Pursuer.arrow.arrow.active)
                                {
                                    RoleClass.Pursuer.arrow.arrow.SetActive(false);
                                }
                            }
                            break;
                        case RoleId.Hawk:
                            Hawk.FixedUpdate.Postfix();
                            break;
                        case RoleId.NiceHawk:
                            NiceHawk.FixedUpdate.Postfix();
                            break;
                        case RoleId.MadHawk:
                            MadHawk.FixedUpdate.Postfix();
                            break;
                        case RoleId.Vampire:
                            Vampire.FixedUpdate.VampireOnly();
                            break;
                        case RoleId.Vulture:
                            if (RoleClass.Vulture.ShowArrows) Vulture.FixedUpdate.Postfix();
                            break;
                        case RoleId.Amnesiac:
                            if (RoleClass.Amnesiac.ShowArrows) Vulture.FixedUpdate.Postfix();
                            break;
                        case RoleId.Mafia:
                            Mafia.FixedUpdate();
                            break;
                        case RoleId.SerialKiller:
                            SerialKiller.FixedUpdate();
                            break;
                        case RoleId.Kunoichi:
                            Kunoichi.Update();
                            break;
                        case RoleId.Revolutionist:
                            Revolutionist.FixedUpdate();
                            break;
                        case RoleId.Spelunker:
                            Spelunker.FixedUpdate();
                            break;
                        case RoleId.SuicidalIdeation:
                            SuicidalIdeation.Postfix();
                            break;
                        case RoleId.Doctor:
                            Doctor.FixedUpdate();
                            break;
                        case RoleId.Psychometrist:
                            Psychometrist.PsychometristFixedUpdate();
                            break;
                        case RoleId.SeeThroughPerson:
                            SeeThroughPerson.FixedUpdate();
                            break;
                        case RoleId.Hitman:
                            Hitman.FixedUpdate();
                            break;
                        case RoleId.Photographer:
                            Photographer.FixedUpdate();
                            break;
                        case RoleId.Doppelganger:
                            Doppelganger.FixedUpdate();
                            break;
                        case RoleId.Pavlovsowner:
                            Pavlovsdogs.OwnerFixedUpdate();
                            break;
                        case RoleId.WaveCannonJackal:
                            JackalSeer.JackalSeerFixedPatch.JackalSeerPlayerOutLineTarget();
                            break;
                        case RoleId.ConnectKiller:
                            ConnectKiller.Update();
                            break;
                        case RoleId.ShiftActor:
                            ShiftActor.FixedUpdate();
                            break;
                        case RoleId.Cupid:
                            Cupid.FixedUpdate();
                            break;
                        case RoleId.Dependents:
                            Vampire.FixedUpdate.DependentsOnly();
                            break;
                        case RoleId.Pteranodon:
                            Pteranodon.FixedUpdate();
                            break;
                        case RoleId.EvilSeer:
                            EvilSeer.Ability.DeadBodyArrowFixedUpdate();
                            break;
                        case RoleId.PoliceSurgeon:
                            PoliceSurgeon.FixedUpdate();
                            break;
                        case RoleId.Sauner:
                            Sauner.FixedUpdate();
                            break;
                    }
                }
                else // -- 死亡時 --
                {
                    if (Mode.PlusMode.PlusGameOptions.IsClairvoyantZoom)
                    {
                        Clairvoyant.FixedUpdate.Postfix();
                    }
                    switch (PlayerControl.LocalPlayer.GetRole())
                    {
                        case RoleId.Bait:
                            if (!RoleClass.Bait.Reported)
                            {
                                Bait.BaitUpdate.Postfix();
                            }
                            break;
                        case RoleId.SideKiller:
                            if (!RoleClass.SideKiller.IsUpMadKiller)
                            {
                                var sideplayer = RoleClass.SideKiller.GetSidePlayer(PlayerControl.LocalPlayer);
                                if (sideplayer != null)
                                {
                                    sideplayer.RPCSetRoleUnchecked(RoleTypes.Impostor);
                                    RoleClass.SideKiller.IsUpMadKiller = true;
                                }
                            }
                            break;
                        case RoleId.Vulture:
                        case RoleId.Amnesiac:
                        case RoleId.ShermansServant:
                            foreach (var arrow in RoleClass.Vulture.DeadPlayerArrows)
                            {
                                if (arrow.Value?.arrow != null)
                                    Object.Destroy(arrow.Value.arrow);
                                RoleClass.Vulture.DeadPlayerArrows.Remove(arrow.Key);
                            }
                            break;
                        case RoleId.EvilSeer:
                            foreach (var arrow in EvilSeer.RoleData.DeadPlayerArrows)
                            {
                                if (arrow.Value?.arrow != null)
                                    Object.Destroy(arrow.Value.arrow);
                                EvilSeer.RoleData.DeadPlayerArrows.Remove(arrow.Key);
                            };
                            break;
                    }
                }
                break;
            case ModeId.SuperHostRoles:
                Mode.SuperHostRoles.FixedUpdate.Update();
                Penguin.FixedUpdate();
                if (PlayerControl.LocalPlayer.IsRole(RoleId.Mafia))
                {
                    Mafia.FixedUpdate();
                }
                SerialKiller.SHRFixedUpdate(PlayerControl.LocalPlayer.GetRole());
                Camouflager.SHRFixedUpdate();
                if (PlayerControl.LocalPlayer.IsAlive())
                {
                    if (PlayerControl.LocalPlayer.IsImpostor()) { SetTarget.ImpostorSetTarget(); }
                }

                break;
            case ModeId.NotImpostorCheck:
                if (AmongUsClient.Instance.AmHost)
                {
                    BlockTool.FixedUpdate();
                }
                Mode.NotImpostorCheck.NameSet.Postfix();
                break;
            default:
                if (AmongUsClient.Instance.AmHost)
                {
                    BlockTool.FixedUpdate();
                }
                ModeHandler.FixedUpdate(__instance);
                break;
        }
    }
}