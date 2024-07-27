using TMPro;

namespace SuperNewRoles.Modules;

//守護天使メッセージの改変
class ProctedMessager
{
    private static string ProctedMessages;  //守護メッセージたち

    public static void StartMeeting(MeetingIntroAnimation __instance)
    {
        __instance.ProtectedRecently.SetActive(false);
        SoundManager.Instance.StopSound(__instance.ProtectedRecentlySound);
        //このターンで誰か守った？
        bool AnythingPlayerProcted = false;
        foreach (PlayerControl player in CachedPlayer.AllPlayers.AsSpan())
        {
            if (player.protectedByGuardianThisRound)
            {
                player.protectedByGuardianThisRound = false;
                if (player.Data != null && player.IsAlive())
                {
                    AnythingPlayerProcted = true;
                }
            }
        }

        //誰か守ってたら音声あり
        if (AnythingPlayerProcted || ProctedMessages != "")
        {
            __instance.ProtectedRecently.SetActive(true);
            SoundManager.Instance.PlaySound(__instance.ProtectedRecentlySound, false, 1f);
        }
        else
        {
            __instance.ProtectedRecently.SetActive(false);
            SoundManager.Instance.StopSound(__instance.ProtectedRecentlySound);
        }

        TMP_Text Text = __instance.ProtectedRecently.GetComponentInChildren<TMP_SubMesh>().textComponent;
        Text.text = ProctedMessages;

        Init();
    }

    //初期化
    public static void Init()
    {
        ProctedMessages = "";
    }

    //スケジュール
    public static void ScheduleProctedMessage(string Text)
    {
        SuperNewRolesPlugin.Logger.LogDebug("守護メッセージがスケジュールされました。:" + Text);
        //もしProctedMessagesが空なら行替えなしに、空じゃなきゃ行替えありに
        ProctedMessages = ProctedMessages == "" ? Text : string.Concat(ProctedMessages, "\n", Text);
    }
}