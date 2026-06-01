using HarmonyLib;
using System.Linq;
using TMPro;

namespace SuperNewRoles.Modules;

/// <summary>会議開始時の守護通知欄へ、任意の通知メッセージを積む。</summary>
public static class ProctedMessager
{
    private static string ProctedMessages = string.Empty;

    public static void Init()
    {
        ProctedMessages = string.Empty;
    }

    public static void ScheduleProctedMessage(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        Logger.Info("Procted message scheduled: " + text, "ProctedMessager");
        ProctedMessages = string.IsNullOrEmpty(ProctedMessages)
            ? text
            : string.Concat(ProctedMessages, "\n", text);
    }

    public static void UnscheduleProctedMessage(string text)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(ProctedMessages))
            return;

        var messages = ProctedMessages.Split('\n').ToList();
        if (!messages.Remove(text))
            return;

        ProctedMessages = string.Join("\n", messages);
    }

    public static void StartMeeting(MeetingIntroAnimation instance)
    {
        bool hasMessage = !string.IsNullOrEmpty(ProctedMessages);
        if (!hasMessage)
            return;

        if (instance?.ProtectedRecently == null)
        {
            Init();
            return;
        }

        bool guardianMessageActive = instance.ProtectedRecently.activeSelf;
        instance.ProtectedRecently.SetActive(true);
        if (!guardianMessageActive && instance.ProtectedRecentlySound != null)
        {
            SoundManager.Instance.PlaySound(instance.ProtectedRecentlySound, false, 1f);
        }

        TMP_Text text = instance.ProtectedRecently.GetComponentInChildren<TMP_Text>(true);
        if (text == null)
            text = instance.ProtectedRecently.GetComponentInChildren<TMP_SubMesh>(true)?.textComponent;
        if (text != null)
            text.text = !guardianMessageActive || string.IsNullOrEmpty(text.text)
                ? ProctedMessages
                : string.Concat(text.text, "\n", ProctedMessages);

        Init();
    }
}

[HarmonyPatch(typeof(MeetingIntroAnimation), nameof(MeetingIntroAnimation.Init))]
public static class ProctedMessager_MeetingIntroAnimationPatch
{
    public static void Postfix(MeetingIntroAnimation __instance)
    {
        ProctedMessager.StartMeeting(__instance);
    }
}
