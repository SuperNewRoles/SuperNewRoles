
using SuperNewRoles.Helpers;
using SuperNewRoles.Intro;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Mode.SuperHostRoles
{
    class main
    {
        public static void ClearAndReloads()
        {
            RealExiled = null;
            Chat.WinCond = null;
           // FixedUpdate.UpdateTime = new Dictionary<byte, float>();
            EndGame.OnGameEndPatch.EndData = null;
            FixedUpdate.DefaultName = new Dictionary<int, string>();
        }
        public static PlayerControl RealExiled;
        public static void SendAllRoleChat()
        {/*
            if (ModeHandler.isMode(ModeId.SuperHostRoles))
            {
                float Time = 3;
                foreach (PlayerControl p in CachedPlayer.AllPlayers)
                {
                    if (!p.Data.Disconnected && p.PlayerId != 0)
                    {
                        IntroDate RoleIntroDate = IntroDate.GetIntroDate(p.getRole(), p);
                        string Chat = "";
                        if (p.isDead())
                        {
                            Chat = "\n";
                        }
                        SuperNewRolesPlugin.Logger.LogInfo("テスト");
                        string RoleName = ModTranslation.getString(RoleIntroDate.NameKey + "Name");
                        Chat += "あなたの役職は「" + RoleName + "」です！\n";
                        Chat += IntroDate.GetTitle(RoleIntroDate.NameKey, RoleIntroDate.TitleNum) + "\n";
                        Chat += RoleIntroDate.Description + "\n";
                        Chat += "設定:\n" + RoleHelpers.GetOptionsText(RoleIntroDate.RoleId);
                        SuperNewRolesPlugin.Logger.LogInfo("ChatLen:" + (Chat.Length / 100));
                        if ((Chat.Length / 100) + 1 != 1)
                        {
                            for (int i = 0; i < (Chat.Length / 100 + 1); i++)
                            {
                                try
                                {
                                    new LateTask(() => {
                                    ChatSend(p, Chat.Substring(100 * i, (100 * (i + 1)) - 1), Time);
                                }, Time, "ChatSend");
                                }
                                catch
                                {
                                    new LateTask(() => {
                                        ChatSend(p, Chat.Substring((100 * i) - 1), Time);
                                    }, Time, "ChatSend");
                                }
                                Time += 3;
                            }
                        }
                        else
                        {
                            new LateTask(() => {
                                ChatSend(p, Chat, Time);
                            }, Time, "ChatSend");
                        }
                        void ChatSend(PlayerControl Player, string Chat, float Time)
                        {
                            //                        PlayerControl.LocalPlayer.RpcSetNamePrivate();
                            Player.RPCSendChatPrivate(Chat);
                        }
                        Time += 3;
                    }
                }
                IntroDate RoleIntroDate2 = IntroDate.GetIntroDate(PlayerControl.LocalPlayer.getRole(), PlayerControl.LocalPlayer);
                string Chat2 = "";
                string RoleName2 = ModTranslation.getString(RoleIntroDate2.NameKey + "Name");
                Chat2 = Chat2 + "あなたの役職は「" + RoleName2 + "」です！\n";
                Chat2 += IntroDate.GetTitle(RoleIntroDate2.NameKey, RoleIntroDate2.TitleNum) + "\n";
                Chat2 += RoleIntroDate2.Description + "\n";
                Chat2 += "設定:\n" + RoleHelpers.GetOptionsText(RoleIntroDate2.RoleId);
                FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, Chat2);
            }
        }*/
        }
    }
}
