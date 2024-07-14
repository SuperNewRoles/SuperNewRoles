using System;
using AmongUs.GameOptions;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomObject;
using SuperNewRoles.Helpers;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using UnityEngine;

namespace SuperNewRoles.Roles.Impostor;

public class Conjurer : RoleBase, IImpostor, IWrapUpHandler, ICustomButton
{
    public static new RoleInfo Roleinfo = new(
        typeof(Conjurer),
        (p) => new Conjurer(p),
        RoleId.Conjurer,
        "Conjurer",
        RoleClass.ImpostorRed,
        new(RoleId.Conjurer, TeamTag.Impostor,
            RoleTag.SpecialKiller, RoleTag.Killer, RoleTag.CustomObject),
        TeamRoleType.Impostor,
        TeamType.Impostor
        );
    public static new OptionInfo Optioninfo =
        new(RoleId.Conjurer, 205500, false,
            CoolTimeOption: (10f, 1f, 30f, 0.5f, false),
            optionCreator: CreateOption);
    public static new IntroInfo Introinfo =
        new(RoleId.Conjurer, introSound: RoleTypes.Impostor);

    // 設定
    public static CustomOption CanAddLength;
    public static CustomOption CanKillImpostor;
    public static CustomOption ShowFlash;
    public static void CreateOption()
    {
        CanAddLength = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Impostor, "ConjurerCanAddLengthOption", 10f, 0.5f, 40f, 0.5f, Optioninfo.RoleOption);
        CanKillImpostor = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Impostor, "ConjurerCanKillImpostorOption", false, Optioninfo.RoleOption);
        ShowFlash = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Impostor, "ConjurerShowFlashOption", false, Optioninfo.RoleOption);
    }

    public int Count; // 何回設置したか
    //public static int Round; // 何週目か
    public Vector2[] Positions;

    public CustomButtonInfo[] CustomButtonInfos { get; }

    //ボタン
    public void BeaconButtonOnClick()
    {
        Logger.Info($"Now:{Count}", "Conjurer Add");
        byte[] buff = new byte[sizeof(float) * 2];
        Buffer.BlockCopy(BitConverter.GetBytes(PlayerControl.LocalPlayer.transform.position.x), 0, buff, 0 * sizeof(float), sizeof(float));
        Buffer.BlockCopy(BitConverter.GetBytes(PlayerControl.LocalPlayer.transform.position.y), 0, buff, 1 * sizeof(float), sizeof(float));

        AddBeacon(PlayerControl.LocalPlayer.PlayerId, buff);

        // PositionsのCount番目に現在の座標を保存する
        Positions[Count] = PlayerControl.LocalPlayer.transform.position;

        Count++;
        Logger.Info($"Now:{Count}", "Conjurer Added");

        CustomButtonInfos[0].ResetCoolTime();
    }
    public void StartButtonOnClick()
    {
        //Logger.Info($"Beacon{Round}{Count}", "Beacons");
        foreach (PlayerControl pc in CachedPlayer.AllPlayers.AsSpan())
        {
            // プレイヤーがPositionsで形成された三角形の中にいる
            if (!PointInPolygon(pc.transform.position, Positions))
                continue;
            if (pc.IsDead())
                continue;
            //インポスターがキル対象ではないかつインポスターの場合はパス
            if (!CanKillImpostor.GetBool() && pc.IsImpostor())
                continue;
            if (pc.IsRole(RoleId.Shielder) && RoleClass.Shielder.IsShield.ContainsKey(pc.PlayerId) && RoleClass.Shielder.IsShield[pc.PlayerId])
            {
                MessageWriter msgwriter = RPCHelper.StartRPC(CustomRPC.ShielderProtect);
                msgwriter.Write(CachedPlayer.LocalPlayer.PlayerId);
                msgwriter.Write(pc.PlayerId);
                msgwriter.Write(0);
                msgwriter.EndRPC();
                RPCProcedure.ShielderProtect(CachedPlayer.LocalPlayer.PlayerId, pc.PlayerId, 0);
                continue;
            }
            pc.RpcMurderPlayer(pc, true);
        }
        if (ShowFlash.GetBool())
        { // Rpcにして、全視点光らせる
            RPCHelper.StartRPC(CustomRPC.ShowFlash).EndRPC();
            RPCProcedure.ShowFlash();
        }
        Beacon.ClearBeacons();
        CustomButtonInfos[1].ResetCoolTime();
        Count = 0;
    }

    public Conjurer(PlayerControl p) : base(p, Roleinfo, Optioninfo, Introinfo)
    {
        Count = 0;
        //Round = 0;
        Positions = new Vector2[] { new(), new(), new() };
        CustomButtonInfos = new CustomButtonInfo[2]
        {
            //ビーコンボタン
            new(null, this, BeaconButtonOnClick, (isAlive) => isAlive, CustomButtonCouldType.CanMove, null,
            ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.ConjurerBeaconButton.png", 115f),
            () => Optioninfo.CoolTime, new(0,1,0), "ConjurerBeaconName",
            KeyCode.Q, 8, CouldUse:CanAddBeacon),
            //ビーコンスタートボタン
            new(null, this, StartButtonOnClick, (isAlive) => isAlive,
            CustomButtonCouldType.CanMove, () => CustomButtonInfos[0].ResetCoolTime(),
            ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.ConjurerStartButton.png", 115f),
            null, new(-1.8f, -0.06f, 0),
            "ConjurerAddName", KeyCode.F, 48,
            CouldUse:() => Count >= 3)
        };
    }
    private bool CanAddBeacon()
    {
        if (Count <= 0)
            return true;
        if (Count >= 3)
            return false;
        return
            Vector2.Distance(
                PlayerControl.LocalPlayer.transform.position,
                Positions[Count - 1]
               ) < CanAddLength.GetFloat();
    }
    /// <summary>
    /// pがpolyから形成された多角形の中にあるか
    /// </summary>
    /// <param name="p">調べたい点</param>
    /// <param name="poly">多角形の頂点</param>
    /// <returns>多角形の中にある</returns>
    static bool PointInPolygon(Vector2 p, Vector2[] poly)
    {
        Vector2 p1, p2;
        bool inside = false;
        Vector2 oldPoint = poly[^1];
        for (int i = 0; i < poly.Length; i++)
        {
            Vector2 newPoint = poly[i];
            if (newPoint.x > oldPoint.x) { p1 = oldPoint; p2 = newPoint; }
            else { p1 = newPoint; p2 = oldPoint; }
            if ((p1.x < p.x) == (p.x <= p2.x) && (p.y - p1.y) * (p2.x - p1.x) < (p2.y - p1.y) * (p.x - p1.x))
            {
                inside = !inside;
            }
            oldPoint = newPoint;
        }
        return inside;
    }

    public static void AddBeacon(byte sourceId, byte[] buff)
    {
        PlayerControl source = ModHelpers.PlayerById(sourceId);
        if (source == null)
            return;
        Vector3 position = Vector3.zero;
        position.x = BitConverter.ToSingle(buff, 0 * sizeof(float));
        position.y = BitConverter.ToSingle(buff, 1 * sizeof(float));
        new Beacon(source, position);
    }
    public void OnWrapUp()
    {
        Beacon.ClearBeacons();
        Count = 0;
    }
}