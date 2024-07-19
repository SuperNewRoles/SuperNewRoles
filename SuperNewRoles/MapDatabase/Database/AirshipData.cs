using Agartha;
using UnityEngine;

namespace SuperNewRoles.MapDatabase.Database;

public class AirshipData : MapDatabase
{
    static private Vector2[] MapPositions = new Vector2[]
        { 
        //金庫
        new(-9f, 12.8f), new(-8.7f, 4.9f), new(-12.8f, 8.7f), new(-4.8f, 8.7f), new(-7.1f, 6.8f), new(-10.4f, 6.9f), new(-7f, 10.2f),
        //宿舎前
        new(-0.5f, 8.5f),
        //エンジン上
        new(-0.4f, 5f),
        //エンジン
        new(0f, -1.4f), new(3.6f, 0.1f), new(0.4f, -2.5f), new(-6.9f, 1.1f),
        //コミュ前
        new(-11f, -1f),
        //コミュ
        new(-12.3f, 0.9f),
        //コックピット
        new(-19.9f, -2.6f), new(-19.9f, 0.5f),
        //武器庫
        new(-14.5f, -3.6f), new(-9.9f, -6f), new(-15f, -9.4f),
        //キッチン
        new(-7.5f, -7.5f), new(-7f, -12.8f), new(-2.5f, -11.2f), new(-3.9f, -9.3f),
        //左展望
        new(-13.8f, -11.8f),
        //セキュ
        new(7.3f, -12.3f), new(5.8f, -10.6f),
        //右展望
        new(10.3f, -15f),
        //エレク
        new(10.5f, -8.5f),
        //エレクの9部屋
        new(10.5f, -6.3f), new(13.5f, -6.3f), new(16.5f, -6.3f), new(19.4f, -6.3f), new(13.5f, -8.8f), new(16.5f, -8.8f), new(19.4f, -8.8f), new(16.5f, -11f), new(19.4f, -11f),
        //エレク右上
        new(19.4f, -4.2f),
        //メディカル
        new(25.2f, -9.8f), new(22.9f, -6f), new(25.2f, -9.8f), new(29.5f, -6.3f),
        //貨物
        new(31.8f, -3.3f), new(34f, 1.4f), new(39f, -0.9f), new(37.6f, -3.4f), new(32.8f, 3.6f), new(35.3f, 3.6f),
        //ロミジュリ右
        new(29.8f, -1.5f),
        //ラウンジ
        new(33.7f, 7.1f), new(32.4f, 7.1f), new(30.9f, 7.1f), new(29.2f, 7.1f), new(30.8f, 5.3f), new(24.9f, 4.9f), new(27.1f, 7.3f),
        //レコード
        new(22.3f, 9.1f), new(20f, 11.5f), new(17.6f, 9.4f), new(20.1f, 6.6f),
        //ギャップ右
        new(15.4f, 9.2f), new(11.2f, 8.5f), new(12.6f, 6.2f),
        //シャワー/ロミジュリ左
        new(18.9f, 4.5f), new(17.2f, 5.2f), new(18.5f, 0f), new(21.2f, -2f), new(24f, 0.7f), new(22.3f, 2.5f),
        //メインホール
        new(10.8f, 0f), new(14.8f, 1.9f), new(11.8f, 1.8f), new(9.7f, 2.5f), new(6.2f, 2.4f), new(6.6f, -3f), new(12.7f, -2.9f),
        //ギャップ左
        new(3.8f, 8.8f),
        //ミーティング
        new(6.5f, 15.3f), new(11.8f, 14.1f), new(11.8f, 16f), new(16.3f, 15.2f),
        };

    protected override Vector2[] MapArea => MapPositions;
    protected override Vector2[] NonMapArea => [];

    protected override SystemTypes[] SabotageTypes => new SystemTypes[] { SystemTypes.HeliSabotage, SystemTypes.Comms, SystemTypes.Electrical };
}