using System;
using System.Collections.Generic;
using Hazel;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using UnityEngine;
using static GameData;
using static SuperNewRoles.Buttons.HudManagerStartPatch;
using static SuperNewRoles.Mode.ModeHandler;

namespace SuperNewRoles.Roles.Impostor
{
    public class Camouflager
    {
        public static string[] ColorOption =
        {
            "<color=#C61111>■</color>",//赤
            "<color=#132ED2>■</color>",//青
            "<color=#11802D>■</color>",//緑
            "<color=#EE54BB>■</color>",//ピンク
            "<color=#F07D0D>■</color>",//オレンジ
            "<color=#F6F657>■</color>",//黄
            "<color=#3F474E>■</color>",//黒
            "<color=#D7E1F1>■</color>",//白
            "<color=#6B2FBC>■</color>",//紫
            "<color=#71491E>■</color>",//茶
            "<color=#38FFDD>■</color>",//水
            "<color=#50F039>■</color>",//黄緑
            "<color=#5F1D2E>■</color>",//マルーン
            "<color=#ECC0D3>■</color>",//ローズ
            "<color=#F0E7A8>■</color>",//バナナ
            "<color=#758593>■</color>",//グレー
            "<color=#918877>■</color>",//タン
            "<color=#D76464>■</color>" //コーラル
        };
        public class AttireData
        {
            public string Name { get; set; }
            public byte Color { get; set; }
            public string Skin { get; set; }
            public string Hat { get; set; }
            public string Visor { get; set;}
            public string Pet { get; set; }
        }
        public static Dictionary<byte, AttireData> Attire;
        public static void RpcResetCamouflage()
        {
            MessageWriter writer = RPCHelper.StartRPC(CustomRPC.Camouflage);
            writer.Write(false);
            writer.EndRPC();
            RPCProcedure.Camouflage(false);
        }

        public static void RpcCamouflage()
        {
            if (IsMode(ModeId.SuperHostRoles) && AmongUsClient.Instance.AmHost)
            {
                if (AmongUsClient.Instance.AmHost)
                {
                    CamouflageSHR();
                }
            }
            else
            {
                MessageWriter writer = RPCHelper.StartRPC(CustomRPC.Camouflage);
                writer.Write(true);
                writer.EndRPC();
                RPCProcedure.Camouflage(true);
            }
        }
        public static void Camouflage()
        {
            RoleClass.Camouflager.IsCamouflage = true;
            PlayerOutfit outfit = new()
            {
                PlayerName = "　",
                ColorId = RoleClass.Camouflager.Color,
                SkinId = "",
                HatId = "",
                VisorId = "",
                PetId = "",
            };
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (p == null) continue;
                if (p.Data.Disconnected) continue;
                p.setOutfit(outfit, true);
            }
        }

        public static void ResetCamouflage()
        {
            RoleClass.Camouflager.IsCamouflage = false;
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (p == null) continue;
                if (p.Data.Disconnected) continue;
                p.resetChange();
            }
        }

        public static void CamouflageSHR()
        {
            //全プレイヤーのスキンを保存する部分
            RoleClass.Camouflager.ButtonTimer = DateTime.Now;
            RoleClass.Camouflager.IsCamouflage = true;
            Attire = new();
            foreach (CachedPlayer player in CachedPlayer.AllPlayers)
            {
                AttireData data = new()
                {
                    Name = player.Data.DefaultOutfit.PlayerName,
                    Color = (byte)player.Data.DefaultOutfit.ColorId,
                    Skin = player.Data.DefaultOutfit.SkinId,
                    Hat = player.Data.DefaultOutfit.HatId,
                    Visor = player.Data.DefaultOutfit.VisorId,
                    Pet = player.Data.DefaultOutfit.PetId
                };

                Attire.Add(player.PlayerId, data);
            }
            //全プレイヤーのスキンを変更する部分
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {

                player.RpcSetName("<color=#00000000>" + player.GetDefaultName());
                if (player.IsMod()) player.RpcSetNamePrivate("", player);
                player.RpcSetColor(RoleClass.Camouflager.Color);//カラーを変更
                player.RpcSetSkin(""); //スキンを変更
                player.RpcSetHat("");  //ハットを変更
                player.RpcSetVisor("");//バイザーを変更
                player.RpcSetPet("");  //ペットを変更
                if (IsMode(ModeId.SuperHostRoles))
                {
                    new LateTask(() =>
                    {
                        Mode.SuperHostRoles.FixedUpdate.SetRoleName(player);
                    }, 0.25f);
                }
            }
        }

        public static void ResetCamouflageSHR(PlayerControl target)
        {
            if (Attire.ContainsKey(target.PlayerId))
            {
                target.RpcSetName(Attire[target.PlayerId].Name);
                target.RpcSetColor(Attire[target.PlayerId].Color);
                target.RpcSetSkin(Attire[target.PlayerId].Skin);
                target.RpcSetHat(Attire[target.PlayerId].Hat);
                target.RpcSetVisor(Attire[target.PlayerId].Visor);
                target.RpcSetPet(Attire[target.PlayerId].Pet);
            }
        }

        public static void ResetCamouflageSHR()
        {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                ResetCamouflageSHR(player);
            }
            RoleClass.Camouflager.IsCamouflage = false;
        }
        public static void ResetCoolTime()
        {
            CamouflagerButton.MaxTimer = RoleClass.Camouflager.CoolTime;
            CamouflagerButton.Timer = RoleClass.Camouflager.CoolTime;
            RoleClass.Camouflager.ButtonTimer = DateTime.Now;
        }
        public static void SHRFixedUpdate()
        {
            if (AmongUsClient.Instance.AmHost && IsMode(ModeId.SuperHostRoles))
            {
                if (!RoleClass.IsMeeting)
                {
                    if (RoleClass.Camouflager.IsCamouflage)
                    {
                        RoleClass.Camouflager.Duration -= Time.fixedDeltaTime;
                        if (RoleClass.Camouflager.Duration <= 0)
                        {
                            RoleClass.Camouflager.Duration = 0f;
                            RoleClass.Camouflager.IsCamouflage = false;
                            ResetCamouflage();
                            new LateTask(() =>
                            {
                                Mode.SuperHostRoles.FixedUpdate.SetRoleNames();
                            }, 0.25f);
                        }
                    }
                }
                else
                {
                    if (RoleClass.Camouflager.IsCamouflage)
                    {
                        ResetCamouflage();
                        RoleClass.Camouflager.IsCamouflage = false;
                    }
                }
            }
        }
    }
}