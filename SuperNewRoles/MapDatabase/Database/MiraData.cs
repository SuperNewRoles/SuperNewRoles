using UnityEngine;

namespace SuperNewRoles.MapDatabase.Database;

public class MiraData : MapDatabase
{
    static private Vector2[] MapPositions = new Vector2[]
    {
        //ラウンチパッド
        new(-4.4f, 3.3f),
        //ランチパッド下通路
        new(3.7f, -1.7f),
        //メッドベイ
        new(15.2f, 0.4f),
        //コミュ
        new(14f, 4f),
        //三叉路
        new(12.3f, 6.7f), new(23.6f, 6.8f),
        //ロッカー
        new(9f, 5f), new(8.4f, 1.4f),
        //デコン
        new(6.0f, 5.6f),
        //デコン上通路
        new(6.0f, 11.6f),
        //リアクター
        new(2.5f, 10.3f), new(2.5f, 13f),
        //ラボラトリ
        new(7.6f, 13.9f), new(9.7f, 10.4f), new(10.7f, 12.2f),
        //カフェ
        new(21.8f, 5f), new(10.7f, 12.2f), new(28.3f, 0.2f), new(25.5f, 2.3f), new(22.1f, 2.6f),
        //ストレージ
        new(19.2f, 1.7f), new(18.5f, 4.2f),
        //バルコニー
        new(18.3f, -3.2f), new(23.7f, -1.9f),
        //三叉路上通路
        new(17.8f, 19f),
        //オフィス
        new(15.7f, 17.2f), new(13.7f, 20.4f), new(13.6f, 18.7f),
        //アドミン
        new(20.6f, 20.8f), new(22.3f, 18.6f), new(21.2f, 17.3f), new(19.4f, 17.6f),
        //グリーンハウス
        new(13.2f, 22.3f), new(22.4f, 23.3f), new(20.2f, 24.3f), new(16.5f, 24.4f), new(20.7f, 22.2f), new(18f, 25.3f),
    };

    protected override Vector2[] MapArea => MapPositions;
    protected override Vector2[] NonMapArea => [];
    protected override SystemTypes[] SabotageTypes => new SystemTypes[] { SystemTypes.Reactor, SystemTypes.Comms, SystemTypes.Electrical, SystemTypes.LifeSupp };
}