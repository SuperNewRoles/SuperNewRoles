using System;
using System.Collections.Generic;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomObject;
using SuperNewRoles.Helpers;
using SuperNewRoles.Patches;
using UnityEngine;
using static SuperNewRoles.Modules.CustomOptions;

namespace SuperNewRoles.Roles.Impostor
{
    public class Conjurer
    {
        private const int Id = 999;
        public static CustomRoleOption Option;
        public static CustomOption PlayerCount;
        public static CustomOption CoolDown;
        public static CustomOption CanAddLength;
        public static CustomOption CanKillImpostor;
        public static CustomOption ShowFlash;
        public static void SetupCustomOptions()
        {
            Option = new(Id, false, CustomOptionType.Impostor, "ConjurerName", color, 1);
            PlayerCount = CustomOption.Create(Id + 1, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], Option);
            CoolDown = CustomOption.Create(Id + 2, false, CustomOptionType.Impostor, "CoolDown", 10f, 1f, 30f, 0.5f, Option);
            CanAddLength = CustomOption.Create(Id + 3, false, CustomOptionType.Impostor, "CanAddLength", 10f, 0.5f, 40f, 0.5f, Option);
            CanKillImpostor = CustomOption.Create(Id + 4, false, CustomOptionType.Impostor, "CanKillImpostor", false, Option);
            ShowFlash = CustomOption.Create(Id + 5, false, CustomOptionType.Impostor, "ShowFlash", false, Option);
        }

        public static List<PlayerControl> Player;
        public static Color32 color = RoleClass.ImpostorRed;
        public static int Count; // 何回設置したか
        //public static int Round; // 何週目か
        public static Vector2[] Positions;
        public static void ClearAndReload()
        {
            Player = new();
            Count = 0;
            //Round = 0;
            Positions = new Vector2[] { new(), new(), new() };
            Beacon.AllBeacons = new();
        }

        private static Sprite AddbuttonSprite;
        private static Sprite StartbuttonSprite;
        public static Sprite GetBeaconButtonSprite()
        {
            if (AddbuttonSprite) return AddbuttonSprite;
            AddbuttonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.ConjurerBeaconButton.png", 115f);
            return AddbuttonSprite;
        }
        public static Sprite GetStartButtonSprite()
        {
            if (StartbuttonSprite) return StartbuttonSprite;
            StartbuttonSprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.ConjurerStartButton.png", 115f);
            return StartbuttonSprite;
        }

        private static bool CanAddBeacon()
        {
            if (!PlayerControl.LocalPlayer.CanMove) return false;
            if (Count == 0) return true;

            if (Count != 3)
            {
                if (Vector2.Distance(PlayerControl.LocalPlayer.transform.position, Positions[Count - 1]) < CanAddLength.GetFloat())
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// pがpolyから形成された多角形の中にあるか
        /// </summary>
        /// <param name="p">調べたい点</param>
        /// <param name="poly">多角形の頂点</param>
        /// <returns>多角形の中にある</returns>
        static bool PointInPolygon(Vector2 p, Vector2[] poly)
        {
            Vector2 p1, p2;
            bool inside = false;
            Vector2 oldPoint = poly[^1];
            for (int i = 0; i < poly.Length; i++)
            {
                Vector2 newPoint = poly[i];
                if (newPoint.x > oldPoint.x) { p1 = oldPoint; p2 = newPoint; }
                else { p1 = newPoint; p2 = oldPoint; }
                if ((p1.x < p.x) == (p.x <= p2.x) && (p.y - p1.y) * (p2.x - p1.x) < (p2.y - p1.y) * (p.x - p1.x))
                {
                    inside = !inside;
                }
                oldPoint = newPoint;
            }
            return inside;
        }

        public static void AddBeacon(byte[] buff)
        {
            Vector3 position = Vector3.zero;
            position.x = BitConverter.ToSingle(buff, 0 * sizeof(float));
            position.y = BitConverter.ToSingle(buff, 1 * sizeof(float));
            new Beacon(position);
        }

        public static CustomButton BeaconButton;
        public static CustomButton StartButton;
        public static void SetupCustomButtons(HudManager hm)
        {
            BeaconButton = new(
            () =>
            {
                if (CanAddBeacon())
                {
                    Logger.Info($"Now:{Count}", "Conjurer Add");
                    byte[] buff = new byte[sizeof(float) * 2];
                    Buffer.BlockCopy(BitConverter.GetBytes(PlayerControl.LocalPlayer.transform.position.x), 0, buff, 0 * sizeof(float), sizeof(float));
                    Buffer.BlockCopy(BitConverter.GetBytes(PlayerControl.LocalPlayer.transform.position.y), 0, buff, 1 * sizeof(float), sizeof(float));

                    AddBeacon(buff);

                    // PositionsのCount番目に現在の座標を保存する
                    Positions[Count] = PlayerControl.LocalPlayer.transform.position;

                    Count++;
                    Logger.Info($"Now:{Count}", "Conjurer Added");

                    ResetCoolDown();
                }
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Conjurer; },
            () => { return CanAddBeacon(); },
            () => { ResetCoolDown(); },
            GetBeaconButtonSprite(),
            new Vector3(0, 1, 0),
            hm,
            hm.AbilityButton,
            KeyCode.Q,
            8,
            () => { return false; }
            )
            {
                buttonText = ModTranslation.GetString("ConjurerBeaconName"),
                showButtonText = true
            };

            StartButton = new(
            () =>
            {
                if (PlayerControl.LocalPlayer.CanMove && Count == 3)
                {
                    //Logger.Info($"Beacon{Round}{Count}", "Beacons");
                    foreach (PlayerControl pc in CachedPlayer.AllPlayers)
                    {
                        // プレイヤーがPositionsで形成された三角形の中にいる
                        if (PointInPolygon(pc.transform.position, Positions))
                        {
                            if (pc.IsAlive())
                            {
                                if ((!CanKillImpostor.GetBool() && !pc.IsImpostor()) || CanKillImpostor.GetBool())
                                {
                                    if (pc.IsRole(RoleId.Shielder) && RoleClass.Shielder.IsShield.ContainsKey(pc.PlayerId) && RoleClass.Shielder.IsShield[pc.PlayerId])
                                    {
                                        MessageWriter msgwriter = RPCHelper.StartRPC(CustomRPC.ShielderProtect);
                                        msgwriter.Write(CachedPlayer.LocalPlayer.PlayerId);
                                        msgwriter.Write(pc.PlayerId);
                                        msgwriter.Write(0);
                                        msgwriter.EndRPC();
                                        RPCProcedure.ShielderProtect(CachedPlayer.LocalPlayer.PlayerId, pc.PlayerId, 0);
                                        RoleClass.WaveCannon.CannotMurderPlayers.Add(pc.PlayerId);
                                    }
                                    else
                                    {
                                        pc.RpcMurderPlayer(pc);
                                    }
                                }
                            }
                        }
                    }
                    if (ShowFlash.GetBool())
                    { // Rpcにして、全視点光らせる
                        RPCHelper.StartRPC(CustomRPC.ShowFlash).EndRPC();
                        RPCProcedure.ShowFlash();
                    }
                    Beacon.ClearBeacons();
                    ResetCoolDown();
                    Count = 0;
                    //Round++; // 何週目かを増やす
                    //Logger.Info($"Beacon{Round}{Count}", "Beacons");
                }
            },
            (bool isAlive, RoleId role) => { return isAlive && role == RoleId.Conjurer; },
            () => { return PlayerControl.LocalPlayer.CanMove && Count == 3; },
            () =>
            {
                ResetCoolDown();
                ResetStartCoolDown();
            },
            GetStartButtonSprite(),
            new Vector3(-1.8f, -0.06f, 0),
            hm,
            hm.AbilityButton,
            KeyCode.F,
            48,
            () => { return false; }
            )
            {
                buttonText = ModTranslation.GetString("ConjurerAddName"),
                showButtonText = true
            };
        }

        public static void ResetCoolDown()
        {
            BeaconButton.MaxTimer = CoolDown.GetFloat();
            BeaconButton.Timer = CoolDown.GetFloat();
        }

        public static void ResetStartCoolDown()
        {
            StartButton.MaxTimer = 0;
            StartButton.Timer = 0;
        }
    }
}