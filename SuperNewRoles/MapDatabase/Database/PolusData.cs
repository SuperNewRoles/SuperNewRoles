using Agartha;
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

    protected override Vector2[] MapArea => MapPositions;
    protected override Vector2[] NonMapArea => [];
    protected override SystemTypes[] SabotageTypes => new SystemTypes[] { SystemTypes.Laboratory, SystemTypes.Comms, SystemTypes.Electrical };
}