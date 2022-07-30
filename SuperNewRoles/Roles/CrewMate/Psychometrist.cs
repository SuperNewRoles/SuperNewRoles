using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperNewRoles.Patch;
using UnityEngine;
using TMPro;
using SuperNewRoles.EndGame;

namespace SuperNewRoles.Roles.CrewMate
{
    public static class Psychometrist
    {
        //ここにコードを書きこんでください
        public static void FixedUpdate()
        {
            foreach (var DeathTimeTextData in RoleClass.Psychometrist.DeathTimeTexts)
            {
                string newtext = "";
                if (RoleClass.Psychometrist.IsCheckDeathReason)
                {
                    GameData.PlayerInfo p = ModHelpers.PlayerById(DeathTimeTextData.Item1.ParentId)?.Data;
                    var finalStatus = FinalStatusPatch.FinalStatusData.FinalStatuses[p.PlayerId] =
                        p.Disconnected == true ? FinalStatus.Disconnected :
                        FinalStatusPatch.FinalStatusData.FinalStatuses.ContainsKey(p.PlayerId) ? FinalStatusPatch.FinalStatusData.FinalStatuses[p.PlayerId] :
                        p.IsDead == true ? FinalStatus.Exiled :
                        FinalStatus.Alive;
                    newtext += "死因:" + FinalStatusPatch.GetStatusText(finalStatus);
                }
                if (RoleClass.Psychometrist.IsCheckDeathTime)
                {
                    DeadPlayer deadPlayer = DeadPlayer.deadPlayers?.Where(x => x.player?.PlayerId == DeathTimeTextData.Item1.ParentId)?.FirstOrDefault();
                    int text = (int)(DateTime.UtcNow - deadPlayer.timeOfDeath).TotalSeconds + DeathTimeTextData.Item3;
                    if (text <= 0) text = 0;
                    if (newtext != "") newtext += "\n";
                    newtext += $"{text}秒前";
                    Logger.Info($"{text}");
                }
                DeathTimeTextData.Item2.text = newtext;
            }
        }
        public static void ClickButton()
        {
            DeadBody targetbody = GetTargetDeadBody();
            if (targetbody == null || !PlayerControl.LocalPlayer.CanMove) return;
            TextMeshPro DeathTimeText = GameObject.Instantiate(PlayerControl.LocalPlayer.NameText(), targetbody.transform);
            int count = UnityEngine.Random.Range(RoleClass.Psychometrist.DeathTimeDeviation * -1, RoleClass.Psychometrist.DeathTimeDeviation);
            RoleClass.Psychometrist.DeathTimeTexts.Add((targetbody, DeathTimeText, count));
            DeathTimeText.transform.localPosition = new(-0.2f, 0.5f, 0);
            DeathTimeText.transform.localScale = new(1.5f, 1.5f, 1.5f);
            FixedUpdate();
        }
        public static DeadBody GetTargetDeadBody()
        {
            foreach (Collider2D collider2D in Physics2D.OverlapCircleAll(PlayerControl.LocalPlayer.GetTruePosition(), PlayerControl.LocalPlayer.MaxReportDistance, Constants.PlayersOnlyMask))
            {
                if (collider2D.tag == "DeadBody")
                {
                    DeadBody component = collider2D.GetComponent<DeadBody>();
                    if (component && !component.Reported)
                    {
                        Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
                        Vector2 truePosition2 = component.TruePosition;
                        if (Vector2.Distance(truePosition2, truePosition) <= PlayerControl.LocalPlayer.MaxReportDistance && PlayerControl.LocalPlayer.CanMove && !PhysicsHelpers.AnythingBetween(truePosition, truePosition2, Constants.ShipAndObjectsMask, false))
                        {
                            return component;
                        }
                    }
                }
            }
            return null;
        }
    }
}