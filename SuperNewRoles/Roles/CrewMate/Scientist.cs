using System;
using HarmonyLib;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.Mode;
using UnityEngine;

namespace SuperNewRoles.Roles;

public class Scientist
{
    public static void EndMeeting()
    {
        ScientistEnd();
        ResetCooldown();
    }
    public static void ResetCooldown()
    {
        float CoolTime = PlayerControl.LocalPlayer.IsImpostor() ? RoleClass.EvilScientist.CoolTime : RoleClass.NiceScientist.CoolTime;
        HudManagerStartPatch.ScientistButton.MaxTimer = CoolTime;
        RoleClass.NiceScientist.ButtonTimer = DateTime.Now;
    }
    public static void Start()
    {
        RoleClass.NiceScientist.IsScientist = true;
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetScientistRPC, SendOption.Reliable, -1);
        writer.Write(true);
        writer.Write(CachedPlayer.LocalPlayer.PlayerId);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
        RPCProcedure.SetScientistRPC(true, CachedPlayer.LocalPlayer.PlayerId);
        SpeedBooster.ResetCooldown();
    }
    public static void ScientistEnd()
    {
        RoleClass.NiceScientist.IsScientist = false;
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.SetScientistRPC, SendOption.Reliable, -1);
        writer.Write(false);
        writer.Write(CachedPlayer.LocalPlayer.PlayerId);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
        RPCProcedure.SetScientistRPC(false, CachedPlayer.LocalPlayer.PlayerId);
    }
    public static void SetOpacity(PlayerControl player, float opacity, bool cansee)
    {
        // Sometimes it just doesn't work?
        var color = Color.Lerp(Palette.ClearWhite, Palette.White, opacity);
        try
        {
            if (player.MyRend() != null)
                player.MyRend().color = color;

            if (player.GetSkin().layer != null)
                player.GetSkin().layer.color = color;

            if (player.cosmetics.hat != null)
                player.cosmetics.hat.SpriteColor = color;

            if (player.GetPet() != null)
                player.GetPet().ForEachRenderer(true, (Il2CppSystem.Action<SpriteRenderer>)((render) => render.color = color));

            if (player.VisorSlot() != null)
                player.VisorSlot().Image.color = color;

            if (player.cosmetics.colorBlindText != null)
                player.cosmetics.colorBlindText.color = color;

            if (player.NameText != null)
                if (opacity <= 0.1f)
                {
                    player.NameText().text = "";
                }
        }
        catch { }
    }
    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
    public static class PlayerPhysicsScientist
    {
        public static void Postfix(PlayerPhysics __instance)
        {
            if (AmongUsClient.Instance.GameState != AmongUsClient.GameStates.Started) return;
            if (!ModeHandler.IsMode(ModeId.Default)) return;
            if (__instance.myPlayer.IsRole(RoleId.EvilScientist) || __instance.myPlayer.IsRole(RoleId.NiceScientist))
            {
                var Scientist = __instance.myPlayer;
                if (Scientist == null || Scientist.IsDead()) return;
                var ison = RoleClass.NiceScientist.IsScientistPlayers.ContainsKey(__instance.myPlayer.PlayerId) && GameData.Instance && RoleClass.NiceScientist.IsScientistPlayers[__instance.myPlayer.PlayerId];
                bool canSee =
                    (__instance.myPlayer.IsImpostor() && PlayerControl.LocalPlayer.IsImpostor()) ||
                    PlayerControl.LocalPlayer.IsDead() || !ison;

                var opacity = canSee ? 0.1f : 0.0f;
                if (ison)
                {
                    opacity = Math.Max(opacity, 0);
                    Scientist.MyRend().material.SetFloat("_Outline", 0f);
                }
                else
                {
                    opacity = Math.Max(opacity, 1.5f);
                }
                SetOpacity(Scientist, opacity, canSee);
            }
        }
    }
}