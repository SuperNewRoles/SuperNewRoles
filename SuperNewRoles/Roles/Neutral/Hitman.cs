using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine;
using SuperNewRoles.CustomRPC;

namespace SuperNewRoles.Roles.Neutral
{
    public static class Hitman
    {
        //ここにコードを書きこんでください
        public static void EndMeeting()
        {
            Buttons.HudManagerStartPatch.HitmanKillButton.MaxTimer = RoleClass.Hitman.KillCoolTime;
            Buttons.HudManagerStartPatch.HitmanKillButton.Timer = Buttons.HudManagerStartPatch.HitmanKillButton.MaxTimer;
        }
        public static void FixedUpdate()
        {
            if (RoleClass.IsMeeting) return;
            RoleClass.Hitman.UpdateTime -= Time.fixedDeltaTime;
            if (RoleClass.Hitman.UpdateTime <= 0)
            {
                SetTarget();
                LimitDown();
                RoleClass.Hitman.UpdateTime = RoleClass.Hitman.ChangeTargetTime;
            }
            if (PlayerControl.LocalPlayer.IsDead())
            {
                if (RoleClass.Hitman.cooldownText != null)
                {
                    UnityEngine.Object.Destroy(RoleClass.Hitman.cooldownText.gameObject);
                    RoleClass.Hitman.cooldownText = null;
                }
            } else
            {
                if (RoleClass.Hitman.cooldownText != null)
                {
                    RoleClass.Hitman.cooldownText.text = Mathf.CeilToInt(Mathf.Clamp(RoleClass.Hitman.UpdateTime, 0, RoleClass.Hitman.ChangeTargetTime)).ToString();
                }
                if (RoleClass.Hitman.Target != null)
                {
                    foreach (var icondata in MapOptions.MapOption.playerIcons)
                    {
                        if (icondata.Key == RoleClass.Hitman.Target.PlayerId)
                        {
                            icondata.Value.gameObject.SetActive(true);
                        }
                        else
                        {
                            icondata.Value.gameObject.SetActive(false);
                        }
                    }
                }
            }
        }
        public static void WrapUp()
        {
            if (!PlayerControl.LocalPlayer.IsRole(RoleId.Hitman)) return;
            SetTarget();
            RoleClass.Hitman.UpdateTime = RoleClass.Hitman.ChangeTargetTime;
        }
        public static void SetTarget()
        {
            List<PlayerControl> targets = PlayerControl.AllPlayerControls.ToArray().ToList();
            targets.RemoveAll(player => {
                return player.IsDead() || player.PlayerId == CachedPlayer.LocalPlayer.PlayerId;
            });
            if (targets.Count > 0)
            {
                RoleClass.Hitman.Target = ModHelpers.GetRandom(targets);
            }
        }
        public static void LimitDown()
        {
            if (RoleClass.Hitman.OutMissionLimit == -1) return;
            RoleClass.Hitman.OutMissionLimit--;
            if (RoleClass.Hitman.OutMissionLimit <= 0)
            {
                PlayerControl.LocalPlayer.RpcMurderPlayer(PlayerControl.LocalPlayer);
            }
        }
    }
}