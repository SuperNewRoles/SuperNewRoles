using System.Collections.Generic;
using Hazel;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Patches;

namespace SuperNewRoles.Roles
{
    class Nekomata
    {
        public static void NekomataEnd(GameData.PlayerInfo __instance)
        {
            if (!ModeHandler.IsMode(ModeId.Default)) return;
            if (__instance == null) return;
            if (AmongUsClient.Instance.AmHost)
            {
                //もし 追放された役職が猫なら
                if (__instance.Object.IsRole(RoleId.NiceNekomata) ||
                    __instance.Object.IsRole(RoleId.EvilNekomata) ||
                    __instance.Object.IsRole(RoleId.BlackCat) ||
                    (__instance.Object.IsRole(RoleId.NekoKabocha) && Impostor.NekoKabocha.CanRevengeExile))
                {
                    List<PlayerControl> p = new();//道連れにするプレイヤーの抽選リスト
                    foreach (PlayerControl p1 in CachedPlayer.AllPlayers)
                    {
                        //もし 黒猫・イビル猫又が追放Impostorを道連れしないがオンにゃら
                        if ((__instance.Object.IsRole(RoleId.EvilNekomata) && RoleClass.EvilNekomata.NotImpostorExiled) || (__instance.Object.IsRole(RoleId.BlackCat) && RoleClass.BlackCat.NotImpostorExiled))
                        {
                            //もし 抜き出されたプレイヤーが　追放されたプレイヤーではなく  生きていて  且つ　インポスターでないにゃら
                            if (p1.Data != __instance && p1.IsAlive() && !p1.IsImpostor())
                            {
                                p.Add(p1);//道連れにするプレイヤーの抽選リストに追加する
                                //Logへの記載
                                if (__instance.Object.IsRole(RoleId.BlackCat))
                                    SuperNewRolesPlugin.Logger.LogInfo("[SNR:黒猫Info]Impostorを道連れ対象から除外しました");
                                else if (__instance.Object.IsRole(RoleId.EvilNekomata))
                                    SuperNewRolesPlugin.Logger.LogInfo("[SNR:イビル猫又Info]Impostorを道連れ対象から除外しました");
                                else
                                    SuperNewRolesPlugin.Logger.LogError("[SNR:猫又Error]&[SNR:イビル猫又Error][NotImpostorExiled == true] 異常な抽選リストです");
                            }
                        }
                        // 猫カボチャで。追放道連れにする
                        else if (__instance.Object.IsRole(RoleId.NekoKabocha) && Impostor.NekoKabocha.CanRevengeExile)
                        {
                            if (p1.Data != __instance && p1.IsAlive())
                            {
                                if (p1.IsCrew() && Impostor.NekoKabocha.CanRevengeCrew)
                                {
                                    p.Add(p1);
                                }
                                else if (p1.IsNeutral() && Impostor.NekoKabocha.CanRevengeNeut)
                                {
                                    p.Add(p1);
                                }
                                else if (p1.IsImpostor() && Impostor.NekoKabocha.CanRevengeImp)
                                {
                                    p.Add(p1);
                                }
                            }
                        }
                        //ナイス・設定オフ
                        else
                        {
                            //もし 抜き出されたプレイヤーが　追放されたプレイヤーではない 且つ 生きているにゃら
                            if (p1.Data != __instance && p1.IsAlive())
                            {
                                p.Add(p1);//道連れにするプレイヤーの抽選リストに追加する
                                //Logへの記載
                                if (__instance.Object.IsRole(RoleId.BlackCat))
                                    SuperNewRolesPlugin.Logger.LogInfo("[SNR:黒猫Info]Impostorを道連れ対象から除外しませんでした");
                                else if (__instance.Object.IsRole(RoleId.EvilNekomata))
                                    SuperNewRolesPlugin.Logger.LogInfo("[SNR:イビル猫又Info]Impostorを道連れ対象から除外しませんでした");
                                else
                                    SuperNewRolesPlugin.Logger.LogError("[SNR:猫又Error]&[SNR:イビル猫又Error][NotImpostorExiled != true] 異常な抽選リストです");
                            }
                        }
                    }
                    MessageWriter RPCWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ExiledRPC, SendOption.Reliable, -1);
                    RPCWriter.Write(__instance.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(RPCWriter);
                    RPCProcedure.ExiledRPC(__instance.PlayerId);
                    NekomataProc(p);
                }
            }
        }
        public static void NekomataProc(List<PlayerControl> p)
        {
            var rdm = ModHelpers.GetRandomIndex(p);
            var random = p[rdm];
            SuperNewRolesPlugin.Logger.LogInfo(random.NameText().text);
            if (EvilEraser.IsOKAndTryUse(EvilEraser.BlockTypes.NekomataExiled, random))
            {
                random.RpcExiledUnchecked();
                random.RpcSetFinalStatus(FinalStatus.NekomataExiled);
                if (random.IsRole(RoleId.NiceNekomata) || random.IsRole(RoleId.EvilNekomata) || (random.IsRole(RoleId.BlackCat) && RoleClass.NiceNekomata.IsChain))
                {
                    p.RemoveAt(rdm);
                    NekomataProc(p);
                }
                if (random.IsRole(RoleId.Jester) || random.IsRole(RoleId.MadJester))
                {
                    if (!RoleClass.Jester.IsJesterTaskClearWin || (RoleClass.Jester.IsJesterTaskClearWin && Patches.TaskCount.TaskDateNoClearCheck(random.Data).Item2 - Patches.TaskCount.TaskDateNoClearCheck(random.Data).Item1 == 0))
                    {
                        RPCProcedure.ShareWinner(random.PlayerId);
                        MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareWinner, SendOption.Reliable, -1);
                        Writer.Write(random.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(Writer);
                        RoleClass.Jester.IsJesterWin = true;
                        ShipStatus.RpcEndGame((GameOverReason)CustomGameOverReason.JesterWin, false);
                    }
                    if (!RoleClass.MadJester.IsMadJesterTaskClearWin || (RoleClass.MadJester.IsMadJesterTaskClearWin && Patches.TaskCount.TaskDateNoClearCheck(random.Data).Item2 - Patches.TaskCount.TaskDateNoClearCheck(random.Data).Item1 == 0))
                    {
                        RPCProcedure.ShareWinner(random.PlayerId);
                        MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareWinner, SendOption.Reliable, -1);
                        Writer.Write(random.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(Writer);
                        RoleClass.MadJester.IsMadJesterWin = true;
                        ShipStatus.RpcEndGame((GameOverReason)CustomGameOverReason.MadJesterWin, false);
                    }
                }
            }
        }
    }
}