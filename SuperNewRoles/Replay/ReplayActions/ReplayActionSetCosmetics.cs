using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SuperNewRoles.Replay.ReplayActions;
public enum ReplayCosmeticsType
{
    Name,
    Color,
    Hat,
    Pet,
    Visor,
    NamePlate,
    Skin
}
public class ReplayActionSetCosmetics : ReplayAction
{
    public byte targetPlayer;
    public ReplayCosmeticsType CosType;
    public string ChangeTarget;
    public int colorId;
    public bool dontCensor;
    public override void ReadReplayFile(BinaryReader reader)
    {
        ActionTime = reader.ReadSingle();
        //ここにパース処理書く
        targetPlayer = reader.ReadByte();
        CosType = (ReplayCosmeticsType)reader.ReadByte();
        if (CosType != ReplayCosmeticsType.Color) ChangeTarget = reader.ReadString();
        if (CosType != ReplayCosmeticsType.NamePlate && CosType != ReplayCosmeticsType.Name)
            colorId = reader.ReadInt32();
        else if (CosType == ReplayCosmeticsType.Name)
            dontCensor = reader.ReadBoolean();
    }
    public override void WriteReplayFile(BinaryWriter writer)
    {
        writer.Write(ActionTime);
        //ここにパース処理書く
        writer.Write(targetPlayer);
        writer.Write((byte)CosType);
        if (CosType != ReplayCosmeticsType.Color) writer.Write(ChangeTarget);
        if (CosType != ReplayCosmeticsType.NamePlate && CosType != ReplayCosmeticsType.Name)
            writer.Write(colorId);
        else if (CosType == ReplayCosmeticsType.Name)
            writer.Write(dontCensor);
    }
    public override ReplayActionId GetActionId() => ReplayActionId.SetCosmetics;
    //アクション実行時の処理
    public override void OnAction()
    {
        PlayerControl target = ModHelpers.PlayerById(targetPlayer);
        if (target == null)
        {
            Logger.Info($"アクションを実行しようとしましたが、対象がいませんでした。target:{targetPlayer}");
            return;
        }
        switch (CosType)
        {
            case ReplayCosmeticsType.Name:
                target.SetName(ChangeTarget, dontCensor);
                break;
            case ReplayCosmeticsType.Color:
                target.SetColor(colorId);
                break;
            case ReplayCosmeticsType.Hat:
                target.SetHat(ChangeTarget, colorId);
                break;
            case ReplayCosmeticsType.Pet:
                target.SetPet(ChangeTarget, colorId);
                break;
            case ReplayCosmeticsType.Visor:
                target.SetVisor(ChangeTarget, colorId);
                break;
            case ReplayCosmeticsType.NamePlate:
                target.SetNamePlate(ChangeTarget);
                break;
            case ReplayCosmeticsType.Skin:
                target.SetSkin(ChangeTarget, colorId);
                break;
            default:
                Logger.Info($"アクションを実行しようとしましたが、CosTypeがねえよ。target:{targetPlayer},CosType:{CosType}");
                break;
        }
    }
    public override void OnReplay()
    {
        PlayerControl target = ModHelpers.PlayerById(targetPlayer);
        if (target == null)
        {
            Logger.Info($"アクションを実行しようとしましたが、対象がいませんでした。target:{targetPlayer}");
            return;
        }
        ReplayAction action = GetLastAction(this, (ReplayAction act) => (act as ReplayActionSetCosmetics).targetPlayer == targetPlayer && (act as ReplayActionSetCosmetics).CosType == CosType);
        if (action == null)
        {
            switch (CosType)
            {
                case ReplayCosmeticsType.Name:
                    target.SetName(ChangeTarget, dontCensor);
                    break;
                case ReplayCosmeticsType.Color:
                    target.SetColor(colorId);
                    break;
                case ReplayCosmeticsType.Hat:
                    target.SetHat("", colorId);
                    break;
                case ReplayCosmeticsType.Pet:
                    target.SetPet("", colorId);
                    break;
                case ReplayCosmeticsType.Visor:
                    target.SetVisor("", colorId);
                    break;
                case ReplayCosmeticsType.NamePlate:
                    target.SetNamePlate("");
                    break;
                case ReplayCosmeticsType.Skin:
                    target.SetSkin("", colorId);
                    break;
                default:
                    Logger.Info($"アクションを実行しようとしましたが、CosTypeがねえよ。target:{targetPlayer},CosType:{CosType}");
                    break;
            }
        }
        else
        {
            ReplayActionSetCosmetics rasc = action as ReplayActionSetCosmetics;
            switch (rasc.CosType)
            {
                case ReplayCosmeticsType.Name:
                    target.SetName(rasc.ChangeTarget, rasc.dontCensor);
                    break;
                case ReplayCosmeticsType.Color:
                    target.SetColor(rasc.colorId);
                    break;
                case ReplayCosmeticsType.Hat:
                    target.SetHat(rasc.ChangeTarget, rasc.colorId);
                    break;
                case ReplayCosmeticsType.Pet:
                    target.SetPet(rasc.ChangeTarget, rasc.colorId);
                    break;
                case ReplayCosmeticsType.Visor:
                    target.SetVisor(rasc.ChangeTarget, rasc.colorId);
                    break;
                case ReplayCosmeticsType.NamePlate:
                    target.SetNamePlate(rasc.ChangeTarget);
                    break;
                case ReplayCosmeticsType.Skin:
                    target.SetSkin(rasc.ChangeTarget, rasc.colorId);
                    break;
                default:
                    Logger.Info($"アクションを実行しようとしましたが、CosTypeがねえよ。target:{targetPlayer},CosType:{CosType}");
                    break;
            }
        }

    }
    //試合内でアクションがあったら実行するやつ
    public static ReplayActionSetCosmetics Create(byte targetPlayer, ReplayCosmeticsType costype, string changeTarget, int color = 0, bool dontCensor = false)
    {
        ReplayActionSetCosmetics action = new();
        if (!CheckAndCreate(action)) return null;
        //初期化
        action.targetPlayer = targetPlayer;
        action.CosType = costype;
        action.ChangeTarget = changeTarget;
        action.colorId = color;
        action.dontCensor = dontCensor;
        return action;
    }
}