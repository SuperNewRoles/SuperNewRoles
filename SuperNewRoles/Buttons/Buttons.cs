using System.Collections.ObjectModel;
using HarmonyLib;
using Hazel;
using System;
using UnityEngine;
using SuperNewRoles.Buttons;
using System.Collections.Generic;
using System.Linq;
using SuperNewRoles.Roles;
using SuperNewRoles.Patches;
using System.Collections;
using SuperNewRoles.Mode;
using SuperNewRoles.Helpers;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.EndGame;

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
        public static CustomButton CustomSabotageButton;
        public static CustomButton MovingSetButton;
        public static CustomButton MovingTpButton;
        public static CustomButton TeleporterButton;
        public static CustomButton DoorrDoorButton;
        public static CustomButton SelfBomberButton;
        public static CustomButton DoctorVitalsButton;
        public static CustomButton CountChangerButton;
        public static CustomButton ScientistButton;

        public static CustomButton HawkHawkEyeButton;
        public static CustomButton FreezerFreezeButton;
        public static CustomButton JackalKillButton;
        public static CustomButton JackalSidekickButton;
        public static CustomButton JackalSeerKillButton;
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

        public static TMPro.TMP_Text sheriffNumShotsText;
        public static TMPro.TMP_Text CleanerNumCleanText;
        public static TMPro.TMP_Text GhostMechanicNumRepairText;

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

        private static PlayerControl SheriffKillTarget;

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
                () => { return RoleHelpers.isAlive(PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.isRole(RoleId.FalseCharges); },
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
            );

            FalseChargesFalseChargeButton.buttonText = ModTranslation.getString("FalseChargesButtonTitle");
            FalseChargesFalseChargeButton.showButtonText = true;

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
                  () => { return RoleHelpers.isAlive(PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.truelover) && !RoleClass.truelover.IsCreate; },
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
              );

            trueloverLoveButton.buttonText = ModTranslation.getString("trueloverloveButtonName");
            trueloverLoveButton.showButtonText = true;

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
                  () => { return RoleHelpers.isAlive(PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.Magaziner); },
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
              );

            MagazinerGetButton.buttonText = ModTranslation.getString("MagazinerGetButtonName");
            MagazinerGetButton.showButtonText = true;

            MagazinerAddButton = new CustomButton(
                () =>
                {
                    if (!FastDestroyableSingleton<HudManager>.Instance.KillButton.isCoolingDown && PlayerControl.LocalPlayer.CanMove)
                    {
                        PlayerControl.LocalPlayer.SetKillTimerUnchecked(PlayerControl.GameOptions.KillCooldown);
                        RoleClass.Magaziner.MyPlayerCount++;
                    }
                },
                () => { return RoleHelpers.isAlive(PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.Magaziner); },
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
            );

            MagazinerAddButton.buttonText = ModTranslation.getString("MagazinerAddButtonName");
            MagazinerAddButton.showButtonText = true;

            ScientistButton = new Buttons.CustomButton(
                () =>
                {
                    if (!PlayerControl.LocalPlayer.CanMove) return;
                    Roles.RoleClass.NiceScientist.ButtonTimer = DateTime.Now;
                    ScientistButton.actionButton.cooldownTimerText.color = new Color(0F, 0.8F, 0F);
                    Scientist.Start();
                },
                () => { return RoleHelpers.isAlive(PlayerControl.LocalPlayer) && (PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.NiceScientist) || PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.EvilScientist)); },
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
            );

            ScientistButton.buttonText = ModTranslation.getString("ScientistButtonName");
            ScientistButton.showButtonText = true;

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
               () => { return PlayerControl.LocalPlayer.isAlive() && PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.Hawk) || PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.NiceHawk) || PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.MadHawk); },
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
            );

            HawkHawkEyeButton.buttonText = ModTranslation.getString("HawkButtonName");
            HawkHawkEyeButton.showButtonText = true;

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

                       MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.CountChangerSetRPC, Hazel.SendOption.Reliable, -1);
                       killWriter.Write(LocalID);
                       killWriter.Write(TargetID);
                       AmongUsClient.Instance.FinishRpcImmediately(killWriter);
                   }
               },
               () => { return PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.CountChanger) && !RoleClass.CountChanger.IsSet && RoleClass.CountChanger.Count >= 1 && PlayerControl.LocalPlayer.isAlive(); },
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
            );

            CountChangerButton.buttonText = ModTranslation.getString("CountChangerButtonName");
            CountChangerButton.showButtonText = true;

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
               () => { return PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.Doctor) && PlayerControl.LocalPlayer.CanMove; },
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
            );


            DoctorVitalsButton.buttonText = ModTranslation.getString("DoctorVitalName");
            DoctorVitalsButton.showButtonText = true;

            JackalSidekickButton = new CustomButton(
                () =>
                {
                    var target = Jackal.JackalFixedPatch.JackalsetTarget();
                    if (target && RoleHelpers.isAlive(PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.CanMove && RoleClass.Jackal.IsCreateSidekick)
                    {
                        bool IsFakeSidekick = EvilEraser.IsBlockAndTryUse(EvilEraser.BlockTypes.JackalSidekick, target);
                        MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.CreateSidekick, Hazel.SendOption.Reliable, -1);
                        killWriter.Write(target.PlayerId);
                        killWriter.Write(IsFakeSidekick);
                        AmongUsClient.Instance.FinishRpcImmediately(killWriter);
                        CustomRPC.RPCProcedure.CreateSidekick(target.PlayerId, IsFakeSidekick);
                        RoleClass.Jackal.IsCreateSidekick = false;
                        Jackal.resetCoolDown();
                    }
                },
                () => { return ModeHandler.isMode(ModeId.Default) && RoleHelpers.isAlive(PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.Jackal) && RoleClass.Jackal.IsCreateSidekick; },
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
            );

            JackalSidekickButton.buttonText = ModTranslation.getString("JackalCreateSidekickButtonName");
            JackalSidekickButton.showButtonText = true;

            JackalSeerSidekickButton = new CustomButton(
                () =>
                {
                    var target_JS = JackalSeer.JackalSeerFixedPatch.JackalSeersetTarget();
                    if (target_JS && RoleHelpers.isAlive(PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.CanMove && RoleClass.JackalSeer.IsCreateSidekick)
                    {
                        bool IsFakeSidekickSeer = EvilEraser.IsBlockAndTryUse(EvilEraser.BlockTypes.JackalSeerSidekick, target_JS);
                        MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.CreateSidekickSeer, Hazel.SendOption.Reliable, -1);
                        killWriter.Write(target_JS.PlayerId);
                        killWriter.Write(IsFakeSidekickSeer);
                        AmongUsClient.Instance.FinishRpcImmediately(killWriter);
                        CustomRPC.RPCProcedure.CreateSidekickSeer(target_JS.PlayerId, IsFakeSidekickSeer);
                        RoleClass.JackalSeer.IsCreateSidekick = false;
                        JackalSeer.resetCoolDown();
                    }
                },
                () => { return ModeHandler.isMode(ModeId.Default) && RoleHelpers.isAlive(PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.JackalSeer) && RoleClass.JackalSeer.IsCreateSidekick; },
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
            );
            JackalSeerSidekickButton.buttonText = ModTranslation.getString("JackalCreateSidekickButtonName");
            JackalSeerSidekickButton.showButtonText = true;

            JackalKillButton = new CustomButton(
                () =>
                {
                    if (Jackal.JackalFixedPatch.JackalsetTarget() && RoleHelpers.isAlive(PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.CanMove)
                    {
                        ModHelpers.checkMuderAttemptAndKill(PlayerControl.LocalPlayer, Jackal.JackalFixedPatch.JackalsetTarget());
                        Jackal.resetCoolDown();
                    }
                    if (TeleportingJackal.JackalFixedPatch.TeleportingJackalsetTarget() && RoleHelpers.isAlive(PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.CanMove)
                    {
                        ModHelpers.checkMuderAttemptAndKill(PlayerControl.LocalPlayer, TeleportingJackal.JackalFixedPatch.TeleportingJackalsetTarget());
                        TeleportingJackal.resetCoolDown();
                    }
                    if (JackalSeer.JackalSeerFixedPatch.JackalSeersetTarget() && RoleHelpers.isAlive(PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.CanMove)
                    {
                        ModHelpers.checkMuderAttemptAndKill(PlayerControl.LocalPlayer, JackalSeer.JackalSeerFixedPatch.JackalSeersetTarget());
                        JackalSeer.resetCoolDown();
                    }
                },
                () => { return ModeHandler.isMode(ModeId.Default) && RoleHelpers.isAlive(PlayerControl.LocalPlayer) && RoleClass.Jackal.JackalPlayer.IsCheckListPlayerControl(PlayerControl.LocalPlayer) || RoleHelpers.isAlive(PlayerControl.LocalPlayer) && RoleClass.TeleportingJackal.TeleportingJackalPlayer.IsCheckListPlayerControl(PlayerControl.LocalPlayer) && RoleHelpers.isAlive(PlayerControl.LocalPlayer) || RoleClass.JackalSeer.JackalSeerPlayer.IsCheckListPlayerControl(PlayerControl.LocalPlayer); },
                () =>
                {
                    return Jackal.JackalFixedPatch.JackalsetTarget() && PlayerControl.LocalPlayer.CanMove;
                    return TeleportingJackal.JackalFixedPatch.TeleportingJackalsetTarget() && PlayerControl.LocalPlayer.CanMove;
                    return JackalSeer.JackalSeerFixedPatch.JackalSeersetTarget() && PlayerControl.LocalPlayer.CanMove;
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
            );

            JackalKillButton.buttonText = FastDestroyableSingleton<HudManager>.Instance.KillButton.buttonLabelText.text;
            JackalKillButton.showButtonText = true;

            SelfBomberButton = new Buttons.CustomButton(
                () =>
                {
                    if (PlayerControl.LocalPlayer.CanMove)
                    {
                        SelfBomber.SelfBomb();
                    }
                },
                () => { return ModeHandler.isMode(ModeId.Default) && RoleHelpers.isAlive(PlayerControl.LocalPlayer) && SelfBomber.isSelfBomber(PlayerControl.LocalPlayer); },
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
            );

            SelfBomberButton.buttonText = ModTranslation.getString("SelfBomberButtonName");
            SelfBomberButton.showButtonText = true;

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
                () => { return RoleHelpers.isAlive(PlayerControl.LocalPlayer) && Doorr.isDoorr(PlayerControl.LocalPlayer); },
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
            );

            DoorrDoorButton.buttonText = ModTranslation.getString("DoorrButtonText");
            DoorrDoorButton.showButtonText = true;

            TeleporterButton = new Buttons.CustomButton(
                () =>
                {
                    if (!PlayerControl.LocalPlayer.CanMove) return;
                    RoleClass.Clergyman.ButtonTimer = DateTime.Now;
                    TeleporterButton.actionButton.cooldownTimerText.color = new Color(0F, 0.8F, 0F);
                    Teleporter.TeleportStart();
                    Teleporter.ResetCoolDown();
                },
                () => { return RoleHelpers.isAlive(PlayerControl.LocalPlayer) && (Teleporter.IsTeleporter(PlayerControl.LocalPlayer) || RoleClass.Levelinger.IsPower(RoleClass.Levelinger.LevelPowerTypes.Teleporter)) || RoleHelpers.isAlive(PlayerControl.LocalPlayer) && (NiceTeleporter.IsNiceTeleporter(PlayerControl.LocalPlayer)) || RoleHelpers.isAlive(PlayerControl.LocalPlayer) && (TeleportingJackal.IsTeleportingJackal(PlayerControl.LocalPlayer)); },
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
            );

            TeleporterButton.buttonText = ModTranslation.getString("TeleporterTeleportButton");
            TeleporterButton.showButtonText = true;

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
                () => { return RoleHelpers.isAlive(PlayerControl.LocalPlayer) && (Moving.IsMoving(PlayerControl.LocalPlayer) || RoleClass.Levelinger.IsPower(RoleClass.Levelinger.LevelPowerTypes.Moving)) && !Moving.IsSetPostion(); },
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
            );

            MovingSetButton.buttonText = ModTranslation.getString("MovingButtonSetName");
            MovingSetButton.showButtonText = true;

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
                () => { return RoleHelpers.isAlive(PlayerControl.LocalPlayer) && (Moving.IsMoving(PlayerControl.LocalPlayer) || RoleClass.Levelinger.IsPower(RoleClass.Levelinger.LevelPowerTypes.Moving)) && Moving.IsSetPostion(); },
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
            );

            MovingTpButton.buttonText = ModTranslation.getString("MovingButtonTpName");
            MovingTpButton.showButtonText = true;

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

                            MessageWriter killWriter = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.SheriffKill, Hazel.SendOption.Reliable, -1);
                            killWriter.Write(LocalID);
                            killWriter.Write(TargetID);
                            killWriter.Write(misfire);
                            AmongUsClient.Instance.FinishRpcImmediately(killWriter);
                            Sheriff.ResetKillCoolDown();
                        }
                    }
                },
                () => { return RoleHelpers.isAlive(PlayerControl.LocalPlayer) && ModeHandler.isMode(ModeId.Default) && Sheriff.IsSheriffButton(PlayerControl.LocalPlayer); },
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
                () => { return PlayerControl.LocalPlayer.isAlive() && Clergyman.isClergyman(PlayerControl.LocalPlayer); },
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
            );

            ClergymanLightOutButton.buttonText = ModTranslation.getString("ClergymanLightOutButtonName");
            ClergymanLightOutButton.showButtonText = true;

            SpeedBoosterBoostButton = new Buttons.CustomButton(
                () =>
                {
                    Roles.RoleClass.SpeedBooster.ButtonTimer = DateTime.Now;
                    SpeedBoosterBoostButton.actionButton.cooldownTimerText.color = new Color(0F, 0.8F, 0F);
                    SpeedBooster.BoostStart();
                },
                () => { return RoleHelpers.isAlive(PlayerControl.LocalPlayer) && SpeedBooster.IsSpeedBooster(PlayerControl.LocalPlayer); },
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
            );

            SpeedBoosterBoostButton.buttonText = ModTranslation.getString("SpeedBoosterBoostButtonName");
            SpeedBoosterBoostButton.showButtonText = true;
            SpeedBoosterBoostButton.HasEffect = true;

            EvilSpeedBoosterBoostButton = new Buttons.CustomButton(
                () =>
                {
                    Roles.RoleClass.EvilSpeedBooster.ButtonTimer = DateTime.Now;
                    EvilSpeedBoosterBoostButton.actionButton.cooldownTimerText.color = new Color(0F, 0.8F, 0F);
                    EvilSpeedBooster.BoostStart();
                },
                () => { return RoleHelpers.isAlive(PlayerControl.LocalPlayer) && (EvilSpeedBooster.IsEvilSpeedBooster(PlayerControl.LocalPlayer) || RoleClass.Levelinger.IsPower(RoleClass.Levelinger.LevelPowerTypes.SpeedBooster)); },
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
            );

            EvilSpeedBoosterBoostButton.buttonText = ModTranslation.getString("EvilSpeedBoosterBoostButtonName");
            EvilSpeedBoosterBoostButton.showButtonText = true;

            LighterLightOnButton = new Buttons.CustomButton(
                () =>
                {
                    RoleClass.Lighter.IsLightOn = true;
                    Roles.RoleClass.Lighter.ButtonTimer = DateTime.Now;
                    LighterLightOnButton.actionButton.cooldownTimerText.color = new Color(0F, 0.8F, 0F);
                    Lighter.LightOnStart();
                },
                () => { return RoleHelpers.isAlive(PlayerControl.LocalPlayer) && Lighter.isLighter(PlayerControl.LocalPlayer); },
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
            );

            LighterLightOnButton.buttonText = ModTranslation.getString("LighterButtonName");
            LighterLightOnButton.showButtonText = true;

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
                () => { return RoleHelpers.isAlive(PlayerControl.LocalPlayer) && RoleClass.Levelinger.IsPower(RoleClass.Levelinger.LevelPowerTypes.Sidekick) && !RoleClass.Levelinger.IsCreateMadmate; },
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
            );

            ImpostorSidekickButton.buttonText = ModTranslation.getString("SidekickName");
            ImpostorSidekickButton.showButtonText = true;

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
                () => { return RoleHelpers.isAlive(PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.isRole(RoleId.SideKiller) && !RoleClass.SideKiller.IsCreateMadKiller; },
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
            );

            SideKillerSidekickButton.buttonText = ModTranslation.getString("SidekickName");
            SideKillerSidekickButton.showButtonText = true;

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
                        PlayerControl.LocalPlayer.RpcMurderPlayer(PlayerControl.LocalPlayer);
                    }
                },
                () => { return RoleHelpers.isAlive(PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.isRole(RoleId.MadMaker) && ModeHandler.isMode(ModeId.Default) && !RoleClass.MadMaker.IsCreateMadmate; },
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
            );

            MadMakerSidekickButton.buttonText = ModTranslation.getString("SidekickName");
            MadMakerSidekickButton.showButtonText = true;


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
                () => { return Demon.IsButton(); },
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
            );

            DemonButton.buttonText = ModTranslation.getString("DemonButtonName");
            DemonButton.showButtonText = true;

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
                () => { return Arsonist.IsButton(); },
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
            );

            ArsonistDouseButton.buttonText = ModTranslation.getString("ArsonistDouseButtonName");
            ArsonistDouseButton.showButtonText = true;

            ArsonistIgniteButton = new CustomButton(
                () =>
                {
                    Arsonist.SetWinArsonist();
                    RoleClass.Arsonist.TriggerArsonistWin = true;
                    AdditionalTempData.winCondition = EndGame.WinCondition.ArsonistWin;
                    RPCProcedure.ShareWinner(CachedPlayer.LocalPlayer.PlayerId);

                    MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.ShareWinner, SendOption.Reliable, -1);
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
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.CustomEndGame, SendOption.Reliable, -1);
                        writer.Write((byte)reason);
                        writer.Write(false);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                    }
                },
                () => { return Arsonist.IseveryButton(); },
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
            );

            ArsonistIgniteButton.buttonText = ModTranslation.getString("ArsonistIgniteButtonName");
            ArsonistIgniteButton.showButtonText = true;

            SpeederButton = new CustomButton(
                () =>
                {
                    SpeederButton.MaxTimer = RoleClass.Speeder.DurationTime;
                    SpeederButton.Timer = SpeederButton.MaxTimer;
                    SpeederButton.actionButton.cooldownTimerText.color = new Color(0F, 0.8F, 0F);
                    Speeder.DownStart();
                },
                () => { return RoleHelpers.isAlive(PlayerControl.LocalPlayer) && Speeder.IsSpeeder(PlayerControl.LocalPlayer); },
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
            );


            SpeederButton.buttonText = ModTranslation.getString("SpeederButtonName");
            SpeederButton.showButtonText = true;
            SpeederButton.HasEffect = true;


            ChiefSidekickButton = new CustomButton(
               () =>
               {
                   var target = setTarget();
                   if (!target.Data.Role.IsImpostor && target && RoleHelpers.isAlive(PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.CanMove && !RoleClass.Chief.IsCreateSheriff)
                   {
                       target.RpcSetRole(RoleTypes.Crewmate);
                       target.setRoleRPC(RoleId.Sheriff);
                       RoleClass.Chief.IsCreateSheriff = true;
                   }
                   else if (target.Data.Role.IsImpostor)
                   {
                       PlayerControl.LocalPlayer.RpcMurderPlayer(PlayerControl.LocalPlayer);
                   }
               },
               () => { return RoleHelpers.isAlive(PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.isRole(RoleId.Chief) && ModeHandler.isMode(ModeId.Default) && !RoleClass.Chief.IsCreateSheriff; },
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
           );

            ChiefSidekickButton.buttonText = ModTranslation.getString("SidekickName");
            ChiefSidekickButton.showButtonText = true;

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

                                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.CleanBody, Hazel.SendOption.Reliable, -1);
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

                        MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.ShareWinner, SendOption.Reliable, -1);
                        Writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(Writer);
                        if (AmongUsClient.Instance.AmHost)
                        {
                            CheckGameEndPatch.CustomEndGame((GameOverReason)EndGame.CustomGameOverReason.VultureWin, false);
                        }
                        else
                        {
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.CustomEndGame, SendOption.Reliable, -1);
                            writer.Write((byte)EndGame.CustomGameOverReason.VultureWin);
                            writer.Write(false);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                        }
                    }
                },
                () => { return RoleHelpers.isAlive(PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.isRole(RoleId.Vulture); },
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
            );

            VultureButton.buttonText = ModTranslation.getString("VultureButtonName");
            VultureButton.showButtonText = true;

            ShielderButton = new CustomButton(
                () =>
                {
                    MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.SetShielder, SendOption.Reliable, -1);
                    Writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                    Writer.Write(true);
                    AmongUsClient.Instance.FinishRpcImmediately(Writer);
                    RPCProcedure.SetShielder(CachedPlayer.LocalPlayer.PlayerId, true);
                    ShielderButton.actionButton.cooldownTimerText.color = new Color(0F, 0.8F, 0F);
                    ShielderButton.MaxTimer = RoleClass.Shielder.DurationTime;
                    ShielderButton.Timer = ShielderButton.MaxTimer;
                },
                () => { return RoleHelpers.isAlive(PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.isRole(RoleId.Shielder); },
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
            );

            ShielderButton.buttonText = ModTranslation.getString("ShielderButtonName");
            ShielderButton.showButtonText = true;

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

                                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.CleanBody, Hazel.SendOption.Reliable, -1);
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
                () => { return RoleHelpers.isAlive(PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.isRole(RoleId.Cleaner); },

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
            );

            CleanerButton.buttonText = ModTranslation.getString("CleanerButtonName");
            CleanerButton.showButtonText = true;

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

                                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.CleanBody, Hazel.SendOption.Reliable, -1);
                                    writer.Write(playerInfo.PlayerId);
                                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                                    RPCProcedure.CleanBody(playerInfo.PlayerId);
                                    break;
                                }

                            }

                        }
                    }
                },
                () => { return RoleHelpers.isAlive(PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.isRole(RoleId.MadCleaner); },
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
            );

            MadCleanerButton.buttonText = ModTranslation.getString("CleanerButtonName");
            MadCleanerButton.showButtonText = true;

            FreezerButton = new Buttons.CustomButton(
                () =>
                {
                    FreezerButton.MaxTimer = RoleClass.Freezer.DurationTime;
                    FreezerButton.Timer = FreezerButton.MaxTimer;
                    FreezerButton.actionButton.cooldownTimerText.color = new Color(0F, 0.8F, 0F);
                    Freezer.DownStart();
                },
                () => { return RoleHelpers.isAlive(PlayerControl.LocalPlayer) && Freezer.IsFreezer(PlayerControl.LocalPlayer); },
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
            );

            FreezerButton.buttonText = ModTranslation.getString("FreezerButtonName");
            FreezerButton.showButtonText = true;
            FreezerButton.HasEffect = true;

            SamuraiButton = new Buttons.CustomButton(
                () =>
                {
                    if (PlayerControl.LocalPlayer.CanMove)
                    {
                        Samurai.SamuraiKill();
                    }
                },
                () => { return ModeHandler.isMode(ModeId.Default) && RoleHelpers.isAlive(PlayerControl.LocalPlayer) && Samurai.isSamurai(PlayerControl.LocalPlayer) && !RoleClass.Samurai.Sword; },

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
            );

            SamuraiButton.buttonText = ModTranslation.getString("SamuraiButtonName");
            SamuraiButton.showButtonText = true;

            VentMakerButton = new CustomButton(
                () =>
                {
                    RoleClass.VentMaker.VentCount++;
                    MessageWriter writer = RPCHelper.StartRPC(CustomRPC.CustomRPC.MakeVent);
                    writer.Write(CachedPlayer.LocalPlayer.transform.position.x);
                    writer.Write(CachedPlayer.LocalPlayer.transform.position.y);
                    writer.Write(CachedPlayer.LocalPlayer.transform.position.z);
                    writer.EndRPC();
                    CustomRPC.RPCProcedure.MakeVent(CachedPlayer.LocalPlayer.transform.position.x,CachedPlayer.LocalPlayer.transform.position.y,CachedPlayer.LocalPlayer.transform.position.z);
                    GameObject Vent = GameObject.Find("VentMakerVent" + MapUtilities.CachedShipStatus.AllVents.Select(x => x.Id).Max().ToString());

                    RoleClass.VentMaker.Vent = Vent.GetComponent<Vent>();
                    if (RoleClass.VentMaker.VentCount == 2) RoleClass.VentMaker.IsMakeVent = false;
                },
                () => { return RoleClass.VentMaker.IsMakeVent && RoleHelpers.isAlive(PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.isRole(RoleId.VentMaker); },
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
            );

            VentMakerButton.buttonText = ModTranslation.getString("VentMakerButtonName");
            VentMakerButton.showButtonText = true;

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
                () => { return PlayerControl.LocalPlayer.isGhostRole(RoleId.GhostMechanic) && RoleClass.GhostMechanic.LimitCount > 0; },
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
            GhostMechanicRepairButton .showButtonText = true;

            EvilHackerButton = new CustomButton(
               () =>
               {
                   CachedPlayer.LocalPlayer.NetTransform.Halt();
                   Action<MapBehaviour> tmpAction = (MapBehaviour m) => { m.ShowCountOverlay(); };
                   FastDestroyableSingleton<HudManager>.Instance.ShowMap(tmpAction);

               },
               () => { return PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.EvilHacker) && PlayerControl.LocalPlayer.CanMove; },
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
            );

            EvilHackerButton.buttonText = ModTranslation.getString("ADMINButton");
            EvilHackerButton.showButtonText = true;

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
                () => { return RoleHelpers.isAlive(PlayerControl.LocalPlayer) && PlayerControl.LocalPlayer.isRole(RoleId.EvilHacker) && ModeHandler.isMode(ModeId.Default) && RoleClass.EvilHacker.IsCreateMadmate; },
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
            );

            EvilHackerMadmateSetting.buttonText = ModTranslation.getString("SidekickName");
            EvilHackerMadmateSetting.showButtonText = true;

            setCustomButtonCooldowns();


        }

    }
}
