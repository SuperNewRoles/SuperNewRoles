using System.Collections.Generic;
using System.Linq;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomObject;
using SuperNewRoles.Patches;
using SuperNewRoles.ReplayManager;
using SuperNewRoles.Roles.RoleBases;
using UnityEngine;

namespace SuperNewRoles.Roles;

public class Vulture : RoleBase<Vulture>
{
    public static Color32 color = new(205, 133, 63, byte.MaxValue);

    public Vulture()
    {
        RoleId = roleId = RoleId.Vulture;
        //以下いるもののみ変更
        OptionId = 115;
        OptionType = CustomOptionType.Neutral;
        CanUseVentOptionOn = true;
        CanUseVentOptionDefault = false;
    }

    public override void OnMeetingStart() { }
    public override void OnWrapUp() { }
    public override void FixedUpdate()
    {
        if (VultureShowArrows.GetBool()) Arrows();
    }
    public override void MeFixedUpdateAlive() { }
    public override void MeFixedUpdateDead() { }
    public override void OnKill(PlayerControl target) { }
    public override void OnDeath(PlayerControl killer = null) { }
    public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }
    public override void EndUseAbility() { }
    public override void ResetRole() { Logger.Info("ResetRole()実行", "Vulture"); }
    public override void PostInit()
    {
        AbilityLimit = VultureDeadBodyMaxCount.GetInt();
        DeadBodyCount = VultureDeadBodyMaxCount.GetInt();
    }
    public override void UseAbility() { base.UseAbility(); AbilityLimit--; if (AbilityLimit <= 0) EndUseAbility(); }
    public override bool CanUseAbility() { return base.CanUseAbility() && AbilityLimit <= 0; }

    //ボタンが必要な場合のみ(Buttonsの方に記述する必要あり)
    public static CustomButton VultureButton;
    public static void MakeButtons(HudManager hm)
    {
        VultureButton = new(
            () =>
            {
                Vulture role = local;
                RpcCleanDeadBody(role.AbilityLimit);
                role.AbilityLimit--;
                role.DeadBodyCount--;

                if (role.AbilityLimit <= 0)
                {
                    RPCProcedure.ShareWinner(CachedPlayer.LocalPlayer.PlayerId);
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareWinner, SendOption.Reliable, -1);
                    writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    if (AmongUsClient.Instance.AmHost) CheckGameEndPatch.CustomEndGame((GameOverReason)CustomGameOverReason.VultureWin, false);
                    else
                    {
                        MessageWriter writer2 = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomEndGame, SendOption.Reliable, -1);
                        writer2.Write((byte)CustomGameOverReason.VultureWin);
                        writer2.Write(false);
                        AmongUsClient.Instance.FinishRpcImmediately(writer2);
                    }
                }
                SetButtonCooldowns();
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Vulture; },
            () => { return hm.ReportButton.graphic.color == Palette.EnabledColor && PlayerControl.LocalPlayer.CanMove; },
            SetButtonCooldowns,
            ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.VultureButton.png", 115f),
            new Vector3(-2f, 1, 0),
            hm,
            hm.AbilityButton,
            KeyCode.F,
            49,
            () => { return false; }
        )
        {
            buttonText = ModTranslation.GetString("VultureButtonName"),
            showButtonText = true
        };
    }
    public static void SetButtonCooldowns()
    {
        HudManagerStartPatch.VultureButton.MaxTimer = VultureCooldown.GetFloat();
        HudManagerStartPatch.VultureButton.Timer = VultureCooldown.GetFloat();
    }

    public int DeadBodyCount
    {
        get { return ReplayData.CanReplayCheckPlayerView ? (int)GetValueFloat("VultureDeadBodyCount") : _DeadBodyCount; }
        set { if (ReplayData.CanReplayCheckPlayerView) SetValueFloat("VultureDeadBodyCount", value); else _DeadBodyCount = value; }
    }
    public int _DeadBodyCount;

    public static CustomOption VultureCooldown;
    public static CustomOption VultureDeadBodyMaxCount;
    public static CustomOption VultureShowArrows;

    public override void SetupMyOptions()
    {
        Logger.Info("実行");
        VultureCooldown = CustomOption.Create(OptionId, false, CustomOptionType.Neutral, "VultureCooldownSetting", 30f, 2.5f, 60f, 2.5f, RoleOption, format: "unitSeconds"); OptionId++;
        VultureDeadBodyMaxCount = CustomOption.Create(OptionId, false, CustomOptionType.Neutral, "VultureDeadBodyCountSetting", 3f, 1f, 6f, 1f, RoleOption); OptionId++;
        VultureShowArrows = CustomOption.Create(OptionId, false, CustomOptionType.Neutral, "VultureShowArrowsSetting", false, RoleOption); OptionId++;
    }


    public static Dictionary<DeadBody, Arrow> DeadPlayerArrows;
    public static void Clear()
    {
        players = new();
        DeadPlayerArrows = new();
    }

    public static void RpcCleanDeadBody(int? count)
    {
        foreach (Collider2D collider2D in Physics2D.OverlapCircleAll(PlayerControl.LocalPlayer.GetTruePosition(), PlayerControl.LocalPlayer.MaxReportDistance, Constants.PlayersOnlyMask))
        {
            if (collider2D.tag != "DeadBody") continue;

            DeadBody component = collider2D.GetComponent<DeadBody>();
            if (component && !component.Reported)
            {
                Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
                Vector2 truePosition2 = component.TruePosition;
                if (Vector2.Distance(truePosition2, truePosition) <= PlayerControl.LocalPlayer.MaxReportDistance
                    && PlayerControl.LocalPlayer.CanMove
                    && !PhysicsHelpers.AnythingBetween(truePosition, truePosition2, Constants.ShipAndObjectsMask, false))
                {
                    GameData.PlayerInfo playerInfo = GameData.Instance.GetPlayerById(component.ParentId);

                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CleanBody, SendOption.Reliable, -1);
                    writer.Write(playerInfo.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.CleanBody(playerInfo.PlayerId);
                    if (count != null)
                    {
                        count--;
                        Logger.Info($"DeadBodyCount:{count}", "Vulture");
                    }
                    break;
                }
            }
        }
    }

    public static void Arrows()
    {
        if (PlayerControl.LocalPlayer.IsAlive())
        {
            foreach (var arrow in DeadPlayerArrows)
            {
                bool isTarget = false;
                foreach (DeadBody dead in Object.FindObjectsOfType<DeadBody>())
                {
                    if (arrow.Key.ParentId != dead.ParentId) continue;
                    isTarget = true;
                    break;
                }
                if (isTarget)
                {
                    if (arrow.Value == null) DeadPlayerArrows[arrow.Key] = new(color);
                    arrow.Value.Update(arrow.Key.transform.position, color: color);
                    arrow.Value.arrow.SetActive(true);
                }
                else
                {
                    if (arrow.Value?.arrow != null)
                        Object.Destroy(arrow.Value.arrow);
                    DeadPlayerArrows.Remove(arrow.Key);
                }
            }
            foreach (DeadBody dead in Object.FindObjectsOfType<DeadBody>())
            {
                if (DeadPlayerArrows.Any(x => x.Key.ParentId == dead.ParentId)) continue;
                DeadPlayerArrows.Add(dead, new(color));
                DeadPlayerArrows[dead].Update(dead.transform.position, color: color);
                DeadPlayerArrows[dead].arrow.SetActive(true);
            }
        }
        else
        {
            foreach (var arrow in DeadPlayerArrows)
            {
                if (arrow.Value?.arrow != null)
                    Object.Destroy(arrow.Value.arrow);
                DeadPlayerArrows.Remove(arrow.Key);
            }
        }
    }
}