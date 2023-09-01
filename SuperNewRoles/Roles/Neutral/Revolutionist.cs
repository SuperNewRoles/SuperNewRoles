using System.Collections;
using System.Collections.Generic;
using Hazel;
using Il2CppSystem;
using SuperNewRoles.Patches;
using TMPro;
using UnityEngine;
using static SuperNewRoles.Patches.PlayerControlFixedUpdatePatch;

namespace SuperNewRoles.Roles.Neutral;

public static class Revolutionist
{
    static SpriteRenderer tempBodySprite;
    static Sprite[] FSprites;
    static Sprite[] BSprites;
    public static void MeetingInit(MeetingCalledAnimation __instance, GameData.PlayerInfo reporter)
    {
        if (FSprites == null)
        {
            const int max = 45;
            List<Sprite> sprites = new();
            for (int i = 0; i < max; i++)
            {
                string index = i >= 10 ? i.ToString() : $"0{i}";
                sprites.Add(ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.RevoMeetingAnim.Revolutionist_front-anim_" + index + ".png", 115f));
            }
            FSprites = sprites.ToArray();
        }
        if (BSprites == null)
        {
            const int max = 45;
            List<Sprite> sprites = new();
            for (int i = 0; i < max; i++)
            {
                string index = i >= 10 ? i.ToString() : $"0{i}";
                sprites.Add(ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.RevoMeetingAnim.Revolutionist_back-anim_" + index + ".png", 115f));
            }
            BSprites = sprites.ToArray();
        }
    }
    static float timer;
    static int index;
    static SpriteRenderer back;
    public static IEnumerator ShowMeeting(MeetingCalledAnimation __instance, KillOverlay parent)
    {
        tempBodySprite = __instance.playerParts.cosmetics.currentBodySprite.BodySprite;
        back = new GameObject("BackObject").AddComponent<SpriteRenderer>();
        back.transform.parent = tempBodySprite.transform.parent;
        back.gameObject.layer = parent.gameObject.layer;

        timer = 1 / 30f;
        index = 0;
        __instance.gameObject.SetActive(value: true);
        if (Constants.ShouldPlaySfx())
        {
            SoundManager.Instance.PlaySound(__instance.Stinger, loop: false).volume = __instance.StingerVolume;
        }
        AspectPosition playerTransform = __instance.playerParts.GetComponent<AspectPosition>();
        _ = playerTransform.DistanceFromEdge;
        __instance.transform.FindChild("TextBg").gameObject.SetActive(false);
        TextMeshPro text = __instance.transform.GetComponentInChildren<TextMeshPro>();
        text.GetComponent<TextTranslatorTMP>().enabled = false;
        text.transform.localScale = new(0, 0, 0);
        yield return Effects.Lerp(1.5f, (Action<float>)((float t) =>
        {
            text.transform.localPosition = new(0, -2f, -1f);
            text.outlineColor = Color.white;
            text.text = ModTranslation.GetString("RevolutionistMeeting");
            timer -= Time.deltaTime;
            __instance.playerParts.transform.localPosition = new(0, 0, 0);
            __instance.playerParts.transform.localScale = Vector3.one;
            __instance.playerParts.transform.FindChild("beforeVoting_emergencyButton").gameObject.SetActive(false);
            __instance.playerParts.transform.FindChild("Hand").gameObject.SetActive(false);
            back.transform.localPosition = new(0, 0, 0);
            back.transform.localScale = Vector3.one;
            if (timer <= 0)
            {
                timer = 1 / 30f;
                tempBodySprite.sprite = FSprites[index];
                if (text.transform.localScale.x < 2f)
                    text.transform.localScale += new Vector3(0.1f, 0.1f, 0.1f);
                else
                    text.transform.localScale = new(2, 2, 2);
                back.sprite = BSprites[index];
                index++;
                if (FSprites.Length <= index)
                {
                    index = 0;
                }
            }

        }));
        text.transform.localScale = new(2, 2, 2);
        yield return Effects.Lerp(1f, (Action<float>)((float t) => { }));
        __instance.gameObject.SetActive(value: false);
    }

    public static void FixedUpdate()
    {
        if (RoleClass.Revolutionist.CurrentTarget != null)
        {
            SetPlayerOutline(RoleClass.Revolutionist.CurrentTarget, RoleClass.Revolutionist.color);
            if (RoleClass.Revolutionist.CurrentTarget.IsDead() || Vector2.Distance(RoleClass.Revolutionist.CurrentTarget.GetTruePosition(), CachedPlayer.LocalPlayer.PlayerControl.GetTruePosition()) > 1f)
            {
                RoleClass.Revolutionist.CurrentTarget = null;
                Buttons.HudManagerStartPatch.RevolutionistButton.actionButton.cooldownTimerText.color = new(1f, 1f, 1f, 1f);
                Buttons.HudManagerStartPatch.RevolutionistButton.Timer = 0;
                Buttons.HudManagerStartPatch.RevolutionistButton.MaxTimer = RoleClass.Revolutionist.CoolTime;
            }
            else
            {
                if (Buttons.HudManagerStartPatch.RevolutionistButton.Timer <= 0)
                {
                    RoleClass.Revolutionist.RevolutionedPlayerId.Add(RoleClass.Revolutionist.CurrentTarget.PlayerId);
                    RoleClass.Revolutionist.CurrentTarget = null;
                    Buttons.HudManagerStartPatch.RevolutionistButton.actionButton.cooldownTimerText.color = new(1f, 1f, 1f, 1f);
                    Buttons.HudManagerStartPatch.RevolutionistButton.Timer = RoleClass.Revolutionist.CoolTime;
                    Buttons.HudManagerStartPatch.RevolutionistButton.MaxTimer = RoleClass.Revolutionist.CoolTime;
                }
            }
        }
        else
        {
            PlayerControl target = Buttons.HudManagerStartPatch.SetTarget(untarget: RoleClass.Revolutionist.RevolutionedPlayer);
            SetPlayerOutline(target, RoleClass.Revolutionist.color);
            if (Buttons.HudManagerStartPatch.RevolutionistButton.Timer <= 0)
            {
                if (target != null &&
                    Vector2.Distance(target.GetTruePosition(), CachedPlayer.LocalPlayer.PlayerControl.GetTruePosition()) <= 1f
                    )
                {
                    RoleClass.Revolutionist.CurrentTarget = target;
                    Buttons.HudManagerStartPatch.RevolutionistButton.actionButton.cooldownTimerText.color = new Color(0f, 0.8f, 0f, 1f);
                    Buttons.HudManagerStartPatch.RevolutionistButton.Timer = RoleClass.Revolutionist.TouchTime;
                    Buttons.HudManagerStartPatch.RevolutionistButton.MaxTimer = RoleClass.Revolutionist.TouchTime;
                }
            }
        }
        if (!RoleClass.Revolutionist.IsEndMeeting)
        {
            bool IsFlag = true;
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.IsAlive() && CachedPlayer.LocalPlayer.PlayerId != player.PlayerId && !RoleClass.Revolutionist.RevolutionedPlayerId.Contains(player.PlayerId))
                {
                    IsFlag = false;
                }
            }
            if (IsFlag)
            {
                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.StartRevolutionMeeting, SendOption.Reliable, -1);
                writer.Write(CachedPlayer.LocalPlayer.PlayerId);
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                RPCProcedure.StartRevolutionMeeting(CachedPlayer.LocalPlayer.PlayerId);
                RoleClass.Revolutionist.IsEndMeeting = true;
            }
        }
    }
    public static void WrapUp()
    {
        if (RoleClass.Revolutionist.MeetingTrigger != null)
        {
            if (AmongUsClient.Instance.AmHost)
            {
                if (RoleClass.Revolutionist.WinPlayer != null)
                {
                    MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.ShareWinner, SendOption.Reliable, -1);
                    Writer.Write(RoleClass.Revolutionist.WinPlayer.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(Writer);
                    RPCProcedure.ShareWinner(RoleClass.Revolutionist.WinPlayer.PlayerId);
                    MapUtilities.CachedShipStatus.enabled = false;
                    CheckGameEndPatch.CustomEndGame((GameOverReason)CustomGameOverReason.RevolutionistWin, false);
                }
            }
            RoleClass.Revolutionist.MeetingTrigger = null;
        }
    }
}