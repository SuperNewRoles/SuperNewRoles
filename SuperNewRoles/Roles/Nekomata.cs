using System.Collections.Generic;
using Hazel;
using SuperNewRoles.EndGame;
using SuperNewRoles.Mode;

namespace SuperNewRoles.Roles
{
    class Nekomata
    {
        public static void NekomataEnd(GameData.PlayerInfo __instance)
        {
            if (!ModeHandler.isMode(ModeId.Default)) return;
            if (__instance == null) return;
            if (AmongUsClient.Instance.AmHost)
            {
                //もし 追放された役職が猫であるならば
                if ((__instance != null && RoleClass.NiceNekomata.NiceNekomataPlayer.IsCheckListPlayerControl(__instance.Object)) || RoleClass.EvilNekomata.EvilNekomataPlayer.IsCheckListPlayerControl(__instance.Object) || RoleClass.BlackCat.BlackCatPlayer.IsCheckListPlayerControl(__instance.Object))
                {
                    //道連れにするプレイヤーの抽選リストを作成
                    List<PlayerControl> p = new();
                    foreach (PlayerControl p1 in CachedPlayer.AllPlayers)
                    {

                        //もし イビル猫又が追放され尚且つImpostorを道連れしない設定がオンになっているにゃら 又は 黒猫が追放され尚且つImpostorを道連れしない設定がオンなっているにゃら
                        if ((RoleClass.EvilNekomata.EvilNekomataPlayer.IsCheckListPlayerControl(__instance.Object) && RoleClass.EvilNekomata.NotImpostorExiled) || (RoleClass.BlackCat.BlackCatPlayer.IsCheckListPlayerControl(__instance.Object) && RoleClass.BlackCat.NotImpostorExiled))
                        {
                            //もし 抜き出されたプレイヤーが　追放されたプレイヤーではない 且つ 生きている 且つ インポスターでないにゃら
                            if (p1.Data != __instance && p1.isAlive() && !p1.isImpostor())
                            {
                                //道連れにするプレイヤーの抽選リストに追加する
                                p.Add(p1);
                                SuperNewRolesPlugin.Logger.LogInfo("[SNR:黒猫Info]Impostorを道連れ対象から除外しました");
                            }
                        }
                        //それ以外にゃら(ナイス猫又の追放　あるいは　イビル猫又・黒猫の追放でインポスターを道連れにしない設定がオフになっているにゃら)
                        else
                        {
                            //もし 抜き出されたプレイヤーが　追放されたプレイヤーではない 且つ 生きているにゃら
                            if (p1.Data != __instance && p1.isAlive())
                            {
                                //道連れにするプレイヤーの抽選リストに追加する
                                p.Add(p1);
                                SuperNewRolesPlugin.Logger.LogInfo("[SNR:黒猫Info]Impostorを道連れ対象から除外しませんでした");
                            }
                        }
                    }
                    MessageWriter RPCWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.ExiledRPC, Hazel.SendOption.Reliable, -1);
                    RPCWriter.Write(__instance.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(RPCWriter);
                    CustomRPC.RPCProcedure.ExiledRPC(__instance.PlayerId);
                    NekomataProc(p);
                }

            }
        }
        public static void NekomataProc(List<PlayerControl> p)
        {
            var rdm = ModHelpers.GetRandomIndex(p);
            var random = p[rdm];
            SuperNewRolesPlugin.Logger.LogInfo(random.nameText().text);
            if (EvilEraser.IsOKAndTryUse(EvilEraser.BlockTypes.NekomataExiled, random))
            {
                MessageWriter RPCWriter = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.NekomataExiledRPC, Hazel.SendOption.Reliable, -1);
                RPCWriter.Write(random.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(RPCWriter);
                CustomRPC.RPCProcedure.ExiledRPC(random.PlayerId);
                if ((RoleClass.NiceNekomata.NiceNekomataPlayer.IsCheckListPlayerControl(random) || RoleClass.EvilNekomata.EvilNekomataPlayer.IsCheckListPlayerControl(random) || RoleClass.BlackCat.BlackCatPlayer.IsCheckListPlayerControl(random)) && RoleClass.NiceNekomata.IsChain)
                {
                    p.RemoveAt(rdm);
                    NekomataProc(p);
                }
                if (RoleClass.Jester.JesterPlayer.IsCheckListPlayerControl(random))
                {
                    if (!RoleClass.Jester.IsJesterTaskClearWin || (RoleClass.Jester.IsJesterTaskClearWin && Patch.TaskCount.TaskDateNoClearCheck(random.Data).Item2 - Patch.TaskCount.TaskDateNoClearCheck(random.Data).Item1 == 0))
                    {
                        CustomRPC.RPCProcedure.ShareWinner(random.PlayerId);
                        MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.ShareWinner, Hazel.SendOption.Reliable, -1);
                        Writer.Write(random.PlayerId);
                        AmongUsClient.Instance.FinishRpcImmediately(Writer);
                        RoleClass.Jester.IsJesterWin = true;
                        ShipStatus.RpcEndGame((GameOverReason)CustomGameOverReason.JesterWin, false);
                    }
                }
            }
        }
    }
}
