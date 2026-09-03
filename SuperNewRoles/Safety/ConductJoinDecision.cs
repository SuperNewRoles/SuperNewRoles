namespace SuperNewRoles.Safety;

/// <summary>
/// 行動規範の拒否と部屋参加が重なったときの判定。
/// Unity の接続本体はここには置かず、起きやすい競合だけを固定する。
/// </summary>
public static class ConductJoinDecision
{
    public enum HandshakeJoinAction
    {
        Replay,
        FailOpenReplay,
        Abort,
        Refuse,
    }

    /// <summary>
    /// 身元待ちが終わった直後に JoinGame を再実行していいか。
    /// 拒否とハンドシェイク完了／タイムアウトが同じタイミングでも、拒否が勝つ。
    /// </summary>
    public static HandshakeJoinAction AfterHandshakeWait(
        bool generationChanged,
        bool declined,
        bool accepted,
        bool hasKey)
    {
        if (generationChanged || declined)
            return HandshakeJoinAction.Abort;
        if (accepted)
            return HandshakeJoinAction.Replay;
        if (!hasKey)
            return HandshakeJoinAction.Refuse;
        return HandshakeJoinAction.FailOpenReplay;
    }

    public static bool ShouldResumeDeferredJoin(bool allowed, bool declined)
    {
        return allowed && !declined;
    }

    /// <summary>
    /// 閉じアニメ中や待機中の二度押しは捨てる。終わったあとの再参加は通す。
    /// </summary>
    public static bool ShouldBlockRepeatJoinClick(bool deferring, bool conductBusy, bool warningOpen)
    {
        return deferring || conductBusy || warningOpen;
    }

    /// <summary>
    /// 拒否して ExitGame した切断を「同意が必要」と取り違えると、拒否が消えてポップアップが再出する。
    /// </summary>
    public static bool ShouldReopenConductAfterDisconnect(bool isNeedConduct, bool alreadyDeclined)
    {
        return isNeedConduct && !alreadyDeclined;
    }

    /// <summary>
    /// 拒否はメニューを捨てない。接続待ちだけ解除して、部屋作成も参加もボタンを再度押せるようにする。
    /// </summary>
    public static bool ShouldExitGameAfterDecline() => false;

    public static bool ShouldRestoreConnectUiAfterDecline() => true;
}
