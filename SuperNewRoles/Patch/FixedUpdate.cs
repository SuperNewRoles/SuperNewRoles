using HarmonyLib;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomOption;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Roles;
using SuperNewRoles.Sabotage;
using UnityEngine;

namespace SuperNewRoles.Patch
{
    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.StartGame))]
    public class StartGame
    {
        public static void Postfix()
        {
            MapOptions.RandomMap.Prefix();
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

    [HarmonyPatch(typeof(ControllerManager), nameof(ControllerManager.Update))]
    class DebugManager
    {
        public static void Postfix()
        {
            if (AmongUsClient.Instance.GameState == AmongUsClient.GameStates.Started)
            {
                if (AmongUsClient.Instance.AmHost && Input.GetKeyDown(KeyCode.H) && Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.RightShift))
                {
                    RPCHelper.StartRPC(CustomRPC.CustomRPC.SetHaison).EndRPC();
                    RPCProcedure.SetHaison();
                    ShipStatus.RpcEndGame(GameOverReason.HumansByTask, false);
                    MapUtilities.CachedShipStatus.enabled = false;
                }

                if (Input.GetKeyDown(KeyCode.M) && Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.RightShift) && AmongUsClient.Instance.AmHost)//Mと右左シフトを押したとき
                {
                    MeetingHud.Instance.RpcClose();//会議を強制終了
                }
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

        static void reduceKillCooldown(PlayerControl __instance)
        {
            if (CustomOptions.IsAlwaysReduceCooldown.GetBool())
            {
                // オプションがONの場合はベント内はクールダウン減少を止める
                bool exceptInVent = CustomOptions.IsAlwaysReduceCooldownExceptInVent.GetBool() && PlayerControl.LocalPlayer.inVent;
                // 配電盤タスク中はクールダウン減少を止める
                bool exceptOnTask = CustomOptions.IsAlwaysReduceCooldownExceptOnTask.GetBool() && ElectricPatch.onTask;

                if (!__instance.Data.IsDead && !__instance.CanMove && !exceptInVent && !exceptOnTask)
                    __instance.SetKillTimer(__instance.killTimer - Time.fixedDeltaTime);
            }

        }
        public static bool IsProDown;

        public static void Postfix(PlayerControl __instance)
        {
            if (__instance != PlayerControl.LocalPlayer) return;
            PVCreator.FixedUpdate();
            if (AmongUsClient.Instance.GameState == AmongUsClient.GameStates.Started)
            {
                var MyRole = PlayerControl.LocalPlayer.GetRole();
                SetBasePlayerOutlines();
                VentAndSabo.VentButtonVisibilityPatch.Postfix(__instance);
                LadderDead.FixedUpdate();
                var ThisMode = ModeHandler.GetMode();
                if (ThisMode == ModeId.Default)
                {
                    SabotageManager.Update();
                    SetNameUpdate.Postfix(__instance);
                    Jackal.JackalFixedPatch.Postfix(__instance, MyRole);
                    JackalSeer.JackalSeerFixedPatch.Postfix(__instance, MyRole);
                    Roles.CrewMate.Psychometrist.FixedUpdate();
                    Roles.Impostor.Matryoshka.FixedUpdate();
                    reduceKillCooldown(__instance);
                    if (PlayerControl.LocalPlayer.IsAlive())
                    {
                        if (PlayerControl.LocalPlayer.IsImpostor()) { SetTarget.ImpostorSetTarget(); }
                        switch (MyRole)
                        {
                            case RoleId.Researcher:
                                Researcher.ReseUseButtonSetTargetPatch.Postfix();
                                break;
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
                                Vampire.FixedUpdate.Postfix();
                                break;
                            case RoleId.DarkKiller:
                                DarkKiller.FixedUpdate.Postfix();
                                break;
                            case RoleId.Vulture:
                                Vulture.FixedUpdate.Postfix();
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
                                Roles.Neutral.Revolutionist.FixedUpdate();
                                break;
                            case RoleId.Spelunker:
                                Roles.Neutral.Spelunker.FixedUpdate();
                                break;
                            case RoleId.SuicidalIdeation:
                                SuicidalIdeation.Postfix();
                                break;
                            case RoleId.Doctor:
                                Doctor.FixedUpdate();
                                break;
                            case RoleId.Psychometrist:
                                Roles.CrewMate.Psychometrist.PsychometristFixedUpdate();
                                break;
                            case RoleId.SeeThroughPerson:
                                Roles.CrewMate.SeeThroughPerson.FixedUpdate();
                                break;
                            case RoleId.Hitman:
                                Roles.Neutral.Hitman.FixedUpdate();
                                break;
                            case RoleId.Photographer:
                                Roles.Neutral.Photographer.FixedUpdate();
                                break;
                            default:
                                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                                    NormalButtonDestroy.Postfix(p);
                                break;
                        }
                    }
                    else
                    {
                        if (MapOptions.MapOption.ClairvoyantZoom)
                        {
                            Clairvoyant.FixedUpdate.Postfix();
                        }
                        switch (MyRole)
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
                        }
                    }
                }
                else if (ThisMode == ModeId.SuperHostRoles)
                {
                    Mode.SuperHostRoles.FixedUpdate.Update();
                    switch (MyRole)
                    {
                        case RoleId.Mafia:
                            Mafia.FixedUpdate();
                            break;
                    }
                    SerialKiller.SHRFixedUpdate(MyRole);
                }
                else if (ThisMode == ModeId.NotImpostorCheck)
                {
                    if (AmongUsClient.Instance.AmHost)
                    {
                        BlockTool.FixedUpdate();
                    }
                    Mode.NotImpostorCheck.NameSet.Postfix();
                }
                else
                {
                    if (AmongUsClient.Instance.AmHost)
                    {
                        BlockTool.FixedUpdate();
                    }
                    ModeHandler.FixedUpdate(__instance);
                }
            }
        }
    }
}