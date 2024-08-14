using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AmongUs.Data;
using HarmonyLib;
using Hazel;
using Il2CppSystem;
using SuperNewRoles.Helpers;
using SuperNewRoles.MapCustoms;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using UnityEngine;
using UnityEngine.Audio;
using Object = UnityEngine.Object;

namespace SuperNewRoles.Roles.Impostor;

public class RemoteController : RoleBase, IImpostor, IVanillaButtonEvents, ICustomButton, IIntroHandler, IMeetingHandler, IDeathHandler, IHandleChangeRole, IRpcHandler
{
    public static new RoleInfo Roleinfo = new(
        typeof(RemoteController),
        player => new RemoteController(player),
        RoleId.RemoteController,
        "RemoteController",
        RoleClass.ImpostorRed,
        new(RoleId.RemoteController, TeamTag.Impostor, RoleTag.Killer, RoleTag.SpecialKiller, RoleTag.Hacchan),
        TeamRoleType.Impostor,
        TeamType.Impostor
    );
    public static new OptionInfo Optioninfo = new(RoleId.RemoteController, 206500, false, optionCreator: CreateOption);
    public static new IntroInfo Introinfo = new(RoleId.RemoteController, 1, AmongUs.GameOptions.RoleTypes.Impostor);

    public static CustomOption MarkingCoolTime;
    public static CustomOption DurationOperation;
    public static string[] DurationOperationText
    {
        get
        {
            List<string> selections = ["Permanence"];
            for (float s = 2.5f; s <= 120f; s += 2.5f)
                selections.Add(s.ToString());
            return selections.ToArray();
        }
    }
    private static void CreateOption()
    {
        MarkingCoolTime = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Impostor, "RemoteControllerMarkingCoolTimeOption", 20f, 0f, 60f, 2.5f, Optioninfo.RoleOption);
        DurationOperation = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Impostor, "RemoteControllerDurationOperation", DurationOperationText, Optioninfo.RoleOption);
    }

    public CustomButtonInfo[] CustomButtonInfos { get; }
    public CustomButtonInfo MarkingButton;
    public CustomButtonInfo OperationButton;
    public PlayerControl TargetPlayer;
    public PoolablePlayer TargetIcon;
    public bool _UnderOperation;
    public bool UnderOperation => (Player.AmOwner && AmongUsUtil.CurrentCamTarget == TargetPlayer) || (_UnderOperation && TargetPlayer);
    public float Timer;
    public GameObject LightChild;
    public RemoteController(PlayerControl player) : base(player, Roleinfo, Optioninfo, Introinfo)
    {
        TargetPlayer = null;
        TargetIcon = null;
        _UnderOperation = false;
        Timer = 2.5f;

        LightChild = new("RemoteLightChild") { layer = LayerExpansion.GetShadowLayer() };
        LightChild.transform.position = new();
        LightChild.transform.localScale = Vector3.zero;
        LightSource source = PlayerControl.LocalPlayer.LightPrefab;
        LightChild.AddComponent<MeshFilter>().mesh = source.lightChildMesh;
        LightChild.AddComponent<MeshRenderer>().material.shader = source.LightCutawayMaterial.shader;

        MarkingButton = new(
            null, this, MarkingButtonOnClick, alive => alive, CustomButtonCouldType.SetTarget | CustomButtonCouldType.CanMove, MarkingButtonOnMeetingEnd,
            AssetManager.GetAsset<Sprite>("RemoteControllerOperationButton.png", AssetManager.AssetBundleType.Sprite),
            MarkingCoolTime.GetFloat, new(-1, 1), "RemoteControllerMarkingButtonName", KeyCode.F, CouldUse: () => !TargetPlayer,
            SetTargetUntargetPlayer: () => RoleBaseManager.GetRoleBases<RemoteController>().FindAll(x => x.TargetPlayer).ConvertAll(x => x.TargetPlayer),
            SetTargetCrewmateOnly: () => true
        );
        OperationButton = new(
            null, this, OperationButtonOnClick, alive => alive, CustomButtonCouldType.Always, null,
            AssetManager.GetAsset<Sprite>("RemoteControllerOperationButton.png", AssetManager.AssetBundleType.Sprite),
            () => 0, new(-2, 1), "RemoteControllerOperationButtonName", KeyCode.F,
            DurationTime: () => float.TryParse(DurationOperation.GetString(), out float value) ? value : 0f,
            IsEffectDurationInfinity: DurationOperation.GetSelection() == 0,
            OnEffectEnds: OperationButtonOnEffectEnds, CouldUse: () => TargetPlayer && (UnderOperation || Player.CanMove)
        );
        OperationButton.GetOrCreateButton().effectCancellable = true;
        CustomButtonInfos = new CustomButtonInfo[]
        {
            MarkingButton,
            OperationButton,
        };
    }

    public bool KillButtonDoClick(KillButton button)
    {
        ResetCoolTime();
        if (!UnderOperation) return true;
        if (!button.isActiveAndEnabled || !button.currentTarget || button.isCoolingDown) return true;
        ModHelpers.CheckMurderAttemptAndKill(TargetPlayer, button.currentTarget, showAnimation: true);
        button.SetTarget(null);
        OperationButtonOnEffectEnds();
        return false;
    }

    public bool KillButtonCheckClick(KillButton button, PlayerControl target)
    {
        if (!UnderOperation) return true;
        button.DoClick();
        return false;
    }

    public bool KillButtonSetTarget(KillButton button, PlayerControl target)
    {
        if (!UnderOperation) return true;
        if (TargetPlayer && PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.Data.Role)
        {
            target = PlayerControlFixedUpdatePatch.SetTarget(true, targetingPlayer: TargetPlayer);
            RoleTeamTypes teamType = PlayerControl.LocalPlayer.Data.Role.TeamType;
            if (button.currentTarget && button.currentTarget != target) button.currentTarget.ToggleHighlight(active: false, teamType);
            button.currentTarget = target;
            if (button.currentTarget)
            {
                button.currentTarget.ToggleHighlight(active: true, teamType);
                button.SetEnabled();
            }
            else button.SetDisabled();
        }
        return false;
    }

    public bool UseButtonDoClick(UseButton button)
    {
        if (!UnderOperation) return true;
        if (button.isActiveAndEnabled && TargetPlayer && !button.isCoolingDown)
        {
            if (button.currentTarget.Il2CppIs(out DoorConsole console))
            {
                try
                {
                    Minigame minigame = Object.Instantiate(console.MinigamePrefab, Camera.main.transform);
                    minigame.transform.localPosition = new Vector3(0f, 0f, -50f);
                    minigame.Cast<IDoorMinigame>().SetDoor(console.MyDoor);
                    minigame.Begin(null);
                }
                catch
                {
                    if (Minigame.Instance)
                        Object.Destroy(Minigame.Instance.gameObject);
                }
            }
            else if (button.currentTarget.Il2CppIs(out Ladder ladder))
                TargetPlayer.MyPhysics.RpcClimbLadder(ladder);
            else if (button.currentTarget.Il2CppIs(out PlatformConsole platform))
                TargetPlayer.RpcUsePlatform();
            else if (button.currentTarget.Il2CppIs(out ZiplineConsole zipline))
                TargetPlayer.RpcUseZipline(TargetPlayer, zipline.zipline, zipline.atTop);
        }
        return false;
    }

    public bool UseButtonSetTarget(UseButton button, IUsable target)
    {
        if (!UnderOperation) return true;
        if (button.fastUseSettings == null) return false;
        button.currentTarget = target;
        if (button.currentTarget != null && button.currentTarget.UseIcon == ImageNames.UseButton)
        {
            button.SetFromSettings(button.fastUseSettings[ImageNames.UseButton]);
            button.SetEnabled();
            if (button.currentTarget.Il2CppIs(out IUsableCoolDown usableCoolDown)) button.SetCoolDown(usableCoolDown.CoolDown, usableCoolDown.MaxCoolDown);
            else button.ResetCoolDown();
            button.SetCooldownFill(target?.PercentCool ?? 0f);
        }
        else
        {
            button.ResetCoolDown();
            button.SetFromSettings(button.fastUseSettings[ImageNames.UseButton]);
            button.SetDisabled();
        }
        return false;
    }

    public bool VentButtonDoClick(VentButton button)
    {
        if (!UnderOperation) return true;
        if (button.currentTarget == null) return false;
        VentCanUse(button.currentTarget, out var canUse, out var _);
        if (!canUse) return false;
        FastDestroyableSingleton<AchievementManager>.Instance.OnConsoleUse(button.currentTarget.Cast<IUsable>());
        if (TargetPlayer.inVent && !TargetPlayer.walkingToVent)
        {
            TargetPlayer.MyPhysics.RpcExitVent(button.currentTarget.Id);
            button.currentTarget.SetButtons(enabled: false);
        }
        else if (!TargetPlayer.walkingToVent)
        {
            TargetPlayer.MyPhysics.RpcEnterVent(button.currentTarget.Id);
            button.currentTarget.SetButtons(enabled: true);
        }
        return false;
    }

    public void MarkingButtonOnClick()
    {
        PlayerControl target = MarkingButton.CurrentTarget;
        if (target == null) return;
        new LateTask(() =>
        {
            MessageWriter writer = RpcWriter;
            writer.Write((byte)RpcType.SetTarget);
            writer.Write(target.PlayerId);
            SendRpc(writer);
            new LateTask(SetIconOutfit, 0f, "RemoteController Icon");
        }, 0.1f, "RemoteController");
    }

    public void MarkingButtonOnMeetingEnd() => OperationButtonOnEffectEnds();

    public void OperationButtonOnClick()
    {
        if (TargetPlayer == null) return;
        if (!OperationButton.customButton.isEffectActive)
        {
            AmongUsUtil.SetCamTarget(TargetPlayer);
            MessageWriter writer = RpcWriter;
            writer.Write((byte)RpcType.SetUnderOperation);
            writer.Write(true);
            SendRpc(writer);
            
            if (Constants.ShouldPlaySfx())
            {
                SoundManager.Instance.PlaySound(
                    AssetManager.GetAsset<AudioClip>("OperationSound.mp3", AssetManager.AssetBundleType.Sound),
                    false, audioMixer: SoundManager.Instance.sfxMixer
                ).pitch = FloatRange.Next(0.8f, 1.2f);
            }

            if (TargetPlayer.inVent && ShipStatus.Instance.Systems[SystemTypes.Ventilation].Il2CppIs(out VentilationSystem ventilation))
            {
                if (!ventilation.PlayersInsideVents.TryGetValue(TargetPlayer.PlayerId, out byte value)) return;
                ModHelpers.VentById(value).SetButtons(true);
            }
        }
        else OperationButtonOnEffectEnds();
    }

    public void OperationButtonOnEffectEnds()
    {
        if (!Player.AmOwner) return;
        AmongUsUtil.SetCamTarget(null);
        if (TargetPlayer.inVent && ShipStatus.Instance.Systems[SystemTypes.Ventilation].Il2CppIs(out VentilationSystem ventilation))
        {
            TargetPlayer.MyPhysics.RpcExitVent(ventilation.PlayersInsideVents.TryGetValue(TargetPlayer.PlayerId, out byte value) ? value : 0);
            ModHelpers.VentById(value)?.SetButtons(false);
        }

        MessageWriter writer = RpcWriter;
        writer.Write((byte)RpcType.SetUnderOperation);
        writer.Write(false);
        SendRpc(writer);
        
        writer = RpcWriter;
        writer.Write((byte)RpcType.SetTarget);
        writer.Write(byte.MaxValue);
        SendRpc(writer);

        ResetCoolTime();
        Player.SetKillTimer(GameOptionsManager.Instance.CurrentGameOptions.GetFloat(AmongUs.GameOptions.FloatOptionNames.KillCooldown));
        new LateTask(SetIconOutfit, 0f, "RemoteControllerIcon");
    }

    public void ResetCoolTime()
    {
        MarkingButton.ResetCoolTime();
        OperationButton.ResetCoolTime();
    }

    public void OnIntroDestoryMe(IntroCutscene __instance)
    {
        if (FastDestroyableSingleton<HudManager>.Instance == null) return;
        if (TargetIcon != null) return;

        // 生成
        TargetIcon = Object.Instantiate(__instance.PlayerPrefab, FastDestroyableSingleton<HudManager>.Instance.transform);

        // 位置設定
        TargetIcon.transform.localPosition = new(-4.5f, -2.15f, -9f);
        TargetIcon.transform.localScale = Vector3.one * 0.5f;
        Transform PlayerNames = TargetIcon.NameText().transform.parent;
        PlayerNames.localPosition = new(0, -0.5f, 0f);
        PlayerNames.localScale = Vector3.one * 1.5f;

        SetIconOutfit();
    }

    private void SetIconOutfit()
    {
        if (!Player.AmOwner) return;
        if (TargetPlayer != null)
        {
            var outfit = TargetPlayer.CurrentOutfit;

            TargetIcon.SetFlipX(false);

            TargetIcon.SetBodyColor(outfit.ColorId);
            TargetIcon.SetSkin(outfit.SkinId, outfit.ColorId);
            TargetIcon.cosmetics.SetHat(outfit.HatId, outfit.ColorId);
            TargetIcon.SetVisor(outfit.VisorId, outfit.ColorId);
            TargetIcon.SetPetIdle(outfit.PetId, outfit.ColorId);
            TargetIcon.cosmetics.nameText.text = $"[ {ModTranslation.GetString("RemoteControllerTarget")} ]\n{outfit.PlayerName}";

            TargetIcon.gameObject.SetActive(true);
        }
        else TargetIcon.gameObject.SetActive(false);
    }

    public void StartMeeting() { TargetIcon?.gameObject.SetActive(false); }

    public void CloseMeeting() { }

    public void OnAmDeath(DeathInfo death) => OperationButtonOnEffectEnds();

    public void OnDeath(DeathInfo death)
    {
        if (Player.AmOwner && death.DeathPlayer != TargetPlayer) return;
        MessageWriter writer = RpcWriter;
        writer.Write((byte)RpcType.SetUnderOperation);
        writer.Write(false);
        SendRpc(writer);

        writer = RpcWriter;
        writer.Write((byte)RpcType.SetTarget);
        writer.Write(byte.MaxValue);
        SendRpc(writer);

        OperationButtonOnEffectEnds();
    }

    public void OnChangeRole()
    {
        if (Player.AmOwner)
        {
            AmongUsUtil.SetCamTarget(null);
            LightChild.transform.localScale = Vector3.zero;
        }
        TargetPlayer = null;
        _UnderOperation = false;
    }

    public enum RpcType
    {
        SetTarget,
        SetUnderOperation,
        MoveVent,
    }
    public void RpcReader(MessageReader reader)
    {
        switch ((RpcType)reader.ReadByte())
        {
            case RpcType.SetTarget:
                TargetPlayer = reader.ReadByte().GetPlayerControl();
                SetIconOutfit();
                break;
            case RpcType.SetUnderOperation:
                _UnderOperation = reader.ReadBoolean();
                break;
            case RpcType.MoveVent:
                if (!TargetPlayer.AmOwner || !TargetPlayer.inVent) return;
                VentilationSystem.Update(VentilationSystem.Operation.Move, reader.ReadInt32());
                break;
        }
    }

    public float VentCanUse(Vent vent, out bool can, out bool could)
    {
        can = could = false;
        if (!TargetPlayer) return float.MaxValue;
        if (!ShipStatus.Instance.Systems.TryGetValue(SystemTypes.Ventilation, out var system) ||
            !system.Il2CppIs(out VentilationSystem ventilation) ||
            !ventilation.IsVentCurrentlyBeingCleaned(vent.Id))
            could = true;
        if (could)
        {
            Bounds bounds = TargetPlayer.Collider.bounds;
            Vector3 center = bounds.center;
            Vector3 position = vent.transform.position;
            float distance = Vector2.Distance(center, position);
            can = distance <= vent.UsableDistance && !PhysicsHelpers.AnythingBetween(TargetPlayer.Collider, center, position, Constants.ShipOnlyMask, false);
            return distance;
        }
        return float.MaxValue;
    }

    [HarmonyPatch(typeof(PlayerControl))]
    public static class PlayerControlPatch
    {
        [HarmonyPatch(nameof(PlayerControl.CanMove), MethodType.Getter), HarmonyPostfix]
        public static void CanMoveGetterPostfix(PlayerControl __instance, ref bool __result)
        {
            if (!__instance.AmOwner) return;
            if (!__result) return;
            if (__instance.TryGetRoleBase(out RemoteController remote) && remote.UnderOperation)
            {
                __result = false;
                return;
            }
            if (RoleBaseManager.GetRoleBases<RemoteController>().Any(role => role.TargetPlayer == __instance && role.UnderOperation))
            {
                __result = false;
                return;
            }
        }

        [HarmonyPatch(nameof(PlayerControl.FixedUpdate)), HarmonyPrefix]
        public static bool FixedUpdatePrefix(PlayerControl __instance)
        {
            if (!__instance.AmOwner || __instance.IsDead()) return true;
            if (!PlayerControl.LocalPlayer.TryGetRoleBase(out RemoteController role) || !role.UnderOperation) return true;
            if (!GameData.Instance) return false;

            if (role.TargetPlayer.CanMove || role.TargetPlayer.inVent)
            {
                __instance.newItemsInRange.Clear();
                float distance = float.MaxValue;
                float vent_distance = float.MaxValue;
                IUsable target = null;
                Vent vent_target = null;
                foreach (Collider2D collider in Physics2D.OverlapCircleAll(role.TargetPlayer.GetTruePosition(), __instance.MaxReportDistance, Constants.Usables))
                {
                    if (!__instance.cache.TryGetValue(collider, out var value))
                    {
                        __instance.cache[collider] = collider.GetComponents<IUsable>().ToArray();
                        value = __instance.cache[collider];
                    }
                    if (value == null) continue;
                    foreach (IUsable usable in value)
                    {
                        if (usable.TryCast<Console>()) continue;
                        if (usable.Il2CppIs(out Vent vent))
                        {
                            float d = role.VentCanUse(vent, out bool can, out bool could);
                            if (can || could) __instance.newItemsInRange.Add(usable);
                            if (!can) continue;
                            else if (d < vent_distance)
                            {
                                vent_distance = d;
                                vent_target = vent;
                            }
                        }
                        else
                        {
                            float d = usable.CanUse(role.TargetPlayer.Data, out bool can, out bool could);
                            if (can || could) __instance.newItemsInRange.Add(usable);
                            if (!can) continue;
                            else if (d < distance)
                            {
                                distance = d;
                                target = usable;
                            }
                        }
                    }
                }

                role.Timer -= Time.fixedDeltaTime;
                if (role.Timer <= 0)
                {
                    role.Timer += 2.5f;
                    MessageWriter writer = RPCHelper.StartRPC(CustomRPC.CustomSnapTo, role.TargetPlayer);
                    writer.Write(role.TargetPlayer.PlayerId);
                    NetHelpers.WriteVector2(role.TargetPlayer.transform.position, writer);
                    writer.EndRPC();
                }

                __instance.closest = target;
                FastDestroyableSingleton<HudManager>.Instance.ToggleUseAndPetButton(target, false, false);
                FastDestroyableSingleton<HudManager>.Instance.ReportButton.SetActive(false);
                FastDestroyableSingleton<HudManager>.Instance.ImpostorVentButton.SetTarget(vent_target);
                __instance.Data.Role.SetUsableTarget(vent_target.Il2CppIs(out IUsable v) ? v : null);
            }
            else
            {
                __instance.closest = null;
                FastDestroyableSingleton<HudManager>.Instance.UseButton.SetTarget(null);
                FastDestroyableSingleton<HudManager>.Instance.PetButton.SetDisabled();
                FastDestroyableSingleton<HudManager>.Instance.ReportButton.SetActive(false);
                FastDestroyableSingleton<HudManager>.Instance.ImpostorVentButton.SetTarget(Vent.currentVent);
                __instance.Data.Role.SetUsableTarget(Vent.currentVent.Il2CppIs(out IUsable v) ? v : null);
                if (PlayerCustomizationMenu.Instance) FastDestroyableSingleton<HudManager>.Instance.UseButton.gameObject.SetActive(false);
            }

            if (role.TargetPlayer.CanMove)
            {
                __instance.SetKillTimer(__instance.killTimer - Time.fixedDeltaTime);

                if (!Minigame.Instance)
                {
                    role.TargetPlayer.NetTransform.incomingPosQueue.Clear();
                    role.TargetPlayer.MyPhysics.SetNormalizedVelocity(FastDestroyableSingleton<HudManager>.Instance.joystick.DeltaL);
                    MessageWriter writer = RPCHelper.StartRPC(CustomRPC.SetNormalizedVelocity, role.TargetPlayer);
                    writer.Write(role.TargetPlayer.PlayerId);
                    NetHelpers.WriteVector2(FastDestroyableSingleton<HudManager>.Instance.joystick.DeltaL, writer);
                    writer.EndRPC();
                }
            }

            FastDestroyableSingleton<HudManager>.Instance.SabotageButton.Refresh();
            FastDestroyableSingleton<HudManager>.Instance.AdminButton.Refresh();
            return false;
        }
    }

    [HarmonyPatch(typeof(CustomNetworkTransform))]
    public static class CustomNetworkTransformPatch
    {
        [HarmonyPatch(nameof(CustomNetworkTransform.Deserialize)), HarmonyPrefix]
        public static bool DeserializePrefix(CustomNetworkTransform __instance)
        {
            if (PlayerControl.LocalPlayer.TryGetRoleBase(out RemoteController role) &&
                role.UnderOperation && role.TargetPlayer == __instance.myPlayer) return false;
            return true;
        }
    }

    [HarmonyPatch(typeof(LightSource))]
    public static class LightSourcePatch
    {
        [HarmonyPatch(nameof(LightSource.Update)), HarmonyPostfix]
        public static void UpdatePostfix()
        {
            if (!PlayerControl.LocalPlayer.TryGetRoleBase(out RemoteController role)) return;
            if (role.UnderOperation)
            {
                Vector3 position = role.TargetPlayer.transform.position;
                position.z -= 7f;
                role.LightChild.transform.position = position;
                float size = role.UnderOperation ? (ShipStatus.Instance.MaxLightRadius * GameManager.Instance.LogicOptions.currentGameOptions.GetFloat(AmongUs.GameOptions.FloatOptionNames.ImpostorLightMod) * 5.25f) : 0f;
                role.LightChild.transform.localScale = new(size, size, 1f);
            }
            else role.LightChild.transform.localScale = Vector3.zero;
        }
    }

    [HarmonyPatch(typeof(Vent))]
    public static class VentPatch
    {
        [HarmonyPatch(nameof(Vent.TryMoveToVent)), HarmonyPrefix]
        public static bool TryMoveToVentPrefix(Vent __instance, ref bool __result, Vent otherVent, ref string error)
        {
            if (!PlayerControl.LocalPlayer.TryGetRoleBase(out RemoteController role) || !role.UnderOperation)
            {
                if (RoleBaseManager.GetRoleBases<RemoteController>().Any(role => role.TargetPlayer == __instance && role.UnderOperation))
                {
                    __instance.SetButtons(false);
                    return false;
                }
                return true;
            }
            if (otherVent == null)
            {
                error = "Vent does not exist";
                __result = false;
                return false;
            }
            if (!role.TargetPlayer.inVent)
            {
                error = "Player is not currently inside a vent";
                __result = false;
                return false;
            }
            if (role.TargetPlayer.walkingToVent || role.TargetPlayer.Visible)
            {
                error = "Player was still in the middle of animating into current vent; not allowed to move vents that fast";
                __result = false;
                return false;
            }
            Vector3 position = otherVent.transform.position;
            position -= (Vector3)role.TargetPlayer.Collider.offset;

            MessageWriter writer = RPCHelper.StartRPC(CustomRPC.CustomSnapTo);
            writer.Write(role.TargetPlayer.PlayerId);
            NetHelpers.WriteVector2(position, writer);
            writer.EndRPC();
            role.TargetPlayer.NetTransform.SnapTo(position);

            if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(ShipStatus.Instance.VentMoveSounds.Random(), loop: false).pitch = FloatRange.Next(0.8f, 1.2f);
            __instance.SetButtons(enabled: false);
            otherVent.SetButtons(enabled: true);
            Vent.currentVent = otherVent;

            writer = RPCHelper.StartRPC(CustomRPC.RoleRpcHandler);
            writer.Write(role.Player.PlayerId);
            writer.Write((byte)RpcType.MoveVent);
            writer.Write(otherVent.Id);
            writer.EndRPC();

            error = string.Empty;
            __result = true;
            return false;
        }
    }
}