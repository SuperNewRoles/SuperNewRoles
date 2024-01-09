using AmongUs.GameOptions;
using Hazel;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using System.Collections.Generic;

namespace SuperNewRoles.Roles.Impostor.Robber;

public class Robber : RoleBase, IImpostor, ISupportSHR, IDeathHandler, IRpcHandler
{
    public static new RoleInfo Roleinfo = new(
        typeof(Robber),
        (p) => new Robber(p),
        RoleId.Robber,
        "Robber",
        RoleClass.ImpostorRed,
        new(RoleId.Robber, TeamTag.Impostor),
        TeamRoleType.Impostor,
        TeamType.Impostor
        );
    public static new OptionInfo Optioninfo =
        new(RoleId.Robber, 205900, true,
            optionCreator: CreateOption);
    public static new IntroInfo Introinfo =
        new(RoleId.Robber, introSound: RoleTypes.Impostor);

    public RoleTypes RealRole => RoleTypes.Impostor;

    //巻き戻すタスクの個数
    public static CustomOption RewindTaskCountOption;
    private static void CreateOption()
    {
        RewindTaskCountOption = CustomOption.Create(Optioninfo.OptionId++, true, Optioninfo.RoleOption.type, "RobberRewindTaskCount",
            2, 1, 15, 1, Optioninfo.RoleOption);
    }
    public Robber(PlayerControl p) : base(p, Roleinfo, Optioninfo, Introinfo)
    {
    }
    public void OnMurderPlayer(DeathInfo info)
    {
        if (info.Killer == null)
            return;
        if (info.Killer.PlayerId == Player.PlayerId)
            return;
        if (ModeHandler.IsMode(ModeId.Default))
        {
            MessageWriter writer = RpcWriter;
            writer.Write(info.DeathPlayer.PlayerId);
            uint[] taskIds = AssignRobberTargetTask(info.DeathPlayer);
            writer.Write((ushort)taskIds.Length);
            foreach (uint taskId in taskIds)
            {
                writer.Write(taskId);
            }
            SendRpc(writer);
        }
        else if (ModeHandler.IsMode(ModeId.SuperHostRoles))
        {
            uint[] taskIds = AssignRobberTargetTask(info.DeathPlayer);
            foreach (uint taskId in taskIds)
            {
                Player.Data.FindTaskById(taskId).Complete = false;
            }
            RPCHelper.RpcSyncGameData();
        }
        else
        {
            throw new System.Exception("Robber.OnMurderPlayer: Invalid Mode");
        }
    }
    private uint[] AssignRobberTargetTask(PlayerControl target)
    {
        int RewindTaskCount = RewindTaskCountOption.GetInt();
        int CompletedTaskCount = target.Data.Tasks.FindAll((Il2CppSystem.Predicate<GameData.TaskInfo>)(x => x.Complete)).Count;
        if (RewindTaskCount > CompletedTaskCount)
            RewindTaskCount = CompletedTaskCount;
        uint[] taskIds = new uint[RewindTaskCount];
        List<GameData.TaskInfo> CompletedTasks = target.Data.Tasks.FindAll((Il2CppSystem.Predicate<GameData.TaskInfo>)(x => x.Complete)).ToList();
        for (int i = 0; i < RewindTaskCount; i++)
        {
            int index = ModHelpers.GetRandomIndex(CompletedTasks);
            taskIds[i] = CompletedTasks[index].Id;
            CompletedTasks.RemoveAt(index);
        }
        return taskIds;
    }
    public void RpcReader(MessageReader reader)
    {
        byte targetId = reader.ReadByte();
        PlayerControl target = ModHelpers.PlayerById(targetId);
        int TaskCount = reader.ReadUInt16();
        for (int i = 0; i < TaskCount; i++)
        {
            target.Data.FindTaskById(reader.ReadUInt32()).Complete = false;
        }
    }
}