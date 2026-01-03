using SuperNewRoles.Roles.Impostor;

namespace SuperNewRoles.Roles.Ability;

public class TriggerHappyData
{
    public int UseLimit { get; }
    public float Cooldown { get; }
    public bool SyncKillCoolTime { get; }
    public float Duration { get; }
    public int RequiredHits { get; }
    public float Range { get; }
    public float BulletSize { get; }
    public bool PierceWalls { get; }
    public int BulletSpreadAngle { get; }

    public TriggerHappyData(
        int useLimit,
        float cooldown,
        bool syncKillCoolTime,
        float duration,
        int requiredHits,
        float range,
        float bulletSize,
        bool pierceWalls,
        TriggerHappyBulletSpreadAngleOption bulletSpreadAngleOption)
    {
        UseLimit = useLimit;
        Cooldown = cooldown;
        SyncKillCoolTime = syncKillCoolTime;
        Duration = duration;
        RequiredHits = requiredHits;
        Range = range;
        BulletSize = bulletSize;
        PierceWalls = pierceWalls;
        BulletSpreadAngle = (int)bulletSpreadAngleOption;
    }
}
