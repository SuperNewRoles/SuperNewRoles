using System.Collections.Generic;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomObject;
using SuperNewRoles.Helpers;
using SuperNewRoles.Patches;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SuperNewRoles.Roles.Neutral;

public static class OrientalShaman
{
    private const int OptionId = 301300;
    public static CustomRoleOption OrientalShamanOption;
    public static CustomOption OrientalShamanPlayerCount;
    public static CustomOption OrientalShamanImpostorVision;
    public static CustomOption OrientalShamanVentUseCoolTime;
    public static CustomOption OrientalShamanVentDurationTime;
    public static CustomOption OrientalShamanCrewTaskWinHijack;
    public static CustomOption OrientalShamanWinTask;
    public static CustomOption OrientalShamanIsSettingNumberOfUniqueTasks;
    public static CustomOption OrientalShamanCommonTask;
    public static CustomOption OrientalShamanShortTask;
    public static CustomOption OrientalShamanLongTask;
    public static CustomOption OrientalShamanCreateShermansServant;
    public static CustomOption OrientalShamanCreateShermansServantCool;
    public static CustomOption ShermansServantTransformationCoolTime;
    public static CustomOption ShermansServantSuicideCoolTime;
    public static void SetupCustomOptions()
    {
        OrientalShamanOption = CustomOption.SetupCustomRoleOption(OptionId, false, RoleId.OrientalShaman);
        OrientalShamanPlayerCount = CustomOption.Create(OptionId + 1, false, CustomOptionType.Neutral, "SettingPlayerCountName", CustomOptionHolder.CrewPlayers[0], CustomOptionHolder.CrewPlayers[1], CustomOptionHolder.CrewPlayers[2], CustomOptionHolder.CrewPlayers[3], OrientalShamanOption);
        OrientalShamanImpostorVision = CustomOption.Create(OptionId + 2, false, CustomOptionType.Neutral, "OrientalShamanImpostorVisionSetting", true, OrientalShamanOption);
        OrientalShamanVentUseCoolTime = CustomOption.Create(OptionId + 3, false, CustomOptionType.Neutral, "OrientalShamanVentUseCoolTimeSetting", 15f, 0f, 60f, 2.5f, OrientalShamanOption);
        OrientalShamanVentDurationTime = CustomOption.Create(OptionId + 4, false, CustomOptionType.Neutral, "OrientalShamanVentDurationTimeSetting", 10f, 2.5f, 60f, 2.5f, OrientalShamanOption);
        OrientalShamanCrewTaskWinHijack = CustomOption.Create(OptionId + 5, false, CustomOptionType.Neutral, "OrientalShamanCrewTaskWinHijackSetting", false, OrientalShamanOption);
        OrientalShamanWinTask = CustomOption.Create(OptionId + 6, false, CustomOptionType.Neutral, "OrientalShamanWinTaskSetting", false, OrientalShamanOption);
        OrientalShamanIsSettingNumberOfUniqueTasks = CustomOption.Create(OptionId + 14, false, CustomOptionType.Neutral, "IsSettingNumberOfUniqueTasks", true, OrientalShamanWinTask);
        var OrientalShamanoption = SelectTask.TaskSetting(OptionId + 7, OptionId + 8, OptionId + 9, OrientalShamanIsSettingNumberOfUniqueTasks, CustomOptionType.Neutral, false);
        OrientalShamanCommonTask = OrientalShamanoption.Item1;
        OrientalShamanShortTask = OrientalShamanoption.Item2;
        OrientalShamanLongTask = OrientalShamanoption.Item3;
        OrientalShamanCreateShermansServant = CustomOption.Create(OptionId + 10, false, CustomOptionType.Neutral, "OrientalShamanCreateShermansServantSetting", true, OrientalShamanOption);
        OrientalShamanCreateShermansServantCool = CustomOption.Create(OptionId + 11, false, CustomOptionType.Neutral, "OrientalShamanCreateShermansServantCoolSetting", 30f, 0f, 60f, 2.5f, OrientalShamanCreateShermansServant);
        ShermansServantTransformationCoolTime = CustomOption.Create(OptionId + 12, false, CustomOptionType.Neutral, "ShermansServantTransformationCoolTimeSetting", 0f, 0f, 60f, 2.5f, OrientalShamanCreateShermansServant);
        ShermansServantSuicideCoolTime = CustomOption.Create(OptionId + 13, false, CustomOptionType.Neutral, "ShermansServantSuicideCoolTimeSetting", 30f, 0f, 60f, 2.5f, OrientalShamanCreateShermansServant);
    }

    public static List<PlayerControl> OrientalShamanPlayer;
    public static List<PlayerControl> ShermansServantPlayer;
    public static Dictionary<byte, byte> OrientalShamanCausative;
    public static Color32 color = new(192, 177, 246, byte.MaxValue);
    public static bool CanCreateShermansServant;
    public static bool IsTransformation;
    public static Dictionary<DeadBody, Arrow> DeadPlayerArrows;
    public static Arrow SeePositionArrow;
    public static bool IsDoNotDisplay;
    public static void ClearAndReload()
    {
        OrientalShamanPlayer = new();
        ShermansServantPlayer = new();
        OrientalShamanCausative = new();
        CanCreateShermansServant = true;
        IsTransformation = false;
        DeadPlayerArrows = new();
        SeePositionArrow = null;
        IsDoNotDisplay = false;
        if (ShermansServantTransformationButton != null)
            ShermansServantTransformationButton.Timer = ShermansServantTransformationCoolTime.GetFloat();
        if (ShermansServantSuicideButton != null)
            ShermansServantSuicideButton.Timer = ShermansServantSuicideCoolTime.GetFloat();
    }

    public static CustomButton OrientalShamanShermansServantButtoon;
    public static CustomButton OrientalShamanVentButton;
    public static TMP_Text OrientalShamanVentTimerText;
    public static CustomButton ShermansServantTransformationButton;
    public static CustomButton ShermansServantSuicideButton;
    public static void SetupCustomButtons(HudManager __instance)
    {
        OrientalShamanShermansServantButtoon = new(
            () =>
            {
                PlayerControl target = HudManagerStartPatch.SetTarget();
                if ((!target && !IsKiller(target)) || !CanCreateShermansServant) return;
                CanCreateShermansServant = false;
                target.SetRoleRPC(RoleId.ShermansServant);
                MessageWriter writer = RPCHelper.StartRPC(CustomRPC.CreateShermansServant);
                writer.Write(PlayerControl.LocalPlayer.PlayerId);
                writer.Write(target.PlayerId);
                writer.EndRPC();
                RPCProcedure.CreateShermansServant(PlayerControl.LocalPlayer.PlayerId, target.PlayerId);

            },
            (bool isAlive, RoleId role) => { return role == RoleId.OrientalShaman && isAlive && CanCreateShermansServant && OrientalShamanCreateShermansServant.GetBool(); },
            () =>
            {
                PlayerControl target = HudManagerStartPatch.SetTarget();
                PlayerControlFixedUpdatePatch.SetPlayerOutline(target, color);
                return target && !Frankenstein.IsMonster(target) && !IsKiller(target);
            },
            () =>
            {
                OrientalShamanShermansServantButtoon.MaxTimer = OrientalShamanCreateShermansServantCool.GetFloat();
                OrientalShamanShermansServantButtoon.Timer = OrientalShamanShermansServantButtoon.MaxTimer;
            },
            ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.OrientalShamanButton.png", 115f),
            new(-2, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("OrientalShamanShermansServantButtoonName"),
            showButtonText = true
        };

        OrientalShamanVentButton = new(
            () =>
            {
                if (!ShipStatus.Instance) return;
                Vent inVent = null;
                float min_distance = float.MaxValue;
                foreach (Vent vent in ShipStatus.Instance.AllVents)
                {
                    float distance = Vector2.Distance(PlayerControl.LocalPlayer.transform.position, vent.transform.position);
                    if (distance <= vent.UsableDistance && distance < min_distance)
                    {
                        min_distance = distance;
                        vent.CanUse(PlayerControl.LocalPlayer.Data, out bool canUse, out bool _);
                        if (canUse) inVent = vent;
                        Logger.Info($"選択されたベント : {vent.gameObject.name}, vent.UsableDistance : {vent.UsableDistance}", "OrientalShaman Vent Button");
                    }
                }
                if (!inVent) return;
                if (!OrientalShamanVentButton.isEffectActive)
                {
                    inVent.SetButtons(true);

                    PlayerControl.LocalPlayer.moveable = false;
                    PlayerControl.LocalPlayer.Visible = false;
                    PlayerControl.LocalPlayer.inVent = true;
                    PlayerControl.LocalPlayer.NetTransform.SnapTo(inVent.transform.position);

                    MessageWriter writer = RPCHelper.StartRPC(CustomRPC.SetVisible);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    writer.Write(false);
                    writer.EndRPC();
                }
                else
                {
                    OrientalShamanVentButton.MaxTimer = OrientalShamanVentUseCoolTime.GetFloat();
                    OrientalShamanVentButton.Timer = OrientalShamanVentButton.MaxTimer;
                    OrientalShamanVentButton.actionButton.cooldownTimerText.color = Color.white;
                    inVent.SetButtons(false);

                    PlayerControl.LocalPlayer.moveable = true;
                    PlayerControl.LocalPlayer.Visible = true;
                    PlayerControl.LocalPlayer.inVent = false;

                    MessageWriter writer = RPCHelper.StartRPC(CustomRPC.SetVisible);
                    writer.Write(PlayerControl.LocalPlayer.PlayerId);
                    writer.Write(true);
                    writer.EndRPC();
                }
            },
            (bool isAlive, RoleId role) => { return role is RoleId.OrientalShaman && isAlive; },
            () =>
            {
                if (!ShipStatus.Instance) return false;
                Vent inVent = null;
                float min_distance = float.MaxValue;
                foreach (Vent vent in ShipStatus.Instance.AllVents)
                {
                    vent.myRend.material.SetColor("_OutlineColor", color);
                    vent.myRend.material.SetColor("_AddColor", color);
                    vent.myRend.material.SetFloat("_Outline", 0f);
                    float distance = Vector2.Distance(PlayerControl.LocalPlayer.transform.position, vent.transform.position);
                    if (distance <= vent.UsableDistance && distance < min_distance)
                    {
                        min_distance = distance;
                        vent.CanUse(PlayerControl.LocalPlayer.Data, out bool canUse, out bool _);
                        if (canUse) inVent = vent;
                    }
                }

                if (inVent) inVent.myRend.material.SetFloat("_Outline", 1f);
                return inVent;
            },
            () =>
            {
                OrientalShamanVentButton.MaxTimer = OrientalShamanVentUseCoolTime.GetFloat();
                OrientalShamanVentButton.Timer = OrientalShamanVentButton.MaxTimer;
                OrientalShamanVentButton.EffectDuration = OrientalShamanVentDurationTime.GetFloat();
                OrientalShamanVentButton.effectCancellable = true;
                OrientalShamanVentButton.isEffectActive = false;
                OrientalShamanVentButton.HasEffect = true;
                OrientalShamanVentButton.actionButton.cooldownTimerText.color = Color.white;

                PlayerControl.LocalPlayer.moveable = true;
                PlayerControl.LocalPlayer.Visible = true;
                PlayerControl.LocalPlayer.inVent = false;

                MessageWriter writer = RPCHelper.StartRPC(CustomRPC.SetVisible);
                writer.Write(PlayerControl.LocalPlayer.PlayerId);
                writer.Write(true);
                writer.EndRPC();

                if (!ShipStatus.Instance) return;
                foreach (Vent vent in ShipStatus.Instance.AllVents)
                    vent.SetButtons(false);
            },
            __instance.ImpostorVentButton.graphic.sprite,
            new(-1, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.V,
            50,
            () => { return false; },
            true,
            OrientalShamanVentDurationTime.GetFloat(),
            () =>
            {
                OrientalShamanVentButton.MaxTimer = OrientalShamanVentUseCoolTime.GetFloat();
                OrientalShamanVentButton.Timer = OrientalShamanVentButton.MaxTimer;
                OrientalShamanVentButton.HasEffect = true;
                OrientalShamanVentButton.actionButton.cooldownTimerText.color = Color.white;

                PlayerControl.LocalPlayer.moveable = true;
                PlayerControl.LocalPlayer.Visible = true;
                PlayerControl.LocalPlayer.inVent = false;

                MessageWriter writer = RPCHelper.StartRPC(CustomRPC.SetVisible);
                writer.Write(PlayerControl.LocalPlayer.PlayerId);
                writer.Write(true);
                writer.EndRPC();

                if (!ShipStatus.Instance) return;
                foreach (Vent vent in ShipStatus.Instance.AllVents)
                    vent.SetButtons(false);
            }
        )
        {
            buttonText = FastDestroyableSingleton<HudManager>.Instance.ImpostorVentButton.buttonLabelText.text,
            showButtonText = true
        };

        ShermansServantTransformationButton = new(
            () =>
            {
                if (!IsTransformation)
                {
                    PlayerControl target = null;
                    foreach (var item in OrientalShamanCausative)
                    {
                        if (item.Value == PlayerControl.LocalPlayer.PlayerId)
                        {
                            target = ModHelpers.PlayerById(item.Key);
                            break;
                        }
                    }
                    GameData.PlayerOutfit outfit = target?.Data.DefaultOutfit;
                    SetOutfit(PlayerControl.LocalPlayer, outfit);
                    IsTransformation = true;
                }
                else
                {
                    ShermansServantTransformationButton.MaxTimer = ShermansServantTransformationCoolTime.GetFloat();
                    ShermansServantTransformationButton.Timer = ShermansServantTransformationCoolTime.GetFloat();
                    SetOutfit(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer.Data.DefaultOutfit);
                    IsTransformation = false;
                }
            },
            (bool isAlive, RoleId role) => { return role == RoleId.ShermansServant && isAlive; },
            () => { return PlayerControl.LocalPlayer.CanMove; },
            () =>
            {
                ShermansServantTransformationButton.MaxTimer = ShermansServantTransformationCoolTime.GetFloat();
                ShermansServantTransformationButton.Timer = ShermansServantTransformationCoolTime.GetFloat();
                SetOutfit(PlayerControl.LocalPlayer, PlayerControl.LocalPlayer.Data.DefaultOutfit);
                IsTransformation = false;
            },
            ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.ShermansServantButton.png", 115f),
            new(-2, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("ShermansServantTransformationButtonName"),
            showButtonText = true
        };

        ShermansServantSuicideButton = new(
            () =>
            {
                PlayerControl.LocalPlayer.RpcMurderPlayer(PlayerControl.LocalPlayer, true);
                PlayerControl.LocalPlayer.RpcSetFinalStatus(FinalStatus.WorshiperSelfDeath);
            },
            (bool isAlive, RoleId role) => { return role == RoleId.ShermansServant && isAlive; },
            () => { return PlayerControl.LocalPlayer.CanMove; },
            () =>
            {
                ShermansServantSuicideButton.MaxTimer = ShermansServantSuicideCoolTime.GetFloat();
                ShermansServantSuicideButton.Timer = ShermansServantSuicideCoolTime.GetFloat();
            },
            ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.ShermansServantSuicideButton.png", 115f),
            new(0, 1, 0),
            __instance,
            __instance.AbilityButton,
            KeyCode.Q,
            8,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("WorshiperSuicide"),
            showButtonText = true
        };
    }
    public static void FixedUpdate()
    {
        if (PlayerControl.LocalPlayer.IsRole(RoleId.OrientalShaman))
        {
            if (OrientalShamanCausative.ContainsKey(PlayerControl.LocalPlayer.PlayerId))
            {
                PlayerControl player = ModHelpers.PlayerById(OrientalShamanCausative[PlayerControl.LocalPlayer.PlayerId]);
                if (PlayerControl.LocalPlayer.IsDead() || IsDoNotDisplay || !player.IsRole(RoleId.ShermansServant))
                {
                    if (SeePositionArrow.arrow != null)
                        Object.Destroy(SeePositionArrow.arrow);
                    return;
                }
                if (SeePositionArrow == null) SeePositionArrow = new(color);
                if (!player) return;
                if (player.IsAlive())
                {
                    SeePositionArrow.Update(player.transform.position, color);
                    SeePositionArrow.arrow.SetActive(SeePositionArrow.arrow);
                }
                else if (!IsDoNotDisplay)
                {
                    foreach (DeadBody dead in Object.FindObjectsOfType<DeadBody>())
                    {
                        if (dead.ParentId == player.PlayerId)
                        {
                            SeePositionArrow.Update(dead.transform.position, color);
                            SeePositionArrow.arrow.SetActive(SeePositionArrow.arrow);
                            break;
                        }
                    }
                }
                if (RoleClass.IsMeeting) IsDoNotDisplay = player.IsDead();
            }
        }
        else if (PlayerControl.LocalPlayer.IsRole(RoleId.ShermansServant))
        {
            if (PlayerControl.LocalPlayer.IsAlive())
                Vulture.FixedUpdate.Postfix();

            if (OrientalShamanCausative.ContainsValue(PlayerControl.LocalPlayer.PlayerId))
            {
                PlayerControl player = null;
                foreach (var item in OrientalShamanCausative)
                {
                    if (item.Value == PlayerControl.LocalPlayer.PlayerId)
                    {
                        player = ModHelpers.PlayerById(item.Key);
                        break;
                    }
                }
                if (!player) return;

                if (!player.IsRole(RoleId.OrientalShaman) && PlayerControl.LocalPlayer.IsAlive())
                {
                    PlayerControl.LocalPlayer.RpcMurderPlayer(PlayerControl.LocalPlayer, true);
                    PlayerControl.LocalPlayer.RpcSetFinalStatus(FinalStatus.WorshiperSelfDeath);
                }

                if (PlayerControl.LocalPlayer.IsDead())
                {
                    if (SeePositionArrow.arrow != null)
                        Object.Destroy(SeePositionArrow.arrow);
                    return;
                }
                if (SeePositionArrow == null) SeePositionArrow = new(color);
                if (player.IsDead()) return;
                SeePositionArrow.Update(player.transform.position, color);
                SeePositionArrow.arrow.SetActive(true);
            }
        }
    }
    public static bool IsKiller(PlayerControl target)
    {
        if (target.IsImpostor()) return true;
        if (target.IsJackalTeam() && !target.IsFriendRoles() && !target.IsJackalTeamSidekick()) return true;
        if (target.IsRole(RoleId.Pavlovsdogs, RoleId.Hitman, RoleId.Stefinder, RoleId.Egoist, RoleId.FireFox)) return true;
        if (target.IsRole(RoleId.Sheriff, RoleId.RemoteSheriff)) return true;
        return false;
    }
    public static void SetOutfit(PlayerControl target, GameData.PlayerOutfit outfit)
    {
        MessageWriter writer = RPCHelper.StartRPC(CustomRPC.SetOutfit);
        writer.Write(target.PlayerId);
        writer.Write(outfit.ColorId);
        writer.Write(outfit.HatId);
        writer.Write(outfit.PetId);
        writer.Write(outfit.SkinId);
        writer.Write(outfit.VisorId);
        writer.Write(outfit.PlayerName);
        writer.EndRPC();
        RPCProcedure.SetOutfit(target.PlayerId, outfit.ColorId, outfit.HatId, outfit.PetId, outfit.SkinId, outfit.VisorId, outfit.PlayerName);
    }
}