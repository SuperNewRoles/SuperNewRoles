namespace SuperNewRoles.Roles
{
    public static class AllOpener
    {
        //そのうちリファクタします
        public static void AllDoorsOpen()
        {
            //スケルドはドア開けが効かない
            //ミラはドアがない
            if (PlayerControl.GameOptions.MapId == 0 || PlayerControl.GameOptions.MapId == 1)//スケルド・ミラ
            {
                //コミュサボ
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Comms, 19);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Comms, 18);
                //酸素
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.LifeSupp, 67);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.LifeSupp, 66);
                //リアクター
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Laboratory, 67);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Laboratory, 66);
                //リアクター
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Reactor, 19);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Reactor, 18);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Reactor, 67);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Reactor, 66);

            }
            else if (PlayerControl.GameOptions.MapId == 2 || PlayerControl.GameOptions.MapId == 4)
            {//amount(RpcRepairSystemの数字)がいくつあるかわからないため、とりあえず100
             //ドア開けるやつ
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 0);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 1);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 2);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 3);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 4);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 5);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 6);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 7);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 8);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 9);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 10);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 11);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 12);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 13);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 14);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 15);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 16);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 17);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 18);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 19);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 20);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 21);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 22);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 23);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 24);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 25);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 26);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 27);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 28);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 29);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 30);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 31);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 32);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 33);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 34);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 35);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 36);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 37);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 38);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 39);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 40);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 41);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 42);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 43);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 44);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 45);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 46);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 47);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 48);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 49);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 50);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 51);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 52);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 53);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 54);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 55);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 56);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 57);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 58);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 59);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 60);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 61);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 62);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 63);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 64);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 65);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 66);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 67);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 68);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 69);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 70);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 71);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 72);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 73);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 74);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 75);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 76);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 77);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 78);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 79);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 80);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 81);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 82);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 83);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 84);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 85);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 86);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 87);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 88);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 89);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 90);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 91);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 92);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 93);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 94);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 95);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 96);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 97);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 98);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 99);
                ShipStatus.Instance.RpcRepairSystem(SystemTypes.Doors, 100);
            }
        }
    }
}
