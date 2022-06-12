using HarmonyLib;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.Mode;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Roles
{
    public class Scientist
    {
        public static void EndMeeting()
        {
            ResetScientist();
            ScientistEnd();
            ResetCoolDown();
        }
        public static void ResetCoolDown()
        {
            float CoolTime;
            if (PlayerControl.LocalPlayer.isImpostor())
            {
                CoolTime = RoleClass.EvilScientist.CoolTime;
            } else
            {
                CoolTime = RoleClass.NiceScientist.CoolTime;
            }
            HudManagerStartPatch.ScientistButton.MaxTimer = CoolTime;
            RoleClass.NiceScientist.ButtonTimer = DateTime.Now;
        }
        public static void Start()
        {
            RoleClass.NiceScientist.IsScientist = true;
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.SetScientistRPC, Hazel.SendOption.Reliable, -1);
            writer.Write(true);
            writer.Write(CachedPlayer.LocalPlayer.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            CustomRPC.RPCProcedure.SetScientistRPC(true, CachedPlayer.LocalPlayer.PlayerId);
            SpeedBooster.ResetCoolDown();
        }
        public static void ResetScientist()
        {
        }
        public static void ScientistEnd()
        {
            RoleClass.NiceScientist.IsScientist = false;
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.SetScientistRPC, Hazel.SendOption.Reliable, -1);
            writer.Write(false);
            writer.Write(CachedPlayer.LocalPlayer.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            CustomRPC.RPCProcedure.SetScientistRPC(false, CachedPlayer.LocalPlayer.PlayerId);
            ResetScientist();
        }
        public static void setOpacity(PlayerControl player, float opacity,bool cansee)
        {
            // Sometimes it just doesn't work?
            var color = Color.Lerp(Palette.ClearWhite, Palette.White, opacity);
            try
            {
                if (player.MyPhysics?.rend != null)
                    player.MyPhysics.rend.color = color;

                if (player.MyPhysics?.Skin?.layer != null)
                    player.MyPhysics.Skin.layer.color = color;

                if (player.HatRenderer != null)
                    player.HatRenderer.color = color;

                if (player.CurrentPet?.rend != null)
                    player.CurrentPet.rend.color = color;

                if (player.CurrentPet?.shadowRend != null)
                    player.CurrentPet.shadowRend.color = color;

                if (player.VisorSlot != null)
                    player.VisorSlot.color = color;

                if (player.nameText != null)
                    if (opacity == 0.1f)
                    {
                        player.nameText.text = "";
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
                if (!ModeHandler.isMode(ModeId.Default)) return;
                if (__instance.myPlayer.isRole(CustomRPC.RoleId.EvilScientist) || __instance.myPlayer.isRole(CustomRPC.RoleId.NiceScientist))
                {
                    var Scientist = __instance.myPlayer;
                    if (Scientist == null || Scientist.isDead()) return;
                    var ison = RoleClass.NiceScientist.IsScientistPlayers.ContainsKey(__instance.myPlayer.PlayerId) && GameData.Instance && RoleClass.NiceScientist.IsScientistPlayers[__instance.myPlayer.PlayerId];
                    bool canSee =
                        (__instance.myPlayer.isImpostor() && PlayerControl.LocalPlayer.isImpostor()) ||
                        PlayerControl.LocalPlayer.isDead() || !ison;

                    var opacity = canSee ? 0.1f : 0.0f;
                    if (ison)
                    {
                        opacity = Math.Max(opacity, 0);
                        Scientist.MyRend.material.SetFloat("_Outline", 0f);
                    }
                    else
                    {
                        opacity = Math.Max(opacity, 1.5f);
                    }
                    setOpacity(Scientist, opacity,canSee);
                }
            }
        }
    }
}
