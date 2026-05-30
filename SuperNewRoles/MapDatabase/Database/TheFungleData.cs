using UnityEngine;

namespace SuperNewRoles.MapDatabase.Database;

public class FungleData : MapDatabase
{
    static private Vector2[] MapPositions = new Vector2[]
        { 
        //ドロップシップ
        new(-9.2f,13.4f),
        //カフェテリア
        new(-19.1f, 7.0f),new(-13.6f,5.0f),new(-20.5f,6.0f),
        //カフェ下
        new(-12.9f,2.3f),new(-21.7f,2.41f),
        //スプラッシュゾーン
        new(-20.2f,-0.3f),new(-19.8f,-2.1f),new(-16.1f,-0.1f),new(-15.6f,-1.8f),
        //キャンプファイア周辺
        new(-11.3f,2.0f),new(-0.83f,2.4f),new(-9.4f,0.2f),new(-6.9f,0.2f),
        //スプラッシュゾーン下
        new(-17.3f,-4.5f),
        //キッチン
        new(-15.4f,-9.5f),new(-17.4f,-7.5f),
        //キッチン・ジャングル間通路
        new(-11.2f,-6.1f),new(-5.5f,-14.8f),
        //ミーティング上
        new(-2.8f,2.2f),new(2.2f,1.0f),
        //ストレージ
        new(-0.6f,4.2f),new(2.3f,6.2f),new(3.3f,6.7f),
        //ミーティング・ドーム
        new(-0.15f,-1.77f),new(-4.65f,1.58f),new(-4.8f,-1.44f),
        //ラボ
        new(-7.1f,-11.9f),new(-4.5f,-6.8f),new(-3.3f,-8.9f),new(-5.4f,-10.2f),
        //ジャングル(左)
        new(-1.44f,-13.3f),new(3.8f,-12.5f),
        //ジャングル(中)
        new(7.08f,-15.3f),new(11.6f,-14.3f),
        //ジャングル(上)
        new(2.7f,-6.0f),new(12.1f,-7.3f),
        //グリーンハウス・ジャングル
        new(13.6f,-12.1f),new(6.4f,-10f),
        //ジャングル(右)
        new(15.0f,-6.7f),new(18.1f,-9.1f),
        //ジャングル(下)
        new(14.9f,-16.3f),
        //リアクター
        new(21.1f,-6.7f),
        //高台
        new(15.9f,0.4f),new(15.6f,4.3f),new(19.2f,1.78f),
        //鉱山
        new(12.5f,7.7f),new(13.4f,9.7f),
        //ルックアウト
        new(6.6f,3.8f),new(8.7f,1f),
        //梯子中間
        new(20.1f,7.2f),
        //コミュ
        new(20.9f,10.8f),new(24.1f,13.2f),new(17.9f,12.7f),
        };

    // オルフェウスの儀式死体を優先的に出す座標。空なら境界ランダム探索を使う。
    static private readonly Vector2[] orpheusDeadBodySpawnPositionPool = [
        // ドロップシップ
        new Vector3(-11.169f, 12.341f, 0.012f),
        // 焚き火左
        new Vector3(-16.1866f, 2.4168f, 0f),
        // スプラッシュゾーン
        new Vector3(-18.542f, -2.247f, -0.002f),
        // ジャングル左下
        new Vector3(-6.526f, -15.033f, -0.015f),
        // キッチン
        new Vector3(-17.461f, -9.304f, -0.009f),
        // 展望台
        new Vector3(10.19f, 1.396f, 0.001f),
        // ジャングル右下
        new Vector3(14.308f, -16.523f, -0.017f),
        // 鉱山
        new Vector3(13.937f, 9.407f, 0.009f),
        // 上部エンジン
        new Vector3(22.107f, 2.055f, 0.002f),
        // 宿舎上
        new Vector3(2.084f, 1.664f, 0.002f),
        // ドロシ左
        new Vector3(-11.268f, 8.235f, 0.008f),
        // ジップライン下
        new Vector3(3.543f, 6.969f, 0.007f),
    ];
    static private readonly Vector2[] PlayerSpawnPositionPool =
    [
        new(-9.81f, 0.6f),    // Campfire
        new(-8f, 10.5f),      // Dropship
        new(-16.16f, 7.25f),  // Cafeteria
        new(-15.5f, -7.5f),   // Kitchen
        new(9.25f, -12f),     // Greenhouse
        new(14.75f, 0f),      // UpperEngine
        new(21.65f, 13.75f)   // Comms
    ];
    protected override Vector2[] MapArea => MapPositions;
    protected override Vector2[] NonMapArea => [];
    public override Vector2[] OrpheusDeadBodySpawnPositionPool => orpheusDeadBodySpawnPositionPool;
    protected override Vector2[] PlayerSpawnPositions => PlayerSpawnPositionPool;
    protected override SystemTypes[] SabotageTypes => new SystemTypes[] { SystemTypes.Reactor, SystemTypes.Comms };
}
