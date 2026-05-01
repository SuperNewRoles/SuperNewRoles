using System.Linq;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Roles;

namespace SuperNewRoles.Modules;

public static class GameStateLogger
{
    private const string Source = "SNR.GameState";

    public static void LoadListeners()
    {
        SetRoleEvent.Instance.AddListener(OnSetRole);
        MurderEvent.Instance.AddListener(OnMurder);
        TryKillEvent.Instance.AddListener(OnTryKill);
        DieEvent.Instance.AddListener(OnDie);
        ExileEvent.Instance.AddListener(OnExile);
        WrapUpEvent.Instance.AddListener(OnWrapUp);
        CalledMeetingEvent.Instance.AddListener(OnCalledMeeting);
        MeetingStartEvent.Instance.AddListener(OnMeetingStart);
        VotingCompleteEvent.Instance.AddListener(OnVotingComplete);
        DisconnectEvent.Instance.AddListener(OnDisconnect);
        EndGameEvent.Instance.AddListener(OnEndGame);
        TaskCompleteEvent.Instance.AddListener(OnTaskComplete);
        EnterVentEvent.Instance.AddListener(OnEnterVent);
        ExitVentEvent.Instance.AddListener(OnExitVent);
        SaboStartEvent.Instance.AddListener(OnSaboStart);
        SaboEndEvent.Instance.AddListener(OnSaboEnd);
        ShapeshiftEvent.Instance.AddListener(OnShapeshift);
        GuesserShotEvent.Instance.AddListener(OnGuesserShot);
    }

    private static string FormatPlayer(ExPlayerControl ex)
    {
        if (ex == null) return "null";
        return $"{ex.PlayerId}:{ex.Player?.name ?? "??"}({ex.Role})";
    }

    private static string FormatPlayer(PlayerControl pc)
    {
        if (pc == null) return "null";
        return FormatPlayer((ExPlayerControl)pc);
    }

    private static string FormatPlayer(NetworkedPlayerInfo info)
    {
        if (info == null) return "null";
        return FormatPlayer((ExPlayerControl)info);
    }

    private static string FormatAllAlivePlayers()
    {
        var alive = ExPlayerControl.ExPlayerControls.Where(x => x.IsAlive()).ToList();
        if (alive.Count == 0) return "none";
        return string.Join(", ", alive.Select(FormatPlayer));
    }

    private static void OnSetRole(SetRoleEventData data)
    {
        Logger.Info($"[SetRole] {FormatPlayer(data.player)}: {data.oldRole} -> {data.newRole}", Source);
    }

    private static void OnMurder(MurderEventData data)
    {
        bool succeeded = data.resultFlags.HasFlag(MurderResultFlags.Succeeded);
        Logger.Info($"[Murder] {FormatPlayer(data.killer)} -> {FormatPlayer(data.target)}, Result: {(succeeded ? "Succeeded" : "Failed")}", Source);
    }

    private static void OnTryKill(TryKillEventData data)
    {
        Logger.Info($"[TryKill] {FormatPlayer(data.Killer)} -> {FormatPlayer(data.RefTarget)}, Success: {data.RefSuccess}", Source);
    }

    private static void OnDie(DieEventData data)
    {
        Logger.Info($"[Die] {FormatPlayer(data.player)}", Source);
    }

    private static void OnExile(ExileEventData data)
    {
        Logger.Info($"[Exile] {FormatPlayer(data.exiled)}", Source);
    }

    private static void OnWrapUp(WrapUpEventData data)
    {
        int total = ExPlayerControl.ExPlayerControls.Count;
        int alive = ExPlayerControl.ExPlayerControls.Count(x => x.IsAlive());
        Logger.Info($"[WrapUp] Exiled: {FormatPlayer(data.exiled)}, Alive: {alive}/{total} [{FormatAllAlivePlayers()}]", Source);
    }

    private static void OnCalledMeeting(CalledMeetingEventData data)
    {
        string targetName = data.target != null ? FormatPlayer(data.target.Object) : "Emergency";
        Logger.Info($"[MeetingCalled] {FormatPlayer(data.reporter)} reported {targetName}", Source);
    }

    private static void OnMeetingStart(MeetingStartEventData data)
    {
        int total = ExPlayerControl.ExPlayerControls.Count;
        int alive = ExPlayerControl.ExPlayerControls.Count(x => x.IsAlive());
        Logger.Info($"[MeetingStart] #{data.meetingCount}, Alive: {alive}/{total} [{FormatAllAlivePlayers()}]", Source);
    }

    private static void OnVotingComplete(VotingCompleteEventData data)
    {
        Logger.Info($"[VotingComplete] Exiled: {FormatPlayer(data.Exiled?.Object)}, Tie: {data.IsTie}", Source);
    }

    private static void OnDisconnect(DisconnectEventData data)
    {
        int total = ExPlayerControl.ExPlayerControls.Count;
        int alive = ExPlayerControl.ExPlayerControls.Count(x => x.IsAlive());
        Logger.Info($"[Disconnect] {FormatPlayer(data.disconnectedPlayer)}, Reason: {data.reason}, Alive: {alive}/{total}", Source);
    }

    private static void OnEndGame(EndGameEventData data)
    {
        string winners = data.winners.Count > 0 ? string.Join(", ", data.winners.Select(FormatPlayer)) : "none";
        Logger.Info($"[EndGame] {data.reason}, Winners: [{winners}]", Source);
    }

    private static void OnTaskComplete(TaskCompleteEventData data)
    {
        var ex = (ExPlayerControl)data.player;
        bool allComplete = ex.IsTaskComplete();
        Logger.Info($"[TaskComplete] {FormatPlayer(ex)} task={data.taskIndex}, AllComplete: {allComplete}", Source);
    }

    private static void OnEnterVent(UseVentEventData data)
    {
        Logger.Info($"[Vent] {FormatPlayer(data.user)} entered ventId={data.ventId}", Source);
    }

    private static void OnExitVent(UseVentEventData data)
    {
        Logger.Info($"[Vent] {FormatPlayer(data.user)} exited ventId={data.ventId}", Source);
    }

    private static void OnSaboStart(SaboStartEventData data)
    {
        Logger.Info($"[Sabo] Sabotage started: {data.saboType}", Source);
    }

    private static void OnSaboEnd(SaboEndEventData data)
    {
        Logger.Info($"[Sabo] Sabotage ended: {data.saboType}", Source);
    }

    private static void OnShapeshift(ShapeshiftEventData data)
    {
        Logger.Info($"[Shapeshift] {FormatPlayer(data.shapeshifter)} -> {FormatPlayer(data.target)}, Animate: {data.animate}", Source);
    }

    private static void OnGuesserShot(GuesserShotEventData data)
    {
        string result = data.isMisFire ? "MisFire" : "Hit";
        Logger.Info($"[GuesserShot] {FormatPlayer(data.killer)} -> {FormatPlayer(data.target)}, Result: {result}", Source);
    }
}
