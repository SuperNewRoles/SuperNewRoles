using System;
using System.Collections.Generic;
using System.Text;
using AmongUs.GameOptions;
using SuperNewRoles.Buttons;
using UnityEngine;

namespace SuperNewRoles.Roles.Attribute;

public static class Jumbo
{
    public static void FixedUpdate()
    {
        foreach (PlayerControl p in RoleClass.Jumbo.JumboPlayer.AsSpan())
            if (!RoleClass.Jumbo.BigPlayer.IsCheckListPlayerControl(p))
                RoleClass.Jumbo.BigPlayer.Add(p);
        foreach (PlayerControl p in RoleClass.Jumbo.BigPlayer.AsSpan())
        {
            if (p == null) continue;
            if (!RoleClass.Jumbo.JumboSize.ContainsKey(p.PlayerId)) RoleClass.Jumbo.JumboSize.Add(p.PlayerId, 0f);
            if (!RoleClass.Jumbo.OldPos.ContainsKey(p.PlayerId)) RoleClass.Jumbo.OldPos.Add(p.PlayerId, p.GetTruePosition());
            Logger.Info($"{(CustomOptionHolder.JumboMaxSize.GetFloat() / 10 / RoleClass.Jumbo.JumboSize[p.PlayerId])} : {((CustomOptionHolder.JumboMaxSize.GetFloat() / 10) / RoleClass.Jumbo.JumboSize[p.PlayerId]) >= CustomOptionHolder.JumboWalkSoundSize.GetSelection()} : {RoleClass.Jumbo.OldPos[p.PlayerId] != p.GetTruePosition()} : {RoleClass.Jumbo.OldPos[p.PlayerId]} : {p.GetTruePosition()}");
            if ((CustomOptionHolder.JumboMaxSize.GetFloat() / 10 / RoleClass.Jumbo.JumboSize[p.PlayerId]) >= CustomOptionHolder.JumboWalkSoundSize.GetSelection())
            {
                if (!RoleClass.Jumbo.PlaySound.ContainsKey(p.PlayerId)) RoleClass.Jumbo.PlaySound.Add(p.PlayerId, 0f);
                RoleClass.Jumbo.PlaySound[p.PlayerId] -= Time.deltaTime;
                if (RoleClass.Jumbo.OldPos[p.PlayerId] != p.GetTruePosition())
                {
                    if (RoleClass.Jumbo.PlaySound[p.PlayerId] <= 0f)
                    {
                        float Light = ShipStatus.Instance.CalculateLightRadius(p.Data);
                        if (GameManager.Instance.LogicOptions.currentGameOptions.GameMode == GameModes.HideNSeek) Light = 6f;
                        if (Vector2.Distance(PlayerControl.LocalPlayer.GetTruePosition(), p.GetTruePosition()) <= Light)
                        {
                            Transform AudioObject = p.transform.FindChild("JumboAudio");
                            if (AudioObject == null)
                            {
                                AudioObject = new GameObject("JumboAudio").transform;
                            }
                            AudioObject.transform.parent = p.transform;
                            AudioObject.transform.localPosition = new();
                            AudioSource audio = ModHelpers.PlaySound(AudioObject.transform, ModHelpers.loadAudioClipFromResources("SuperNewRoles.Resources.JumboWalkSound.raw"), false, audioMixer: SoundManager.Instance.SfxChannel);
                            audio.spatialBlend = 1;
                            audio.rolloffMode = AudioRolloffMode.Linear;
                            RoleClass.Jumbo.PlaySound[p.PlayerId] = 0.35f;
                        }
                    }
                }
            }
            RoleClass.Jumbo.OldPos[p.PlayerId] = p.GetTruePosition();
            p.cosmetics.transform.localScale = Vector3.one * ((RoleClass.Jumbo.JumboSize[p.PlayerId] + 1f) * 0.5f);
            p.transform.FindChild("BodyForms").localScale = Vector3.one * (RoleClass.Jumbo.JumboSize[p.PlayerId] + 1f);
            p.transform.FindChild("Animations").localScale = Vector3.one * (RoleClass.Jumbo.JumboSize[p.PlayerId] + 1f);
            if (RoleClass.Jumbo.JumboSize[p.PlayerId] <= CustomOptionHolder.JumboMaxSize.GetFloat() / 10)
            {
                RoleClass.Jumbo.JumboSize[p.PlayerId] += Time.deltaTime * ((CustomOptionHolder.JumboMaxSize.GetFloat() / 10) / CustomOptionHolder.JumboSpeedUpSize.GetFloat());
            }
            else if (!RoleClass.Jumbo.CanKillSeted && p.PlayerId == PlayerControl.LocalPlayer.PlayerId)
            {
                RoleClass.Jumbo.CanKillSeted = true;
                HudManagerStartPatch.JumboKillButton.MaxTimer = GameManager.Instance.LogicOptions.currentGameOptions.GetFloat(FloatOptionNames.KillCooldown);
                HudManagerStartPatch.JumboKillButton.Timer = HudManagerStartPatch.JumboKillButton.MaxTimer;
            }
        }
    }
}