using System;
using System.Collections.Generic;
using System.Linq;
using Agartha;
using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using SuperNewRoles.CustomObject;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Modules;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Crewmate;
using SuperNewRoles.Roles.Impostor;
using SuperNewRoles.Roles.Impostor.MadRole;
using SuperNewRoles.Roles.Neutral;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.WaveCannonObj;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.Buttons;

[HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
static class HudManagerStartPatch
{
    #region Buttons
    public static CustomButton DebuggerButton;
    public static CustomButton SheriffKillButton;
    public static CustomButton ClergymanLightOutButton;
    public static CustomButton SpeedBoosterBoostButton;
    public static CustomButton EvilSpeedBoosterBoostButton;
    public static CustomButton LighterLightOnButton;
    public static CustomButton MovingSetButton;
    public static CustomButton MovingTpButton;
    public static CustomButton TeleporterButton;
    public static CustomButton DoorrDoorButton;
    public static CustomButton SelfBomberButton;
    public static CustomButton DoctorVitalsButton;
    public static CustomButton CountChangerButton;
    public static CustomButton ScientistButton;
    public static CustomButton HawkHawkEyeButton;
    public static CustomButton JackalKillButton;
    public static CustomButton JackalSidekickButton;
    public static CustomButton JackalSeerSidekickButton;
    public static CustomButton MagazinerAddButton;
    public static CustomButton MagazinerGetButton;
    public static CustomButton trueloverLoveButton;
    public static CustomButton ImpostorSidekickButton;
    public static CustomButton SideKillerSidekickButton;
    public static CustomButton FalseChargesFalseChargeButton;
    public static CustomButton MadMakerSidekickButton;
    public static CustomButton DemonButton;
    public static CustomButton WaveCannonButton;
    public static CustomButton ArsonistDouseButton;
    public static CustomButton ArsonistIgniteButton;
    public static CustomButton SpeederButton;
    public static CustomButton ChiefSidekickButton;
    public static CustomButton VultureButton;
    public static CustomButton ShielderButton;
    public static CustomButton CleanerButton;
    public static CustomButton MadCleanerButton;
    public static CustomButton FreezerButton;
    public static CustomButton SamuraiButton;
    public static CustomButton VentMakerButton;
    public static CustomButton GhostMechanicRepairButton;
    public static CustomButton EvilHackerButton;
    public static CustomButton EvilHackerMadmateSetting;
    public static CustomButton PositionSwapperButton;
    public static CustomButton KunoichiKunaiButton;
    public static CustomButton KunoichiHideButton;
    public static CustomButton SecretlyKillerMainButton;
    public static CustomButton SecretlyKillerSecretlyKillButton;
    public static CustomButton DoubleKillerMainKillButton;
    public static CustomButton DoubleKillerSubKillButton;
    public static CustomButton SuicideWisherSuicideButton;
    public static CustomButton FastMakerButton;
    public static CustomButton ToiletFanButton;
    public static CustomButton ButtonerButton;
    public static CustomButton RevolutionistButton;
    public static CustomButton SuicidalIdeationButton;
    public static CustomButton HitmanKillButton;
    public static CustomButton MatryoshkaButton;
    public static CustomButton NunButton;
    public static CustomButton PsychometristButton;
    public static CustomButton PartTimerButton;
    public static CustomButton PainterButton;
    public static CustomButton PhotographerButton;
    public static CustomButton StefinderKillButton;
    public static CustomButton CrackerButton;
    public static CustomButton DoppelgangerButton;
    public static CustomButton PavlovsownerCreatedogButton;
    public static CustomButton PavlovsdogKillButton;
    public static CustomButton CamouflagerButton;
    public static CustomButton PenguinButton;
    public static CustomButton VampireCreateDependentsButton;
    public static CustomButton DependentsKillButton;
    public static CustomButton LoversBreakerButton;
    public static CustomButton JumboKillButton;
    public static CustomButton WiseManButton;
    public static CustomButton MechanicButton;
    public static CustomButton PteranodonButton;

    #endregion

    #region Texts
    public static TMPro.TMP_Text sheriffNumShotsText;
    public static TMPro.TMP_Text PavlovsdogKillSelfText;
    public static TMPro.TMP_Text GhostMechanicNumRepairText;
    public static TMPro.TMP_Text PositionSwapperNumText;
    public static TMPro.TMP_Text SecretlyKillNumText;
    #endregion

    public static void SetCustomButtonCooldowns()
    {
        Sheriff.ResetKillCooldown();
        Clergyman.ResetCooldown();
        Teleporter.ResetCooldown();
        Jackal.ResetCooldown();
        //クールダウンリセット
    }

    public static PlayerControl SetTarget(List<PlayerControl> untarget = null, bool Crewmateonly = false)
    {
        return PlayerControlFixedUpdatePatch.SetTarget(untargetablePlayers: untarget, onlyCrewmates: Crewmateonly);
    }
    public static Vent SetTargetVent(List<Vent> untarget = null, bool forceout = false)
    {
        return ModHelpers.SetTargetVent(untargetablePlayers: untarget, forceout: forceout);
    }

    public static void Postfix(HudManager __instance)
    {
        Roles.Attribute.Debugger.canSeeRole = false;

        PteranodonButton = new(
            () =>
            {
                AirshipStatus status = ShipStatus.Instance.TryCast<AirshipStatus>();
                if (status == null)
                    return;
                Pteranodon.IsPteranodonNow = true;
                Pteranodon.StartPosition = PlayerControl.LocalPlayer.transform.position;
                Pteranodon.CurrentPosition = PlayerControl.LocalPlayer.transform.position;
                bool IsRight = true;
                if (Vector3.Distance(status.GapPlatform.transform.parent.TransformPoint(status.GapPlatform.LeftUsePosition), PlayerControl.LocalPlayer.transform.position) <= 0.9f)
                {
                    Pteranodon.TargetPosition = status.GapPlatform.transform.parent.TransformPoint(status.GapPlatform.RightUsePosition);
                }
                else
                {
                    IsRight = false;
                    Pteranodon.TargetPosition = status.GapPlatform.transform.parent.TransformPoint(status.GapPlatform.LeftUsePosition);
                }
                PlayerControl.LocalPlayer.moveable = false;
                PlayerControl.LocalPlayer.Collider.enabled = false;
                Pteranodon.Timer = Pteranodon.StartTime;

                Vector3 position = PlayerControl.LocalPlayer.transform.position;
                byte[] buff = new byte[sizeof(float) * 3];
                Buffer.BlockCopy(BitConverter.GetBytes(position.x), 0, buff, 0 * sizeof(float), sizeof(float));
                Buffer.BlockCopy(BitConverter.GetBytes(position.y), 0, buff, 1 * sizeof(float), sizeof(float));
                Buffer.BlockCopy(BitConverter.GetBytes(position.z), 0, buff, 2 * sizeof(float), sizeof(float));

                MessageWriter writer = RPCHelper.StartRPC(CustomRPC.PteranodonSetStatus);
                writer.Write(PlayerControl.LocalPlayer.PlayerId);
                writer.Write(true);
                writer.Write(IsRight);
                writer.Write(Pteranodon.TargetPosition.x - Pteranodon.StartPosition.x);
                writer.Write(buff.Length);
                writer.Write(buff);
                writer.EndRPC();
                PteranodonButton.MaxTimer = Pteranodon.PteranodonCoolTime.GetFloat();
                PteranodonButton.Timer = PteranodonButton.MaxTimer;
                //RPCProcedure.PteranodonSetStatus(PlayerControl.LocalPlayer.PlayerId, true);
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Pteranodon; },
            () =>
            {
                if (!PlayerControl.LocalPlayer.CanMove) return false;
                AirshipStatus status = ShipStatus.Instance.TryCast<AirshipStatus>();
                if (status == null)
                    return false;
                if (status.GapPlatform.Target != null && status.GapPlatform.Target.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                {
                    return false;
                }
                bool flag = Vector3.Distance(status.GapPlatform.transform.parent.TransformPoint(status.GapPlatform.LeftUsePosition), PlayerControl.LocalPlayer.transform.position) <= 0.9f || Vector3.Distance(status.GapPlatform.transform.parent.TransformPoint(status.GapPlatform.RightUsePosition), PlayerControl.LocalPlayer.transform.position) <= 0.9f;
                return flag;
            },
            () =>
            {
                PteranodonButton.MaxTimer = Pteranodon.PteranodonCoolTime.GetFloat();
                PteranodonButton.Timer = PteranodonButton.MaxTimer;
            },
            ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.PteranodonButton.png", 115f),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("PteranodonButtonName"),
            showButtonText = true
        };

        WiseManButton = new(
            () =>
            {
                if (WiseManButton.isEffectActive)
                {
                    WiseMan.RpcSetWiseManStatus(0f, false);
                    WiseManButton.MaxTimer = WiseMan.WiseManCoolTime.GetFloat();
                    WiseManButton.Timer = WiseManButton.MaxTimer;
                    Camera.main.GetComponent<FollowerCamera>().Locked = false;
                    PlayerControl.LocalPlayer.moveable = true;
                    return;
                }
                WiseMan.RpcSetWiseManStatus(WiseMan.GetRandomAngle, true);
                Camera.main.GetComponent<FollowerCamera>().Locked = true;
                PlayerControl.LocalPlayer.moveable = false;
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.WiseMan; },
            () =>
            {
                return PlayerControl.LocalPlayer.CanMove;
            },
            () =>
            {
                WiseManButton.MaxTimer = WiseMan.WiseManCoolTime.GetFloat();
                WiseManButton.Timer = WiseManButton.MaxTimer;
                WiseManButton.effectCancellable = false;
                WiseManButton.EffectDuration = WiseMan.WiseManDurationTime.GetFloat();
                WiseManButton.HasEffect = true;
            },
            WiseMan.GetButtonSprite(), new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; },
            true,
            5f,
            () =>
            {
                WiseMan.RpcSetWiseManStatus(0, false);
                Camera.main.GetComponent<FollowerCamera>().Locked = false;
                PlayerControl.LocalPlayer.moveable = true;
                WiseManButton.MaxTimer = WiseMan.WiseManCoolTime.GetFloat();
                WiseManButton.Timer = WiseManButton.MaxTimer;
            }
        )
        {
            buttonText = ModTranslation.GetString("WiseManButtonName"),
            showButtonText = true
        };

        WaveCannonButton = new(
            () =>
            {
                var pos = CachedPlayer.LocalPlayer.transform.position;
                MessageWriter writer = RPCHelper.StartRPC(CustomRPC.WaveCannon);
                writer.Write((byte)WaveCannonObject.RpcType.Spawn);
                writer.Write((byte)0);
                writer.Write(CachedPlayer.LocalPlayer.PlayerPhysics.FlipX);
                writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                writer.Write(pos.x);
                writer.Write(pos.y);
                writer.Write((byte)WaveCannonJackal.WaveCannonJackalAnimTypeOption.GetSelection());
                writer.EndRPC();
                RPCProcedure.WaveCannon((byte)WaveCannonObject.RpcType.Spawn, 0, CachedPlayer.LocalPlayer.PlayerPhysics.FlipX, CachedPlayer.LocalPlayer.PlayerId, pos, (WaveCannonObject.WCAnimType)WaveCannonJackal.WaveCannonJackalAnimTypeOption.GetSelection());
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.WaveCannonJackal && (!WaveCannonJackal.IwasSidekicked.Contains(PlayerControl.LocalPlayer.PlayerId) || WaveCannonJackal.WaveCannonJackalNewJackalHaveWaveCannon.GetBool()); },
            () =>
            {
                return PlayerControl.LocalPlayer.CanMove;
            },
            () =>
            {
                WaveCannonButton.MaxTimer = WaveCannonJackal.WaveCannonJackalCoolTime.GetFloat();
                WaveCannonButton.Timer = WaveCannonButton.MaxTimer;
                WaveCannonButton.effectCancellable = false;
                WaveCannonButton.EffectDuration = WaveCannonJackal.WaveCannonJackalChargeTime.GetFloat();
                WaveCannonButton.HasEffect = true;
            },
            ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.WaveCannonButton.png", 115f),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; },
            true,
            5f,
            () =>
            {
                WaveCannonObject obj = WaveCannonObject.Objects.Values.FirstOrDefault(x => x.Owner != null && x.Owner.PlayerId == CachedPlayer.LocalPlayer.PlayerId && x.Id == WaveCannonObject.Ids[CachedPlayer.LocalPlayer.PlayerId] - 1);
                if (obj == null)
                {
                    Logger.Info("nullなのでreturnしました", "WaveCannonButton");
                    return;
                }
                var pos = CachedPlayer.LocalPlayer.transform.position;
                byte[] buff = new byte[sizeof(float) * 2];
                Buffer.BlockCopy(BitConverter.GetBytes(pos.x), 0, buff, 0 * sizeof(float), sizeof(float));
                Buffer.BlockCopy(BitConverter.GetBytes(pos.y), 0, buff, 1 * sizeof(float), sizeof(float));
                MessageWriter writer = RPCHelper.StartRPC(CustomRPC.WaveCannon);
                writer.Write((byte)WaveCannonObject.RpcType.Shoot);
                writer.Write((byte)obj.Id);
                writer.Write(CachedPlayer.LocalPlayer.PlayerPhysics.FlipX);
                writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                writer.Write(pos.x);
                writer.Write(pos.y);
                writer.Write((byte)0);
                writer.EndRPC();
                RPCProcedure.WaveCannon((byte)WaveCannonObject.RpcType.Shoot, (byte)obj.Id, CachedPlayer.LocalPlayer.PlayerPhysics.FlipX, CachedPlayer.LocalPlayer.PlayerId, pos, WaveCannonObject.WCAnimType.Default);
            }
        )
        {
            buttonText = ModTranslation.GetString("WaveCannonButtonName"),
            showButtonText = true
        };

        MechanicButton = new(
            () =>
            {
                if (MechanicButton.isEffectActive)
                {
                    Vector3 truepos = PlayerControl.LocalPlayer.GetTruePosition();
                    NiceMechanic.RpcSetVentStatusMechanic(PlayerControl.LocalPlayer, SetTargetVent(forceout: true), false, new(truepos.x, truepos.y, truepos.z + 0.0025f));
                    MechanicButton.MaxTimer = PlayerControl.LocalPlayer.IsRole(RoleId.NiceMechanic) ? NiceMechanic.NiceMechanicCoolTime.GetFloat() : EvilMechanic.EvilMechanicCoolTime.GetFloat();
                    MechanicButton.Timer = MechanicButton.MaxTimer;
                    MechanicButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                    return;
                }
                NiceMechanic.RpcSetVentStatusMechanic(PlayerControl.LocalPlayer, SetTargetVent(), true);
            },
            (bool isAlive, RoleId role) => { return isAlive && role is RoleId.NiceMechanic or RoleId.EvilMechanic; },
            () =>
            {
                return PlayerControl.LocalPlayer.CanMove && SetTargetVent();
            },
            () =>
            {
                MechanicButton.MaxTimer = PlayerControl.LocalPlayer.IsRole(RoleId.NiceMechanic) ? NiceMechanic.NiceMechanicCoolTime.GetFloat() : EvilMechanic.EvilMechanicCoolTime.GetFloat();
                MechanicButton.Timer = MechanicButton.MaxTimer;
                MechanicButton.effectCancellable = true;
                MechanicButton.EffectDuration = PlayerControl.LocalPlayer.IsRole(RoleId.NiceMechanic) ? NiceMechanic.NiceMechanicDurationTime.GetFloat() : EvilMechanic.EvilMechanicDurationTime.GetFloat();
                MechanicButton.HasEffect = true;
            },
            // FIXME: EvilMechanicでもNiceMechanicのボタンが表示されている状態です。変える方法分かったら変えて下さい…
            PlayerControl.LocalPlayer.IsImpostor() ? Roles.Impostor.EvilMechanic.GetButtonSprite() : Roles.Crewmate.NiceMechanic.GetButtonSprite(),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; },
            true,
            5f,
            () =>
            {
                Vector3 truepos = PlayerControl.LocalPlayer.GetTruePosition();
                NiceMechanic.RpcSetVentStatusMechanic(PlayerControl.LocalPlayer, SetTargetVent(forceout: true), false, new(truepos.x, truepos.y, truepos.z + 0.0025f));
                MechanicButton.MaxTimer = PlayerControl.LocalPlayer.IsRole(RoleId.NiceMechanic) ? NiceMechanic.NiceMechanicCoolTime.GetFloat() : EvilMechanic.EvilMechanicCoolTime.GetFloat();
                MechanicButton.Timer = MechanicButton.MaxTimer;
            }
        )
        {
            buttonText = ModTranslation.GetString("MechanicButtonName"),
            showButtonText = true
        };

        DebuggerButton = new(
            () =>
            {
                RoleTypes myrole = PlayerControl.LocalPlayer.Data.Role.Role;
                DestroyableSingleton<RoleManager>.Instance.SetRole(PlayerControl.LocalPlayer, RoleTypes.Shapeshifter);
                CachedPlayer.LocalPlayer.Data.Role.TryCast<ShapeshifterRole>().UseAbility();
                DestroyableSingleton<RoleManager>.Instance.SetRole(PlayerControl.LocalPlayer, myrole);
            },
            (bool isAlive, RoleId role) => { return RoleClass.Debugger.AmDebugger; },
            () =>
            {
                return PlayerControl.LocalPlayer.CanMove;
            },
            () => { },
            RoleClass.Debugger.GetButtonSprite(),
            new Vector3(0, 2, 0),
            __instance,
            __instance.AbilityButton,
            null,
            0,
            () => { return false; }
        )
        {
            Timer = 0f,
            MaxTimer = 0f,
            buttonText = ModTranslation.GetString("DebuggerButtonName"),
        };

        JumboKillButton = new(
            () =>
            {
                float killTimer = PlayerControl.LocalPlayer.killTimer;
                ModHelpers.CheckMurderAttemptAndKill(PlayerControl.LocalPlayer, SetTarget(Crewmateonly: true));
                RoleClass.Jumbo.Killed = true;
                PlayerControl.LocalPlayer.killTimer = killTimer;
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Jumbo && PlayerControl.LocalPlayer.IsImpostor() && !RoleClass.Jumbo.Killed && RoleClass.Jumbo.JumboSize.ContainsKey(PlayerControl.LocalPlayer.PlayerId) && RoleClass.Jumbo.JumboSize[PlayerControl.LocalPlayer.PlayerId] >= (CustomOptionHolder.JumboMaxSize.GetFloat() / 10); },
            () =>
            {
                return SetTarget(Crewmateonly: true) && PlayerControl.LocalPlayer.CanMove;
            },
            () =>
            {
                JumboKillButton.MaxTimer = GameManager.Instance.LogicOptions.currentGameOptions.GetFloat(FloatOptionNames.KillCooldown);
                JumboKillButton.Timer = JumboKillButton.MaxTimer;
            },
            __instance.KillButton.graphic.sprite,
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.KillButton,
            KeyCode.F,
            49,
            () => false
        )
        {
            buttonText = FastDestroyableSingleton<HudManager>.Instance.KillButton.buttonLabelText.text,
            showButtonText = true
        };

        LoversBreakerButton = new(
            () =>
            {
                PlayerControl Target = SetTarget();
                if (Target.IsLovers() || Target.IsRole(RoleId.truelover, RoleId.Cupid))
                {
                    PlayerControl.LocalPlayer.RpcMurderPlayer(Target, true);
                    Target.RpcSetFinalStatus(FinalStatus.LoversBreakerKill);
                    LoversBreakerButton.MaxTimer = CustomOptionHolder.LoversBreakerCoolTime.GetFloat();
                    LoversBreakerButton.Timer = LoversBreakerButton.MaxTimer;
                    if (Target.IsRole(RoleId.Cupid) && !Target.IsLovers()) return;
                    RoleClass.LoversBreaker.BreakCount--;
                    if (RoleClass.LoversBreaker.BreakCount <= 0)
                    {
                        bool IsAliveLovers = false;
                        foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                        {
                            if (p.IsAlive() && (p.IsLovers() || p.IsRole(RoleId.truelover) || (p.TryGetRoleBase<Cupid>(out Cupid cupid) & cupid.Created)))
                            {
                                IsAliveLovers = true;
                                break;
                            }
                        }
                        if (!IsAliveLovers)
                        {
                            MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareWinner, SendOption.Reliable, -1);
                            Writer.Write(PlayerControl.LocalPlayer.PlayerId);
                            AmongUsClient.Instance.FinishRpcImmediately(Writer);
                            RPCProcedure.ShareWinner(PlayerControl.LocalPlayer.PlayerId);
                            if (AmongUsClient.Instance.AmHost)
                            {
                                GameManager.Instance.RpcEndGame((GameOverReason)CustomGameOverReason.LoversBreakerWin, false);
                            }
                            else
                            {
                                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomEndGame, SendOption.Reliable, -1);
                                writer.Write((byte)CustomGameOverReason.LoversBreakerWin);
                                writer.Write(false);
                                AmongUsClient.Instance.FinishRpcImmediately(writer);
                            }
                        }
                        else
                        {
                            MessageWriter writer = RPCHelper.StartRPC(CustomRPC.SetLoversBreakerWinner);
                            writer.Write(PlayerControl.LocalPlayer.PlayerId);
                            writer.EndRPC();
                            RPCProcedure.SetLoversBreakerWinner(PlayerControl.LocalPlayer.PlayerId);
                        }
                    }
                }
                else
                {
                    PlayerControl.LocalPlayer.RpcMurderPlayer(PlayerControl.LocalPlayer, true);
                    PlayerControl.LocalPlayer.RpcSetFinalStatus(FinalStatus.SuicideWisherSelfDeath);
                }
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.LoversBreaker; },
            () =>
            {
                PlayerControl Target = SetTarget();
                PlayerControlFixedUpdatePatch.SetPlayerOutline(Target, RoleClass.LoversBreaker.color);
                return Target && PlayerControl.LocalPlayer.CanMove;
            },
            () =>
            {
                LoversBreakerButton.MaxTimer = CustomOptionHolder.LoversBreakerCoolTime.GetFloat();
                LoversBreakerButton.Timer = LoversBreakerButton.MaxTimer;
            },
            ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.LoversBreakerButton.png", 115f),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("LoversBreakerButtonName"),
            showButtonText = true
        };

        DependentsKillButton = new(
            () =>
            {
                ModHelpers.CheckMurderAttemptAndKill(PlayerControl.LocalPlayer, SetTarget(untarget: RoleClass.Vampire.VampirePlayer));
                DependentsKillButton.MaxTimer = CustomOptionHolder.VampireDependentsKillCoolTime.GetFloat();
                DependentsKillButton.Timer = DependentsKillButton.MaxTimer;
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Dependents; },
            () =>
            {
                return SetTarget(untarget: RoleClass.Vampire.VampirePlayer) && PlayerControl.LocalPlayer.CanMove;
            },
            () =>
            {
                DependentsKillButton.MaxTimer = CustomOptionHolder.VampireDependentsKillCoolTime.GetFloat();
                DependentsKillButton.Timer = DependentsKillButton.MaxTimer;
            },
            __instance.KillButton.graphic.sprite,
            new Vector3(0, 1, 0),
            __instance,
            __instance.KillButton,
            KeyCode.Q,
            8,
            () => { return false; }
        )
        {
            buttonText = FastDestroyableSingleton<HudManager>.Instance.KillButton.buttonLabelText.text,
            showButtonText = true
        };

        VampireCreateDependentsButton = new(
            () =>
            {
                var target = SetTarget(Crewmateonly: true);
                target.SetRoleRPC(RoleId.Dependents);
                target.RPCSetRoleUnchecked(RoleTypes.Crewmate);
                RoleClass.Vampire.CreatedDependents = true;
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Vampire && !RoleClass.Vampire.CreatedDependents; },
            () =>
            {
                PlayerControl target = SetTarget(Crewmateonly: true);
                return target && !Frankenstein.IsMonster(target) && PlayerControl.LocalPlayer.CanMove;
            },
            () =>
            {
                VampireCreateDependentsButton.MaxTimer = CustomOptionHolder.VampireCreateDependentsCoolTime.GetFloat();
                VampireCreateDependentsButton.Timer = VampireCreateDependentsButton.MaxTimer;
            },
            RoleClass.Vampire.GetButtonSprite(),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("VampireDependentsButtonName"),
            showButtonText = true
        };

        PavlovsdogKillButton = new(
            () =>
            {
                PlayerControl target = Pavlovsdogs.SetTarget(false);
                ModHelpers.CheckMurderAttemptAndKill(PlayerControl.LocalPlayer, target);
                PavlovsdogKillButton.MaxTimer = RoleClass.Pavlovsdogs.IsOwnerDead ? CustomOptionHolder.PavlovsdogRunAwayKillCoolTime.GetFloat() : CustomOptionHolder.PavlovsdogKillCoolTime.GetFloat();
                PavlovsdogKillButton.Timer = PavlovsdogKillButton.MaxTimer;
                if (target.IsRole(RoleId.Fox) && RoleClass.Fox.Killer.Contains(PlayerControl.LocalPlayer.PlayerId)) return;
                RoleClass.Pavlovsdogs.DeathTime = CustomOptionHolder.PavlovsdogRunAwayDeathTime.GetFloat();
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Pavlovsdogs; },
            () =>
            {
                if (RoleClass.Pavlovsdogs.IsOwnerDead && CachedPlayer.LocalPlayer.IsAlive())
                {
                    RoleClass.Pavlovsdogs.DeathTime -= Time.deltaTime;
                    PavlovsdogKillSelfText.text = RoleClass.Pavlovsdogs.DeathTime > 0 ? string.Format(ModTranslation.GetString("SerialKillerSuicideText"), ((int)RoleClass.Pavlovsdogs.DeathTime) + 1) : "";
                    if (RoleClass.Pavlovsdogs.DeathTime <= 0)
                    {
                        PlayerControl.LocalPlayer.RpcMurderPlayer(PlayerControl.LocalPlayer, true);
                    }
                }
                var Target = SetTarget();
                PlayerControlFixedUpdatePatch.SetPlayerOutline(Target, RoleClass.Pavlovsdogs.color);
                return Pavlovsdogs.SetTarget(false) && PlayerControl.LocalPlayer.CanMove;
            },
            () =>
            {
                if (CustomOptionHolder.PavlovsdogRunAwayDeathTimeIsMeetingReset.GetBool()) RoleClass.Pavlovsdogs.DeathTime = CustomOptionHolder.PavlovsdogRunAwayDeathTime.GetFloat();
                PavlovsdogKillButton.MaxTimer = RoleClass.Pavlovsdogs.IsOwnerDead ? CustomOptionHolder.PavlovsdogRunAwayKillCoolTime.GetFloat() : CustomOptionHolder.PavlovsdogKillCoolTime.GetFloat();
                PavlovsdogKillButton.Timer = PavlovsdogKillButton.MaxTimer;
            },
            __instance.KillButton.graphic.sprite,
            new Vector3(0, 1, 0),
            __instance,
            __instance.KillButton,
            KeyCode.Q,
            8,
            () => { return RoleClass.IsMeeting; }
        )
        {
            buttonText = FastDestroyableSingleton<HudManager>.Instance.KillButton.buttonLabelText.text,
            showButtonText = true
        };

        PavlovsdogKillSelfText = GameObject.Instantiate(PavlovsdogKillButton.actionButton.cooldownTimerText, PavlovsdogKillButton.actionButton.cooldownTimerText.transform.parent);
        PavlovsdogKillSelfText.text = "";
        PavlovsdogKillSelfText.enableWordWrapping = false;
        PavlovsdogKillSelfText.transform.localScale = Vector3.one * 0.5f;
        PavlovsdogKillSelfText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

        PavlovsownerCreatedogButton = new(
            () =>
            {
                PlayerControl target = Pavlovsdogs.SetTarget();
                RoleClass.Pavlovsowner.CreateLimit--;
                bool isSelfDeath = target.IsImpostor() && CustomOptionHolder.PavlovsownerIsTargetImpostorDeath.GetBool();
                MessageWriter writer = RPCHelper.StartRPC(CustomRPC.PavlovsOwnerCreateDog);
                writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                writer.Write(target.PlayerId);
                writer.Write(isSelfDeath);
                writer.EndRPC();
                RPCProcedure.PavlovsOwnerCreateDog(CachedPlayer.LocalPlayer.PlayerId, target.PlayerId, isSelfDeath);
                RoleClass.Pavlovsowner.CurrentChildPlayer = target;
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Pavlovsowner && RoleClass.Pavlovsowner.CanCreateDog; },
            () =>
            {
                var target = SetTarget();
                PlayerControlFixedUpdatePatch.SetPlayerOutline(target, RoleClass.Pavlovsdogs.color);
                target = Pavlovsdogs.SetTarget();
                return PlayerControl.LocalPlayer.CanMove && target && !Frankenstein.IsMonster(target);
            },
            () =>
            {
                PavlovsownerCreatedogButton.MaxTimer = CustomOptionHolder.PavlovsownerCreateCoolTime.GetFloat();
                PavlovsownerCreatedogButton.Timer = PavlovsownerCreatedogButton.MaxTimer;
            },
            RoleClass.Pavlovsowner.GetButtonSprite(),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("PavlovsownerCreatedogButtonName"),
            showButtonText = true
        };

        PenguinButton = new(
            () =>
            {
                PlayerControl target = SetTarget(null, true);
                MessageWriter writer = RPCHelper.StartRPC(CustomRPC.PenguinHikizuri);
                writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                writer.Write(target.PlayerId);
                writer.EndRPC();
                RPCProcedure.PenguinHikizuri(CachedPlayer.LocalPlayer.PlayerId, target.PlayerId);
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Penguin; },
            () =>
            {
                if (PenguinButton.isEffectActive) CustomButton.FillUp(PenguinButton);
                return PlayerControl.LocalPlayer.CanMove && SetTarget(null, true);
            },
            () =>
            {
                PenguinButton.MaxTimer = ModeHandler.IsMode(ModeId.Default) ? CustomOptionHolder.PenguinCoolTime.GetFloat() : RoleClass.IsFirstMeetingEnd ? GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown) : 10;
                PenguinButton.Timer = PenguinButton.MaxTimer;
                PenguinButton.effectCancellable = false;
                PenguinButton.EffectDuration = CustomOptionHolder.PenguinDurationTime.GetFloat();
                PenguinButton.HasEffect = true;
                PenguinButton.Sprite = RoleClass.Penguin.GetButtonSprite();
            },
            RoleClass.Penguin.GetButtonSprite(),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; },
            true,
            5f,
            () =>
            {
                if (ModeHandler.IsMode(ModeId.Default))
                    PlayerControl.LocalPlayer.UncheckedMurderPlayer(RoleClass.Penguin.currentTarget);
            }
        )
        {
            buttonText = ModTranslation.GetString("PenguinButtonName"),
            showButtonText = true
        };

        PhotographerButton = new(
            () =>
            {
                List<byte> targets = Photographer.SetTarget();
                RoleClass.Photographer.PhotedPlayerIds.AddRange(targets);
                PhotographerButton.Timer = RoleClass.Photographer.BonusCount > 0 && targets.Count >= RoleClass.Photographer.BonusCount ? CustomOptionHolder.PhotographerBonusCoolTime.GetFloat() : PhotographerButton.MaxTimer;
                if (CustomOptionHolder.PhotographerIsNotification.GetBool())
                {
                    RPCHelper.StartRPC(CustomRPC.SharePhotograph).EndRPC();
                    RPCProcedure.SharePhotograph();
                }
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Photographer; },
            () =>
            {
                return PlayerControl.LocalPlayer.CanMove && Photographer.SetTarget().Count > 0;
            },
            () =>
            {
                PhotographerButton.MaxTimer = CustomOptionHolder.PhotographerCoolTime.GetFloat();
                PhotographerButton.Timer = PhotographerButton.MaxTimer;
            },
            RoleClass.Photographer.GetButtonSprite(),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; }
            )
        {
            buttonText = ModTranslation.GetString("PhotographerButtonName"),
            showButtonText = true
        };

        KunoichiKunaiButton = new(
            () => { RoleClass.Kunoichi.Kunai.kunai.SetActive(!RoleClass.Kunoichi.Kunai.kunai.active); },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Kunoichi; },
            () => { return PlayerControl.LocalPlayer.CanMove; },
            () =>
            {
                KunoichiKunaiButton.MaxTimer = 0f;
                KunoichiKunaiButton.Timer = 0f;
            },
            RoleClass.Kunoichi.GetButtonSprite(),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("KunoichiKunai"),
            showButtonText = true
        };
        KunoichiHideButton = new CustomButton(
            () =>
            {
                /*  Kunoichi.cs Update() にある、
                    「透明化に必要な待機時間の取得と処理 (ボタン動作の時)」コメント以降のif文の中で透明化の処理を行っている。*/
                RoleClass.Kunoichi.IsHideButton = true;
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Kunoichi && RoleClass.Kunoichi.IsWaitAndPressTheButtonToHide; },
            () =>
            {
                return PlayerControl.LocalPlayer.CanMove;
            },
            () => { Kunoichi.HideOff(); },
            RoleClass.Kunoichi.GetHideButtonSprite(),
            new Vector3(-2.925f, -0.06f, 0),
            __instance,
            __instance.AbilityButton,
            null,
            0,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("ScientistButtonName"),
            showButtonText = true
        };

        CrackerButton = new(
            () =>
            {
                byte targetId = SetTarget(RoleClass.Cracker.CurrentCrackedPlayerControls).PlayerId;
                RoleClass.Cracker.currentCrackedPlayers.Add(targetId);
                RPCHelper.SendSinglePlayerRpc(CustomRPC.CrackerCrack, targetId);
                RPCProcedure.CrackerCrack(targetId);
                RoleClass.Cracker.TurnCount--;
                RoleClass.Cracker.MaxTurnCount--;
                CrackerButton.Timer = CrackerButton.MaxTimer;
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Cracker && RoleClass.Cracker.TurnCount > 0 && RoleClass.Cracker.MaxTurnCount > 0; },
            () =>
            {
                return SetTarget(RoleClass.Cracker.CurrentCrackedPlayerControls) && PlayerControl.LocalPlayer.CanMove;
            },
            () =>
            {
                CrackerButton.MaxTimer = CustomOptionHolder.CrackerCoolTime.GetFloat();
                CrackerButton.Timer = CrackerButton.MaxTimer;
            },
            ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.CrackerButton.png", 115f),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("CrackerButtonName"),
            showButtonText = true
        };

        FalseChargesFalseChargeButton = new(
            () =>
            {
                PlayerControl target = SetTarget();
                if (target && RoleHelpers.IsAlive(PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.CanMove)
                {
                    if (ModeHandler.IsMode(ModeId.SuperHostRoles))
                    {
                        PlayerControl.LocalPlayer.CmdCheckMurder(target);
                    }
                    else
                    {
                        RoleClass.FalseCharges.FalseChargePlayer = target.PlayerId;
                        ModHelpers.UncheckedMurderPlayer(target, PlayerControl.LocalPlayer);
                        PlayerControl.LocalPlayer.RpcSetFinalStatus(FinalStatus.FalseChargesFalseCharge);
                        RoleClass.FalseCharges.Turns = RoleClass.FalseCharges.DefaultTurn;
                    }
                }
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.FalseCharges; },
            () =>
            {
                var Target = SetTarget();
                PlayerControlFixedUpdatePatch.SetPlayerOutline(Target, RoleClass.FalseCharges.color);
                return PlayerControl.LocalPlayer.CanMove && Target;
            },
            () =>
            {
                FalseChargesFalseChargeButton.MaxTimer = RoleClass.FalseCharges.CoolTime;
                FalseChargesFalseChargeButton.Timer = RoleClass.FalseCharges.CoolTime;
            },
            __instance.KillButton.graphic.sprite,
            new Vector3(0, 1, 0),
            __instance,
            __instance.KillButton,
            KeyCode.Q,
            8,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("FalseChargesButtonTitle"),
            showButtonText = true
        };

        trueloverLoveButton = new(
            () =>
            {
                if (PlayerControl.LocalPlayer.CanMove && !RoleClass.Truelover.IsCreate && !PlayerControl.LocalPlayer.IsLovers())
                {
                    var target = SetTarget();
                    if (target == null || target.IsLovers() || target.IsRole(RoleId.LoversBreaker)) return;
                    RoleClass.Truelover.IsCreate = true;
                    RoleHelpers.SetLovers(PlayerControl.LocalPlayer, target);
                    RoleHelpers.SetLoversRPC(PlayerControl.LocalPlayer, target);
                }
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.truelover && !RoleClass.Truelover.IsCreate; },
            () =>
            {
                var Target = SetTarget();
                PlayerControlFixedUpdatePatch.SetPlayerOutline(Target, RoleClass.Truelover.color);
                return PlayerControl.LocalPlayer.CanMove && Target;
            },
            () => { trueloverLoveButton.Timer = 0f; trueloverLoveButton.MaxTimer = 0f; },
            RoleClass.Truelover.GetButtonSprite(),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("trueloverloveButtonName"),
            showButtonText = true
        };

        MagazinerGetButton = new(
            () =>
            {
                if (PlayerControl.LocalPlayer.CanMove && RoleClass.Magaziner.MyPlayerCount >= 1 && FastDestroyableSingleton<HudManager>.Instance.KillButton.isCoolingDown && RoleClass.Magaziner.IsOKSet)
                {
                    PlayerControl.LocalPlayer.SetKillTimerUnchecked(RoleClass.Magaziner.SetTime);
                    RoleClass.Magaziner.MyPlayerCount--;
                    if (RoleClass.Magaziner.SetTime != 0)
                    {
                        RoleClass.Magaziner.IsOKSet = false;
                        new LateTask(() =>
                        {
                            RoleClass.Magaziner.IsOKSet = true;
                        }, 1f, "IsOkSetSet");
                    }
                }
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Magaziner; },
            () =>
            {
                return PlayerControl.LocalPlayer.CanMove && FastDestroyableSingleton<HudManager>.Instance.KillButton.isCoolingDown && RoleClass.Magaziner.MyPlayerCount >= 1;
            },
            () => { MagazinerGetButton.Timer = 0f; MagazinerGetButton.MaxTimer = 0f; },
            RoleClass.Magaziner.GetGetButtonSprite(),
            new Vector3(-2.925f, -0.06f, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("MagazinerGetButtonName"),
            showButtonText = true
        };

        MagazinerAddButton = new(
            () =>
            {
                if (!FastDestroyableSingleton<HudManager>.Instance.KillButton.isCoolingDown && PlayerControl.LocalPlayer.CanMove)
                {
                    PlayerControl.LocalPlayer.SetKillTimerUnchecked(GameManager.Instance.LogicOptions.currentGameOptions.GetFloat(FloatOptionNames.KillCooldown));
                    RoleClass.Magaziner.MyPlayerCount++;
                }
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Magaziner; },
            () =>
            {
                return PlayerControl.LocalPlayer.CanMove && !FastDestroyableSingleton<HudManager>.Instance.KillButton.isCoolingDown;
            },
            () => { MagazinerAddButton.Timer = 0f; MagazinerAddButton.MaxTimer = 0f; },
            RoleClass.Magaziner.GetAddButtonSprite(),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("MagazinerAddButtonName"),
            showButtonText = true
        };

        ScientistButton = new CustomButton(
            () =>
            {
                if (!PlayerControl.LocalPlayer.CanMove) return;
                RoleClass.NiceScientist.ButtonTimer = DateTime.Now;
                ScientistButton.actionButton.cooldownTimerText.color = new Color(0F, 0.8F, 0F);
                Scientist.Start();
            },
            (bool isAlive, RoleId role) => { return isAlive && (role == RoleId.NiceScientist || role == RoleId.EvilScientist); },
            () =>
            {
                if (RoleClass.NiceScientist.IsScientist) CustomButton.FillUp(ScientistButton);
                return PlayerControl.LocalPlayer.CanMove;
            },
            () => { Scientist.EndMeeting(); },
            RoleClass.NiceScientist.GetButtonSprite(),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("ScientistButtonName"),
            showButtonText = true
        };

        HawkHawkEyeButton = new(
            () =>
            {
                if (!PlayerControl.LocalPlayer.CanMove) return;
                var role = PlayerControl.LocalPlayer.GetRole();
                switch (role)
                {
                    case RoleId.Hawk:
                        RoleClass.Hawk.Timer = RoleClass.Hawk.DurationTime;
                        RoleClass.Hawk.ButtonTimer = DateTime.Now;
                        HawkHawkEyeButton.MaxTimer = RoleClass.Hawk.CoolTime;
                        HawkHawkEyeButton.Timer = RoleClass.Hawk.CoolTime;
                        break;
                    case RoleId.NiceHawk:
                        RoleClass.NiceHawk.Timer = RoleClass.NiceHawk.DurationTime;
                        RoleClass.NiceHawk.ButtonTimer = DateTime.Now;
                        HawkHawkEyeButton.MaxTimer = RoleClass.NiceHawk.CoolTime;
                        HawkHawkEyeButton.Timer = RoleClass.NiceHawk.CoolTime;
                        RoleClass.NiceHawk.Postion = CachedPlayer.LocalPlayer.transform.localPosition;
                        RoleClass.NiceHawk.timer1 = 10;
                        RoleClass.NiceHawk.Timer2 = DateTime.Now;
                        break;
                    case RoleId.MadHawk:
                        RoleClass.MadHawk.Timer = RoleClass.MadHawk.DurationTime;
                        RoleClass.MadHawk.ButtonTimer = DateTime.Now;
                        HawkHawkEyeButton.MaxTimer = RoleClass.MadHawk.CoolTime;
                        HawkHawkEyeButton.Timer = RoleClass.MadHawk.CoolTime;
                        RoleClass.MadHawk.Postion = CachedPlayer.LocalPlayer.transform.localPosition;
                        RoleClass.MadHawk.timer1 = 10;
                        RoleClass.MadHawk.Timer2 = DateTime.Now;
                        break;
                }
                RoleClass.Hawk.IsHawkOn = true;
            },
            (bool isAlive, RoleId role) => { return isAlive && (role is RoleId.Hawk or RoleId.NiceHawk or RoleId.MadHawk); },
            () =>
            {
                return PlayerControl.LocalPlayer.CanMove;
            },
            () =>
            {
                float cool = PlayerControl.LocalPlayer.GetRole() switch
                {
                    RoleId.Hawk => RoleClass.Hawk.CoolTime,
                    RoleId.NiceHawk => RoleClass.NiceHawk.CoolTime,
                    RoleId.MadHawk => RoleClass.MadHawk.CoolTime,
                    _ => 0f
                };
                HawkHawkEyeButton.MaxTimer = cool;
                HawkHawkEyeButton.Timer = cool;

                RoleClass.Hawk.IsHawkOn = false;
            },
            RoleClass.Hawk.GetButtonSprite(),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("HawkButtonName"),
            showButtonText = true
        };

        CountChangerButton = new(
            () =>
            {
                if (RoleClass.CountChanger.Count >= 1 && SetTarget() && PlayerControl.LocalPlayer.CanMove)
                {
                    RoleClass.CountChanger.IsSet = true;
                    RoleClass.CountChanger.Count--;
                    var target = PlayerControlFixedUpdatePatch.SetTarget(onlyCrewmates: true);
                    var targetId = target.PlayerId;
                    var localId = CachedPlayer.LocalPlayer.PlayerId;

                    RPCProcedure.CountChangerSetRPC(localId, targetId);
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CountChangerSetRPC, SendOption.Reliable, -1);
                    writer.Write(localId);
                    writer.Write(targetId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                }
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.CountChanger && !RoleClass.CountChanger.IsSet && RoleClass.CountChanger.Count >= 1; },
            () =>
            {
                return PlayerControl.LocalPlayer.CanMove && PlayerControlFixedUpdatePatch.SetTarget(onlyCrewmates: true);
            },
            () =>
            {
                CountChangerButton.MaxTimer = GameManager.Instance.LogicOptions.currentGameOptions.GetFloat(FloatOptionNames.KillCooldown);
                CountChangerButton.Timer = GameManager.Instance.LogicOptions.currentGameOptions.GetFloat(FloatOptionNames.KillCooldown);
            },
            RoleClass.CountChanger.GetButtonSprite(),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("CountChangerButtonName"),
            showButtonText = true
        };

        DoctorVitalsButton = new(
            () =>
            {
                if (RoleClass.Doctor.Vital == null)
                {
                    var moto = PlayerControl.LocalPlayer.Data.Role.Role;
                    FastDestroyableSingleton<RoleManager>.Instance.SetRole(PlayerControl.LocalPlayer, RoleTypes.Scientist);
                    CachedPlayer.LocalPlayer.Data.Role.TryCast<ScientistRole>().UseAbility();
                    FastDestroyableSingleton<RoleManager>.Instance.SetRole(PlayerControl.LocalPlayer, moto);
                    RoleClass.Doctor.Vital = GameObject.FindObjectOfType<VitalsMinigame>();
                }
                RoleClass.Doctor.MyPanelFlag = true;
            },
            (bool isAlive, RoleId role) => { return role == RoleId.Doctor && isAlive; },
            () =>
            {
                if (RoleClass.Doctor.IsChargingNow)
                {
                    DoctorVitalsButton.MaxTimer = 10f;
                    Logger.Info(RoleClass.Doctor.Battery.ToString());
                    DoctorVitalsButton.Timer = RoleClass.Doctor.Battery <= 0 ? 10f : RoleClass.Doctor.Battery / 10f;
                }
                else if (RoleClass.Doctor.Battery > 0)
                {
                    DoctorVitalsButton.MaxTimer = 0f;
                    DoctorVitalsButton.Timer = 0f;
                }
                return (PlayerControl.LocalPlayer.CanMove && RoleClass.Doctor.Battery > 0) || RoleClass.Doctor.IsChargingNow;
            },
            () => { },
            RoleClass.Doctor.GetVitalsSprite(),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("DoctorVitalName"),
            showButtonText = true
        };

        JackalSidekickButton = new(
            () =>
            {
                var target = PlayerControlFixedUpdatePatch.JackalSetTarget();
                if (target && PlayerControl.LocalPlayer.CanMove && RoleClass.Jackal.CanCreateSidekick)
                {
                    if (target.IsRole(RoleId.SideKiller)) // サイドキック相手がマッドキラーの場合
                    {
                        if (!RoleClass.SideKiller.IsUpMadKiller) // サイドキラーが未昇格の場合
                        {
                            var sidePlayer = RoleClass.SideKiller.GetSidePlayer(target); // targetのサイドキラーを取得
                            if (sidePlayer != null && sidePlayer.IsAlive()) // null(作っていない)ならば処理しない
                            {
                                sidePlayer.RPCSetRoleUnchecked(RoleTypes.Impostor);
                                RoleClass.SideKiller.IsUpMadKiller = true;
                            }
                        }
                    }
                    if (RoleClass.Jackal.CanCreateFriend)
                    {
                        Jackal.CreateJackalFriends(target); //クルーにして フレンズにする
                    }
                    else
                    {
                        bool isFakeSidekick = EvilEraser.IsBlockAndTryUse(EvilEraser.BlockTypes.JackalSidekick, target);
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CreateSidekick, SendOption.Reliable, -1);
                        writer.Write(target.PlayerId);
                        writer.Write(isFakeSidekick);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.CreateSidekick(target.PlayerId, isFakeSidekick);
                    }
                    RoleClass.Jackal.CanCreateSidekick = false;
                }
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Jackal && ModeHandler.IsMode(ModeId.Default) && RoleClass.Jackal.CanCreateSidekick; },
            () =>
            {
                PlayerControl target = PlayerControlFixedUpdatePatch.JackalSetTarget();
                return target && !Frankenstein.IsMonster(target) && PlayerControl.LocalPlayer.CanMove;
            },
            () =>
            {
                if (PlayerControl.LocalPlayer.IsRole(RoleId.Jackal)) { Jackal.EndMeeting(); }
            },
            RoleClass.Jackal.GetButtonSprite(),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("JackalCreateSidekickButtonName"),
            showButtonText = true
        };

        JackalSeerSidekickButton = new(
            () =>
            {
                var target = PlayerControlFixedUpdatePatch.JackalSetTarget();
                if (target && RoleHelpers.IsAlive(PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.CanMove && RoleClass.JackalSeer.CanCreateSidekick)
                {
                    if (target.IsRole(RoleId.SideKiller)) // サイドキック相手がマッドキラーの場合
                    {
                        if (!RoleClass.SideKiller.IsUpMadKiller) // サイドキラーが未昇格の場合
                        {
                            var sidePlayer = RoleClass.SideKiller.GetSidePlayer(target); // targetのサイドキラーを取得
                            if (sidePlayer != null && sidePlayer.IsAlive()) // null(作っていない)ならば処理しない
                            {
                                sidePlayer.RPCSetRoleUnchecked(RoleTypes.Impostor);
                                RoleClass.SideKiller.IsUpMadKiller = true;
                            }
                        }
                    }
                    if (RoleClass.JackalSeer.CanCreateFriend)
                    {
                        Jackal.CreateJackalFriends(target); //クルーにして フレンズにする
                    }
                    else
                    {
                        bool IsFakeSidekickSeer = EvilEraser.IsBlockAndTryUse(EvilEraser.BlockTypes.JackalSeerSidekick, target);
                        MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CreateSidekickSeer, SendOption.Reliable, -1);
                        killWriter.Write(target.PlayerId);
                        killWriter.Write(IsFakeSidekickSeer);
                        AmongUsClient.Instance.FinishRpcImmediately(killWriter);
                        RPCProcedure.CreateSidekickSeer(target.PlayerId, IsFakeSidekickSeer);
                    }
                    RoleClass.JackalSeer.CanCreateSidekick = false;
                }
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.JackalSeer && ModeHandler.IsMode(ModeId.Default) && RoleClass.JackalSeer.CanCreateSidekick; },
            () =>
            {
                PlayerControl target = PlayerControlFixedUpdatePatch.JackalSetTarget();
                return target && !Frankenstein.IsMonster(target) && PlayerControl.LocalPlayer.CanMove;
            },
            () =>
            {
                if (PlayerControl.LocalPlayer.IsRole(RoleId.JackalSeer)) { JackalSeer.EndMeeting(); }
            },
            RoleClass.Jackal.GetButtonSprite(),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("JackalCreateSidekickButtonName"),
            showButtonText = true
        };

        WaveCannonJackal.MakeButtons(__instance);

        JackalKillButton = new(
            () =>
            {
                if (PlayerControlFixedUpdatePatch.JackalSetTarget() && RoleHelpers.IsAlive(PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.CanMove)
                {
                    ModHelpers.CheckMurderAttemptAndKill(PlayerControl.LocalPlayer, PlayerControlFixedUpdatePatch.JackalSetTarget());
                    switch (PlayerControl.LocalPlayer.GetRole())
                    {
                        case RoleId.Jackal:
                            Jackal.ResetCooldown();
                            break;
                        case RoleId.JackalSeer:
                            JackalSeer.ResetCooldown();
                            break;
                        case RoleId.TeleportingJackal:
                            TeleportingJackal.ResetCooldowns();
                            break;
                        case RoleId.WaveCannonJackal:
                            WaveCannonJackal.ResetCooldowns();
                            break;
                    }
                }
            },
            (bool isAlive, RoleId role) => { return isAlive && (role == RoleId.Jackal || role == RoleId.TeleportingJackal || role == RoleId.JackalSeer || role == RoleId.WaveCannonJackal) && ModeHandler.IsMode(ModeId.Default); },
            () =>
            {
                return PlayerControlFixedUpdatePatch.JackalSetTarget() && PlayerControl.LocalPlayer.CanMove;
            },
            () =>
            {
                if (PlayerControl.LocalPlayer.IsRole(RoleId.Jackal)) { Jackal.EndMeeting(); }
                else if (PlayerControl.LocalPlayer.IsRole(RoleId.JackalSeer)) { JackalSeer.EndMeeting(); }
                else if (PlayerControl.LocalPlayer.IsRole(RoleId.WaveCannonJackal)) { WaveCannonJackal.EndMeeting(); }
            },
            __instance.KillButton.graphic.sprite,
            new Vector3(0, 1, 0),
            __instance,
            __instance.KillButton,
            KeyCode.Q,
            8,
            () => { return false; }
        )
        {
            buttonText = FastDestroyableSingleton<HudManager>.Instance.KillButton.buttonLabelText.text,
            showButtonText = true
        };

        SelfBomberButton = new(
            () =>
            {
                if (PlayerControl.LocalPlayer.CanMove)
                {
                    SelfBomber.SelfBomb();
                }
            },
            (bool isAlive, RoleId role) => { return isAlive && ModeHandler.IsMode(ModeId.Default) && PlayerControl.LocalPlayer.IsRole(RoleId.SelfBomber); },
            () =>
            {
                return PlayerControl.LocalPlayer.CanMove;
            },
            () => { SelfBomber.ResetCooldown(); },
            RoleClass.SelfBomber.GetButtonSprite(),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("SelfBomberButtonName"),
            showButtonText = true
        };

        DoorrDoorButton = new(
            () =>
            {
                Doorr.DoorrBtn();
                Doorr.ResetCooldown();
            },
            (bool isAlive, RoleId role) => { return isAlive && Doorr.IsDoorr(PlayerControl.LocalPlayer); },
            () =>
            {
                return Doorr.CheckTarget() && PlayerControl.LocalPlayer.CanMove;
            },
            () => { Doorr.EndMeeting(); },
            RoleClass.Doorr.GetButtonSprite(),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("DoorrButtonText"),
            showButtonText = true
        };

        TeleporterButton = new(
            () =>
            {
                if (!PlayerControl.LocalPlayer.CanMove) return;
                RoleClass.Clergyman.ButtonTimer = DateTime.Now;
                TeleporterButton.actionButton.cooldownTimerText.color = new Color(0F, 0.8F, 0F);
                Teleporter.TeleportStart();
                Teleporter.ResetCooldown();
            },
            (bool isAlive, RoleId role) => { return isAlive && (role == RoleId.Teleporter || role == RoleId.TeleportingJackal || role == RoleId.NiceTeleporter || (role == RoleId.Levelinger && RoleClass.Levelinger.IsPower(RoleClass.Levelinger.LevelPowerTypes.Teleporter))); },
            () =>
            {
                return true && PlayerControl.LocalPlayer.CanMove;
            },
            () => { Teleporter.EndMeeting(); },
            RoleClass.Teleporter.GetButtonSprite(),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("TeleporterTeleportButton"),
            showButtonText = true
        };

        MovingSetButton = new(
            () =>
            {
                if (!PlayerControl.LocalPlayer.CanMove) return;
                if (!Moving.IsSetPostion())
                {
                    Moving.SetPostion();
                }
                Moving.ResetCooldown();
            },
            (bool isAlive, RoleId role) => { return isAlive && (role == RoleId.Moving || role == RoleId.EvilMoving || RoleClass.Levelinger.IsPower(RoleClass.Levelinger.LevelPowerTypes.Moving)) && !Moving.IsSetPostion(); },
            () =>
            {
                return true && PlayerControl.LocalPlayer.CanMove;
            },
            () => { Moving.EndMeeting(); },
            RoleClass.Moving.GetNoSetButtonSprite(),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("MovingButtonSetName"),
            showButtonText = true
        };

        MovingTpButton = new CustomButton(
            () =>
            {
                if (!PlayerControl.LocalPlayer.CanMove) return;
                if (Moving.IsSetPostion())
                {
                    Moving.TP();
                }
                Moving.ResetCooldown();
            },
            (bool isAlive, RoleId role) => { return isAlive && (role == RoleId.Moving || role == RoleId.EvilMoving || RoleClass.Levelinger.IsPower(RoleClass.Levelinger.LevelPowerTypes.Moving)) && Moving.IsSetPostion(); },
            () =>
            {
                return true && PlayerControl.LocalPlayer.CanMove;
            },
            () => { Moving.EndMeeting(); },
            RoleClass.Moving.GetSetButtonSprite(),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("MovingButtonTpName"),
            showButtonText = true
        };

        SheriffKillButton = new(
            () =>
            {
                if (PlayerControl.LocalPlayer.IsRole(RoleId.RemoteSheriff))
                {
                    RoleHelpers.UseShapeshift();
                }
                else if (PlayerControl.LocalPlayer.IsRole(RoleId.Sheriff))
                {
                    if (RoleClass.Sheriff.KillMaxCount > 0 && SetTarget())
                    {
                        var target = PlayerControlFixedUpdatePatch.SetTarget();
                        PlayerControlFixedUpdatePatch.SetPlayerOutline(target, RoleClass.Sheriff.color);

                        (var killResult, var suicideResult) = Sheriff.SheriffKillResult(CachedPlayer.LocalPlayer, target);
                        if (killResult.Item1 && target.IsRole(RoleId.Squid) && Squid.IsVigilance.ContainsKey(target.PlayerId) && Squid.IsVigilance[target.PlayerId])
                        {
                            killResult.Item1 = false;
                            Squid.SetVigilance(target, false);
                            Squid.SetSpeedBoost(target);
                            RPCHelper.StartRPC(CustomRPC.ShowFlash, target).EndRPC();
                        }

                        var localId = CachedPlayer.LocalPlayer.PlayerId;
                        var targetId = target.PlayerId;

                        RPCProcedure.SheriffKill(localId, targetId, killResult.Item1, suicideResult.Item1);
                        MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SheriffKill, SendOption.Reliable, -1);
                        killWriter.Write(localId);
                        killWriter.Write(targetId);
                        killWriter.Write(killResult.Item1);
                        killWriter.Write(suicideResult.Item1);
                        AmongUsClient.Instance.FinishRpcImmediately(killWriter);

                        if (killResult.Item1) FinalStatusClass.RpcSetFinalStatus(target, killResult.Item2);
                        if (suicideResult.Item1) FinalStatusClass.RpcSetFinalStatus(CachedPlayer.LocalPlayer, suicideResult.Item2);

                        Sheriff.ResetKillCooldown();
                        RoleClass.Sheriff.KillMaxCount--;
                    }
                }
            },
            (bool isAlive, RoleId role) => { return isAlive && (role == RoleId.RemoteSheriff || (role == RoleId.Sheriff && ModeHandler.IsMode(ModeId.Default))); },
            () =>
            {
                float killCount = 0f;
                bool flag = false;
                if (PlayerControl.LocalPlayer.IsRole(RoleId.RemoteSheriff))
                {
                    killCount = RoleClass.RemoteSheriff.KillMaxCount;
                    flag = true;
                }
                else if (PlayerControl.LocalPlayer.IsRole(RoleId.Sheriff))
                {
                    killCount = RoleClass.Sheriff.KillMaxCount;
                    flag = PlayerControlFixedUpdatePatch.SetTarget() && PlayerControl.LocalPlayer.CanMove;
                    var Target = SetTarget();
                    PlayerControlFixedUpdatePatch.SetPlayerOutline(Target, RoleClass.Sheriff.color);
                }
                if (!Sheriff.IsSheriffButton(PlayerControl.LocalPlayer)) flag = false;
                sheriffNumShotsText.text = killCount > 0 ? string.Format(ModTranslation.GetString("SheriffNumTextName"), killCount) : ModTranslation.GetString("CannotUse");
                return flag;
            },
            () => { Sheriff.EndMeeting(); },
            RoleClass.Sheriff.GetButtonSprite(),
            new Vector3(0f, 1f, 0),
            __instance,
            __instance.KillButton,
            KeyCode.Q,
            8,
            () => { return false; }
        );
        sheriffNumShotsText = GameObject.Instantiate(SheriffKillButton.actionButton.cooldownTimerText, SheriffKillButton.actionButton.cooldownTimerText.transform.parent);
        sheriffNumShotsText.text = "";
        sheriffNumShotsText.enableWordWrapping = false;
        sheriffNumShotsText.transform.localScale = Vector3.one * 0.5f;
        sheriffNumShotsText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);
        SheriffKillButton.buttonText = ModTranslation.GetString("SheriffKillButtonName");
        SheriffKillButton.showButtonText = true;

        ClergymanLightOutButton = new(
            () =>
            {
                if (ClergymanLightOutButton.isEffectActive)
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.RPCClergymanLightOut, SendOption.Reliable, -1);
                    writer.Write(false);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.RPCClergymanLightOut(false);
                    ClergymanLightOutButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
                    ClergymanLightOutButton.MaxTimer = RoleClass.Clergyman.CoolTime;
                    ClergymanLightOutButton.Timer = ClergymanLightOutButton.MaxTimer;
                }
                else
                {
                    Clergyman.LightOutStart();
                }
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Clergyman; },
            () =>
            {
                if (ClergymanLightOutButton.isEffectActive) CustomButton.FillUp(ClergymanLightOutButton);
                return PlayerControl.LocalPlayer.CanMove;
            },
            () => { Clergyman.EndMeeting(); },
            RoleClass.Clergyman.GetButtonSprite(),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; },
            true,
            5f,
            () =>
            {
                ClergymanLightOutButton.MaxTimer = RoleClass.Clergyman.CoolTime;
                ClergymanLightOutButton.Timer = ClergymanLightOutButton.MaxTimer;
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.RPCClergymanLightOut, SendOption.Reliable, -1);
                writer.Write(false);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.RPCClergymanLightOut(false);
            }
        )
        {
            buttonText = ModTranslation.GetString("ClergymanLightOutButtonName"),
            showButtonText = true
        };

        SpeedBoosterBoostButton = new CustomButton(
            () =>
            {
                RoleClass.SpeedBooster.ButtonTimer = DateTime.Now;
                SpeedBoosterBoostButton.actionButton.cooldownTimerText.color = new Color(0F, 0.8F, 0F);
                SpeedBooster.BoostStart();
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.SpeedBooster; },
            () =>
            {
                if (RoleClass.SpeedBooster.IsSpeedBoost) CustomButton.FillUp(SpeedBoosterBoostButton);
                return SpeedBoosterBoostButton.Timer <= 0;
            },
            () => { SpeedBooster.EndMeeting(); },
            RoleClass.SpeedBooster.GetSpeedBoostButtonSprite(),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("SpeedBoosterBoostButtonName"),
            showButtonText = true,
            HasEffect = true
        };

        EvilSpeedBoosterBoostButton = new CustomButton(
            () =>
            {
                RoleClass.EvilSpeedBooster.ButtonTimer = DateTime.Now;
                EvilSpeedBoosterBoostButton.actionButton.cooldownTimerText.color = new Color(0F, 0.8F, 0F);
                EvilSpeedBooster.BoostStart();
            },
            (bool isAlive, RoleId role) => { return isAlive && (role == RoleId.EvilSpeedBooster || RoleClass.Levelinger.IsPower(RoleClass.Levelinger.LevelPowerTypes.SpeedBooster)); },
            () =>
            {
                if (RoleClass.EvilSpeedBooster.IsSpeedBoost) CustomButton.FillUp(EvilSpeedBoosterBoostButton);
                return EvilSpeedBoosterBoostButton.Timer <= 0;
            },
            () => { EvilSpeedBooster.EndMeeting(); },
            RoleClass.SpeedBooster.GetSpeedBoostButtonSprite(),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("EvilSpeedBoosterBoostButtonName"),
            showButtonText = true
        };

        LighterLightOnButton = new CustomButton(
            () =>
            {
                RoleClass.Lighter.IsLightOn = true;
                RoleClass.Lighter.ButtonTimer = DateTime.Now;
                LighterLightOnButton.actionButton.cooldownTimerText.color = new Color(0F, 0.8F, 0F);
                Lighter.LightOnStart();
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Lighter; },
            () =>
            {
                return LighterLightOnButton.Timer <= 0;
            },
            () => { Lighter.EndMeeting(); },
            RoleClass.Lighter.GetButtonSprite(),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("LighterButtonName"),
            showButtonText = true
        };

        ImpostorSidekickButton = new(
            () =>
            {
                var target = SetTarget(Crewmateonly: true);
                if (target && RoleHelpers.IsAlive(PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.CanMove && !RoleClass.Levelinger.IsCreateMadmate)
                {
                    target.SetRoleRPC(RoleId.Madmate);
                    RoleClass.Levelinger.IsCreateMadmate = true;
                }
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Levelinger && RoleClass.Levelinger.IsPower(RoleClass.Levelinger.LevelPowerTypes.Sidekick) && !RoleClass.Levelinger.IsCreateMadmate; },
            () =>
            {
                PlayerControl target = SetTarget(Crewmateonly: true);
                return target && !Frankenstein.IsMonster(target) && PlayerControl.LocalPlayer.CanMove;
            },
            () =>
            {
                ImpostorSidekickButton.MaxTimer = GameManager.Instance.LogicOptions.currentGameOptions.GetFloat(FloatOptionNames.KillCooldown);
                ImpostorSidekickButton.Timer = GameManager.Instance.LogicOptions.currentGameOptions.GetFloat(FloatOptionNames.KillCooldown);
            },
            RoleClass.Jackal.GetButtonSprite(),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("CreateMadmateButton"),
            showButtonText = true
        };

        SideKillerSidekickButton = new(
            () =>
            {
                var target = SetTarget(Crewmateonly: true);
                if (target && RoleHelpers.IsAlive(PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.CanMove && !RoleClass.SideKiller.IsCreateMadKiller)
                {
                    MessageWriter writer = RPCHelper.StartRPC(CustomRPC.SetMadKiller);
                    writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                    writer.Write(target.PlayerId);
                    writer.EndRPC();
                    RPCProcedure.SetMadKiller(CachedPlayer.LocalPlayer.PlayerId, target.PlayerId);
                    RoleClass.SideKiller.IsCreateMadKiller = true;
                    PlayerControl.LocalPlayer.killTimer = RoleClass.SideKiller.KillCoolTime;
                }
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.SideKiller && !RoleClass.SideKiller.IsCreateMadKiller; },
            () =>
            {
                PlayerControl target = SetTarget(Crewmateonly: true);
                return target && !Frankenstein.IsMonster(target) && PlayerControl.LocalPlayer.CanMove;
            },
            () =>
            {
                SideKillerSidekickButton.MaxTimer = RoleClass.SideKiller.KillCoolTime;
                SideKillerSidekickButton.Timer = RoleClass.SideKiller.KillCoolTime;
            },
            RoleClass.Jackal.GetButtonSprite(),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("SideKillerSidekickButtonName"),
            showButtonText = true
        };

        MadMakerSidekickButton = new(
            () =>
            {
                var target = SetTarget();
                if (!target.Data.Role.IsImpostor && target && RoleHelpers.IsAlive(PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.CanMove && !RoleClass.MadMaker.IsCreateMadmate)
                {
                    Madmate.CreateMadmate(target);
                    RoleClass.MadMaker.IsCreateMadmate = true;
                }
                else if (target.Data.Role.IsImpostor)
                {
                    if (ModeHandler.IsMode(ModeId.Default))
                    {
                        if (PlayerControl.LocalPlayer.IsRole(RoleId.MadMaker))
                        {
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.RPCMurderPlayer, SendOption.Reliable, -1);
                            writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                            writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                            writer.Write(byte.MaxValue);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            RPCProcedure.RPCMurderPlayer(CachedPlayer.LocalPlayer.PlayerId, CachedPlayer.LocalPlayer.PlayerId, byte.MaxValue);
                            PlayerControl.LocalPlayer.RpcSetFinalStatus(FinalStatus.MadmakerMisSet);
                        }
                    }
                    else if (ModeHandler.IsMode(ModeId.SuperHostRoles))
                    {
                        PlayerControl.LocalPlayer.CmdCheckMurder(target);
                    }
                }
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.MadMaker && ModeHandler.IsMode(ModeId.Default) && !RoleClass.MadMaker.IsCreateMadmate; },
            () =>
            {
                PlayerControl target = SetTarget();
                return target && !Frankenstein.IsMonster(target) && PlayerControl.LocalPlayer.CanMove;
            },
            () => { },
            RoleClass.Jackal.GetButtonSprite(),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("MadMakerSidekickButtonName"),
            showButtonText = true
        };

        RoleClass.SerialKiller.SuicideKillText = GameObject.Instantiate(FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText, FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText.transform.parent);
        RoleClass.SerialKiller.SuicideKillText.text = "";
        RoleClass.SerialKiller.SuicideKillText.enableWordWrapping = false;
        RoleClass.SerialKiller.SuicideKillText.transform.localScale = Vector3.one * 0.5f;
        RoleClass.SerialKiller.SuicideKillText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

        DemonButton = new(
            () =>
            {
                Demon.DemonCurse(SetTarget(untarget: Demon.GetUntarget()));
                DemonButton.Timer = DemonButton.MaxTimer;
            },
            (bool isAlive, RoleId role) => { return Demon.IsButton(); },
            () =>
            {
                var Target = SetTarget(untarget: Demon.GetUntarget());
                PlayerControlFixedUpdatePatch.SetPlayerOutline(Target, RoleClass.Demon.color);
                return PlayerControl.LocalPlayer.CanMove && Target;
            },
            () =>
            {
                DemonButton.MaxTimer = RoleClass.Demon.CoolTime;
                DemonButton.Timer = RoleClass.Demon.CoolTime;
            },
            RoleClass.Demon.GetButtonSprite(),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("DemonButtonName"),
            showButtonText = true
        };

        ArsonistDouseButton = new(
            () =>
            {
                var target = SetTarget(untarget: Arsonist.GetUntarget());
                RoleClass.Arsonist.DouseTarget = target;
                ArsonistDouseButton.MaxTimer = RoleClass.Arsonist.DurationTime;
                ArsonistDouseButton.Timer = ArsonistDouseButton.MaxTimer;
                ArsonistDouseButton.actionButton.cooldownTimerText.color = new Color(0F, 0.8F, 0F);
                RoleClass.Arsonist.IsDouse = true;
                //SuperNewRolesPlugin.Logger.LogInfo("アーソニストが塗るボタンを押した");
            },
            (bool isAlive, RoleId role) => { return Arsonist.IsButton(); },
            () =>
            {
                var Target = SetTarget(untarget: Arsonist.GetUntarget());
                PlayerControlFixedUpdatePatch.SetPlayerOutline(Target, RoleClass.Arsonist.color);
                return PlayerControl.LocalPlayer.CanMove && Target;
            },
            () =>
            {
                ArsonistDouseButton.MaxTimer = RoleClass.Arsonist.CoolTime;
                ArsonistDouseButton.Timer = RoleClass.Arsonist.CoolTime;
            },
            RoleClass.Arsonist.GetDouseButtonSprite(),
            new Vector3(0f, 1f, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.Q,
            8,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("ArsonistDouseButtonName"),
            showButtonText = true
        };

        ArsonistIgniteButton = new(
            () =>
            {
                Arsonist.SetWinArsonist();
                RoleClass.Arsonist.TriggerArsonistWin = true;
                AdditionalTempData.winCondition = WinCondition.ArsonistWin;
                RPCProcedure.ShareWinner(CachedPlayer.LocalPlayer.PlayerId);
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareWinner, SendOption.Reliable, -1);
                writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);

                Arsonist.SettingAfire();

                writer = RPCHelper.StartRPC(CustomRPC.SetWinCond);
                writer.Write((byte)CustomGameOverReason.ArsonistWin);
                writer.EndRPC();
                RPCProcedure.SetWinCond((byte)CustomGameOverReason.ArsonistWin);
                //SuperNewRolesPlugin.Logger.LogInfo("CheckAndEndGame");
                var reason = (GameOverReason)CustomGameOverReason.ArsonistWin;
                if (ModeHandler.IsMode(ModeId.SuperHostRoles)) reason = GameOverReason.ImpostorByKill;
                if (AmongUsClient.Instance.AmHost)
                {
                    CheckGameEndPatch.CustomEndGame(reason, false);
                }
                else
                {
                    MessageWriter writer2 = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomEndGame, SendOption.Reliable, -1);
                    writer2.Write((byte)reason);
                    writer2.Write(false);
                    AmongUsClient.Instance.FinishRpcImmediately(writer2);
                }
            },
            (bool isAlive, RoleId role) => { return Arsonist.IseveryButton(); },
            () =>
            {
                return Arsonist.IsWin(PlayerControl.LocalPlayer);
            },
            () =>
            {
                ArsonistIgniteButton.MaxTimer = 0;
                ArsonistIgniteButton.Timer = 0;
            },
            RoleClass.Arsonist.GetIgniteButtonSprite(),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("ArsonistIgniteButtonName"),
            showButtonText = true
        };

        SpeederButton = new(
            () =>
            {
                Speeder.DownStart();
                SpeederButton.MaxTimer = RoleClass.Speeder.CoolTime;
                SpeederButton.Timer = SpeederButton.MaxTimer;
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Speeder; },
            () =>
            {
                if (SpeederButton.isEffectActive) CustomButton.FillUp(SpeederButton);
                return PlayerControl.LocalPlayer.CanMove;
            },
            Speeder.EndMeeting,
            RoleClass.Speeder.GetButtonSprite(),

            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; },
            true,
            5f,
            Speeder.SpeedDownEnd
        )
        {
            buttonText = ModTranslation.GetString("SpeederButtonName"),
            showButtonText = true,
            HasEffect = true
        };

        ChiefSidekickButton = new(
            () =>
            {
                var target = SetTarget();
                if (target && !RoleClass.Chief.IsCreateSheriff)
                {
                    if (!target.IsImpostor())
                    {
                        MessageWriter writer = RPCHelper.StartRPC(CustomRPC.ChiefSidekick);
                        writer.Write(target.PlayerId);
                        writer.Write(target.IsClearTask());
                        RPCHelper.EndRPC(writer);
                        RPCProcedure.ChiefSidekick(target.PlayerId, target.IsClearTask());
                        RoleClass.Chief.IsCreateSheriff = true;
                    }
                    else
                    {
                        PlayerControl.LocalPlayer.RpcMurderPlayer(PlayerControl.LocalPlayer, true);
                        PlayerControl.LocalPlayer.RpcSetFinalStatus(FinalStatus.ChiefMisSet);
                    }
                }
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Chief && ModeHandler.IsMode(ModeId.Default) && !RoleClass.Chief.IsCreateSheriff; },
            () =>
            {
                var target = SetTarget();
                PlayerControlFixedUpdatePatch.SetPlayerOutline(target, RoleClass.Chief.color);
                return target && !Frankenstein.IsMonster(target) && PlayerControl.LocalPlayer.CanMove;
            },
            () => { },
            RoleClass.Chief.GetButtonSprite(),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("ChiefSidekickButtonName"),
            showButtonText = true
        };

        VultureButton = new(
            () =>
            {
                Vulture.RpcCleanDeadBody(RoleClass.Vulture.DeadBodyCount);
                RoleClass.Vulture.DeadBodyCount--;

                if (RoleClass.Vulture.DeadBodyCount <= 0)
                {
                    RPCProcedure.ShareWinner(CachedPlayer.LocalPlayer.PlayerId);
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareWinner, SendOption.Reliable, -1);
                    writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    if (AmongUsClient.Instance.AmHost)
                    {
                        CheckGameEndPatch.CustomEndGame((GameOverReason)CustomGameOverReason.VultureWin, false);
                    }
                    else
                    {
                        MessageWriter writer2 = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomEndGame, SendOption.Reliable, -1);
                        writer2.Write((byte)CustomGameOverReason.VultureWin);
                        writer2.Write(false);
                        AmongUsClient.Instance.FinishRpcImmediately(writer2);
                    }
                }
                Vulture.ResetCoolDown();
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Vulture; },
            () =>
            {
                return __instance.ReportButton.graphic.color == Palette.EnabledColor && PlayerControl.LocalPlayer.CanMove;
            },
            () =>
            {
                Vulture.EndMeeting();
            },
            RoleClass.Vulture.GetButtonSprite(),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("VultureButtonName"),
            showButtonText = true
        };

        ShielderButton = new(
            () =>
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetShielder, SendOption.Reliable, -1);
                writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                writer.Write(true);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.SetShielder(CachedPlayer.LocalPlayer.PlayerId, true);
                ShielderButton.actionButton.cooldownTimerText.color = new Color(0F, 0.8F, 0F);
                ShielderButton.MaxTimer = RoleClass.Shielder.DurationTime;
                ShielderButton.Timer = ShielderButton.MaxTimer;
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Shielder; },
            () =>
            {
                if (RoleClass.Shielder.IsShield.ContainsValue(true)) CustomButton.FillUp(ShielderButton);
                return PlayerControl.LocalPlayer.CanMove;
            },
            () =>
            {
                ShielderButton.MaxTimer = RoleClass.Shielder.CoolTime;
                ShielderButton.Timer = RoleClass.Shielder.CoolTime;
            },
            RoleClass.Shielder.GetButtonSprite(),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("ShielderButtonName"),
            showButtonText = true
        };

        CleanerButton = new(
            () =>
            {
                Vulture.RpcCleanDeadBody(RoleClass.Cleaner.CleanMaxCount);
                RoleClass.Cleaner.CoolTime = CleanerButton.Timer = CleanerButton.MaxTimer;
                PlayerControl.LocalPlayer.killTimer = RoleClass.Cleaner.CoolTime;
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Cleaner; },
            () =>
            {
                return __instance.ReportButton.graphic.color == Palette.EnabledColor && PlayerControl.LocalPlayer.CanMove;
            },
            () =>
            {
                CleanerButton.MaxTimer = RoleClass.Cleaner.CoolTime;
                CleanerButton.Timer = RoleClass.Cleaner.CoolTime;
            },
            RoleClass.Cleaner.GetButtonSprite(),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("CleanerButtonName"),
            showButtonText = true
        };

        MadCleanerButton = new(
            () =>
            {
                Vulture.RpcCleanDeadBody(null);
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.MadCleaner; },
            () =>
            {
                return __instance.ReportButton.graphic.color == Palette.EnabledColor && PlayerControl.LocalPlayer.CanMove;
            },
            () =>
            {
                MadCleanerButton.MaxTimer = RoleClass.MadCleaner.CoolTime;
                MadCleanerButton.Timer = RoleClass.MadCleaner.CoolTime;
            },
            RoleClass.MadCleaner.GetButtonSprite(),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("CleanerButtonName"),
            showButtonText = true
        };

        FreezerButton = new(
            () =>
            {
                Freezer.DownStart();
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Freezer; },
            () =>
            {
                if (FreezerButton.isEffectActive) CustomButton.FillUp(FreezerButton);
                return PlayerControl.LocalPlayer.CanMove;
            },
            () => { Freezer.EndMeeting(); },
            RoleClass.Freezer.GetButtonSprite(),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; },
            true,
            5f,
            () =>
            {
                Freezer.SpeedDownEnd();
            }
        )
        {
            buttonText = ModTranslation.GetString("FreezerButtonName"),
            showButtonText = true,
        };

        SamuraiButton = new(
            () =>
            {
                if (PlayerControl.LocalPlayer.CanMove)
                {
                    Samurai.SamuraiKill();
                    RoleClass.Samurai.Sword = true;
                }
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Samurai && ModeHandler.IsMode(ModeId.Default) && !RoleClass.Samurai.Sword; },
            () =>
            {
                return PlayerControl.LocalPlayer.CanMove;
            },
            () => { Samurai.ResetCooldown(); },
            RoleClass.Samurai.GetButtonSprite(),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("SamuraiButtonName"),
            showButtonText = true
        };

        VentMakerButton = new(
            () =>
            {
                RoleClass.VentMaker.VentCount++;
                MessageWriter writer = RPCHelper.StartRPC(CustomRPC.MakeVent);
                writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                writer.Write(CachedPlayer.LocalPlayer.transform.position.x);
                writer.Write(CachedPlayer.LocalPlayer.transform.position.y);
                writer.Write(CachedPlayer.LocalPlayer.transform.position.z);
                writer.Write(RoleClass.VentMaker.VentCount == 2);
                writer.EndRPC();
                RPCProcedure.MakeVent(CachedPlayer.LocalPlayer.PlayerId, CachedPlayer.LocalPlayer.transform.position.x, CachedPlayer.LocalPlayer.transform.position.y, CachedPlayer.LocalPlayer.transform.position.z, RoleClass.VentMaker.VentCount == 2);
                if (RoleClass.VentMaker.VentCount >= 2) RoleClass.VentMaker.IsMakeVent = false;
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.VentMaker && RoleClass.VentMaker.IsMakeVent; },
            () =>
            {
                return PlayerControl.LocalPlayer.CanMove;
            },
            () => { },
            RoleClass.VentMaker.GetButtonSprite(),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("VentMakerButtonName"),
            showButtonText = true
        };

        GhostMechanicRepairButton = new(
            () =>
            {
                RoleClass.GhostMechanic.LimitCount--;

                if (!PlayerControl.LocalPlayer.IsMushroomMixupActive())
                {
                    foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks)
                    {
                        if (task.TaskType is TaskTypes.FixLights or TaskTypes.RestoreOxy or TaskTypes.ResetReactor or TaskTypes.ResetSeismic or TaskTypes.FixComms or TaskTypes.StopCharles)
                        {
                            Sabotage.FixSabotage.RepairProcsee.ReceiptOfSabotageFixing(task.TaskType);
                            break;
                        }
                    }
                }
                else
                {
                    Sabotage.FixSabotage.RepairProcsee.ReceiptOfSabotageFixing(TaskTypes.MushroomMixupSabotage);
                }

                if (RoleClass.GhostMechanic.LimitCount <= 0)
                {
                    GhostMechanicNumRepairText.text = "";
                }

                GhostMechanic.ResetCool();
            },
            (bool isAlive, RoleId role) => { return !isAlive && GhostMechanic.ButtonDisplayCondition(); },
            () =>
            {
                bool sabotageActive = RoleHelpers.IsSabotage();
                GhostMechanicNumRepairText.text = string.Format(ModTranslation.GetString("GhostMechanicCountText"), RoleClass.GhostMechanic.LimitCount);
                if (ModeHandler.IsMode(ModeId.Default, ModeId.Werewolf)) return sabotageActive && PlayerControl.LocalPlayer.CanMove;
                else return sabotageActive && PlayerControl.LocalPlayer.CanMove && PlayerControlFixedUpdatePatch.GhostRoleSetTarget();
            },
            () => { GhostMechanic.ResetCool(true); },
            RoleClass.GhostMechanic.GetButtonSprite(),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; }
        );
        GhostMechanicNumRepairText = GameObject.Instantiate(GhostMechanicRepairButton.actionButton.cooldownTimerText, GhostMechanicRepairButton.actionButton.cooldownTimerText.transform.parent);
        GhostMechanicNumRepairText.text = "";
        GhostMechanicNumRepairText.enableWordWrapping = false;
        GhostMechanicNumRepairText.transform.localScale = Vector3.one * 0.5f;
        GhostMechanicNumRepairText.transform.localPosition += new Vector3(0f, 0.7f, 0);
        GhostMechanicRepairButton.buttonText = ModTranslation.GetString("GhostMechanicButtonName");
        GhostMechanicRepairButton.showButtonText = true;

        PositionSwapperButton = new(
            () =>
            {
                RoleClass.PositionSwapper.SwapCount--;

                PositionSwapperButton.actionButton.cooldownTimerText.color = new Color(255F, 255F, 255F);
                PositionSwapper.SwapStart();
                PositionSwapper.ResetCooldown();
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.PositionSwapper; },
            () =>
            {
                float swapCount = RoleClass.PositionSwapper.SwapCount;
                PositionSwapperNumText.text = swapCount > 0
                    ? string.Format(ModTranslation.GetString("PositionSwapperNumTextName"), swapCount)
                    : string.Format(ModTranslation.GetString("PositionSwapperNumTextName"), "0");
                return PlayerControl.LocalPlayer.CanMove
                        && RoleClass.PositionSwapper.SwapCount > 0 && true && PlayerControl.LocalPlayer.CanMove;
            },
            () => { PositionSwapper.EndMeeting(); },
            RoleClass.PositionSwapper.GetButtonSprite(),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; }
        );
        {
            PositionSwapperNumText = GameObject.Instantiate(PositionSwapperButton.actionButton.cooldownTimerText, PositionSwapperButton.actionButton.cooldownTimerText.transform.parent);
            PositionSwapperNumText.text = "";
            PositionSwapperNumText.enableWordWrapping = false;
            PositionSwapperNumText.transform.localScale = Vector3.one * 0.5f;
            PositionSwapperNumText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);
            PositionSwapperButton.buttonText = ModTranslation.GetString("PositionSwapperButtonName");
            PositionSwapperButton.showButtonText = true;
        };

        SecretlyKillerMainButton = new(
            () =>
            {
                ModHelpers.CheckMurderAttemptAndKill(PlayerControl.LocalPlayer, RoleClass.SecretlyKiller.target);
                SecretlyKiller.MainResetCooldown();
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.SecretlyKiller; },
            () =>
            {
                //クールでブロック
                RoleClass.SecretlyKiller.MainCool = HudManagerStartPatch.SecretlyKillerMainButton.Timer;
                RoleClass.SecretlyKiller.SecretlyCool = HudManagerStartPatch.SecretlyKillerSecretlyKillButton.Timer;
                if (RoleClass.SecretlyKiller.SecretlyCool > 0f && RoleClass.SecretlyKiller.IsKillCoolChange) return false;
                if (RoleClass.SecretlyKiller.MainCool > 0f) return false;

                RoleClass.SecretlyKiller.target = SetTarget();
                PlayerControlFixedUpdatePatch.SetPlayerOutline(RoleClass.SecretlyKiller.target, RoleClass.SecretlyKiller.color);
                return RoleClass.SecretlyKiller.target != null
                        && !RoleClass.SecretlyKiller.target.IsImpostor() && PlayerControl.LocalPlayer.CanMove;
            },
            () => { SecretlyKiller.EndMeeting(); },
            __instance.KillButton.graphic.sprite,
            new Vector3(0, 1, 0),
            __instance,
            __instance.KillButton,
            KeyCode.Q,
            8,
            () =>
            {
                return !PlayerControl.LocalPlayer.CanMove;
            }
        )
        {
            buttonText = FastDestroyableSingleton<HudManager>.Instance.KillButton.buttonLabelText.text,
            showButtonText = true
        };

        SecretlyKillerSecretlyKillButton = new(
            () =>
            {
                RoleClass.SecretlyKiller.SecretlyKillLimit--;
                SecretlyKiller.SecretlyKill();
                SecretlyKiller.SecretlyResetCooldown();
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.SecretlyKiller; },
            () =>
            {
                //テキストぉ
                float SecretKillLimit = RoleClass.SecretlyKiller.SecretlyKillLimit;
                SecretlyKillNumText.text = SecretKillLimit > 0
                    ? string.Format(ModTranslation.GetString("PositionSwapperNumTextName"), SecretKillLimit)
                    : string.Format(ModTranslation.GetString("PositionSwapperNumTextName"), "0");

                if (RoleClass.SecretlyKiller.MainCool > 0f && RoleClass.SecretlyKiller.IsKillCoolChange) return false;
                if (RoleClass.SecretlyKiller.SecretlyKillLimit < 1 || RoleClass.SecretlyKiller.SecretlyCool > 0f) return false;
                //メイン
                RoleClass.SecretlyKiller.target = SetTarget();
                return RoleClass.SecretlyKiller.target != null
                        && !RoleClass.SecretlyKiller.target.IsImpostor() && PlayerControl.LocalPlayer.CanMove;
            },
            () => { SecretlyKiller.EndMeeting(); },
            __instance.KillButton.graphic.sprite,
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.KillButton,
            KeyCode.F,
            49,
            () =>
            {
                SwitchSystem ma = null;
                if (MapUtilities.CachedShipStatus.Systems.ContainsKey(SystemTypes.Electrical))
                    ma = MapUtilities.CachedShipStatus.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();
                return (ma == null || ma.IsActive) && (!RoleClass.SecretlyKiller.IsBlackOutKillCharge || !PlayerControl.LocalPlayer.CanMove);
            }
        );
        {
            SecretlyKillNumText = GameObject.Instantiate(SecretlyKillerSecretlyKillButton.actionButton.cooldownTimerText, SecretlyKillerSecretlyKillButton.actionButton.cooldownTimerText.transform.parent);
            SecretlyKillNumText.text = "";
            SecretlyKillNumText.enableWordWrapping = false;
            SecretlyKillNumText.transform.localScale = Vector3.one * 0.5f;
            SecretlyKillNumText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);
            SecretlyKillerSecretlyKillButton.buttonText = ModTranslation.GetString("SecretlyKillButtonName");
            SecretlyKillerSecretlyKillButton.showButtonText = true;
        };

        Clairvoyant.SetupCustomButtons(__instance);

        DoubleKillerMainKillButton = new(
            () =>
            {
                if (PlayerControlFixedUpdatePatch.SetTarget() && RoleHelpers.IsAlive(PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.CanMove)
                {
                    ModHelpers.CheckMurderAttemptAndKill(PlayerControl.LocalPlayer, PlayerControlFixedUpdatePatch.SetTarget());
                    switch (PlayerControl.LocalPlayer.GetRole())
                    {
                        case RoleId.DoubleKiller:
                            DoubleKiller.ResetMainCooldown();
                            break;
                        case RoleId.Smasher:
                            Smasher.ResetCooldown();
                            break;
                    }
                }
            },
            (bool isAlive, RoleId role) => { return (isAlive && (role == RoleId.DoubleKiller) && ModeHandler.IsMode(ModeId.Default)) || (isAlive && (role == RoleId.Smasher) && ModeHandler.IsMode(ModeId.Default)); },
            () =>
            {
                var target = PlayerControlFixedUpdatePatch.SetTarget();
                PlayerControlFixedUpdatePatch.SetPlayerOutline(target, RoleClass.DoubleKiller.color);
                return target && PlayerControl.LocalPlayer.CanMove;
            },
            () =>
            {
                if (PlayerControl.LocalPlayer.IsRole(RoleId.DoubleKiller)) { DoubleKiller.EndMeeting(); }
            },
            __instance.KillButton.graphic.sprite,
            new Vector3(0, 1, 0),
            __instance,
            __instance.KillButton,
            KeyCode.Q,
            8,
            () => { return false; }
        )
        {
            buttonText = FastDestroyableSingleton<HudManager>.Instance.KillButton.buttonLabelText.text,
            showButtonText = true
        };

        DoubleKillerSubKillButton = new(
            () =>
            {
                if (PlayerControlFixedUpdatePatch.SetTarget() && RoleHelpers.IsAlive(PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.CanMove)
                {
                    ModHelpers.CheckMurderAttemptAndKill(PlayerControl.LocalPlayer, PlayerControlFixedUpdatePatch.SetTarget());
                    switch (PlayerControl.LocalPlayer.GetRole())
                    {
                        case RoleId.DoubleKiller:
                            DoubleKiller.ResetSubCooldown();
                            break;
                        case RoleId.Smasher:
                            Smasher.ResetSmashCooldown();
                            break;
                    }
                }
                if (PlayerControl.LocalPlayer.IsRole(RoleId.Smasher))
                {
                    RoleClass.Smasher.SmashOn = true;
                }
            },
            (bool isAlive, RoleId role) => { return (isAlive && (role == RoleId.DoubleKiller) && ModeHandler.IsMode(ModeId.Default)) || (isAlive && (role == RoleId.Smasher) && ModeHandler.IsMode(ModeId.Default) && !RoleClass.Smasher.SmashOn); },
            () =>
            {
                return PlayerControlFixedUpdatePatch.SetTarget() && PlayerControl.LocalPlayer.CanMove;
            },
            () =>
            {
                if (PlayerControl.LocalPlayer.IsRole(RoleId.DoubleKiller)) { DoubleKiller.EndMeeting(); }
            },
            __instance.KillButton.graphic.sprite,
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.KillButton,
            KeyCode.F,
            49,
            () => { return false; }
        )
        {
            buttonText = FastDestroyableSingleton<HudManager>.Instance.KillButton.buttonLabelText.text,
            showButtonText = true
        };
        SuicideWisherSuicideButton = new(
            () =>
            {
                //自殺
                PlayerControl.LocalPlayer.RpcMurderPlayer(PlayerControl.LocalPlayer, true);
                PlayerControl.LocalPlayer.RpcSetFinalStatus(FinalStatus.SuicideWisherSelfDeath);
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.SuicideWisher && ModeHandler.IsMode(ModeId.Default); },
            () =>
            {
                return true;
            },
            () => { },
            RoleClass.SuicideWisher.GetButtonSprite(),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("SuicideName"),
            showButtonText = true
        };

        FastMakerButton = new(
            () =>
            {
                var target = SetTarget();
                // マッド作ってないなら
                if (target && PlayerControl.LocalPlayer.CanMove && !RoleClass.FastMaker.IsCreatedMadmate)
                {
                    PlayerControl.LocalPlayer.RpcShowGuardEffect(target); // 守護エフェクトの表示
                    RoleClass.FastMaker.CreatePlayers.Add(PlayerControl.LocalPlayer.PlayerId);
                    Madmate.CreateMadmate(target); // くるぅにして、マッドにする
                    RoleClass.FastMaker.IsCreatedMadmate = true; // 作ったことに
                    FastMakerButton.MaxTimer = RoleClass.DefaultKillCoolDown > 0 ? RoleClass.DefaultKillCoolDown / 2f : 0.00001f;
                    FastMakerButton.Timer = FastMakerButton.MaxTimer;
                    if (!ModeHandler.IsMode(ModeId.SuperHostRoles)) FastMakerButton.buttonText = ModTranslation.GetString("KillName"); // ボタン名を戻す
                    Logger.Info($"守護を発動させている為、設定キルクールの半分の値である<{FastMakerButton.MaxTimer}s>にリセットしました。", "FastMakerButton");
                    Logger.Info($"マッドを作成しました。IsCreatedMadmate == {RoleClass.FastMaker.IsCreatedMadmate}", "FastMakerButton");
                }
                else
                {
                    // 作ってたらキル
                    ModHelpers.CheckMurderAttemptAndKill(PlayerControl.LocalPlayer, target);
                    FastMakerButton.MaxTimer = RoleClass.DefaultKillCoolDown;
                    FastMakerButton.Timer = FastMakerButton.MaxTimer;
                    Logger.Info($"Mad作成済みの為キルしました。デフォルトキルクールである<{FastMakerButton.MaxTimer}s>にリセットしました。", "FastMakerButton");
                }
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.FastMaker && !ModeHandler.IsMode(ModeId.SuperHostRoles); },
            () =>
            {
                PlayerControl target = SetTarget();
                return target && !Frankenstein.IsMonster(target) && PlayerControl.LocalPlayer.CanMove;
            },
            () =>
            {
                FastMakerButton.MaxTimer = RoleClass.IsFirstMeetingEnd ? RoleClass.DefaultKillCoolDown : 10f;
                FastMakerButton.Timer = FastMakerButton.MaxTimer;
            },
            __instance.KillButton.graphic.sprite,
            new Vector3(-1, 1, 0),
            __instance,
            __instance.KillButton,
            KeyCode.Q,
            8,
            () => { return false; }
        )
        {
            buttonText = ModeHandler.IsMode(ModeId.SuperHostRoles) ? ModTranslation.GetString("KillName") : ModTranslation.GetString("CreateMadmateFastVerButton"),
            showButtonText = true
        };

        ToiletFanButton = new(
            () => { RPCHelper.RpcOpenToilet(); },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.ToiletFan; },
            () =>
            {
                return PlayerControl.LocalPlayer.CanMove;
            },
            () =>
            {
                ToiletFanButton.MaxTimer = RoleClass.ToiletFan.ToiletCool;
                ToiletFanButton.Timer = RoleClass.ToiletFan.ToiletCool;
            },
            RoleClass.ToiletFan.GetButtonSprite(),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("ToiletName"),
            showButtonText = true
        };

        ButtonerButton = new(
            () =>
            {
                if (PlayerControl.LocalPlayer.IsRole(RoleId.EvilButtoner))
                    RoleClass.EvilButtoner.SkillCount--;
                else if (PlayerControl.LocalPlayer.IsRole(RoleId.NiceButtoner))
                    RoleClass.NiceButtoner.SkillCount--;
                else
                    return;
                EvilButtoner.EvilButtonerStartMeeting(PlayerControl.LocalPlayer);
            },
            (bool isAlive, RoleId role) => { return isAlive && role is RoleId.EvilButtoner or RoleId.NiceButtoner; },
            () =>
            {
                if (!PlayerControl.LocalPlayer.CanMove) return false;
                if (PlayerControl.LocalPlayer.IsRole(RoleId.NiceButtoner)) return RoleClass.NiceButtoner.SkillCount != 0;
                else if (PlayerControl.LocalPlayer.IsRole(RoleId.EvilButtoner)) return RoleClass.EvilButtoner.SkillCount != 0;
                return false;
            },
            () =>
            {
                if (PlayerControl.LocalPlayer.IsRole(RoleId.NiceButtoner) && RoleClass.NiceButtoner.SkillCount == 0) return;
                if (PlayerControl.LocalPlayer.IsRole(RoleId.EvilButtoner) && RoleClass.EvilButtoner.SkillCount == 0) return;
                //イビルボタナーなら
                if (PlayerControl.LocalPlayer.IsRole(RoleId.EvilButtoner))
                {
                    ButtonerButton.MaxTimer = RoleClass.EvilButtoner.CoolTime;
                    ButtonerButton.Timer = RoleClass.EvilButtoner.CoolTime;
                }
                //ナイスボタナーなら
                else
                {
                    ButtonerButton.MaxTimer = RoleClass.NiceButtoner.CoolTime;
                    ButtonerButton.Timer = RoleClass.NiceButtoner.CoolTime;
                }
            },
            RoleClass.EvilButtoner.GetButtonSprite(),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; }

        )
        {
            buttonText = ModTranslation.GetString("ButtonerButtonName"),
            showButtonText = true
        };

        RevolutionistButton = new(
            () => { },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Revolutionist; },
            () => { return true; },
            () =>
            {
                RevolutionistButton.MaxTimer = RoleClass.Revolutionist.CoolTime;
                RevolutionistButton.Timer = RoleClass.Revolutionist.CoolTime;
            },
            RoleClass.Moving.GetNoSetButtonSprite(),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("RevolutionistButtonName"),
            showButtonText = true,
            color = Color.yellow
        };

        SuicidalIdeationButton = new CustomButton(
            () => { },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.SuicidalIdeation; },
            () => { return true; },
            () =>
            {
                SuicidalIdeationButton.MaxTimer = CustomOptionHolder.SuicidalIdeationTimeLeft.GetFloat();
                SuicidalIdeationButton.Timer = SuicidalIdeationButton.MaxTimer;
            },
            RoleClass.SuicidalIdeation.GetButtonSprite(),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () =>
            {
                return RoleClass.IsMeeting;
            }
        )
        {
            buttonText = ModTranslation.GetString("SuicidalIdeationButtonName"),
            showButtonText = true
        };

        HitmanKillButton = new(
            () =>
            {
                PlayerControl target = SetTarget();
                if (RoleClass.Hitman.Target.PlayerId != target.PlayerId)
                {
                    Hitman.LimitDown();
                }
                else
                {
                    Hitman.KillSuc();
                }
                ModHelpers.CheckMurderAttemptAndKill(PlayerControl.LocalPlayer, target);
                target.RpcSetFinalStatus(FinalStatus.HitmanKill);
                RoleClass.Hitman.UpdateTime = CustomOptionHolder.HitmanChangeTargetTime.GetFloat();
                RoleClass.Hitman.ArrowUpdateTime = 0;
                Hitman.SetTarget();
                HitmanKillButton.Timer = HitmanKillButton.MaxTimer;
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Hitman; },
            () =>
            {
                var Target = SetTarget();
                PlayerControlFixedUpdatePatch.SetPlayerOutline(Target, RoleClass.Hitman.color);
                return PlayerControl.LocalPlayer.CanMove && Target;
            },
            () =>
            {
                Hitman.EndMeeting();
            },
            __instance.KillButton.graphic.sprite,
            new Vector3(0, 1, 0),
            __instance,
            __instance.KillButton,
            KeyCode.Q,
            8,
            () => { return false; }
        )
        {
            buttonText = FastDestroyableSingleton<HudManager>.Instance.KillButton.buttonLabelText.text,
            showButtonText = true
        };

        MatryoshkaButton = new(
            () =>
            {
                if (MatryoshkaButton.isEffectActive)
                {
                    RoleClass.Matryoshka.WearLimit--;
                    Matryoshka.RpcSet(null, false);
                    MatryoshkaButton.MaxTimer = CustomOptionHolder.MatryoshkaCoolTime.GetFloat();
                    MatryoshkaButton.Timer = MatryoshkaButton.MaxTimer;
                    return;
                }
                foreach (Collider2D collider2D in Physics2D.OverlapCircleAll(PlayerControl.LocalPlayer.GetTruePosition(), PlayerControl.LocalPlayer.MaxReportDistance, Constants.PlayersOnlyMask))
                {
                    if (collider2D.tag == "DeadBody")
                    {
                        DeadBody component = collider2D.GetComponent<DeadBody>();
                        Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
                        Vector2 truePosition2 = component.TruePosition;
                        Logger.Info((!component.Reported).ToString() + $"{truePosition2} : {truePosition} : {Vector2.Distance(truePosition2, truePosition) <= PlayerControl.LocalPlayer.MaxReportDistance} : {Vector2.Distance(truePosition2, truePosition)} : {PlayerControl.LocalPlayer.MaxReportDistance}");
                        if (!component.Reported)
                        {
                            if (Vector2.Distance(truePosition2, truePosition) <= PlayerControl.LocalPlayer.MaxReportDistance && !PhysicsHelpers.AnythingBetween(truePosition, truePosition2, Constants.ShipAndObjectsMask, false))
                            {
                                if (RoleClass.Matryoshka.Data.Values.All(data =>
                                {
                                    return data == null || data.ParentId != component.ParentId;
                                }))
                                {
                                    GameData.PlayerInfo playerInfo = GameData.Instance.GetPlayerById(component.ParentId);
                                    Matryoshka.RpcSet(playerInfo.Object, true);
                                    RoleClass.Matryoshka.MyKillCoolTime += CustomOptionHolder.MatryoshkaAddKillCoolTime.GetFloat();
                                    break;
                                }
                            }
                        }
                    }
                }
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Matryoshka && RoleClass.Matryoshka.WearLimit > 0; },
            () =>
            {
                if (RoleClass.Matryoshka.IsLocalOn)
                {
                    MatryoshkaButton.Sprite = RoleClass.Matryoshka.TakeOffButtonSprite;
                    MatryoshkaButton.buttonText = ModTranslation.GetString("MatryoshkaTakeOffButtonName");
                }
                else
                {
                    MatryoshkaButton.Sprite = RoleClass.Matryoshka.PutOnButtonSprite;
                    MatryoshkaButton.buttonText = ModTranslation.GetString("MatryoshkaPutOnButtonName");
                }
                MatryoshkaButton.HasEffect = __instance.ReportButton.graphic.color == Palette.EnabledColor;
                if (MatryoshkaButton.isEffectActive) CustomButton.FillUp(MatryoshkaButton);
                return (__instance.ReportButton.graphic.color == Palette.EnabledColor || RoleClass.Matryoshka.IsLocalOn) && PlayerControl.LocalPlayer.CanMove;
            },
            () =>
            {
                MatryoshkaButton.MaxTimer = CustomOptionHolder.MatryoshkaCoolTime.GetFloat();
                MatryoshkaButton.Timer = MatryoshkaButton.MaxTimer;
                MatryoshkaButton.effectCancellable = true;
                MatryoshkaButton.EffectDuration = CustomOptionHolder.MatryoshkaWearTime.GetFloat();
                if (RoleClass.Matryoshka.IsLocalOn)
                {
                    RoleClass.Matryoshka.WearLimit--;
                }
                Matryoshka.RpcSet(null, false);
            },
            RoleClass.Matryoshka.PutOnButtonSprite,
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () =>
            {
                return false;
            },
            true,
            5f,
            () =>
            {
                RoleClass.Matryoshka.WearLimit--;
                Matryoshka.RpcSet(null, false);
                MatryoshkaButton.MaxTimer = CustomOptionHolder.MatryoshkaCoolTime.GetFloat();
                MatryoshkaButton.Timer = MatryoshkaButton.MaxTimer;
            }
            )
        {
            buttonText = ModTranslation.GetString("MatryoshkaPutOnButtonName"),
            showButtonText = true
        };

        NunButton = new(
            () =>
            {
                MessageWriter writer = RPCHelper.StartRPC(CustomRPC.UncheckedUsePlatform);
                writer.Write((byte)255);
                writer.Write(false);
                writer.EndRPC();
                RPCProcedure.UncheckedUsePlatform((byte)255, false);
                NunButton.Timer = NunButton.MaxTimer;
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Nun; },
            () =>
            {
                return PlayerControl.LocalPlayer.CanMove;
            },
            () =>
            {
                NunButton.MaxTimer = CustomOptionHolder.NunCoolTime.GetFloat();
                NunButton.Timer = NunButton.MaxTimer;
            },
            RoleClass.Nun.GetButtonSprite(),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () =>
            {
                return false;
            }
        )
        {
            buttonText = ModTranslation.GetString("NunButtonName"),
            showButtonText = true
        };

        PsychometristButton = new(
            () =>
            {
                foreach (Collider2D collider2D in Physics2D.OverlapCircleAll(PlayerControl.LocalPlayer.GetTruePosition(), PlayerControl.LocalPlayer.MaxReportDistance, Constants.PlayersOnlyMask))
                {
                    if (collider2D.tag == "DeadBody")
                    {
                        DeadBody component = collider2D.GetComponent<DeadBody>();
                        if (component && !component.Reported)
                        {
                            Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
                            Vector2 truePosition2 = component.TruePosition;
                            if (Vector2.Distance(truePosition2 - new Vector2(0.15f, 0.2f), truePosition) <= RoleClass.Psychometrist.Distance && PlayerControl.LocalPlayer.CanMove && !PhysicsHelpers.AnythingBetween(truePosition, truePosition2, Constants.ShipAndObjectsMask, false))
                            {
                                RoleClass.Psychometrist.CurrentTarget = component;
                            }
                        }
                    }
                }
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Psychometrist; },
            () =>
            {
                if (PsychometristButton.isEffectActive)
                {
                    if (RoleClass.Psychometrist.CurrentTarget == null || __instance.ReportButton.graphic.color != Palette.EnabledColor || Vector2.Distance(RoleClass.Psychometrist.CurrentTarget.TruePosition - new Vector2(0.15f, 0.2f), CachedPlayer.LocalPlayer.PlayerControl.GetTruePosition()) > RoleClass.Psychometrist.Distance)
                    {
                        RoleClass.Psychometrist.CurrentTarget = null;
                        PsychometristButton.Timer = 0f;
                        PsychometristButton.isEffectActive = false;
                    }
                }
                bool Is = __instance.ReportButton.graphic.color == Palette.EnabledColor && PlayerControl.LocalPlayer.CanMove;
                if (!Is) return false;
                Is = false;
                foreach (Collider2D collider2D in Physics2D.OverlapCircleAll(PlayerControl.LocalPlayer.GetTruePosition(), PlayerControl.LocalPlayer.MaxReportDistance, Constants.PlayersOnlyMask))
                {
                    if (collider2D.tag == "DeadBody")
                    {
                        DeadBody component = collider2D.GetComponent<DeadBody>();
                        if (component && !component.Reported)
                        {
                            Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
                            Vector2 truePosition2 = component.TruePosition;
                            if (Vector2.Distance(truePosition2 - new Vector2(0.15f, 0.2f), truePosition) <= RoleClass.Psychometrist.Distance && PlayerControl.LocalPlayer.CanMove && !PhysicsHelpers.AnythingBetween(truePosition, truePosition2, Constants.ShipAndObjectsMask, false))
                            {
                                Is = true;
                                break;
                            }
                        }
                    }
                }
                return Is;
            },
            () =>
            {
                PsychometristButton.MaxTimer = CustomOptionHolder.PsychometristCoolTime.GetFloat();
                PsychometristButton.Timer = PsychometristButton.MaxTimer;
                PsychometristButton.effectCancellable = false;
                PsychometristButton.EffectDuration = CustomOptionHolder.PsychometristReadTime.GetFloat();
                PsychometristButton.isEffectActive = false;
            },
            RoleClass.Psychometrist.GetButtonSprite(),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () =>
            {
                return false;
            },
            true,
            CustomOptionHolder.PsychometristReadTime.GetFloat(),
            () =>
            {
                if (RoleClass.IsMeeting) return;
                Psychometrist.ClickButton();
            }
        )
        {
            buttonText = ModTranslation.GetString("PsychometristButtonName"),
            showButtonText = true
        };

        PartTimerButton = new(
            () =>
            {
                MessageWriter writer = RPCHelper.StartRPC(CustomRPC.PartTimerSet);
                writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                writer.Write(SetTarget().PlayerId);
                writer.EndRPC();
                RPCProcedure.PartTimerSet(CachedPlayer.LocalPlayer.PlayerId, SetTarget().PlayerId);
                PartTimerButton.Timer = PartTimerButton.MaxTimer;
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.PartTimer && !RoleClass.PartTimer.IsLocalOn; },
            () =>
            {
                var Target = SetTarget();
                PlayerControlFixedUpdatePatch.SetPlayerOutline(Target, RoleClass.PartTimer.color);
                return PlayerControl.LocalPlayer.CanMove && Target;
            },
            () =>
            {
                PartTimerButton.MaxTimer = CustomOptionHolder.PartTimerCoolTime.GetFloat();
                PartTimerButton.Timer = PartTimerButton.MaxTimer;
            },
            RoleClass.PartTimer.GetButtonSprite(),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () =>
            {
                return false;
            }
        )
        {
            buttonText = ModTranslation.GetString("PartTimerButtonName"),
            showButtonText = true
        };

        PainterButton = new(
            () =>
            {
                Painter.SetTarget(SetTarget());
                PainterButton.Timer = PainterButton.MaxTimer;
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Painter && RoleClass.Painter.CurrentTarget == null; },
            () =>
            {
                var Target = SetTarget();
                PlayerControlFixedUpdatePatch.SetPlayerOutline(Target, RoleClass.Painter.color);
                return PlayerControl.LocalPlayer.CanMove && Target;
            },
            () =>
            {
                PainterButton.MaxTimer = CustomOptionHolder.PainterCoolTime.GetFloat();
                PainterButton.Timer = PainterButton.MaxTimer;
            },
            RoleClass.Painter.GetButtonSprite(),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () =>
            {
                return false;
            }
        )
        {
            buttonText = ModTranslation.GetString("PainterButtonName"),
            showButtonText = true
        };

        StefinderKillButton = new(
            () =>
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.StefinderIsKilled, SendOption.Reliable, -1);
                writer.Write(PlayerControl.LocalPlayer.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);

                RPCProcedure.StefinderIsKilled(PlayerControl.LocalPlayer.PlayerId);
                RoleClass.Stefinder.IsKill = true;
                ModHelpers.CheckMurderAttemptAndKill(PlayerControl.LocalPlayer, RoleClass.Stefinder.target);
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Stefinder && !RoleClass.Stefinder.IsKill; },
            () =>
            {
                RoleClass.Stefinder.target = SetTarget();
                return RoleClass.Stefinder.target != null && PlayerControl.LocalPlayer.CanMove;
            },
            () =>
            {
                StefinderKillButton.MaxTimer = CustomOptionHolder.StefinderKillCooldown.GetFloat();
                StefinderKillButton.Timer = StefinderKillButton.MaxTimer;
            },
            __instance.KillButton.graphic.sprite,
            new Vector3(0, 1, 0),
            __instance,
            __instance.KillButton,
            KeyCode.Q,
            8,
            () =>
            {
                return !PlayerControl.LocalPlayer.CanMove;
            }
        )
        {
            buttonText = FastDestroyableSingleton<HudManager>.Instance.KillButton.buttonLabelText.text,
            showButtonText = true
        };

        DoppelgangerButton = new(
            () => { Doppelganger.DoppelgangerShape(); },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Doppelganger && ModeHandler.IsMode(ModeId.Default); },
            () => { return PlayerControl.LocalPlayer.CanMove; },
            () =>
            {
                DoppelgangerButton.MaxTimer = RoleClass.Doppelganger.CoolTime;
                DoppelgangerButton.Timer = RoleClass.Doppelganger.CoolTime;
            },
            RoleClass.Doppelganger.GetButtonSprite(),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("DoppelgangerButtonName"),
            showButtonText = true
        };
        RoleClass.Doppelganger.DoppelgangerDurationText = GameObject.Instantiate(FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText, FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText.transform.parent);
        RoleClass.Doppelganger.DoppelgangerDurationText.text = "";
        RoleClass.Doppelganger.DoppelgangerDurationText.enableWordWrapping = false;
        RoleClass.Doppelganger.DoppelgangerDurationText.transform.localScale = Vector3.one * 0.5f;
        RoleClass.Doppelganger.DoppelgangerDurationText.transform.localPosition += new Vector3(-2.575f, -0.95f, 0);

        GM.CreateButton(__instance);

        CamouflagerButton = new(
            () =>
            {
                if (CamouflagerButton.isEffectActive)
                {
                    Camouflager.RpcResetCamouflage();
                    CamouflagerButton.MaxTimer = RoleClass.Camouflager.CoolTime;
                    CamouflagerButton.Timer = CamouflagerButton.MaxTimer;
                }
                else
                {
                    Camouflager.RpcCamouflage();
                }
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Camouflager && ModeHandler.IsMode(ModeId.Default); },
            () =>
            {
                if (CamouflagerButton.isEffectActive) CustomButton.FillUp(CamouflagerButton);
                return PlayerControl.LocalPlayer.CanMove;
            },
            () =>
            {
                CamouflagerButton.MaxTimer = RoleClass.Camouflager.CoolTime;
                CamouflagerButton.Timer = CamouflagerButton.MaxTimer;
                CamouflagerButton.effectCancellable = false;
                CamouflagerButton.EffectDuration = CustomOptionHolder.CamouflagerDurationTime.GetFloat();
                CamouflagerButton.HasEffect = true;
            },
            RoleClass.Camouflager.GetButtonSprite(),
            new Vector3(-2f, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; },
            true,
            5f,
            () =>
            {
                CamouflagerButton.MaxTimer = RoleClass.Camouflager.CoolTime;
                CamouflagerButton.Timer = CamouflagerButton.MaxTimer;
            }
        )
        {
            buttonText = ModTranslation.GetString("CamouflagerButtonName"),
            showButtonText = true
        };

        Roles.Impostor.MadRole.Worshiper.SetupCustomButtons(__instance);

        FireFox.SetupCustomButtons(__instance);

        Squid.SetusCustomButton(__instance);

        OrientalShaman.SetupCustomButtons(__instance);

        BlackHatHacker.SetupCustomButtons(__instance);

        MadRaccoon.Button.SetupCustomButtons(__instance);

        JumpDancer.SetUpCustomButtons(__instance);

        Bat.Button.SetupCustomButtons(__instance);

        Rocket.Button.SetupCustomButtons(__instance);

        WellBehaver.SetupCustomButtons(__instance);

        Spider.Button.SetupCustomButtons(__instance);

        Frankenstein.SetupCustomButtons(__instance);

        // SetupCustomButtons

        SetCustomButtonCooldowns();
    }
}