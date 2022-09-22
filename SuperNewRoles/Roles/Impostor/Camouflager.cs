using System;
using System.Collections.Generic;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using UnityEngine;
using static SuperNewRoles.Buttons.HudManagerStartPatch;
using static SuperNewRoles.Mode.ModeHandler;

namespace SuperNewRoles.Roles.Impostor
{
    public class Camouflager
    {
        public static string[] ColorOption1 =
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
        public static string[] ColorOption2 =
        {
            "colorRed",   //赤
            "colorBlue",  //青
            "colorGreen", //緑
            "colorPink",  //ピンク
            "colorOrange",//オレンジ
            "colorYellow",//黄
            "colorBlack", //黒
            "colorWhite", //白
            "colorPurple",//紫
            "colorBrown", //茶
            "colorCyan",  //水
            "colorLime",  //黄緑
            "colorMaroon",//マルーン
            "colorRose",  //ローズ
            "colorBanana",//バナナ
            "colorGray",  //グレー
            "colorTan",   //タン
            "colorCoral"  //コーラル
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
        public static void Camouflage()
        {
            //全プレイヤーのスキンを保存する部分
            Attire = new();
            foreach (CachedPlayer player in CachedPlayer.AllPlayers)
            {
                var data = new AttireData
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

        public static void ResetCamouflage()
        {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                player.RpcSetName(Attire[player.PlayerId].Name);
                player.RpcSetColor(Attire[player.PlayerId].Color);
                player.RpcSetSkin(Attire[player.PlayerId].Skin);
                player.RpcSetHat(Attire[player.PlayerId].Hat);
                player.RpcSetVisor(Attire[player.PlayerId].Visor);
                player.RpcSetPet(Attire[player.PlayerId].Pet);
            }
        }
        public static void ResetCoolTime()
        {
            CamouflagerButton.MaxTimer = RoleClass.Camouflager.CoolTime;
            CamouflagerButton.Timer = RoleClass.Camouflager.CoolTime;
            RoleClass.Camouflager.ButtonTimer = DateTime.Now;
        }
        public static void SHRFixedUpdate()
        {
            if (AmongUsClient.Instance.AmHost)
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