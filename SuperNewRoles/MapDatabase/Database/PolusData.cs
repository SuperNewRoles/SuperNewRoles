using UnityEngine;

namespace SuperNewRoles.MapDatabase.Database;

public class PolusData : MapDatabase
{
    static private Vector2[] MapPositions = new Vector2[]
    {
        //ドロップシップ
        new(16.7f, -2.6f),
        //ドロップシップ下
        new(14.1f, -10f), new(22.0f, -7.1f),
        //エレクトリカル
        new(7.5f, -9.7f), new(3.1f, -11.7f), new(5.4f, -11.5f), new(9.6f, -12.1f),
        //O2
        new(4.7f, -19f), new(2.4f, -17f), new(3.1f, -21.7f), new(1.9f, -19.4f), new(2.4f, -23.6f), new(6.3f, -21.3f),
        //Elec,O2,Comm周辺外
        new(7.9f, -23.6f), new(9.4f, -20.1f), new(8.2f, -16.0f), new(8.0f, -14.3f), new(13.4f, -13f),
        //左上リアクター前通路
        new(10.3f, -7.4f),
        //左上リアクター
        new(4.6f, -5f),
        //Comm
        new(11.4f, -15.9f), new(11.7f, -17.3f),
        //Weapons
        new(13f, -23.5f),
        //Storage
        new(19.4f, -11.2f),
        //オフィス左下
        new(18f, -24.5f),new(14.5f, -24.2f),
        //オフィス
        new(18.6f, -21.5f), new(20.2f, -19.2f), new(19.6f, -17.6f), new(19.6f, -16.4f), new(26.5f, -17.4f),new(17.3f,-18.6f),new(22.3f,-18.6f),
        //アドミン
        new(20f, -22.5f), new(21.4f, -25.2f), new(22.4f, -22.6f), new(25f, -20.8f),
        //デコン（左）
        new(24.1f, -24.7f),
        //スペシメン左通路
        new(27.7f, -24.7f), new(33f, -20.6f),
        //スペシメン
        new(36.8f, -21.6f), new(36.5f, -19.3f),
        //スペシメン右通路
        new(39.2f, -15.2f),
        //デコン(上)
        new(39.8f, -10f),
        //ラボ
        new(34.7f, -10.2f), new(36.4f, -8f), new(40.5f, -7.6f), new(34.5f, -6.2f), new(31.2f, -7.6f), new(28.4f, -9.6f), new(26.5f, -7f), new(26.5f, -8.3f),
        //右リアクター
        new(24.2f, -4.5f),
        //ストレージ・ラボ下・オフィス右
        new(24f, -14.6f), new(26f, -12.2f), new(29.8f, -15.7f)
    };

    // オルフェウスの儀式死体を優先的に出す座標。空なら境界ランダム探索を使う。
    static private readonly Vector2[] orpheusDeadBodySpawnPositionPool = [
        // ボイラー
        new(0.795f, -23.533f),
        // ウェポン左外下
        new(9.014f, -25.306f),
        // ウェポン内部
        new(12.555f, -24.76f),
        // アドミン下
        new(22.235f, -25.142f),
        // 通信室
        new(12.846f, -17.317f),
        // ストレージ
        new(19.749f, -12.479f),
        // 木の部屋
        new(1.131f, -17.595f),
        // ボンベ室
        new(1.264f, -18.782f),
        // 外大岩右
        new(32.704f, -13.292f),
        // 標本室
        new(36.211f, -22.082f),
        // ラボベント
        new(32.952f, -9.308f),
        // 通信外左
        new(7.089f, -17.392f),
        // 左リアクター下
        new(3.767f, -7.685f),
        // オフィス左下外
        new(19.254f, -25.699f),
    ];
    static private readonly Vector2[] PlayerSpawnPositionPool =
    [
        new(25.7343f, -12.8777f), // BackRock
        new(3.3584f, -21.68f),    // Oxygen
        new(5.3372f, -9.7048f),   // Electrical
        new(23.9309f, -22.5169f), // Admin
        new(19.5145f, -17.4998f), // Office
        new(12.0384f, -23.34f),   // Weapons
        new(10.6821f, -16.0105f), // Comms
        new(20.5637f, -11.9088f), // Storage
        new(16.6458f, -3.2058f),  // Dropship
        new(34.3056f, -7.8901f)   // Laboratory / Specimens
    ];
    protected override Vector2[] MapArea => MapPositions;
    protected override Vector2[] NonMapArea => [];
    public override Vector2[] OrpheusDeadBodySpawnPositionPool => orpheusDeadBodySpawnPositionPool;
    protected override Vector2[] PlayerSpawnPositions => PlayerSpawnPositionPool;
    protected override SystemTypes[] SabotageTypes => new SystemTypes[] { SystemTypes.Laboratory, SystemTypes.Comms, SystemTypes.Electrical };
}
