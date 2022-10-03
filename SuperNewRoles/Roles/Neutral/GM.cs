using System;
using System.Collections.Generic;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.Helpers;
using SuperNewRoles.MapOptions;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral
{
    public static class GM
    {
        private static List<CustomButton> gmButtons;
        private static List<CustomButton> gmKillButtons;
        private static CustomButton gmZoomIn;
        private static CustomButton gmZoomOut;
        public static PlayerControl target;
        public static Dictionary<string, Action> ActionDatas = new()
        {
            {
                "GMTeleport",
                () =>
                {
                    if (RoleClass.GM.gm.transform.position != target.transform.position)
                    {
                        RoleClass.GM.gm.transform.position = target.transform.position;
                    }
                    Minigame.Instance.Close();
                }
            },
            {
                "GMKill",
                () =>
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.RPCMurderPlayer, SendOption.Reliable, -1);
            writer.Write(CachedPlayer.LocalPlayer.PlayerId);
            writer.Write(target.PlayerId);
            writer.Write(0);
            AmongUsClient.Instance.FinishRpcImmediately(writer);
            RPCProcedure.RPCMurderPlayer(CachedPlayer.LocalPlayer.PlayerId, target.PlayerId, 0);
            Minigame.Instance.Close();
        }
            },
            {
                "GMRevive",
                () =>
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ReviveRPC, SendOption.Reliable, -1);
                    writer.Write(target.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.ReviveRPC(target.PlayerId);
                    Minigame.Instance.Close();
                }
            },
            {
                "GMExile",//"追放(死体なしキル)",
                () =>
                {
                    target.RpcExiledUnchecked();
                    Minigame.Instance.Close();
                }
            },
            {
                "GMCleanDeadbody",//"死体削除",
                () =>
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CleanBody, SendOption.Reliable, -1);
                    writer.Write(target.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.CleanBody(target.PlayerId);
                    Minigame.Instance.Close();
                }
            },
            {
                "GMStartMeeting",//"会議開始",
                () =>
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.UncheckedMeeting, SendOption.Reliable, -1);
                    writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.UncheckedMeeting(CachedPlayer.LocalPlayer.PlayerId);
                    Minigame.Instance.Close();
                }
            },
            {
                "GMCleanDeadbodyAndRevive",//"死体を削除して復活",
                () =>
                {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ReviveRPC, SendOption.Reliable, -1);
                    writer.Write(target.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.ReviveRPC(target.PlayerId);
                    writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CleanBody, SendOption.Reliable, -1);
                    writer.Write(target.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.CleanBody(target.PlayerId);
                    Minigame.Instance.Close();
                }
            },
            {
                "GMSpawnDeadBody",//"死体のみ発生",
                () =>
                {
                    bool IsAlive = target.IsAlive();
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.RPCMurderPlayer, SendOption.Reliable, -1);
                    writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                    writer.Write(target.PlayerId);
                    writer.Write(0);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.RPCMurderPlayer(CachedPlayer.LocalPlayer.PlayerId, target.PlayerId, 0);
                    if (IsAlive)
                    {
                        writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ReviveRPC, SendOption.Reliable, -1);
                        writer.Write(target.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.ReviveRPC(target.PlayerId);
                    }
                    Minigame.Instance.Close();
                }
            },
            {
                "GMEndMeeting",//"会議を終了",
                () =>
                {
                    if (MeetingHud.Instance != null)
                    {
                        MeetingHud.Instance.RpcClose();
                    }
                    else
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.UncheckedMeeting, SendOption.Reliable, -1);
                        writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.UncheckedMeeting(CachedPlayer.LocalPlayer.PlayerId);
                        new LateTask(() => MeetingHud.Instance.RpcClose(), 0.25f);
                    }
                    Minigame.Instance.Close();
                }
            }
        };
        public static void AssignGM()
        {
            if (CustomOptions.GMOption.GetBool())
            {
                PlayerControl.LocalPlayer.SetRoleRPC(RoleId.GM);
                PlayerControl.LocalPlayer.RpcExiledUnchecked();
                PlayerControl.LocalPlayer.Data.IsDead = true;
                PlayerControl.LocalPlayer.RpcSetRole(RoleTypes.Crewmate);
            }
        }
        public static void CreateButton(HudManager hm)
        {

            gmButtons = new List<CustomButton>();
            gmKillButtons = new List<CustomButton>();

            Vector3 gmCalcPos(byte index)
            {
                return new Vector3(-0.25f, -0.25f, 1.0f) + Vector3.right * index * 0.55f;
            }

            Action gmKillButtonOnClick(byte index)
            {
                return () =>
                {
                    PlayerControl target = ModHelpers.PlayerById(index);
                    if (!MapOption.playerIcons.ContainsKey(index) || target.Data.Disconnected)
                    {
                        return;
                    }

                    if (!target.Data.IsDead)
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.RPCMurderPlayer, SendOption.Reliable, -1);
                        writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                        writer.Write(index);
                        writer.Write(0);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.RPCMurderPlayer(CachedPlayer.LocalPlayer.PlayerId, index, 0);
                    }
                    else
                    {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ReviveRPC, SendOption.Reliable, -1);
                        writer.Write(index);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        RPCProcedure.ReviveRPC(index);
                    }
                };
            };

            for (byte i = 0; i < 15; i++)
            {
                //TheOtherRolesPlugin.Instance.Log.LogInfo($"Added {i}");
                void Create(byte i)
                {
                    new CustomButton(
                        () =>
                        {
                            Logger.Info("クリック");
                            PlayerControl player = ModHelpers.PlayerById(i);
                            if (player.IsDead())
                            {
                                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ReviveRPC, SendOption.Reliable, -1);
                                writer.Write(i);
                                AmongUsClient.Instance.FinishRpcImmediately(writer);
                                RPCProcedure.ReviveRPC(i);
                            } else
                            {
                                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.RPCMurderPlayer, SendOption.Reliable, -1);
                                writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                                writer.Write(i);
                                writer.Write(0);
                                AmongUsClient.Instance.FinishRpcImmediately(writer);
                                RPCProcedure.RPCMurderPlayer(CachedPlayer.LocalPlayer.PlayerId, i, 0);
                            }
                        },
                        (bool isAlive, RoleId role) => { return role == RoleId.GM && MapOption.playerIcons.ContainsKey(i) && !ModHelpers.PlayerById(i).Data.Disconnected; },
                        () =>
                        {
                            return true;
                        },
                        () =>
                        {
                        },
                        hm.KillButton.graphic.sprite,
                        new Vector3(0, 1, 0),
                        hm,
                        hm.KillButton,
                        KeyCode.Q,
                        8,
                        () =>
                        {
                            return true;
                        }
                    )
                    {
                        buttonText = ModTranslation.GetString("FinalStatusKill"),
                        showButtonText = true,
                        MaxTimer = 0,
                        Timer = 0
                    };
                    /*

                    var buttonPos = gmKillButton.actionButton.buttonLabelText.transform.localPosition;
                    gmKillButton.actionButton.buttonLabelText.transform.localPosition = new Vector3(buttonPos.x, buttonPos.y + 0.6f, -500f);
                    gmKillButton.actionButton.buttonLabelText.transform.localScale = new Vector3(1.5f, 1.8f, 1.0f);
                    */
                }
                //Create(i); 
            }

            gmZoomOut = new CustomButton(
                () =>
                {

                    if (Camera.main.orthographicSize < 18.0f)
                    {
                        Camera.main.orthographicSize *= 1.5f;
                        hm.UICamera.orthographicSize *= 1.5f;
                    }

                    if (hm.transform.localScale.x < 6.0f)
                    {
                        hm.transform.localScale *= 1.5f;
                    }

                    /*TheOtherRolesPlugin.Instance.Log.LogInfo($"Camera zoom {Camera.main.orthographicSize} / {TaskPanelBehaviour.Instance.transform.localPosition.x}");*/
                },
                (bool IsAlive, RoleId role) => { return role == RoleId.GM; },
                () => { return true; },
                () => { },
                ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.GMZoomOut.png", 115f),
                // position
                Vector3.zero + Vector3.up * 3.75f + Vector3.right * 0.55f,
                // hudmanager
                hm,
                hm.UseButton,
                // keyboard shortcut
                KeyCode.PageDown,
                0,
                () => false
            )
            {
                Timer = 0.0f,
                MaxTimer = 0.0f,
                showButtonText = false,
                LocalScale = Vector3.one * 0.275f
            };

            gmZoomIn = new CustomButton(
                () =>
                {

                    if (Camera.main.orthographicSize > 3.0f)
                    {
                        Camera.main.orthographicSize /= 1.5f;
                        hm.UICamera.orthographicSize /= 1.5f;
                    }

                    if (hm.transform.localScale.x > 1.0f)
                    {
                        hm.transform.localScale /= 1.5f;
                    }

                    /*TheOtherRolesPlugin.Instance.Log.LogInfo($"Camera zoom {Camera.main.orthographicSize} / {TaskPanelBehaviour.Instance.transform.localPosition.x}");*/
                },
                (bool IsAlive, RoleId role) => { return role == RoleId.GM; },
                () => { return true; },
                () => { },
                ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.GMZoomIn.png", 115f),
                // position
                Vector3.zero + Vector3.up * 3.75f + Vector3.right * 0.2f,
                // hudmanager
                hm,
                hm.UseButton,
                // keyboard shortcut
                KeyCode.PageUp,
                0,
                () => false
            )
            {
                Timer = 0.0f,
                MaxTimer = 0.0f,
                showButtonText = false,
                LocalScale = Vector3.one * 0.275f
            };
        }
    }
}