using System.Collections.Generic;
using System.Linq;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomRPC;
using UnityEngine;
using SuperNewRoles.Helpers;

namespace SuperNewRoles.Roles
{
    class Jackal
    {
        public static void resetCoolDown()
        {
            HudManagerStartPatch.JackalKillButton.MaxTimer = RoleClass.Jackal.KillCoolDown;
            HudManagerStartPatch.JackalKillButton.Timer = RoleClass.Jackal.KillCoolDown;
            HudManagerStartPatch.JackalSidekickButton.MaxTimer = RoleClass.Jackal.KillCoolDown;
            HudManagerStartPatch.JackalSidekickButton.Timer = RoleClass.Jackal.KillCoolDown;
        }
        public static void EndMeeting()
        {
            resetCoolDown();
        }
        public static void CreateFriend()
        {
            var target = Jackal.JackalFixedPatch.JackalsetTarget();
            target.RpcProtectPlayer(target, 0);//ジャッカルフレンズにできたことを示すモーションとしての守護をかける

            //キルする前に守護を発動させるためのLateTask
            new LateTask(() =>
                {
                    PlayerControl.LocalPlayer.RpcMurderPlayer(target);//キルをして守護モーションの発動(守護解除)
                    target.RPCSetRoleUnchecked(RoleTypes.Crewmate);//くるぅにして

                    var RoleName = PlayerControl.LocalPlayer.getRole();//ログに表示する為、此処にたどり着いた役職の名前を取得
                    target.setRoleRPC(RoleId.JackalFriends);//ジャッカルフレンズにする
                    RoleClass.Jackal.IsCreatedFriend = true;//作ったことに
                    SuperNewRolesPlugin.Logger.LogInfo("[CreateFriend_RoleName:" + RoleName + "]フレンズを作ったから普通のキルボタンに戻すよ!");

                }, 0.1f);

        }
        public static void setPlayerOutline(PlayerControl target, Color color)
        {
            if (target == null || target.MyRend() == null) return;

            target.MyRend().material.SetFloat("_Outline", 1f);
            target.MyRend().material.SetColor("_OutlineColor", color);
        }
        public class JackalFixedPatch
        {
            public static PlayerControl JackalsetTarget(bool onlyCrewmates = false, bool targetPlayersInVents = false, List<PlayerControl> untargetablePlayers = null, PlayerControl targetingPlayer = null)
            {
                PlayerControl result = null;
                float num = GameOptionsData.KillDistances[Mathf.Clamp(PlayerControl.GameOptions.KillDistance, 0, 2)];
                if (!MapUtilities.CachedShipStatus) return result;
                if (targetingPlayer == null) targetingPlayer = PlayerControl.LocalPlayer;
                if (targetingPlayer.Data.IsDead || targetingPlayer.inVent) return result;

                if (untargetablePlayers == null)
                {
                    untargetablePlayers = new();
                }

                Vector2 truePosition = targetingPlayer.GetTruePosition();
                Il2CppSystem.Collections.Generic.List<GameData.PlayerInfo> allPlayers = GameData.Instance.AllPlayers;
                for (int i = 0; i < allPlayers.Count; i++)
                {
                    GameData.PlayerInfo playerInfo = allPlayers[i];
                    //下記Jackalがbuttonのターゲットにできない役職の設定
                    if (playerInfo.Object.isAlive() && playerInfo.PlayerId != targetingPlayer.PlayerId && !playerInfo.Object.IsJackalTeamJackal() && !playerInfo.Object.IsJackalTeamSidekick())
                    {
                        PlayerControl @object = playerInfo.Object;
                        if (untargetablePlayers.Any(x => x == @object))
                        {
                            // if that player is not targetable: skip check
                            continue;
                        }

                        if (@object && (!@object.inVent || targetPlayersInVents))
                        {
                            Vector2 vector = @object.GetTruePosition() - truePosition;
                            float magnitude = vector.magnitude;
                            if (magnitude <= num && !PhysicsHelpers.AnyNonTriggersBetween(truePosition, vector.normalized, magnitude, Constants.ShipAndObjectsMask))
                            {
                                result = @object;
                                num = magnitude;
                            }
                        }
                    }
                }
                return result;
            }
            static void JackalPlayerOutLineTarget()
            {
                setPlayerOutline(JackalsetTarget(), RoleClass.Jackal.color);
            }
            public static void Postfix(PlayerControl __instance, RoleId role)
            {
                if (AmongUsClient.Instance.AmHost)
                {
                    if (RoleClass.Jackal.SidekickPlayer.Count > 0)
                    {
                        var upflag = true;
                        foreach (PlayerControl p in RoleClass.Jackal.JackalPlayer)
                        {
                            if (p.isAlive())
                            {
                                upflag = false;
                            }
                        }
                        if (upflag)
                        {
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.SidekickPromotes, Hazel.SendOption.Reliable, -1);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            RPCProcedure.SidekickPromotes();
                        }
                    }
                }
                if (role == RoleId.Jackal)
                {
                    JackalPlayerOutLineTarget();
                }
            }
        }
    }
}
