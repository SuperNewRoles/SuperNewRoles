using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperNewRoles.Patch;
using UnityEngine;
using TMPro;
using SuperNewRoles.EndGame;
using Hazel;
using SuperNewRoles.Helpers;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomObject;

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
        public static void PsychometristFixedUpdate()
        {
            if (RoleClass.Psychometrist.FootprintsPosition.Count != 0)
            {
                RoleClass.Psychometrist.UpdateTime -= Time.fixedDeltaTime;
                foreach (var data in RoleClass.Psychometrist.FootprintsPosition.ToArray())
                {
                    if (data.Value.Item2)
                    {
                        if (RoleClass.Psychometrist.UpdateTime <= 0)
                        {
                            RoleClass.Psychometrist.UpdateTime = 0.1f;
                            RoleClass.Psychometrist.FootprintsPosition[(data.Key.Item1, data.Key.Item2)].Item1.Add(ModHelpers.PlayerById(data.Key.Item1).GetTruePosition());
                        }
                        RoleClass.Psychometrist.FootprintsDeathTime[(data.Key.Item1, data.Key.Item2)] -= Time.fixedDeltaTime;
                        Logger.Info($"{RoleClass.Psychometrist.FootprintsDeathTime[(data.Key.Item1, data.Key.Item2)]}");
                        if (RoleClass.Psychometrist.FootprintsDeathTime[(data.Key.Item1, data.Key.Item2)] <= 0)
                        {
                            RoleClass.Psychometrist.FootprintsPosition[(data.Key.Item1, data.Key.Item2)] = (RoleClass.Psychometrist.FootprintsPosition[(data.Key.Item1, data.Key.Item2)].Item1, false);
                        }
                    }
                }
            }
        }
        public static void MurderPlayer(PlayerControl source, PlayerControl target)
        {
            if (RoleClass.Psychometrist.IsCheckFootprints)
            {
                RoleClass.Psychometrist.FootprintsPosition[(source.PlayerId, target.PlayerId)] = (new(), true);
                RoleClass.Psychometrist.FootprintObjects[(source.PlayerId, target.PlayerId)] = new();
                RoleClass.Psychometrist.FootprintsDeathTime[(source.PlayerId, target.PlayerId)] = RoleClass.Psychometrist.CanCheckFootprintsTime;
            }
        }
        public static void ClickButton()
        {
            DeadBody targetbody = RoleClass.Psychometrist.CurrentTarget;
            if (targetbody == null || !PlayerControl.LocalPlayer.CanMove) return;
            DeadPlayer deadPlayer = DeadPlayer.deadPlayers?.Where(x => x.player?.PlayerId == targetbody.ParentId)?.FirstOrDefault();
            TextMeshPro DeathTimeText = GameObject.Instantiate(PlayerControl.LocalPlayer.NameText(), targetbody.transform);
            int count = UnityEngine.Random.Range(RoleClass.Psychometrist.DeathTimeDeviation * -1, RoleClass.Psychometrist.DeathTimeDeviation);
            RoleClass.Psychometrist.DeathTimeTexts.Add((targetbody, DeathTimeText, count));
            DeathTimeText.transform.localPosition = new(-0.2f, 0.5f, 0);
            DeathTimeText.transform.localScale = new(1.5f, 1.5f, 1.5f);
            DeathTimeText.color = Color.white;
            if (!RoleClass.Psychometrist.IsReportCheckedReportDeadbody)
            {
                MessageWriter writer = RPCHelper.StartRPC(CustomRPC.CustomRPC.BlockReportDeadBody);
                writer.Write(targetbody.ParentId);
                writer.EndRPC();
                CustomRPC.RPCProcedure.BlockReportDeadBody(targetbody.ParentId, false);
            }
            if (RoleClass.Psychometrist.IsCheckFootprints)
            {
                var index = (deadPlayer.killerIfExisting.PlayerId, deadPlayer.player.PlayerId);
                var Lists = RoleClass.Psychometrist.FootprintsPosition[index].Item1;
                Color color = Palette.PlayerColors[deadPlayer.killerIfExisting.Data.DefaultOutfit.ColorId];
                foreach (var data in Lists)
                {
                    RoleClass.Psychometrist.FootprintObjects[index].Add(new Footprint(-1, true, data));
                }
            }
            FixedUpdate();
            HudManagerStartPatch.PsychometristButton.MaxTimer = RoleClass.Psychometrist.CoolTime;
            HudManagerStartPatch.PsychometristButton.Timer = HudManagerStartPatch.PsychometristButton.MaxTimer;
        }
    }
}