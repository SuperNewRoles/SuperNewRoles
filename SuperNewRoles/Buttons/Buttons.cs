using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using HarmonyLib;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomOption;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.EndGame;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles;
using UnityEngine;

namespace SuperNewRoles.Buttons
{
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
    static class HudManagerStartPatch
    {
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
        public static CustomButton ConjurerFirstAddButton;
        public static CustomButton ConjurerSecondAddButton;
        public static CustomButton ConjurerThirdAddButton;
        public static CustomButton ConjurerStartButton;
        public static CustomButton PositionSwapperButton;

        public static TMPro.TMP_Text sheriffNumShotsText;
        public static TMPro.TMP_Text GhostMechanicNumRepairText;
        public static TMPro.TMP_Text PositionSwapperNumText;

        public static void setCustomButtonCooldowns()
        {
            Sheriff.ResetKillCoolDown();
            Clergyman.ResetCoolDown();
            Teleporter.ResetCoolDown();
            Jackal.resetCoolDown();
            //クールダウンリセット
        }

        public static PlayerControl setTarget(List<PlayerControl> untarget = null, bool Crewmateonly = false)
        {
            return PlayerControlFixedUpdatePatch.setTarget(untargetablePlayers: untarget, onlyCrewmates: Crewmateonly);
        }

        public static void Postfix(HudManager __instance)
        {
            FalseChargesFalseChargeButton = new CustomButton(
                () =>
                {
                    if (setTarget() && RoleHelpers.isAlive(PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.CanMove)
                    {
                        if (ModeHandler.isMode(ModeId.SuperHostRoles))
                        {
                            PlayerControl.LocalPlayer.CmdCheckMurder(setTarget());
                        }
                        else
                        {
                            ModHelpers.UncheckedMurderPlayer(setTarget(), PlayerControl.LocalPlayer);
                            RoleClass.FalseCharges.FalseChargePlayer = setTarget().PlayerId;
                            RoleClass.FalseCharges.Turns = RoleClass.FalseCharges.DefaultTurn;
                        }
                    }
                },
                (bool isAlive, RoleId role) => { return isAlive && role == RoleId.FalseCharges; },
                () =>
                {
                    return setTarget() && PlayerControl.LocalPlayer.CanMove;
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
                8
            )
            {
                buttonText = ModTranslation.getString("FalseChargesButtonTitle"),
                showButtonText = true
            };

            trueloverLoveButton = new CustomButton(
                () =>
                {
                    if (PlayerControl.LocalPlayer.CanMove && !RoleClass.truelover.IsCreate && !PlayerControl.LocalPlayer.IsLovers())
                    {
                        var target = setTarget();
                        if (target == null || target.IsLovers()) return;
                        RoleClass.truelover.IsCreate = true;
                        RoleHelpers.SetLovers(PlayerControl.LocalPlayer, target);
                        RoleHelpers.SetLoversRPC(PlayerControl.LocalPlayer, target);
                    }
                },
                (bool isAlive, RoleId role) => { return isAlive && role == RoleId.truelover && !RoleClass.truelover.IsCreate; },
                () =>
                {
                    return PlayerControl.LocalPlayer.CanMove && setTarget();
                },
                () => { trueloverLoveButton.Timer = 0f; trueloverLoveButton.MaxTimer = 0f; },
                RoleClass.truelover.getButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.AbilityButton,
                KeyCode.F,
                49
            )
            {
                buttonText = ModTranslation.getString("trueloverloveButtonName"),
                showButtonText = true
            };

            MagazinerGetButton = new CustomButton(
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
                RoleClass.Magaziner.getGetButtonSprite(),
                new Vector3(-2.7f, -0.06f, 0),
                __instance,
                __instance.AbilityButton,
                KeyCode.F,
                49
            )
            {
                buttonText = ModTranslation.getString("MagazinerGetButtonName"),
                showButtonText = true
            };

            MagazinerAddButton = new CustomButton(
                () =>
                {
                    if (!FastDestroyableSingleton<HudManager>.Instance.KillButton.isCoolingDown && PlayerControl.LocalPlayer.CanMove)
                    {
                        PlayerControl.LocalPlayer.SetKillTimerUnchecked(PlayerControl.GameOptions.KillCooldown);
                        RoleClass.Magaziner.MyPlayerCount++;
                    }
                },
                (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Magaziner; },
                () =>
                {
                    return PlayerControl.LocalPlayer.CanMove && !FastDestroyableSingleton<HudManager>.Instance.KillButton.isCoolingDown;
                },
                () => { MagazinerAddButton.Timer = 0f; MagazinerAddButton.MaxTimer = 0f; },
                RoleClass.Magaziner.getAddButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.AbilityButton,
                KeyCode.F,
                49
            )
            {
                buttonText = ModTranslation.getString("MagazinerAddButtonName"),
                showButtonText = true
            };

            ScientistButton = new Buttons.CustomButton(
                () =>
                {
                    if (!PlayerControl.LocalPlayer.CanMove) return;
                    Roles.RoleClass.NiceScientist.ButtonTimer = DateTime.Now;
                    ScientistButton.actionButton.cooldownTimerText.color = new Color(0F, 0.8F, 0F);
                    Scientist.Start();
                },
                (bool isAlive, RoleId role) => { return isAlive && (role == RoleId.NiceScientist || role == RoleId.EvilScientist); },
                () =>
                {
                    return PlayerControl.LocalPlayer.CanMove;
                },
                () => { Scientist.EndMeeting(); },
                RoleClass.NiceScientist.getButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.AbilityButton,
                KeyCode.F,
                49
            )
            {
                buttonText = ModTranslation.getString("ScientistButtonName"),
                showButtonText = true
            };

            HawkHawkEyeButton = new CustomButton(
                () =>
                {
                    if (PlayerControl.LocalPlayer.CanMove)
                    {
                        if (PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.Hawk))
                        {
                            RoleClass.Hawk.Timer = RoleClass.Hawk.DurationTime;
                            RoleClass.Hawk.ButtonTimer = DateTime.Now;
                            HawkHawkEyeButton.MaxTimer = RoleClass.Hawk.CoolTime;
                            HawkHawkEyeButton.Timer = RoleClass.Hawk.CoolTime;
                        }
                        if (PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.NiceHawk))
                        {
                            RoleClass.NiceHawk.Timer = RoleClass.NiceHawk.DurationTime;
                            RoleClass.NiceHawk.ButtonTimer = DateTime.Now;
                            HawkHawkEyeButton.MaxTimer = RoleClass.NiceHawk.CoolTime;
                            HawkHawkEyeButton.Timer = RoleClass.NiceHawk.CoolTime;
                            RoleClass.NiceHawk.Postion = CachedPlayer.LocalPlayer.transform.localPosition;
                            RoleClass.NiceHawk.timer1 = 10;
                            RoleClass.NiceHawk.Timer2 = DateTime.Now;
                        }
                        if (PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.MadHawk))
                        {
                            RoleClass.MadHawk.Timer = RoleClass.MadHawk.DurationTime;
                            RoleClass.MadHawk.ButtonTimer = DateTime.Now;
                            HawkHawkEyeButton.MaxTimer = RoleClass.MadHawk.CoolTime;
                            HawkHawkEyeButton.Timer = RoleClass.MadHawk.CoolTime;
                            RoleClass.MadHawk.Postion = CachedPlayer.LocalPlayer.transform.localPosition;
                            RoleClass.MadHawk.timer1 = 10;
                            RoleClass.MadHawk.Timer2 = DateTime.Now;
                        }
                        RoleClass.Hawk.IsHawkOn = true;
                    }
                },
                (bool isAlive, RoleId role) => { return (PlayerControl.LocalPlayer.isAlive() && PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.Hawk)) || PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.NiceHawk) || PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.MadHawk); },
                () =>
                {
                    return PlayerControl.LocalPlayer.CanMove;
                },
                () =>
                {
                    if (PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.Hawk))
                    {
                        HawkHawkEyeButton.MaxTimer = RoleClass.Hawk.CoolTime;
                        HawkHawkEyeButton.Timer = RoleClass.Hawk.CoolTime;
                    }
                    if (PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.NiceHawk))
                    {
                        HawkHawkEyeButton.MaxTimer = RoleClass.NiceHawk.CoolTime;
                        HawkHawkEyeButton.Timer = RoleClass.NiceHawk.CoolTime;
                    }
                    if (PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.MadHawk))
                    {
                        HawkHawkEyeButton.MaxTimer = RoleClass.MadHawk.CoolTime;
                        HawkHawkEyeButton.Timer = RoleClass.MadHawk.CoolTime;
                    }
                    RoleClass.Hawk.IsHawkOn = false;
                },
                RoleClass.Hawk.getButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.AbilityButton,
                KeyCode.F,
                49
            )
            {
                buttonText = ModTranslation.getString("HawkButtonName"),
                showButtonText = true
            };

            CountChangerButton = new CustomButton(
                () =>
                {
                    if (RoleClass.CountChanger.Count >= 1 && setTarget() && PlayerControl.LocalPlayer.CanMove)
                    {
                        RoleClass.CountChanger.IsSet = true;
                        RoleClass.CountChanger.Count--;
                        var Target = PlayerControlFixedUpdatePatch.setTarget(onlyCrewmates: true);
                        var TargetID = Target.PlayerId;
                        var LocalID = CachedPlayer.LocalPlayer.PlayerId;

                        CustomRPC.RPCProcedure.CountChangerSetRPC(LocalID, TargetID);
                        MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.CountChangerSetRPC, Hazel.SendOption.Reliable, -1);
                        killWriter.Write(LocalID);
                        killWriter.Write(TargetID);
                        AmongUsClient.Instance.FinishRpcImmediately(killWriter);
                    }
                },
                (bool isAlive, RoleId role) => { return isAlive && role == RoleId.CountChanger && !RoleClass.CountChanger.IsSet && RoleClass.CountChanger.Count >= 1; },
                () =>
                {
                    return PlayerControl.LocalPlayer.CanMove && PlayerControlFixedUpdatePatch.setTarget(onlyCrewmates: true);
                },
                () =>
                {
                    CountChangerButton.MaxTimer = PlayerControl.GameOptions.KillCooldown;
                    CountChangerButton.Timer = PlayerControl.GameOptions.KillCooldown;
                },
                RoleClass.CountChanger.getButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.AbilityButton,
                KeyCode.F,
                49
            )
            {
                buttonText = ModTranslation.getString("CountChangerButtonName"),
                showButtonText = true
            };

            DoctorVitalsButton = new CustomButton(
                () =>
                {
                    if (RoleClass.Doctor.Vital == null)
                    {
                        var e = UnityEngine.Object.FindObjectsOfType<SystemConsole>().FirstOrDefault(x => x.gameObject.name.Contains("panel_vitals"));
                        if (e == null || Camera.main == null) return;
                        RoleClass.Doctor.Vital = UnityEngine.Object.Instantiate(e.MinigamePrefab, Camera.main.transform, false);
                    }
                    RoleClass.Doctor.Vital.transform.SetParent(Camera.main.transform, false);
                    RoleClass.Doctor.Vital.transform.localPosition = new Vector3(0.0f, 0.0f, -50f);
                    RoleClass.Doctor.Vital.Begin(null);
                },
                (bool isAlive, RoleId role) => { return role == RoleId.Doctor && isAlive; },
                () =>
                {
                    return PlayerControl.LocalPlayer.CanMove;
                },
                () =>
                {
                },
                RoleClass.Doctor.getVitalsSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.AbilityButton,
                KeyCode.F,
                49
            )
            {
                buttonText = ModTranslation.getString("DoctorVitalName"),
                showButtonText = true
            };

            JackalSidekickButton = new CustomButton(
                () =>
                {
                    var target = Jackal.JackalFixedPatch.JackalsetTarget();
                    if (target && RoleHelpers.isAlive(PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.CanMove && RoleClass.Jackal.IsCreateSidekick)
                    {
                        bool IsFakeSidekick = EvilEraser.IsBlockAndTryUse(EvilEraser.BlockTypes.JackalSidekick, target);
                        MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.CreateSidekick, Hazel.SendOption.Reliable, -1);
                        killWriter.Write(target.PlayerId);
                        killWriter.Write(IsFakeSidekick);
                        AmongUsClient.Instance.FinishRpcImmediately(killWriter);
                        CustomRPC.RPCProcedure.CreateSidekick(target.PlayerId, IsFakeSidekick);
                        RoleClass.Jackal.IsCreateSidekick = false;
                        Jackal.resetCoolDown();
                    }
                },
                (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Jackal && ModeHandler.isMode(ModeId.Default) && RoleClass.Jackal.IsCreateSidekick; },
                () =>
                {
                    return Jackal.JackalFixedPatch.JackalsetTarget() && PlayerControl.LocalPlayer.CanMove;
                },
                () =>
                {
                    if (PlayerControl.LocalPlayer.isRole(RoleId.Jackal)) { Jackal.EndMeeting(); }
                },
                RoleClass.Jackal.getButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.AbilityButton,
                KeyCode.F,
                49
            )
            {
                buttonText = ModTranslation.getString("JackalCreateSidekickButtonName"),
                showButtonText = true
            };

            JackalSeerSidekickButton = new CustomButton(
                () =>
                {
                    var target_JS = JackalSeer.JackalSeerFixedPatch.JackalSeersetTarget();
                    if (target_JS && RoleHelpers.isAlive(PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.CanMove && RoleClass.JackalSeer.IsCreateSidekick)
                    {
                        bool IsFakeSidekickSeer = EvilEraser.IsBlockAndTryUse(EvilEraser.BlockTypes.JackalSeerSidekick, target_JS);
                        MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.CreateSidekickSeer, Hazel.SendOption.Reliable, -1);
                        killWriter.Write(target_JS.PlayerId);
                        killWriter.Write(IsFakeSidekickSeer);
                        AmongUsClient.Instance.FinishRpcImmediately(killWriter);
                        CustomRPC.RPCProcedure.CreateSidekickSeer(target_JS.PlayerId, IsFakeSidekickSeer);
                        RoleClass.JackalSeer.IsCreateSidekick = false;
                        JackalSeer.resetCoolDown();
                    }
                },
                (bool isAlive, RoleId role) => { return isAlive && role == RoleId.JackalSeer && ModeHandler.isMode(ModeId.Default) && RoleClass.JackalSeer.IsCreateSidekick; },
                () =>
                {
                    return JackalSeer.JackalSeerFixedPatch.JackalSeersetTarget() && PlayerControl.LocalPlayer.CanMove;
                },
                () =>
                {
                    if (PlayerControl.LocalPlayer.isRole(RoleId.JackalSeer)) { JackalSeer.EndMeeting(); }
                },
                RoleClass.Jackal.getButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.AbilityButton,
                KeyCode.F,
                49
            )
            {
                buttonText = ModTranslation.getString("JackalCreateSidekickButtonName"),
                showButtonText = true
            };

            JackalKillButton = new CustomButton(
                () =>
                {
                    if (Jackal.JackalFixedPatch.JackalsetTarget() && RoleHelpers.isAlive(PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.CanMove)
                    {
                        ModHelpers.checkMuderAttemptAndKill(PlayerControl.LocalPlayer, Jackal.JackalFixedPatch.JackalsetTarget());
                        switch (PlayerControl.LocalPlayer.getRole())
                        {
                            case RoleId.Jackal:
                                Jackal.resetCoolDown();
                                break;
                            case RoleId.JackalSeer:
                                JackalSeer.resetCoolDown();
                                break;
                            case RoleId.TeleportingJackal:
                                TeleportingJackal.resetCoolDown();
                                break;
                        }
                    }
                },
                (bool isAlive, RoleId role) => { return isAlive && (role == RoleId.Jackal || role == RoleId.TeleportingJackal || role == RoleId.JackalSeer) && ModeHandler.isMode(ModeId.Default); },
                () =>
                {
                    return Jackal.JackalFixedPatch.JackalsetTarget() && PlayerControl.LocalPlayer.CanMove;
                },
                () =>
                {
                    if (PlayerControl.LocalPlayer.isRole(RoleId.Jackal)) { Jackal.EndMeeting(); }
                    if (PlayerControl.LocalPlayer.isRole(RoleId.JackalSeer)) { JackalSeer.EndMeeting(); }
                },
                __instance.KillButton.graphic.sprite,
                new Vector3(0, 1, 0),
                __instance,
                __instance.KillButton,
                KeyCode.Q,
                8
            )
            {
                buttonText = FastDestroyableSingleton<HudManager>.Instance.KillButton.buttonLabelText.text,
                showButtonText = true
            };

            SelfBomberButton = new Buttons.CustomButton(
                () =>
                {
                    if (PlayerControl.LocalPlayer.CanMove)
                    {
                        SelfBomber.SelfBomb();
                    }
                },
                (bool isAlive, RoleId role) => { return isAlive && ModeHandler.isMode(ModeId.Default) && SelfBomber.isSelfBomber(PlayerControl.LocalPlayer); },
                () =>
                {
                    return PlayerControl.LocalPlayer.CanMove;
                },
                () => { SelfBomber.EndMeeting(); },
                RoleClass.SelfBomber.GetButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.AbilityButton,
                KeyCode.F,
                49
            )
            {
                buttonText = ModTranslation.getString("SelfBomberButtonName"),
                showButtonText = true
            };

            DoorrDoorButton = new Buttons.CustomButton(
                () =>
                {
                    if (Doorr.CheckTarget() && PlayerControl.LocalPlayer.CanMove)
                    {
                        Doorr.DoorrBtn();
                        Roles.RoleClass.Doorr.ButtonTimer = DateTime.Now;
                        Doorr.ResetCoolDown();
                    }
                },
                (bool isAlive, RoleId role) => { return isAlive && Doorr.isDoorr(PlayerControl.LocalPlayer); },
                () =>
                {
                    return Doorr.CheckTarget() && PlayerControl.LocalPlayer.CanMove;
                },
                () => { Doorr.EndMeeting(); },
                RoleClass.Doorr.GetButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.AbilityButton,
                KeyCode.F,
                49
            )
            {
                buttonText = ModTranslation.getString("DoorrButtonText"),
                showButtonText = true
            };

            TeleporterButton = new Buttons.CustomButton(
                () =>
                {
                    if (!PlayerControl.LocalPlayer.CanMove) return;
                    RoleClass.Clergyman.ButtonTimer = DateTime.Now;
                    TeleporterButton.actionButton.cooldownTimerText.color = new Color(0F, 0.8F, 0F);
                    Teleporter.TeleportStart();
                    Teleporter.ResetCoolDown();
                },
                (bool isAlive, RoleId role) => { return isAlive && (role == RoleId.Teleporter || role == RoleId.TeleportingJackal || role == RoleId.NiceTeleporter || (role == RoleId.Levelinger && RoleClass.Levelinger.IsPower(RoleClass.Levelinger.LevelPowerTypes.Teleporter))); },
                () =>
                {
                    return true && PlayerControl.LocalPlayer.CanMove;
                },
                () => { Teleporter.EndMeeting(); },
                RoleClass.Teleporter.GetButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.AbilityButton,
                KeyCode.F,
                49
            )
            {
                buttonText = ModTranslation.getString("TeleporterTeleportButton"),
                showButtonText = true
            };

            MovingSetButton = new Buttons.CustomButton(
                () =>
                {
                    if (!PlayerControl.LocalPlayer.CanMove) return;
                    if (!Moving.IsSetPostion())
                    {
                        Moving.SetPostion();
                    }
                    Moving.ResetCoolDown();
                },
                (bool isAlive, RoleId role) => { return isAlive && (role == RoleId.Moving || role == RoleId.EvilMoving || RoleClass.Levelinger.IsPower(RoleClass.Levelinger.LevelPowerTypes.Moving)) && !Moving.IsSetPostion(); },
                () =>
                {
                    return true && PlayerControl.LocalPlayer.CanMove;
                },
                () => { Moving.EndMeeting(); },
                RoleClass.Moving.getNoSetButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.AbilityButton,
                KeyCode.F,
                49
            )
            {
                buttonText = ModTranslation.getString("MovingButtonSetName"),
                showButtonText = true
            };

            MovingTpButton = new Buttons.CustomButton(
                () =>
                {
                    if (!PlayerControl.LocalPlayer.CanMove) return;
                    if (Moving.IsSetPostion())
                    {
                        Moving.TP();
                    }
                    Moving.ResetCoolDown();
                },
                (bool isAlive, RoleId role) => { return isAlive && (role == RoleId.Moving || role == RoleId.EvilMoving || RoleClass.Levelinger.IsPower(RoleClass.Levelinger.LevelPowerTypes.Moving)) && Moving.IsSetPostion(); },
                () =>
                {
                    return true && PlayerControl.LocalPlayer.CanMove;
                },
                () => { Moving.EndMeeting(); },
                RoleClass.Moving.getSetButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.AbilityButton,
                KeyCode.F,
                49
            )
            {
                buttonText = ModTranslation.getString("MovingButtonTpName"),
                showButtonText = true
            };

            SheriffKillButton = new Buttons.CustomButton(
                () =>
                {
                    if (PlayerControl.LocalPlayer.isRole(RoleId.RemoteSheriff))
                    {
                        DestroyableSingleton<RoleManager>.Instance.SetRole(PlayerControl.LocalPlayer, RoleTypes.Shapeshifter);
                        foreach (CachedPlayer p in CachedPlayer.AllPlayers)
                        {
                            p.Data.Role.NameColor = Color.white;
                        }
                        CachedPlayer.LocalPlayer.Data.Role.TryCast<ShapeshifterRole>().UseAbility();
                        foreach (CachedPlayer p in CachedPlayer.AllPlayers)
                        {
                            if (p.PlayerControl.isImpostor())
                            {
                                p.Data.Role.NameColor = RoleClass.ImpostorRed;
                            }
                        }
                        DestroyableSingleton<RoleManager>.Instance.SetRole(PlayerControl.LocalPlayer, RoleTypes.Crewmate);
                    }
                    else if (PlayerControl.LocalPlayer.isRole(RoleId.Sheriff))
                    {
                        if (RoleClass.Sheriff.KillMaxCount >= 1 && setTarget())
                        {
                            RoleClass.Sheriff.KillMaxCount--;
                            var Target = PlayerControlFixedUpdatePatch.setTarget();
                            var misfire = !Roles.Sheriff.IsSheriffKill(Target);
                            var TargetID = Target.PlayerId;
                            var LocalID = CachedPlayer.LocalPlayer.PlayerId;

                            CustomRPC.RPCProcedure.SheriffKill(LocalID, TargetID, misfire);
                            MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.SheriffKill, Hazel.SendOption.Reliable, -1);
                            killWriter.Write(LocalID);
                            killWriter.Write(TargetID);
                            killWriter.Write(misfire);
                            AmongUsClient.Instance.FinishRpcImmediately(killWriter);
                            Sheriff.ResetKillCoolDown();
                        }
                    }
                },
                (bool isAlive, RoleId role) => { return isAlive && ModeHandler.isMode(ModeId.Default) && Sheriff.IsSheriffButton(PlayerControl.LocalPlayer); },
                () =>
                {
                    float killcount = 0f;
                    bool flag = false;
                    if (PlayerControl.LocalPlayer.isRole(RoleId.RemoteSheriff))
                    {
                        killcount = RoleClass.RemoteSheriff.KillMaxCount;
                        flag = true;
                    }
                    else if (PlayerControl.LocalPlayer.isRole(RoleId.Sheriff))
                    {
                        killcount = RoleClass.Sheriff.KillMaxCount;
                        flag = PlayerControlFixedUpdatePatch.setTarget() && PlayerControl.LocalPlayer.CanMove;
                    }
                    if (killcount > 0)
                        sheriffNumShotsText.text = String.Format(ModTranslation.getString("SheriffNumTextName"), killcount);
                    else
                        sheriffNumShotsText.text = "";
                    return flag;
                },
                () => { Sheriff.EndMeeting(); },
                RoleClass.Sheriff.getButtonSprite(),
                new Vector3(0f, 1f, 0),
                __instance,
                __instance.KillButton,
                KeyCode.Q,
                8
            );
            sheriffNumShotsText = GameObject.Instantiate(SheriffKillButton.actionButton.cooldownTimerText, SheriffKillButton.actionButton.cooldownTimerText.transform.parent);
            sheriffNumShotsText.text = "";
            sheriffNumShotsText.enableWordWrapping = false;
            sheriffNumShotsText.transform.localScale = Vector3.one * 0.5f;
            sheriffNumShotsText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);
            SheriffKillButton.buttonText = ModTranslation.getString("SheriffKillButtonName");
            SheriffKillButton.showButtonText = true;

            ClergymanLightOutButton = new Buttons.CustomButton(
                () =>
                {
                    RoleClass.Clergyman.IsLightOff = true;
                    Roles.RoleClass.Clergyman.ButtonTimer = DateTime.Now;
                    ClergymanLightOutButton.actionButton.cooldownTimerText.color = new Color(0F, 0.8F, 0F);
                    Clergyman.LightOutStart();
                },
                (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Clergyman; },
                () =>
                {
                    return ClergymanLightOutButton.Timer <= 0;
                },
                () => { Clergyman.EndMeeting(); },
                RoleClass.Clergyman.getButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.AbilityButton,
                KeyCode.F,
                49
            )
            {
                buttonText = ModTranslation.getString("ClergymanLightOutButtonName"),
                showButtonText = true
            };

            SpeedBoosterBoostButton = new Buttons.CustomButton(
                () =>
                {
                    Roles.RoleClass.SpeedBooster.ButtonTimer = DateTime.Now;
                    SpeedBoosterBoostButton.actionButton.cooldownTimerText.color = new Color(0F, 0.8F, 0F);
                    SpeedBooster.BoostStart();
                },
                (bool isAlive, RoleId role) => { return isAlive && role == RoleId.SpeedBooster; },
                () =>
                {
                    if (SpeedBoosterBoostButton.Timer <= 0)
                    {
                        return true;
                    }
                    return false;
                },
                () => { SpeedBooster.EndMeeting(); },
                RoleClass.SpeedBooster.GetSpeedBoostButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.AbilityButton,
                KeyCode.F,
                49
            )
            {
                buttonText = ModTranslation.getString("SpeedBoosterBoostButtonName"),
                showButtonText = true,
                HasEffect = true
            };

            EvilSpeedBoosterBoostButton = new Buttons.CustomButton(
                () =>
                {
                    Roles.RoleClass.EvilSpeedBooster.ButtonTimer = DateTime.Now;
                    EvilSpeedBoosterBoostButton.actionButton.cooldownTimerText.color = new Color(0F, 0.8F, 0F);
                    EvilSpeedBooster.BoostStart();
                },
                (bool isAlive, RoleId role) => { return isAlive && (role == RoleId.EvilSpeedBooster || RoleClass.Levelinger.IsPower(RoleClass.Levelinger.LevelPowerTypes.SpeedBooster)); },
                () =>
                {
                    if (EvilSpeedBoosterBoostButton.Timer <= 0)
                    {
                        return true;
                    }
                    return false;
                },
                () => { EvilSpeedBooster.EndMeeting(); },
                RoleClass.SpeedBooster.GetSpeedBoostButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.AbilityButton,
                KeyCode.F,
                49
            )
            {
                buttonText = ModTranslation.getString("EvilSpeedBoosterBoostButtonName"),
                showButtonText = true
            };

            LighterLightOnButton = new Buttons.CustomButton(
                () =>
                {
                    RoleClass.Lighter.IsLightOn = true;
                    Roles.RoleClass.Lighter.ButtonTimer = DateTime.Now;
                    LighterLightOnButton.actionButton.cooldownTimerText.color = new Color(0F, 0.8F, 0F);
                    Lighter.LightOnStart();
                },
                (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Lighter; },
                () =>
                {
                    if (LighterLightOnButton.Timer <= 0)
                    {
                        return true;
                    }
                    return false;
                },
                () => { Lighter.EndMeeting(); },
                RoleClass.Lighter.getButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.AbilityButton,
                KeyCode.F,
                49
            )
            {
                buttonText = ModTranslation.getString("LighterButtonName"),
                showButtonText = true
            };

            ImpostorSidekickButton = new CustomButton(
                () =>
                {
                    var target = setTarget(Crewmateonly: true);
                    if (target && RoleHelpers.isAlive(PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.CanMove && !RoleClass.Levelinger.IsCreateMadmate)
                    {
                        target.setRoleRPC(RoleId.MadMate);
                        RoleClass.Levelinger.IsCreateMadmate = true;
                    }
                },
                (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Levelinger && RoleClass.Levelinger.IsPower(RoleClass.Levelinger.LevelPowerTypes.Sidekick) && !RoleClass.Levelinger.IsCreateMadmate; },
                () =>
                {
                    return setTarget(Crewmateonly: true) && PlayerControl.LocalPlayer.CanMove;
                },
                () =>
                {
                    ImpostorSidekickButton.MaxTimer = PlayerControl.GameOptions.KillCooldown;
                    ImpostorSidekickButton.Timer = PlayerControl.GameOptions.KillCooldown;
                },
                RoleClass.Jackal.getButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.AbilityButton,
                KeyCode.F,
                49
            )
            {
                buttonText = ModTranslation.getString("SidekickName"),
                showButtonText = true
            };

            SideKillerSidekickButton = new CustomButton(
                () =>
                {
                    var target = setTarget(Crewmateonly: true);
                    if (target && RoleHelpers.isAlive(PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.CanMove && !RoleClass.SideKiller.IsCreateMadKiller)
                    {
                        MessageWriter writer = RPCHelper.StartRPC(CustomRPC.CustomRPC.SetMadKiller);
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
                    return setTarget(Crewmateonly: true) && PlayerControl.LocalPlayer.CanMove;
                },
                () =>
                {
                    SideKillerSidekickButton.MaxTimer = RoleClass.SideKiller.KillCoolTime;
                    SideKillerSidekickButton.Timer = RoleClass.SideKiller.KillCoolTime;
                },
                RoleClass.Jackal.getButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.AbilityButton,
                KeyCode.F,
                49
            )
            {
                buttonText = ModTranslation.getString("SidekickName"),
                showButtonText = true
            };

            MadMakerSidekickButton = new CustomButton(
                () =>
                {
                    var target = setTarget();
                    if (!target.Data.Role.IsImpostor && target && RoleHelpers.isAlive(PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.CanMove && !RoleClass.MadMaker.IsCreateMadmate)
                    {
                        target.RPCSetRoleUnchecked(RoleTypes.Crewmate);
                        target.setRoleRPC(RoleId.MadMate);
                        RoleClass.MadMaker.IsCreateMadmate = true;
                    }
                    else if (target.Data.Role.IsImpostor)
                    {
                        if (ModeHandler.isMode(ModeId.Default))
                        {
                            if (PlayerControl.LocalPlayer.isRole(RoleId.MadMaker))
                            {
                                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.RPCMurderPlayer, SendOption.Reliable, -1);
                                writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                                writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                                writer.Write(byte.MaxValue);
                                AmongUsClient.Instance.FinishRpcImmediately(writer);
                                RPCProcedure.RPCMurderPlayer(CachedPlayer.LocalPlayer.PlayerId, CachedPlayer.LocalPlayer.PlayerId, byte.MaxValue);
                            }
                        }
                        else if (ModeHandler.isMode(ModeId.SuperHostRoles))
                        {
                            if (AmongUsClient.Instance.AmHost)
                            {
                                foreach (PlayerControl p in RoleClass.MadMaker.MadMakerPlayer)
                                {
                                    p.RpcMurderPlayer(p);
                                }
                            }
                        }
                    }
                },
                (bool isAlive, RoleId role) => { return isAlive && role == RoleId.MadMaker && ModeHandler.isMode(ModeId.Default) && !RoleClass.MadMaker.IsCreateMadmate; },
                () =>
                {
                    return setTarget() && PlayerControl.LocalPlayer.CanMove;
                },
                () => { },
                RoleClass.Jackal.getButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.AbilityButton,
                KeyCode.F,
                49
            )
            {
                buttonText = ModTranslation.getString("SidekickName"),
                showButtonText = true
            };

            RoleClass.SerialKiller.SuicideKillText = GameObject.Instantiate(FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText, FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText.transform.parent);
            RoleClass.SerialKiller.SuicideKillText.text = "";
            RoleClass.SerialKiller.SuicideKillText.enableWordWrapping = false;
            RoleClass.SerialKiller.SuicideKillText.transform.localScale = Vector3.one * 0.5f;
            RoleClass.SerialKiller.SuicideKillText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);

            DemonButton = new CustomButton(
                () =>
                {
                    Demon.DemonCurse(setTarget(untarget: Demon.GetUntarget()));
                    DemonButton.Timer = DemonButton.MaxTimer;
                },
                (bool isAlive, RoleId role) => { return Demon.IsButton(); },
                () =>
                {
                    return setTarget(untarget: Demon.GetUntarget()) && PlayerControl.LocalPlayer.CanMove;
                },
                () =>
                {
                    DemonButton.MaxTimer = RoleClass.Demon.CoolTime;
                    DemonButton.Timer = RoleClass.Demon.CoolTime;
                },
                RoleClass.Demon.getButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.AbilityButton,
                KeyCode.F,
                49
            )
            {
                buttonText = ModTranslation.getString("DemonButtonName"),
                showButtonText = true
            };

            ArsonistDouseButton = new CustomButton(
                () =>
                {
                    var Target = setTarget(untarget: Arsonist.GetUntarget());
                    RoleClass.Arsonist.DouseTarget = Target;
                    ArsonistDouseButton.MaxTimer = RoleClass.Arsonist.DurationTime;
                    ArsonistDouseButton.Timer = ArsonistDouseButton.MaxTimer;
                    ArsonistDouseButton.actionButton.cooldownTimerText.color = new Color(0F, 0.8F, 0F);
                    RoleClass.Arsonist.IsDouse = true;
                    //SuperNewRolesPlugin.Logger.LogInfo("アーソニストが塗るボタンを押した");
                },
                (bool isAlive, RoleId role) => { return Arsonist.IsButton(); },
                () =>
                {
                    return setTarget(untarget: Arsonist.GetUntarget()) && PlayerControl.LocalPlayer.CanMove;
                },
                () =>
                {
                    ArsonistDouseButton.MaxTimer = RoleClass.Arsonist.CoolTime;
                    ArsonistDouseButton.Timer = RoleClass.Arsonist.CoolTime;
                },
                RoleClass.Arsonist.getDouseButtonSprite(),
                new Vector3(0f, 1f, 0),
                __instance,
                __instance.AbilityButton,
                KeyCode.Q,
                49
            )
            {
                buttonText = ModTranslation.getString("ArsonistDouseButtonName"),
                showButtonText = true
            };

            ArsonistIgniteButton = new CustomButton(
                () =>
                {
                    Arsonist.SetWinArsonist();
                    RoleClass.Arsonist.TriggerArsonistWin = true;
                    AdditionalTempData.winCondition = EndGame.WinCondition.ArsonistWin;
                    RPCProcedure.ShareWinner(CachedPlayer.LocalPlayer.PlayerId);
                    MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.ShareWinner, SendOption.Reliable, -1);
                    Writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(Writer);

                    Writer = RPCHelper.StartRPC(CustomRPC.CustomRPC.SetWinCond);
                    Writer.Write((byte)CustomGameOverReason.ArsonistWin);
                    Writer.EndRPC();
                    RPCProcedure.SetWinCond((byte)CustomGameOverReason.ArsonistWin);
                    //SuperNewRolesPlugin.Logger.LogInfo("CheckAndEndGame");
                    var reason = (GameOverReason)EndGame.CustomGameOverReason.ArsonistWin;
                    if (ModeHandler.isMode(ModeId.SuperHostRoles)) reason = GameOverReason.ImpostorByKill;
                    if (AmongUsClient.Instance.AmHost)
                    {
                        CheckGameEndPatch.CustomEndGame(reason, false);
                    }
                    else
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.CustomEndGame, SendOption.Reliable, -1);
                        writer.Write((byte)reason);
                        writer.Write(false);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                    }
                },
                (bool isAlive, RoleId role) => { return Arsonist.IseveryButton(); },
                () =>
                {
                    if (Arsonist.IsWin(PlayerControl.LocalPlayer))
                    {
                        return true;
                    }
                    return false;
                },
                () =>
                {
                    ArsonistIgniteButton.MaxTimer = 0;
                    ArsonistIgniteButton.Timer = 0;
                },
                RoleClass.Arsonist.getIgniteButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.AbilityButton,
                KeyCode.F,
                49
            )
            {
                buttonText = ModTranslation.getString("ArsonistIgniteButtonName"),
                showButtonText = true
            };

            SpeederButton = new CustomButton(
                () =>
                {
                    SpeederButton.MaxTimer = RoleClass.Speeder.DurationTime;
                    SpeederButton.Timer = SpeederButton.MaxTimer;
                    SpeederButton.actionButton.cooldownTimerText.color = new Color(0F, 0.8F, 0F);
                    Speeder.DownStart();
                },
                (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Speeder; },
                () =>
                {
                    return PlayerControl.LocalPlayer.CanMove;
                },
                () => { Speeder.EndMeeting(); },
                RoleClass.Speeder.GetButtonSprite(),

                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.AbilityButton,
                KeyCode.F,
                49
            )
            {
                buttonText = ModTranslation.getString("SpeederButtonName"),
                showButtonText = true,
                HasEffect = true
            };

            ChiefSidekickButton = new CustomButton(
                () =>
                {
                    var target = setTarget();
                    if (!target.Data.Role.IsImpostor && target && RoleHelpers.isAlive(PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.CanMove && !RoleClass.Chief.IsCreateSheriff)
                    {
                        target.RPCSetRoleUnchecked(RoleTypes.Crewmate);
                        target.setRoleRPC(RoleId.Sheriff);
                        RoleClass.Chief.IsCreateSheriff = true;
                    }
                    else if (target.Data.Role.IsImpostor)
                    {
                        if (ModeHandler.isMode(ModeId.Default))
                        {
                            if (PlayerControl.LocalPlayer.isRole(RoleId.Chief))
                            {
                                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.RPCMurderPlayer, SendOption.Reliable, -1);
                                writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                                writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                                writer.Write(byte.MaxValue);
                                AmongUsClient.Instance.FinishRpcImmediately(writer);
                                RPCProcedure.RPCMurderPlayer(CachedPlayer.LocalPlayer.PlayerId, CachedPlayer.LocalPlayer.PlayerId, byte.MaxValue);
                            }
                        }
                        else if (ModeHandler.isMode(ModeId.SuperHostRoles))
                        {
                            if (AmongUsClient.Instance.AmHost)
                            {
                                foreach (PlayerControl p in RoleClass.Chief.ChiefPlayer)
                                {
                                    p.RpcMurderPlayer(p);
                                }
                            }
                        }
                    }
                },
                (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Chief && ModeHandler.isMode(ModeId.Default) && !RoleClass.Chief.IsCreateSheriff; },
                () =>
                {
                    return setTarget() && PlayerControl.LocalPlayer.CanMove;
                },
                () => { },
                RoleClass.Chief.getButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.AbilityButton,
                KeyCode.F,
                49
            )
            {
                buttonText = ModTranslation.getString("SidekickName"),
                showButtonText = true
            };

            VultureButton = new CustomButton(
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
                                if (Vector2.Distance(truePosition2, truePosition) <= PlayerControl.LocalPlayer.MaxReportDistance && PlayerControl.LocalPlayer.CanMove && !PhysicsHelpers.AnythingBetween(truePosition, truePosition2, Constants.ShipAndObjectsMask, false))
                                {
                                    GameData.PlayerInfo playerInfo = GameData.Instance.GetPlayerById(component.ParentId);

                                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.CleanBody, Hazel.SendOption.Reliable, -1);
                                    writer.Write(playerInfo.PlayerId);
                                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                                    RPCProcedure.CleanBody(playerInfo.PlayerId);
                                    RoleClass.Vulture.DeadBodyCount--;
                                    SuperNewRolesPlugin.Logger.LogInfo("DeadBodyCount:" + RoleClass.Vulture.DeadBodyCount);
                                    VultureButton.Timer = VultureButton.MaxTimer;
                                    break;
                                }
                            }
                        }
                    }
                    if (RoleClass.Vulture.DeadBodyCount < 0)
                    {
                        CustomRPC.RPCProcedure.ShareWinner(CachedPlayer.LocalPlayer.PlayerId);
                        MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.ShareWinner, SendOption.Reliable, -1);
                        Writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(Writer);
                        if (AmongUsClient.Instance.AmHost)
                        {
                            CheckGameEndPatch.CustomEndGame((GameOverReason)EndGame.CustomGameOverReason.VultureWin, false);
                        }
                        else
                        {
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.CustomEndGame, SendOption.Reliable, -1);
                            writer.Write((byte)EndGame.CustomGameOverReason.VultureWin);
                            writer.Write(false);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                        }
                    }
                },
                (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Vulture; },
                () =>
                {
                    return __instance.ReportButton.graphic.color == Palette.EnabledColor && PlayerControl.LocalPlayer.CanMove;
                },
                () =>
                {
                    VultureButton.MaxTimer = RoleClass.Vulture.CoolTime;
                    VultureButton.Timer = RoleClass.Vulture.CoolTime;
                },
                RoleClass.Vulture.getButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.AbilityButton,
                KeyCode.F,
                49
            )
            {
                buttonText = ModTranslation.getString("VultureButtonName"),
                showButtonText = true
            };

            ShielderButton = new CustomButton(
                () =>
                {
                    MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.SetShielder, SendOption.Reliable, -1);
                    Writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                    Writer.Write(true);
                    AmongUsClient.Instance.FinishRpcImmediately(Writer);
                    RPCProcedure.SetShielder(CachedPlayer.LocalPlayer.PlayerId, true);
                    ShielderButton.actionButton.cooldownTimerText.color = new Color(0F, 0.8F, 0F);
                    ShielderButton.MaxTimer = RoleClass.Shielder.DurationTime;
                    ShielderButton.Timer = ShielderButton.MaxTimer;
                },
                (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Shielder; },
                () =>
                {
                    return PlayerControl.LocalPlayer.CanMove;
                },
                () =>
                {
                    ShielderButton.MaxTimer = RoleClass.Shielder.CoolTime;
                    ShielderButton.Timer = RoleClass.Shielder.CoolTime;
                },
                RoleClass.Shielder.GetButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.AbilityButton,
                KeyCode.F,
                49
            )
            {
                buttonText = ModTranslation.getString("ShielderButtonName"),
                showButtonText = true
            };

            CleanerButton = new CustomButton(
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
                                if (Vector2.Distance(truePosition2, truePosition) <= PlayerControl.LocalPlayer.MaxReportDistance && PlayerControl.LocalPlayer.CanMove && !PhysicsHelpers.AnythingBetween(truePosition, truePosition2, Constants.ShipAndObjectsMask, false))
                                {
                                    GameData.PlayerInfo playerInfo = GameData.Instance.GetPlayerById(component.ParentId);

                                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.CleanBody, Hazel.SendOption.Reliable, -1);
                                    writer.Write(playerInfo.PlayerId);
                                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                                    RPCProcedure.CleanBody(playerInfo.PlayerId);
                                    CleanerButton.Timer = CleanerButton.MaxTimer;
                                    RoleClass.Cleaner.CleanMaxCount--;
                                    SuperNewRolesPlugin.Logger.LogInfo("DeadBodyCount:" + RoleClass.Cleaner.CleanMaxCount);
                                    CleanerButton.Timer = CleanerButton.MaxTimer;

                                    RoleClass.Cleaner.CoolTime = CleanerButton.Timer = CleanerButton.MaxTimer;
                                    PlayerControl.LocalPlayer.killTimer = RoleClass.Cleaner.CoolTime;
                                    break;
                                }
                            }
                        }
                    }
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
                RoleClass.Cleaner.getButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.AbilityButton,
                KeyCode.F,
                49
            )
            {
                buttonText = ModTranslation.getString("CleanerButtonName"),
                showButtonText = true
            };

            MadCleanerButton = new CustomButton(
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
                                if (Vector2.Distance(truePosition2, truePosition) <= PlayerControl.LocalPlayer.MaxReportDistance && PlayerControl.LocalPlayer.CanMove && !PhysicsHelpers.AnythingBetween(truePosition, truePosition2, Constants.ShipAndObjectsMask, false))
                                {
                                    GameData.PlayerInfo playerInfo = GameData.Instance.GetPlayerById(component.ParentId);

                                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.CleanBody, Hazel.SendOption.Reliable, -1);
                                    writer.Write(playerInfo.PlayerId);
                                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                                    RPCProcedure.CleanBody(playerInfo.PlayerId);
                                    break;
                                }
                            }
                        }
                    }
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
                RoleClass.MadCleaner.getButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.AbilityButton,
                KeyCode.F,
                49
            )
            {
                buttonText = ModTranslation.getString("CleanerButtonName"),
                showButtonText = true
            };

            FreezerButton = new Buttons.CustomButton(
                () =>
                {
                    FreezerButton.MaxTimer = RoleClass.Freezer.DurationTime;
                    FreezerButton.Timer = FreezerButton.MaxTimer;
                    FreezerButton.actionButton.cooldownTimerText.color = new Color(0F, 0.8F, 0F);
                    Freezer.DownStart();
                },
                (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Freezer; },
                () =>
                {
                    return PlayerControl.LocalPlayer.CanMove;
                },
                () => { Freezer.EndMeeting(); },
                RoleClass.Freezer.GetButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.AbilityButton,
                KeyCode.F,
                49
            )
            {
                buttonText = ModTranslation.getString("FreezerButtonName"),
                showButtonText = true,
                HasEffect = true
            };

            SamuraiButton = new Buttons.CustomButton(
                () =>
                {
                    if (PlayerControl.LocalPlayer.CanMove)
                    {
                        Samurai.SamuraiKill();
                    }
                },
                (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Samurai && ModeHandler.isMode(ModeId.Default) && !RoleClass.Samurai.Sword; },
                () =>
                {
                    return PlayerControl.LocalPlayer.CanMove;
                },
                () => { Samurai.EndMeeting(); },
                RoleClass.Samurai.GetButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.AbilityButton,
                KeyCode.F,
                49
            )
            {
                buttonText = ModTranslation.getString("SamuraiButtonName"),
                showButtonText = true
            };

            VentMakerButton = new CustomButton(
                () =>
                {
                    RoleClass.VentMaker.VentCount++;
                    MessageWriter writer = RPCHelper.StartRPC(CustomRPC.CustomRPC.MakeVent);
                    writer.Write(CachedPlayer.LocalPlayer.transform.position.x);
                    writer.Write(CachedPlayer.LocalPlayer.transform.position.y);
                    writer.Write(CachedPlayer.LocalPlayer.transform.position.z);
                    writer.EndRPC();
                    CustomRPC.RPCProcedure.MakeVent(CachedPlayer.LocalPlayer.transform.position.x, CachedPlayer.LocalPlayer.transform.position.y, CachedPlayer.LocalPlayer.transform.position.z);
                    GameObject Vent = GameObject.Find("VentMakerVent" + MapUtilities.CachedShipStatus.AllVents.Select(x => x.Id).Max().ToString());

                    RoleClass.VentMaker.Vent = Vent.GetComponent<Vent>();
                    if (RoleClass.VentMaker.VentCount == 2) RoleClass.VentMaker.IsMakeVent = false;
                },
                (bool isAlive, RoleId role) => { return isAlive && role == RoleId.VentMaker && RoleClass.VentMaker.IsMakeVent; },
                () =>
                {
                    return PlayerControl.LocalPlayer.CanMove;
                },
                () => { },
                RoleClass.VentMaker.getButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.AbilityButton,
                KeyCode.F,
                49
            )
            {
                buttonText = ModTranslation.getString("VentMakerButtonName"),
                showButtonText = true
            };

            GhostMechanicRepairButton = new CustomButton(
                () =>
                {
                    RoleClass.GhostMechanic.LimitCount--;

                    foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks.GetFastEnumerator())
                    {
                        if (task.TaskType == TaskTypes.FixLights)
                        {
                            RPCHelper.StartRPC(CustomRPC.CustomRPC.FixLights).EndRPC();
                            RPCProcedure.FixLights();
                        }
                        else if (task.TaskType == TaskTypes.RestoreOxy)
                        {
                            MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.LifeSupp, 0 | 64);
                            MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.LifeSupp, 1 | 64);
                        }
                        else if (task.TaskType == TaskTypes.ResetReactor)
                        {
                            MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Reactor, 16);
                        }
                        else if (task.TaskType == TaskTypes.ResetSeismic)
                        {
                            MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Laboratory, 16);
                        }
                        else if (task.TaskType == TaskTypes.FixComms)
                        {
                            MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Comms, 16 | 0);
                            MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Comms, 16 | 1);
                        }
                        else if (task.TaskType == TaskTypes.StopCharles)
                        {
                            MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Reactor, 0 | 16);
                            MapUtilities.CachedShipStatus.RpcRepairSystem(SystemTypes.Reactor, 1 | 16);
                        }
                    }
                    if (RoleClass.GhostMechanic.LimitCount <= 0)
                    {
                        GhostMechanicNumRepairText.text = "";
                    }
                },
                (bool isAlive, RoleId role) => { return !isAlive && PlayerControl.LocalPlayer.isGhostRole(RoleId.GhostMechanic) && RoleClass.GhostMechanic.LimitCount > 0; },
                () =>
                {
                    bool sabotageActive = false;
                    foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks.GetFastEnumerator())
                        if (task.TaskType == TaskTypes.FixLights || task.TaskType == TaskTypes.RestoreOxy || task.TaskType == TaskTypes.ResetReactor || task.TaskType == TaskTypes.ResetSeismic || task.TaskType == TaskTypes.FixComms || task.TaskType == TaskTypes.StopCharles
                            || (SubmergedCompatibility.isSubmerged() && task.TaskType == SubmergedCompatibility.RetrieveOxygenMask))
                        {
                            sabotageActive = true;
                            break;
                        }
                    GhostMechanicNumRepairText.text = String.Format(ModTranslation.getString("GhostMechanicCountText"), RoleClass.GhostMechanic.LimitCount);
                    return sabotageActive && PlayerControl.LocalPlayer.CanMove;
                },
                () => { GhostMechanicRepairButton.MaxTimer = 0f; GhostMechanicRepairButton.Timer = 0f; },
                RoleClass.GhostMechanic.getButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.AbilityButton,
                KeyCode.F,
                49
            );
            GhostMechanicNumRepairText = GameObject.Instantiate(GhostMechanicRepairButton.actionButton.cooldownTimerText, GhostMechanicRepairButton.actionButton.cooldownTimerText.transform.parent);
            GhostMechanicNumRepairText.text = "";
            GhostMechanicNumRepairText.enableWordWrapping = false;
            GhostMechanicNumRepairText.transform.localScale = Vector3.one * 0.5f;
            GhostMechanicNumRepairText.transform.localPosition += new Vector3(0f, 0.7f, 0);
            GhostMechanicRepairButton.buttonText = ModTranslation.getString("GhostMechanicButtonName");
            GhostMechanicRepairButton.showButtonText = true;

            EvilHackerButton = new CustomButton(
                () =>
                {
                    CachedPlayer.LocalPlayer.NetTransform.Halt();
                    Action<MapBehaviour> tmpAction = (MapBehaviour m) => { m.ShowCountOverlay(); };
                    FastDestroyableSingleton<HudManager>.Instance.ShowMap(tmpAction);
                },
                (bool isAlive, RoleId role) => { return role == RoleId.EvilHacker; },
                () =>
                {
                    return PlayerControl.LocalPlayer.CanMove;
                },
                () =>
                {
                    EvilHackerButton.MaxTimer = 0f;
                    EvilHackerButton.Timer = 0f;
                },
                RoleClass.EvilHacker.getButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.AbilityButton,
                KeyCode.F,
                49
            )
            {
                buttonText = ModTranslation.getString("ADMINButton"),
                showButtonText = true
            };

            EvilHackerMadmateSetting = new CustomButton(
                () =>
                {
                    var target = setTarget();
                    if (!target.Data.Role.IsImpostor && target && RoleHelpers.isAlive(PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.CanMove && RoleClass.EvilHacker.IsCreateMadmate)
                    {
                        target.RPCSetRoleUnchecked(RoleTypes.Crewmate);
                        target.setRoleRPC(RoleId.MadMate);
                        RoleClass.EvilHacker.IsCreateMadmate = false;
                    }
                    else if (target.Data.Role.IsImpostor)
                    {
                        PlayerControl.LocalPlayer.RpcMurderPlayer(PlayerControl.LocalPlayer);
                    }
                },
                (bool isAlive, RoleId role) => { return isAlive && role == RoleId.EvilHacker && ModeHandler.isMode(ModeId.Default) && RoleClass.EvilHacker.IsCreateMadmate; },
                () =>
                {
                    return setTarget() && PlayerControl.LocalPlayer.CanMove;
                },
                () => { },
                RoleClass.Jackal.getButtonSprite(),
                new Vector3(-2.7f, -0.06f, 0),
                __instance,
                __instance.AbilityButton,
                KeyCode.Q,
                8
            )
            {
                buttonText = ModTranslation.getString("SidekickName"),
                showButtonText = true
            };

            PositionSwapperButton = new CustomButton(
                () =>
                {
                    RoleClass.PositionSwapper.SwapCount--;
                    /*if (RoleClass.PositionSwapper.SwapCount >= 1){
                        PositionSwapperNumText.text = String.Format(ModTranslation.getString("SheriffNumTextName"), RoleClass.PositionSwapper.SwapCount);
                    }
                    else{
                        PositionSwapperNumText.text = "";
                    }*/
                    //RoleClass.PositionSwapper.ButtonTimer = DateTime.Now;
                    PositionSwapperButton.actionButton.cooldownTimerText.color = new Color(255F, 255F, 255F);
                    PositionSwapper.SwapStart();
                    PositionSwapper.ResetCoolDown();
                },
                (bool isAlive, RoleId role) => { return isAlive && role == RoleId.PositionSwapper; },
                () =>
                {
                    float swapcount = RoleClass.PositionSwapper.SwapCount;
                    if (swapcount > 0)
                        PositionSwapperNumText.text = String.Format(ModTranslation.getString("PositionSwapperNumTextName"), swapcount);
                    else
                        PositionSwapperNumText.text = String.Format(ModTranslation.getString("PositionSwapperNumTextName"), "0");
                    if (!PlayerControl.LocalPlayer.CanMove) return false;
                    if (RoleClass.PositionSwapper.SwapCount <= 0) return false;
                    return true && PlayerControl.LocalPlayer.CanMove;
                },
                () => { PositionSwapper.EndMeeting(); },
                RoleClass.PositionSwapper.getButtonSprite(),
                new Vector3(-1.8f, -0.06f, 0),
                __instance,
                __instance.AbilityButton,
                KeyCode.F,
                49
            );
            {
                PositionSwapperNumText = GameObject.Instantiate(PositionSwapperButton.actionButton.cooldownTimerText, PositionSwapperButton.actionButton.cooldownTimerText.transform.parent);
                PositionSwapperNumText.text = "";
                PositionSwapperNumText.enableWordWrapping = false;
                PositionSwapperNumText.transform.localScale = Vector3.one * 0.5f;
                PositionSwapperNumText.transform.localPosition += new Vector3(-0.05f, 0.7f, 0);
                PositionSwapperButton.buttonText = ModTranslation.getString("PositionSwapperButtonName");
                PositionSwapperButton.showButtonText = true;
            };

            ConjurerFirstAddButton = new CustomButton(
               () =>
               {
                   //マーカー設置
                   var pos = PlayerControl.LocalPlayer.transform.position;
                   byte[] buff = new byte[sizeof(float) * 2];
                   Buffer.BlockCopy(BitConverter.GetBytes(pos.x), 0, buff, 0 * sizeof(float), sizeof(float));
                   Buffer.BlockCopy(BitConverter.GetBytes(pos.y), 0, buff, 1 * sizeof(float), sizeof(float));

                   MessageWriter writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.AddMarker, Hazel.SendOption.Reliable);
                   writer.WriteBytesAndSize(buff);
                   writer.EndMessage();
                   RPCProcedure.AddMarker(buff);


                   //1回目カウントをtrueにする
                   RoleClass.Conjurer.FirstAdd = true;

                   //全ボタンのクールリセット
                   Conjurer.AllCoolReset();

               },
               (bool isAlive, RoleId role) => { return role == RoleId.Conjurer && !Conjurer.IsFirstAdded(); },
               () =>
               {
                   return PlayerControl.LocalPlayer.CanMove;
               },
                () =>
                {
                    ConjurerFirstAddButton.MaxTimer = RoleClass.Conjurer.CoolTime;
                    ConjurerFirstAddButton.Timer = RoleClass.Conjurer.CoolTime;
                },
               RoleClass.Conjurer.getAddButtonSprite(),
               new Vector3(0f, 1f, 0f),
               __instance,
               __instance.AbilityButton,
               KeyCode.F,
               49
            )
            {
                buttonText = ModTranslation.getString("1stAdd"),
                showButtonText = true
            };



            ConjurerSecondAddButton = new CustomButton(
               () =>
               {
                   //マーカー設置
                   var pos = PlayerControl.LocalPlayer.transform.position;
                   byte[] buff = new byte[sizeof(float) * 2];
                   Buffer.BlockCopy(BitConverter.GetBytes(pos.x), 0, buff, 0 * sizeof(float), sizeof(float));
                   Buffer.BlockCopy(BitConverter.GetBytes(pos.y), 0, buff, 1 * sizeof(float), sizeof(float));

                   MessageWriter writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.AddMarker, Hazel.SendOption.Reliable);
                   writer.WriteBytesAndSize(buff);
                   writer.EndMessage();
                   RPCProcedure.AddMarker(buff);

                   //2回目カウントをtrueにする
                   RoleClass.Conjurer.SecondAdd = true;

                   //全ボタンのクールリセット
                   Conjurer.AllCoolReset();

               },
               (bool isAlive, RoleId role) => { return role == RoleId.Conjurer && Conjurer.IsFirstAdded() && !Conjurer.IsSecondAdded(); },
               () =>
               {
                   return PlayerControl.LocalPlayer.CanMove;
               },
                () =>
                {
                    ConjurerSecondAddButton.MaxTimer = RoleClass.Conjurer.CoolTime;
                    ConjurerSecondAddButton.Timer = RoleClass.Conjurer.CoolTime;
                },
               RoleClass.Conjurer.getAddButtonSprite(),
               new Vector3(0f, 1f, 0f),
               __instance,
               __instance.AbilityButton,
               KeyCode.F,
               49
            )
            {
                buttonText = ModTranslation.getString("2ndAdd"),
                showButtonText = true
            };



            ConjurerThirdAddButton = new CustomButton(
               () =>
               {
                   //マーカー設置
                   var pos = PlayerControl.LocalPlayer.transform.position;
                   byte[] buff = new byte[sizeof(float) * 2];
                   Buffer.BlockCopy(BitConverter.GetBytes(pos.x), 0, buff, 0 * sizeof(float), sizeof(float));
                   Buffer.BlockCopy(BitConverter.GetBytes(pos.y), 0, buff, 1 * sizeof(float), sizeof(float));

                   MessageWriter writer = AmongUsClient.Instance.StartRpc(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.AddMarker, Hazel.SendOption.Reliable);
                   writer.WriteBytesAndSize(buff);
                   writer.EndMessage();
                   RPCProcedure.AddMarker(buff);

                   //3回目カウントをtrueにする
                   RoleClass.Conjurer.ThirdAdd = true;

                   //全ボタンのクールリセット
                   Conjurer.AllCoolReset();


               },
               (bool isAlive, RoleId role) => { return role == RoleId.Conjurer && Conjurer.IsFirstAdded() && Conjurer.IsSecondAdded() && !Conjurer.IsThirdAdded(); },
               () =>
               {
                   return PlayerControl.LocalPlayer.CanMove;
               },
                () =>
                {
                    ConjurerThirdAddButton.MaxTimer = RoleClass.Conjurer.CoolTime;
                    ConjurerThirdAddButton.Timer = RoleClass.Conjurer.CoolTime;
                },
               RoleClass.Conjurer.getAddButtonSprite(),
               new Vector3(0f, 1f, 0f),
               __instance,
               __instance.AbilityButton,
               KeyCode.F,
               49
            )
            {
                buttonText = ModTranslation.getString("3rdAdd"),
                showButtonText = true
            };


            ConjurerStartButton = new CustomButton(
                    () =>
                    {
                        //ニセレに任せた
                        //発光(色)
                        Conjurer.ShowFlash(new Color(139f / 139f, 0f / 0f, 205f / 205f));

                        //全ボタンのクールリセット
                        Conjurer.AllCoolReset();

                        //カウントを全部falseに
                        Conjurer.AllClear();

                        //全部Marker消す
                        JackInTheBox.clearJackInTheBoxes();
                    },
                    (bool isAlive, RoleId role) => { return role == RoleId.Conjurer && Conjurer.IsThirdAdded(); },
                    () =>
                    {
                        return PlayerControl.LocalPlayer.CanMove;
                    },
                    () =>
                        {
                            ConjurerStartButton.MaxTimer = 0f;
                            ConjurerStartButton.Timer = 0f;
                            Conjurer.AllCoolReset();
                        },
                    RoleClass.Conjurer.getStartButtonSprite(),
                    new Vector3(0f, 1f, 0f),
                    __instance,
                    __instance.AbilityButton,
                    KeyCode.F,
                    49
                )
            {
                buttonText = ModTranslation.getString("Kill"),
                showButtonText = true
            };

            setCustomButtonCooldowns();
        }
    }
}
