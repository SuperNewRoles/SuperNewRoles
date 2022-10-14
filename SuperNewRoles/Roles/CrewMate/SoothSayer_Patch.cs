using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

using UnityEngine;

namespace SuperNewRoles.Roles
{
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.UpdateButtons))]
    class SoothSayer_updatepatch
    {
        static void Postfix(MeetingHud __instance)
        {
            if (PlayerControl.LocalPlayer.IsDead() && PlayerControl.LocalPlayer.IsRole(RoleId.SoothSayer))
            {
                __instance.playerStates.ToList().ForEach(x => { if (x.transform.FindChild("SoothSayerButton") != null) Object.Destroy(x.transform.FindChild("SoothSayerButton").gameObject); });
            }
        }
    }
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    public static class SoothSayer_Patch
    {
        private static string namedate;
        static void SoothSayerOnClick(int Index, MeetingHud __instance)
        {
            var Target = ModHelpers.PlayerById(__instance.playerStates[Index].TargetPlayerId);
            var introdate = Target.GetRole();
            if (RoleClass.SoothSayer.DisplayMode)
            {
                if (Target.IsImpostor()) namedate = "Impostor";
                if (Target.IsHauntedWolf()) namedate = "Impostor";
                else if (Target.IsNeutral()) namedate = "Neutral";
                else if (Target.IsCrew()) namedate = "CrewMate";
            }
            else
            {
                namedate = IntroDate.GetIntroDate(introdate, Target).NameKey;
            }
            var name = ModTranslation.GetString(namedate + "Name");
            FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, string.Format(ModTranslation.GetString("SoothSayerGetChat"), Target.NameText().text, name));

            RoleClass.SoothSayer.Count--;
            if (!RoleClass.SoothSayer.DisplayedPlayer.Contains(Target.PlayerId))
            {
                RoleClass.SoothSayer.DisplayedPlayer.Add(Target.PlayerId);
                __instance.playerStates.ToList().ForEach(x => { if (x.transform.FindChild("SoothSayerButton") != null && x.TargetPlayerId == Target.PlayerId) UnityEngine.Object.Destroy(x.transform.FindChild("SoothSayerButton").gameObject); });
            }
            if (RoleClass.SoothSayer.Count <= 0)
            {
                __instance.playerStates.ToList().ForEach(x => { if (x.transform.FindChild("SoothSayerButton") != null) UnityEngine.Object.Destroy(x.transform.FindChild("SoothSayerButton").gameObject); });
            }
        }
        static void Event(MeetingHud __instance)
        {
            if (PlayerControl.LocalPlayer.IsRole(RoleId.SoothSayer) && PlayerControl.LocalPlayer.IsAlive() && RoleClass.SoothSayer.Count >= 1)
            {
                for (int i = 0; i < __instance.playerStates.Length; i++)
                {
                    PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                    var player = ModHelpers.PlayerById(__instance.playerStates[i].TargetPlayerId);
                    if (player.IsAlive() && !RoleClass.SoothSayer.DisplayedPlayer.Contains(player.PlayerId) && player.PlayerId != CachedPlayer.LocalPlayer.PlayerId)
                    {
                        GameObject template = playerVoteArea.Buttons.transform.Find("CancelButton").gameObject;
                        GameObject targetBox = Object.Instantiate(template, playerVoteArea.transform);
                        targetBox.name = "SoothSayerButton";
                        targetBox.transform.localPosition = new Vector3(1f, 0.03f, -1f);
                        SpriteRenderer renderer = targetBox.GetComponent<SpriteRenderer>();
                        renderer.sprite = RoleClass.SoothSayer.GetButtonSprite();
                        PassiveButton button = targetBox.GetComponent<PassiveButton>();
                        button.OnClick.RemoveAllListeners();
                        int copiedIndex = i;
                        button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => SoothSayerOnClick(copiedIndex, __instance)));
                    }
                }
            }
        }

        static void SpiritOnClick(int Index, MeetingHud __instance)
        {
            var Target = ModHelpers.PlayerById(__instance.playerStates[Index].TargetPlayerId);
            var introdate = Target.GetRole();
            namedate = IntroDate.GetIntroDate(introdate, Target).NameKey;
            if (RoleClass.SpiritMedium.DisplayMode)
            {
                if (Target.IsImpostor()) namedate = "Impostor";
                if (Target.IsHauntedWolf()) namedate = "Impostor";
                else if (Target.IsNeutral()) namedate = "Neutral";
                else if (Target.IsCrew()) namedate = "CrewMate";
            }
            else
            {
                namedate = IntroDate.GetIntroDate(introdate, Target).NameKey;
            }
            var name = ModTranslation.GetString(namedate + "Name");
            FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, string.Format(ModTranslation.GetString("SoothSayerGetChat"), Target.NameText().text, name));
            RoleClass.SpiritMedium.MaxCount--;
            if (!RoleClass.SoothSayer.DisplayedPlayer.Contains(Target.PlayerId))
            {
                RoleClass.SoothSayer.DisplayedPlayer.Add(Target.PlayerId);
            }
            if (RoleClass.SpiritMedium.MaxCount <= 0)
            {
                __instance.playerStates.ToList().ForEach(x => { if (x.transform.FindChild("SoothSayerButton") != null && x.TargetPlayerId == Target.PlayerId) Object.Destroy(x.transform.FindChild("SoothSayerButton").gameObject); });
                __instance.playerStates.ToList().ForEach(x => { if (x.transform.FindChild("SoothSayerButton") != null) Object.Destroy(x.transform.FindChild("SoothSayerButton").gameObject); });
            }
        }
        static void SpiritEvent(MeetingHud __instance)
        {
            if (CustomOptions.SpiritMediumIsAutoMode.GetBool()) return;
            if (PlayerControl.LocalPlayer.IsRole(RoleId.SpiritMedium) && PlayerControl.LocalPlayer.IsAlive() && RoleClass.SpiritMedium.MaxCount >= 1)
            {
                for (int i = 0; i < __instance.playerStates.Length; i++)
                {
                    PlayerVoteArea playerVoteArea = __instance.playerStates[i];

                    var player = ModHelpers.PlayerById(__instance.playerStates[i].TargetPlayerId);
                    if (!player.Data.Disconnected && player.IsDead() && !RoleClass.SoothSayer.DisplayedPlayer.Contains(player.PlayerId) && player.PlayerId != CachedPlayer.LocalPlayer.PlayerId)
                    {
                        GameObject template = playerVoteArea.Buttons.transform.Find("CancelButton").gameObject;
                        GameObject targetBox = Object.Instantiate(template, playerVoteArea.transform);

                        targetBox.name = "SoothSayerButton";
                        targetBox.transform.localPosition = new Vector3(1f, 0.03f, -1f);
                        SpriteRenderer renderer = targetBox.GetComponent<SpriteRenderer>();
                        renderer.sprite = RoleClass.SoothSayer.GetButtonSprite();
                        PassiveButton button = targetBox.GetComponent<PassiveButton>();
                        button.OnClick.RemoveAllListeners();
                        int copiedIndex = i;
                        button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() => SpiritOnClick(copiedIndex, __instance)));
                    }
                }
            }
        }

        static void Postfix(MeetingHud __instance)
        {
            Event(__instance);
            SpiritEvent(__instance);
            StartMeeting();
        }
        public static void StartMeeting()
        {
            if (PlayerControl.LocalPlayer.IsRole(RoleId.SoothSayer) && RoleClass.SoothSayer.CanFirstWhite)
            {
                RoleClass.SoothSayer.CanFirstWhite = false;
                List<PlayerControl> WhitePlayers = PlayerControl.AllPlayerControls.ToArray().ToList().FindAll(x => x.IsAlive() && x.IsCrew() && x.PlayerId != CachedPlayer.LocalPlayer.PlayerId);
                if (WhitePlayers.Count <= 0) { FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(CachedPlayer.LocalPlayer, ModTranslation.GetString("SoothSayerNoneTarget")); return; }
                PlayerControl Target = WhitePlayers.GetRandom();
                FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(CachedPlayer.LocalPlayer, Target.Data.PlayerName + ModTranslation.GetString("SoothSayerCrewmateText"));
            }
            else if (PlayerControl.LocalPlayer.IsRole(RoleId.SpiritMedium) && CustomOptions.SpiritMediumIsAutoMode.GetBool())
            {
                if (RoleClass.SpiritMedium.ExilePlayer == null) return;
                string CrewText = RoleClass.SpiritMedium.ExilePlayer.IsCrew() ? ModTranslation.GetString("SoothSayerCrewmateText") : ModTranslation.GetString("SoothSayerNotCrewmateText");
                FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(CachedPlayer.LocalPlayer, RoleClass.SpiritMedium.ExilePlayer.Data.PlayerName + CrewText);
                RoleClass.SpiritMedium.ExilePlayer = null;
            }
        }
        public static void WrapUp(PlayerControl exiled)
        {
            RoleClass.SpiritMedium.ExilePlayer = exiled;
        }
    }
}