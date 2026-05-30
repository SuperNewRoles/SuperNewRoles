using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using InnerNet;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using SuperNewRoles.CustomObject;
using SuperNewRoles.CustomOptions;
using SuperNewRoles.Events;
using SuperNewRoles.Events.PCEvents;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using SuperNewRoles.Roles.Ability;
using SuperNewRoles.Roles.Ability.CustomButton;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SuperNewRoles.Roles.Impostor;

/// <summary>オルフェウスで WrapUp ごとにスポーンする偽死体の識別用。</summary>
public sealed class OrpheusRitualCorpseMarker : MonoBehaviour
{
    public bool ReportDisabledByRange;
}

/// <summary>オルフェウス役職。</summary>
internal sealed class Orpheus : RoleBase<Orpheus>
{
    public override RoleId Role { get; } = RoleId.Orpheus;
    public override Color32 RoleColor { get; } = Palette.ImpostorRed;
    public override List<Func<AbilityBase>> Abilities { get; } = [
        () => new OrpheusMainAbility(OrpheusCorpseTurnCount, OrpheusReviveCooldown)
    ];

    public override QuoteMod QuoteMod { get; } = QuoteMod.SuperNewRoles;
    public override RoleTypes IntroSoundType { get; } = RoleTypes.Impostor;
    public override short IntroNum { get; } = 1;
    public override AssignedTeamType AssignedTeam { get; } = AssignedTeamType.Impostor;
    public override WinnerTeamType WinnerTeam { get; } = WinnerTeamType.Impostor;
    public override TeamTag TeamTag { get; } = TeamTag.Impostor;
    public override RoleTag[] RoleTags { get; } = [RoleTag.Support, RoleTag.ImpostorTeam];
    public override RoleOptionMenuType OptionTeam { get; } = RoleOptionMenuType.Impostor;

    [CustomOptionInt(nameof(OrpheusCorpseTurnCount), 1, 5, 1, 2)]
    public static int OrpheusCorpseTurnCount;

    [CustomOptionFloat(nameof(OrpheusReviveCooldown), 2.5f, 60f, 2.5f, 30f)]
    public static float OrpheusReviveCooldown;

    [CustomOptionFloat(nameof(OrpheusRitualCorpseReportDistancePercent), 0f, 100f, 5f, 50f, suffix: "%")]
    public static float OrpheusRitualCorpseReportDistancePercent;
}

/// <summary>キル・ベント・インポ視界と蘇生ボタン、管理死体フローを束ねる。</summary>
public sealed class OrpheusMainAbility : AbilityBase
{
    private const int RandomRitualPositionAttempts = 48;
    private const int RitualCorpseEffectFrameCount = 8;
    private const int RitualCorpseEffectFrameRate = 9;
    private const float RitualCorpseEffectScale = 0.22f;

    /// <summary>
    /// RoundsRemaining: あと何回の WrapUp で儀式死体をスポーンするか（死亡直後のタスクフェーズは儀式死体なし）。
    /// </summary>
    private sealed class CorpseEntry
    {
        public int RoundsRemaining;
        public bool PendingRevive;
        public short StoredRoleId;
        public bool Targetable;
    }

    private static readonly Dictionary<byte, CorpseEntry> HostEntries = new();
    private static readonly HashSet<byte> ActiveRitualCorpseVictims = new();
    // Destroy はフレーム終端まで遅延するため、会議イントロ側でも除外できるよう記録する。
    private static readonly HashSet<byte> HiddenMeetingCorpseVictims = new();
    private static byte CurrentMeetingReportedBodyId = byte.MaxValue;

    private static EventListener<DieEventData> _dieListener;
    private static EventListener<WrapUpEventData> _wrapListener;
    private static EventListener<ReportDeadBodyHostEventData> _reportListener;
    private static EventListener _hudUpdateListener;
    private static EventListener _fixedUpdateListener;

    private static int _attachedCount;
    private static ShipStatus _stateShipStatus;

    public int _turnCount { get; }
    public float _reviveCooldown { get; }

    public OrpheusMainAbility(int turnCount, float reviveCooldown)
    {
        _turnCount = turnCount;
        _reviveCooldown = reviveCooldown;
    }

    public override void AttachToAlls()
    {
        base.AttachToAlls();
        EnsureStateForCurrentShip();
        _attachedCount++;
        RegisterSharedListeners();

        OrpheusReviveButtonAbility reviveButton = new();
        reviveButton.BindParent(this);
        Player.AttachAbility(reviveButton, new AbilityParentAbility(this));
    }

    public override void DetachToAlls()
    {
        _attachedCount = Math.Max(0, _attachedCount - 1);
        if (_attachedCount == 0)
            UnregisterSharedListeners();
        base.DetachToAlls();
    }

    public static void ResetForNewGame()
    {
        UnregisterSharedListeners();
        ResetSharedState();
        _attachedCount = 0;
        _stateShipStatus = ShipStatus.Instance;
    }

    public static bool IsManagedCorpseBody(DeadBody body) =>
        body != null && body.GetComponent<OrpheusRitualCorpseMarker>() != null;

    internal static void HideRitualCorpsesForMeeting(NetworkedPlayerInfo reportedBody)
    {
        HiddenMeetingCorpseVictims.Clear();
        byte reportedId = reportedBody?.PlayerId ?? byte.MaxValue;
        CurrentMeetingReportedBodyId = reportedId;

        foreach (byte victimId in ActiveRitualCorpseVictims.ToArray())
        {
            if (victimId != reportedId)
                HiddenMeetingCorpseVictims.Add(victimId);
            ActiveRitualCorpseVictims.Remove(victimId);
        }

        foreach (DeadBody b in Object.FindObjectsOfType<DeadBody>())
        {
            if (b == null || !IsManagedCorpseBody(b))
                continue;
            if (b.ParentId == reportedId)
                continue;

            HiddenMeetingCorpseVictims.Add(b.ParentId);
            b.gameObject.SetActive(false);
            Object.Destroy(b.gameObject);
        }
    }

    internal static void FilterRitualCorpseVictimsFromMeetingIntro(ref Il2CppReferenceArray<NetworkedPlayerInfo> deadBodies)
    {
        if (deadBodies == null)
        {
            ClearMeetingFilterState();
            return;
        }

        byte reportedId = CurrentMeetingReportedBodyId;
        List<NetworkedPlayerInfo> filtered = new();
        for (int i = 0; i < deadBodies.Length; i++)
        {
            NetworkedPlayerInfo player = deadBodies[i];
            if (player == null)
                continue;
            if (player.PlayerId == reportedId)
            {
                filtered.Add(player);
                continue;
            }
            if (HiddenMeetingCorpseVictims.Contains(player.PlayerId) || ActiveRitualCorpseVictims.Contains(player.PlayerId))
                continue;
            filtered.Add(player);
        }

        Il2CppReferenceArray<NetworkedPlayerInfo> filteredArray = new(filtered.Count);
        for (int i = 0; i < filtered.Count; i++)
            filteredArray[i] = filtered[i];
        deadBodies = filteredArray;
        ClearMeetingFilterState();
    }

    private static void ClearMeetingFilterState()
    {
        HiddenMeetingCorpseVictims.Clear();
        ActiveRitualCorpseVictims.Clear();
        CurrentMeetingReportedBodyId = byte.MaxValue;
    }

    /// <summary>ローカル：報告距離内・LOS あり・オルフェウス儀式死体のうち最寄り。</summary>
    internal bool TryGetNearestManagedBody(out DeadBody body, out byte victimId)
    {
        body = null;
        victimId = byte.MaxValue;
        if (!Player.AmOwner)
            return false;

        Vector2 pos = PlayerControl.LocalPlayer.GetTruePosition();
        float maxDist = PlayerControl.LocalPlayer.MaxReportDistance;
        float best = float.MaxValue;

        foreach (Collider2D collider in Physics2D.OverlapCircleAll(pos, maxDist, Constants.PlayersOnlyMask))
        {
            if (!collider.CompareTag("DeadBody"))
                continue;
            DeadBody db = collider.GetComponent<DeadBody>();
            if (db == null || !db.gameObject.activeInHierarchy)
                continue;
            OrpheusRitualCorpseMarker marker = db.GetComponent<OrpheusRitualCorpseMarker>();
            if (marker == null)
                continue;
            if (db.Reported && !marker.ReportDisabledByRange)
                continue;

            Vector2 bp = db.TruePosition;
            float d = Vector2.Distance(pos, bp);
            if (d > maxDist || PhysicsHelpers.AnythingBetween(pos, bp, Constants.ShipAndObjectsMask, false))
                continue;
            if (d >= best)
                continue;
            best = d;
            body = db;
            victimId = db.ParentId;
        }

        return body != null;
    }

    [CustomRPC]
    public void RpcTryConsume(byte victimId)
    {
        if (!AmongUsClient.Instance.AmHost)
            return;
        if (Player.Role != RoleId.Orpheus || !Player.IsAlive())
            return;
        if (!HostEntries.TryGetValue(victimId, out CorpseEntry entry) || entry.PendingRevive)
            return;
        if (!HasRitualCorpseForVictim(victimId))
            return;

        entry.PendingRevive = true;
        RpcConsumeCorpse(victimId);
    }

    private static bool HasRitualCorpseForVictim(byte victimId)
    {
        foreach (DeadBody b in Object.FindObjectsOfType<DeadBody>())
        {
            if (b != null && b.ParentId == victimId && IsManagedCorpseBody(b))
                return true;
        }

        return false;
    }

    private static void RegisterSharedListeners()
    {
        UnregisterSharedListeners();
        _dieListener = DieEvent.Instance.AddListener(OnDie);
        _wrapListener = WrapUpEvent.Instance.AddListener(OnWrapUp);
        _reportListener = ReportDeadBodyHostEvent.Instance.AddListener(OnReport);
        _hudUpdateListener = HudUpdateEvent.Instance.AddListener(UpdateRitualCorpseReportability);
        _fixedUpdateListener = FixedUpdateEvent.Instance.AddListener(UpdateRitualCorpseReportability);
    }

    private static void UnregisterSharedListeners()
    {
        _dieListener?.RemoveListener();
        _dieListener = null;
        _wrapListener?.RemoveListener();
        _wrapListener = null;
        _reportListener?.RemoveListener();
        _reportListener = null;
        _hudUpdateListener?.RemoveListener();
        _hudUpdateListener = null;
        _fixedUpdateListener?.RemoveListener();
        _fixedUpdateListener = null;
    }

    private static void EnsureStateForCurrentShip()
    {
        if (_stateShipStatus == ShipStatus.Instance)
            return;

        UnregisterSharedListeners();
        ResetSharedState();
        _stateShipStatus = ShipStatus.Instance;
        _attachedCount = 0;
    }

    private static void ResetSharedState()
    {
        HostEntries.Clear();
        ActiveRitualCorpseVictims.Clear();
        HiddenMeetingCorpseVictims.Clear();
        CurrentMeetingReportedBodyId = byte.MaxValue;
    }

    private static bool AnyOrpheusAlive() =>
        ExPlayerControl.ExPlayerControls.Any(p => p != null && p.Role == RoleId.Orpheus && p.IsAlive());

    private static void OnDie(DieEventData data)
    {
        if (data.player == null)
            return;
        EnsureStateForCurrentShip();

        if (!AmongUsClient.Instance.AmHost)
            return;

        if (!AnyOrpheusAlive())
            return;

        ExPlayerControl victim = ExPlayerControl.ById(data.player.PlayerId);
        if (!CanCreateManagedCorpse(victim))
            return;

        byte victimId = victim.PlayerId;
        if (HostEntries.ContainsKey(victimId))
            return;

        HostEntries[victimId] = new CorpseEntry
        {
            RoundsRemaining = GetMaxTurnCountAliveOrpheus(),
            PendingRevive = false,
            StoredRoleId = (short)victim.Role,
            Targetable = false
        };
    }

    private static void ExpireUnconsumedEntriesFromHost()
    {
        foreach (var kv in HostEntries.Where(k => !k.Value.PendingRevive).ToArray())
            RpcExpireCorpse(kv.Key);
    }

    private static bool CanCreateManagedCorpse(ExPlayerControl victim) =>
        victim != null && victim.IsImpostor() && victim.Role != RoleId.Orpheus;

    private static void OnReport(ReportDeadBodyHostEventData data)
    {
        if (!AmongUsClient.Instance.AmHost || data.target == null)
            return;
        byte id = data.target.PlayerId;
        if (!HostEntries.TryGetValue(id, out CorpseEntry entry))
            return;

        // 偽死体生成前であれば対象外にする
        if (!entry.Targetable)
            return;

        if (!CanReporterReportRitualCorpse(data.reporter, id))
        {
            data.CanReport = false;
            return;
        }

        if (entry.PendingRevive)
        {
            HostEntries.Remove(id);
            RpcRollbackConsumedCorpse(id);
            return;
        }

        bool reportedRitualCorpse = HasRitualCorpseForVictim(id);
        HostEntries.Remove(id);
        if (!reportedRitualCorpse)
            DestroyRitualCorpsesForVictim(id);
    }

    private static void OnWrapUp(WrapUpEventData _)
    {
        if (!AmongUsClient.Instance.AmHost)
            return;
        ApplyPendingRevivals();
        if (!AnyOrpheusAlive())
        {
            ExpireUnconsumedEntriesFromHost();
            return;
        }
        ProcessOrpheusRitualCorpseTurns();
    }

    private static void ApplyPendingRevivals()
    {
        foreach (var kv in HostEntries.Where(k => k.Value.PendingRevive).ToArray())
        {
            RpcReviveVictim(kv.Key, kv.Value.StoredRoleId);
            HostEntries.Remove(kv.Key);
        }
    }

    /// <summary>各 WrapUp で儀式死体を差し替え、残り回数を減らす。</summary>
    private static void ProcessOrpheusRitualCorpseTurns()
    {
        foreach (var kv in HostEntries.Where(k => !k.Value.PendingRevive).ToArray())
        {
            byte victimId = kv.Key;
            CorpseEntry entry = kv.Value;
            DestroyRitualCorpsesForVictim(victimId);
            if (entry.RoundsRemaining <= 0)
            {
                HostEntries.Remove(victimId);
                continue;
            }

            Vector2 pos = TryPickRandomRitualPosition(victimId);
            RpcSpawnOrpheusRitualCorpse(victimId, pos.x, pos.y);
            entry.RoundsRemaining--;
            entry.Targetable = true;
        }
    }

    private static Vector2 TryPickRandomRitualPosition(byte victimId)
    {
        if (SuperNewRoles.MapDatabase.MapDatabase.TryGetRandomDeadBodySpawnPositionForCurrentMap(out Vector2 pooledPosition))
            return pooledPosition;

        SuperNewRoles.MapDatabase.MapDatabase mapData = SuperNewRoles.MapDatabase.MapDatabase.GetCurrentMapData();
        if (mapData != null
            && SuperNewRoles.MapDatabase.MapDatabase.TryGetSpawnScanBoundsForCurrentMap(out Vector2 min, out Vector2 max))
        {
            for (int i = 0; i < RandomRitualPositionAttempts; i++)
            {
                float x = ModHelpers.GetRandomFloat(max.x, min.x);
                float y = ModHelpers.GetRandomFloat(max.y, min.y);
                Vector2 p = new(x, y);
                if (mapData.CheckMapArea(p) && !mapData.IsNearPlayerSpawnPosition(p))
                    return p;
            }
        }

        ExPlayerControl victim = ExPlayerControl.ById(victimId);
        return GetFallbackCorpsePosition(victim);
    }

    private static Vector2 GetFallbackCorpsePosition(ExPlayerControl victim)
    {
        if (victim?.Player == null)
            return Vector2.zero;

        Vector3 pos = victim.Player.transform.position;
        KillAnimation killAnimation = victim.Player.KillAnimations?.FirstOrDefault();
        if (killAnimation != null)
            pos += killAnimation.BodyOffset;
        return pos;
    }

    [CustomRPC]
    public static void RpcSpawnOrpheusRitualCorpse(byte victimId, float x, float y)
    {
        EnsureStateForCurrentShip();
        ExPlayerControl victim = ExPlayerControl.ById(victimId);
        if (!CanCreateManagedCorpse(victim))
            return;
        if (!AnyOrpheusAlive())
        {
            if (AmongUsClient.Instance.AmHost)
                ExpireUnconsumedEntriesFromHost();
            return;
        }

        CreateClientRitualDeadBody(victimId, new Vector2(x, y));
    }

    private static int GetMaxTurnCountAliveOrpheus() =>
        ExPlayerControl.ExPlayerControls.Max(x => x != null && x.IsAlive() && x.TryGetAbility<OrpheusMainAbility>(out var ability) ? ability._turnCount : 0);

    private static Vector3 DeadBodyWorldPosition(Vector2 pos) =>
        new(pos.x, pos.y, pos.y / 1000f);

    private static void CreateClientRitualDeadBody(byte victimId, Vector2 pos)
    {
        if (GameManager.Instance?.deadBodyPrefab == null || GameManager.Instance.deadBodyPrefab.Count == 0)
            return;

        DeadBody deadBody = Object.Instantiate(GameManager.Instance.deadBodyPrefab[0]);
        deadBody.enabled = true;
        deadBody.gameObject.SetActive(true);
        deadBody.ParentId = victimId;
        deadBody.gameObject.AddComponent<OrpheusRitualCorpseMarker>();
        ActiveRitualCorpseVictims.Add(victimId);
        NetworkedPlayerInfo info = GameData.Instance?.GetPlayerById(victimId);
        PlayerControl outfitSrc = info?.Object;
        if (outfitSrc != null)
        {
            foreach (SpriteRenderer b in deadBody.bodyRenderers)
                outfitSrc.SetPlayerMaterialColors(b);
            outfitSrc.SetPlayerMaterialColors(deadBody.bloodSplatter);
        }
        deadBody.transform.position = DeadBodyWorldPosition(pos);
        AttachRitualCorpseEffect(deadBody);
        UpdateRitualCorpseReportability();
    }

    private static void UpdateRitualCorpseReportability()
    {
        PlayerControl reporter = PlayerControl.LocalPlayer;
        if (reporter == null)
            return;

        foreach (DeadBody body in Object.FindObjectsOfType<DeadBody>())
        {
            if (body == null || !body.gameObject.activeInHierarchy)
                continue;
            OrpheusRitualCorpseMarker marker = body.GetComponent<OrpheusRitualCorpseMarker>();
            if (marker == null)
                continue;

            bool canReport = IsWithinRitualCorpseReportDistance(reporter, body);
            if (!canReport)
            {
                if (!body.Reported)
                    marker.ReportDisabledByRange = true;
                body.Reported = true;
                continue;
            }

            if (marker.ReportDisabledByRange)
            {
                body.Reported = false;
                marker.ReportDisabledByRange = false;
            }
        }
    }

    private static bool CanReporterReportRitualCorpse(PlayerControl reporter, byte victimId)
    {
        foreach (DeadBody body in Object.FindObjectsOfType<DeadBody>())
        {
            if (body != null
                && body.ParentId == victimId
                && IsManagedCorpseBody(body)
                && IsWithinRitualCorpseReportDistance(reporter, body))
                return true;
        }

        return false;
    }

    private static bool IsWithinRitualCorpseReportDistance(PlayerControl reporter, DeadBody body)
    {
        if (reporter == null || body == null)
            return false;

        float maxDist = reporter.MaxReportDistance * (Orpheus.OrpheusRitualCorpseReportDistancePercent / 100f);
        Vector2 reporterPos = reporter.GetTruePosition();
        Vector2 bodyPos = body.TruePosition;
        return Vector2.Distance(reporterPos, bodyPos) <= maxDist
            && !PhysicsHelpers.AnythingBetween(reporterPos, bodyPos, Constants.ShipAndObjectsMask, false);
    }

    private static void AttachRitualCorpseEffect(DeadBody deadBody)
    {
        Sprite[] sprites = GetRitualCorpseEffectSprites();
        if (sprites == null || sprites.Length == 0)
            return;

        GameObject effect = new("OrpheusRitualCorpseEffect");
        effect.transform.SetParent(deadBody.transform, false);
        effect.transform.localPosition = new Vector3(-0.3f, 0.1f, 0.001f);
        effect.transform.localScale = new Vector3(RitualCorpseEffectScale, RitualCorpseEffectScale, 1f);
        effect.layer = deadBody.gameObject.layer;

        SpriteRenderer renderer = effect.AddComponent<SpriteRenderer>();
        renderer.sprite = sprites[0];
        renderer.maskInteraction = SpriteMaskInteraction.None;

        CustomAnimationObject animation = effect.AddComponent<CustomAnimationObject>();
        animation.Init(new CustomAnimationObjectOption(sprites, true, RitualCorpseEffectFrameRate), renderer);
    }

    private static Sprite[] GetRitualCorpseEffectSprites()
    {
        // 後ろに2フレーム入れて自然にする
        Sprite[] sprites = new Sprite[RitualCorpseEffectFrameCount + 2];
        for (int i = 0; i < RitualCorpseEffectFrameCount; i++)
        {
            string spriteName = $"OrpheusDeadBodyEffect_{i + 1:00}";
            sprites[i] = AssetManager.GetAsset<Sprite>(spriteName) ?? AssetManager.GetAsset<Sprite>($"{spriteName}.png");
            if (sprites[i] == null)
                return null;
        }

        sprites[RitualCorpseEffectFrameCount] = sprites[RitualCorpseEffectFrameCount - 1];
        sprites[RitualCorpseEffectFrameCount + 1] = sprites[RitualCorpseEffectFrameCount - 1];

        return sprites;
    }

    [CustomRPC]
    public static void RpcExpireCorpse(byte victimId)
    {
        DestroyRitualCorpsesForVictim(victimId);
        if (AmongUsClient.Instance.AmHost)
            HostEntries.Remove(victimId);
    }

    [CustomRPC]
    public static void RpcConsumeCorpse(byte victimId)
    {
        DestroyDeadBodiesForVictim(victimId);
        ProctedMessager.ScheduleProctedMessage(ModTranslation.GetString("OrpheusMeetingBanner"));
    }

    [CustomRPC]
    public static void RpcRollbackConsumedCorpse(byte victimId)
    {
        ProctedMessager.UnscheduleProctedMessage(ModTranslation.GetString("OrpheusMeetingBanner"));
    }

    [CustomRPC]
    public static void RpcReviveVictim(byte victimId, short roleId)
    {
        ExPlayerControl ex = ExPlayerControl.ById(victimId);
        if (ex?.Player == null)
            return;

        ex.Player.Revive();
        RoleManager.Instance.SetRole(ex.Player, RoleTypes.Impostor);
        ex.Data.IsDead = false;
        FinalStatusManager.SetFinalStatus(ex, FinalStatus.Alive);
        MurderDataManager.RevivedMurderData(ex);
        ex.SetRole((RoleId)roleId);
        NameText.UpdateAllNameInfo();
    }

    private static void DestroyRitualCorpsesForVictim(byte victimId)
    {
        ActiveRitualCorpseVictims.Remove(victimId);
        foreach (DeadBody b in Object.FindObjectsOfType<DeadBody>())
        {
            if (b != null && b.ParentId == victimId && IsManagedCorpseBody(b))
                Object.Destroy(b.gameObject);
        }
    }

    private static void DestroyDeadBodiesForVictim(byte victimId)
    {
        ActiveRitualCorpseVictims.Remove(victimId);
        foreach (DeadBody b in Object.FindObjectsOfType<DeadBody>())
        {
            if (b != null && b.ParentId == victimId)
                Object.Destroy(b.gameObject);
        }
    }
}

/// <summary>管理死体付近で押下して、親Abilityへ消費要求を送る。</summary>
public sealed class OrpheusReviveButtonAbility : CustomButtonBase
{
    private OrpheusMainAbility _parent;

    public void BindParent(OrpheusMainAbility parent) => _parent = parent;

    public override float DefaultTimer => _parent._reviveCooldown;
    public override string buttonText => ModTranslation.GetString("OrpheusReviveButton");
    public override Sprite Sprite => AssetManager.GetAsset<Sprite>("OrpheusButton.png");
    protected override KeyType keytype => KeyType.Ability1;

    public override bool CheckIsAvailable()
    {
        if (_parent == null || !Player.IsAlive() || MeetingHud.Instance != null)
            return false;
        return _parent.TryGetNearestManagedBody(out _, out _);
    }

    public override void OnClick()
    {
        if (_parent == null || !_parent.TryGetNearestManagedBody(out _, out byte victimId))
            return;
        _parent.RpcTryConsume(victimId);
    }
}

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.StartMeeting))]
public static class OrpheusShipStatusStartMeetingPatch
{
    public static void Prefix(NetworkedPlayerInfo target)
    {
        OrpheusMainAbility.HideRitualCorpsesForMeeting(target);
    }
}

[HarmonyPatch(typeof(MeetingIntroAnimation), nameof(MeetingIntroAnimation.Init))]
public static class OrpheusMeetingIntroAnimationInitPatch
{
    public static void Prefix(ref Il2CppReferenceArray<NetworkedPlayerInfo> deadBodies)
    {
        OrpheusMainAbility.FilterRitualCorpseVictimsFromMeetingIntro(ref deadBodies);
    }
}
