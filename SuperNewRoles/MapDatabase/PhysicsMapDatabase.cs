using UnityEngine;

namespace SuperNewRoles.MapDatabase;

/// <summary>
/// バニラ参照点を持たないマップ（LevelImposter 等）向け。固体コライダー上だけ歩行不可とみなす。
/// </summary>
public sealed class PhysicsMapDatabase : MapDatabase
{
    private static readonly SystemTypes[] DefaultSabotageTypes =
    [
        SystemTypes.Electrical,
        SystemTypes.Reactor,
        SystemTypes.LifeSupp,
        SystemTypes.Comms,
    ];

    private static bool _boundsValid;
    private static Vector2 _cachedMin;
    private static Vector2 _cachedMax;

    protected override Vector2[] MapArea => [];
    protected override Vector2[] NonMapArea => [];
    protected override SystemTypes[] SabotageTypes => DefaultSabotageTypes;

    public static void InvalidateBoundsCache()
    {
        _boundsValid = false;
        _cachedMin = Vector2.zero;
        _cachedMax = Vector2.zero;
    }

    public override bool CheckMapArea(Vector2 position)
    {
        int num = Physics2D.OverlapCircleNonAlloc(position, 0.1f, PhysicsHelpers.colliderHits, Constants.ShipAndAllObjectsMask);
        if (num <= 0 || num > PhysicsHelpers.colliderHits.Length)
            return true;

        for (int i = 0; i < num; i++)
        {
            Collider2D hit = PhysicsHelpers.colliderHits[i];
            if (hit != null && !hit.isTrigger)
                return false;
        }

        return true;
    }

    public override bool TryGetSpawnScanBounds(out Vector2 min, out Vector2 max)
    {
        if (_boundsValid)
        {
            min = _cachedMin;
            max = _cachedMax;
            return true;
        }

        min = max = Vector2.zero;
        ShipStatus ship = ShipStatus.Instance;
        if (ship == null)
            return false;

        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float maxX = float.MinValue;
        float maxY = float.MinValue;
        bool any = false;

        void Acc(Vector2 p)
        {
            any = true;
            minX = Mathf.Min(minX, p.x);
            minY = Mathf.Min(minY, p.y);
            maxX = Mathf.Max(maxX, p.x);
            maxY = Mathf.Max(maxY, p.y);
        }

        Acc(ship.InitialSpawnCenter);
        Acc(ship.MeetingSpawnCenter);
        Acc(ship.MeetingSpawnCenter2);

        if (ship.AllVents != null)
        {
            foreach (Vent vent in ship.AllVents)
            {
                if (vent == null)
                    continue;
                Acc(vent.transform.position);
            }
        }

        if (ship.AllRooms != null)
        {
            foreach (PlainShipRoom room in ship.AllRooms)
            {
                if (room?.roomArea == null)
                    continue;
                Bounds b = room.roomArea.bounds;
                Acc(b.min);
                Acc(b.max);
            }
        }

        if (!any)
            return false;

        const float pad = 2f;
        _cachedMin = new Vector2(minX - pad, minY - pad);
        _cachedMax = new Vector2(maxX + pad, maxY + pad);
        _boundsValid = true;
        min = _cachedMin;
        max = _cachedMax;
        return true;
    }
}
