using System.Linq;
using HarmonyLib;
using Hazel;
using SuperNewRoles.Helpers;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using UnityEngine;

namespace SuperNewRoles.Roles.Impostor;

public class RemoteController : RoleBase, IImpostor, IKillButtonEvent, IUseButtonEvent, ICustomButton, IIntroHandler, IDeathHandler, IHandleChangeRole, IRpcHandler
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
    public static new IntroInfo Introinfo = new(RoleId.RemoteController, 2, AmongUs.GameOptions.RoleTypes.Impostor);

    public static CustomOption MarkingCoolTime;
    public static CustomOption OperationCoolTime;
    private static void CreateOption()
    {
        MarkingCoolTime = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Impostor, "RemoteControllerMarkingCoolTimeOption", 20f, 0f, 60f, 2.5f, Optioninfo.RoleOption);
        OperationCoolTime = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Impostor, "RemoteControllerOperationCoolTimeOption", 30f, 2.5f, 60f, 2.5f, Optioninfo.RoleOption);
    }

    public CustomButtonInfo[] CustomButtonInfos { get; }
    public CustomButtonInfo MarkingButton;
    public CustomButtonInfo OperationButton;
    public PlayerControl TargetPlayer;
    public PoolablePlayer TargetIcon;
    public bool _UnderOperation;
    public bool UnderOperation => (Player.AmOwner && AmongUsUtil.CurrentCamTarget == TargetPlayer) || (_UnderOperation && TargetPlayer);
    public float Timer;
    public RemoteController(PlayerControl player) : base(player, Roleinfo, Optioninfo, Introinfo)
    {
        TargetPlayer = null;
        TargetIcon = null;
        _UnderOperation = false;
        Timer = 2.5f;
        MarkingButton = new(
            null, this, MarkingButtonOnClick, alive => alive, CustomButtonCouldType.SetTarget | CustomButtonCouldType.CanMove, MarkingButtonOnMeetingEnd,
            ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.RemoteControllerOperationButton.png", 115f),
            MarkingCoolTime.GetFloat, new(-1, 1), "RemoteControllerMarkingButtonName", KeyCode.F, CouldUse: () => !TargetPlayer,
            SetTargetUntargetPlayer: () => RoleBaseManager.GetRoleBases<RemoteController>().FindAll(x => x.TargetPlayer).ConvertAll(x => x.TargetPlayer)
        );
        OperationButton = new(
            null, this, OperationButtonOnClick, alive => alive, CustomButtonCouldType.Always, null,
            ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.RemoteControllerOperationButton.png", 115f),
            OperationCoolTime.GetFloat, new(-2, 1), "RemoteControllerOperationButtonName", KeyCode.F,
            DurationTime: () => 0f, IsEffectDurationInfinity: true, OnEffectEnds: OperationButtonOnEffectEnds, CouldUse: () => TargetPlayer && (UnderOperation || Player.CanMove)
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
        TargetPlayer.CmdCheckMurder(button.currentTarget);
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

    public void MarkingButtonOnClick()
    {
        PlayerControl target = MarkingButton.SetTarget();
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
        if (!OperationButton.GetOrCreateButton().isEffectActive)
        {
            AmongUsUtil.SetCamTarget(TargetPlayer);
            MessageWriter writer = RpcWriter;
            writer.Write((byte)RpcType.SetUnderOperation);
            writer.Write(true);
            SendRpc(writer);
        }
        else OperationButtonOnEffectEnds();
    }

    public void OperationButtonOnEffectEnds()
    {
        if (!Player.AmOwner) return;
        AmongUsUtil.SetCamTarget(null);
        
        MessageWriter writer = RpcWriter;
        writer.Write((byte)RpcType.SetUnderOperation);
        writer.Write(false);
        SendRpc(writer);
        
        writer = RpcWriter;
        writer.Write((byte)RpcType.SetTarget);
        writer.Write(byte.MaxValue);
        SendRpc(writer);

        ResetCoolTime();
        float time = GameOptionsManager.Instance.CurrentGameOptions.GetFloat(AmongUs.GameOptions.FloatOptionNames.KillCooldown);
        HudManager.Instance.KillButton.SetCoolDown(time, time);
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
        if (TargetPlayer != null)
        {
            var outfit = TargetPlayer.CurrentOutfit;

            TargetIcon.SetFlipX(true);

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

    public void OnAmDeath(DeathInfo death) => OperationButtonOnEffectEnds();

    public void OnChangeRole()
    {
        if (Player.AmOwner) AmongUsUtil.SetCamTarget(null);
        TargetPlayer = null;
        _UnderOperation = false;
    }

    public enum RpcType
    {
        SetTarget,
        SetUnderOperation,
        SetNormalizedVelocity,
    }
    public void RpcReader(MessageReader reader)
    {
        switch ((RpcType)reader.ReadByte())
        {
            case RpcType.SetTarget:
                TargetPlayer = reader.ReadByte().GetPlayerControl();
                break;
            case RpcType.SetUnderOperation:
                _UnderOperation = reader.ReadBoolean();
                break;
            case RpcType.SetNormalizedVelocity:
                TargetPlayer.MyPhysics.SetNormalizedVelocity(NetHelpers.ReadVector2(reader));
                break;
        }
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
            if (ShipStatus.Instance && __instance.lightSource)
            {
                float num1 = ShipStatus.Instance.CalculateLightRadius(role.TargetPlayer.Data);
                if (!Mathf.Approximately(num1, __instance.lightSource.ViewDistance)) __instance.AdjustLighting();
                __instance.lightSource.SetViewDistance(num1);
            }

            __instance.newItemsInRange.Clear();
            float distance = float.MaxValue;
            IUsable target = null;
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
                    float d = usable.CanUse(role.TargetPlayer.Data, out bool can, out bool could);
                    if (can || could) __instance.newItemsInRange.Add(usable);
                    if (can && d < distance)
                    {
                        if (usable.TryCast<Vent>()) continue;
                        distance = d;
                        target = usable;
                    }
                }
            }

            role.Timer -= Time.fixedDeltaTime;
            if (role.Timer <= 0)
            {
                role.Timer += 2.5f;
                MessageWriter writer = RPCHelper.StartRPC(RpcCalls.SnapTo, role.TargetPlayer);
                NetHelpers.WriteVector2(role.TargetPlayer.transform.position, writer);
                writer.Write((ushort)(role.TargetPlayer.NetTransform.lastSequenceId + 2));
                writer.EndRPC();
            }

            if (!Minigame.Instance && role.TargetPlayer.CanMove)
            {
                role.TargetPlayer.NetTransform.incomingPosQueue.Clear();
                role.TargetPlayer.MyPhysics.SetNormalizedVelocity(DestroyableSingleton<HudManager>.Instance.joystick.DeltaL);
                MessageWriter writer = RPCHelper.StartRPC(CustomRPC.RoleRpcHandler, role.TargetPlayer);
                writer.Write(__instance?.PlayerId ?? 255);
                writer.Write((byte)RpcType.SetNormalizedVelocity);
                NetHelpers.WriteVector2(DestroyableSingleton<HudManager>.Instance.joystick.DeltaL, writer);
                writer.EndRPC();
            }

            __instance.closest = target;
            FastDestroyableSingleton<HudManager>.Instance.UseButton.Show();
            FastDestroyableSingleton<HudManager>.Instance.PetButton.Hide();
            FastDestroyableSingleton<HudManager>.Instance.UseButton.SetTarget(target);
            FastDestroyableSingleton<HudManager>.Instance.ReportButton.SetActive(false);
            FastDestroyableSingleton<HudManager>.Instance.ImpostorVentButton.SetTarget(null);
            __instance.Data.Role.SetUsableTarget(null);

            FastDestroyableSingleton<HudManager>.Instance.SabotageButton.Refresh();
            FastDestroyableSingleton<HudManager>.Instance.AdminButton.Refresh();
            return false;
        }
    }

    [HarmonyPatch(typeof(CustomNetworkTransform))]
    public static class PlayerPhysicsPatch
    {
        [HarmonyPatch(nameof(CustomNetworkTransform.Deserialize)), HarmonyPrefix]
        public static bool DeserializePrefix(CustomNetworkTransform __instance)
        {
            if (!PlayerControl.LocalPlayer.TryGetRoleBase(out RemoteController role) ||
                !role.UnderOperation || role.TargetPlayer != __instance.myPlayer) return true;
            return false;
        }
    }

    //*
    [HarmonyPatch(typeof(LightSource))]
    public static class LightSourcePatch
    {
        [HarmonyPatch(nameof(LightSource.Update)), HarmonyPrefix]
        public static bool UpdatePrefix(LightSource __instance)
        {
            if (!PlayerControl.LocalPlayer.TryGetRoleBase(out RemoteController role) || !role.UnderOperation) return true;
            Vector3 position = role.TargetPlayer.transform.position;
            position.z -= 7f;
            __instance.UpdateFlashlightAngle();
            __instance.LightCutawayMaterial.SetFloat("_PlayerRadius", __instance.PlayerRadius);
            __instance.LightCutawayMaterial.SetFloat("_LightRadius", __instance.ViewDistance);
            __instance.LightCutawayMaterial.SetVector("_LightOffset", __instance.LightOffset);
            __instance.LightCutawayMaterial.SetFloat("_FlashlightSize", __instance.FlashlightSize);
            __instance.LightCutawayMaterial.SetFloat("_FlashlightAngle", PlayerControl.LocalPlayer.FlashlightAngle);
            __instance.lightChild.transform.position = position;
            __instance.renderer.Render(position);
            return false;
        }
    }
    //*/
}