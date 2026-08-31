using SuperNewRoles.Safety.Patches;

namespace SuperNewRoles.Safety;

/// <summary>
/// 行動規範を拒否したとき、進行中の部屋接続と身元ハンドシェイクを打ち切る。
/// メニューは捨てず、接続待ちだけ解除して作成・参加ボタンを再度押せるようにする。
/// </summary>
public static class ConductDeclineAbort
{
    public static void Run()
    {
        IdentityPreJoinGate.Reset();
        if (!ConductJoinDecision.ShouldRestoreConnectUiAfterDecline())
            return;
        if (DestroyableSingleton<MatchMaker>.InstanceExists)
            DestroyableSingleton<MatchMaker>.Instance.NotConnecting();
    }
}
