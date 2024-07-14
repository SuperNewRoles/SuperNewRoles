using System.Collections.Generic;
using AmongUs.GameOptions;
using Hazel;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using UnityEngine;
using static Il2CppSystem.Globalization.CultureInfo;

namespace SuperNewRoles.Roles.Impostor.Robber;

public class Robber : RoleBase, IImpostor, IDeathHandler, IRpcHandler
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
        new(RoleId.Robber, 206000, false,
            optionCreator: CreateOption);
    public static new IntroInfo Introinfo =
        new(RoleId.Robber, introSound: RoleTypes.Impostor);

    //巻き戻すタスクの個数
    public static CustomOption RewindTaskCountOption;
    private static void CreateOption()
    {
        RewindTaskCountOption = CustomOption.Create(Optioninfo.OptionId++, Optioninfo.SupportSHR, Optioninfo.RoleOption.type, "RobberRewindTaskCount",
            2, 1, 15, 1, Optioninfo.RoleOption);
    }
    public Robber(PlayerControl p) : base(p, Roleinfo, Optioninfo, Introinfo)
    {
    }
    public void OnMurderPlayer(DeathInfo info)
    {
        if (info.Killer == null)
            return;
        if (info.Killer.PlayerId != Player.PlayerId)
            return;
        if (info.Killer.PlayerId != PlayerControl.LocalPlayer.PlayerId)
            return;
        if (ModeHandler.IsMode(ModeId.Default))
        {
            MessageWriter writer = RpcWriter;
            writer.Write(info.DeathPlayer.PlayerId);
            HashSet<uint> taskIds = AssignRobberTargetTask(info.DeathPlayer);
            writer.Write((ushort)taskIds.Count);
            foreach (uint taskId in taskIds)
            {
                writer.Write(taskId);
            }
            SendRpc(writer);
        }
        /*
        else if (ModeHandler.IsMode(ModeId.SuperHostRoles))
        {
            HashSet<uint> taskIds = AssignRobberTargetTask(info.DeathPlayer);
            foreach (uint taskId in taskIds)
            {
                info.DeathPlayer.Data.FindTaskById(taskId).Complete = false;
            }
            foreach (PlayerTask task in info.DeathPlayer.myTasks)
            {
                if (!taskIds.Contains(task.Id))
                    continue;
                NormalPlayerTask normalTask = task.TryCast<NormalPlayerTask>();
                if (normalTask != null)
                    normalTask.taskStep = 0;
            }
            RPCHelper.RpcSyncGameData();
        }*/
        else
        {
            throw new System.Exception($"Robber.OnMurderPlayer: Invalid {ModeHandler.GetMode()} Mode");
        }
    }
    private HashSet<uint> AssignRobberTargetTask(PlayerControl target)
    {
        int RewindTaskCount = RewindTaskCountOption.GetInt();
        int CompletedTaskCount = target.Data.Tasks.FindAll((Il2CppSystem.Predicate<NetworkedPlayerInfo.TaskInfo>)(x => x.Complete)).Count;
        if (RewindTaskCount > CompletedTaskCount)
            RewindTaskCount = CompletedTaskCount;
        HashSet<uint> taskIds = new();
        List<NetworkedPlayerInfo.TaskInfo> CompletedTasks = target.Data.Tasks.FindAll((Il2CppSystem.Predicate<NetworkedPlayerInfo.TaskInfo>)(x => x.Complete)).ToList();
        for (int i = 0; i < RewindTaskCount; i++)
        {
            int index = ModHelpers.GetRandomIndex(CompletedTasks);
            taskIds.Add(CompletedTasks[index].Id);
            CompletedTasks.RemoveAt(index);
        }
        return taskIds;
    }
    public void RpcReader(MessageReader reader)
    {
        byte targetId = reader.ReadByte();
        PlayerControl target = ModHelpers.PlayerById(targetId);
        int TaskCount = reader.ReadUInt16();
        List<uint> taskIds = new();
        for (int i = 0; i < TaskCount; i++)
        {
            taskIds.Add(reader.ReadUInt32());
        }
        for (int i = target.myTasks.Count - 1; i >= 0; i--)
        {
            PlayerTask task = target.myTasks[i];
            if (!taskIds.Contains(task.Id))
                continue;
            NetworkedPlayerInfo.TaskInfo taskInfo = target.Data.FindTaskById(task.Id);
            if (taskInfo != null)
                taskInfo.Complete = false;
            target.myTasks.RemoveAt(i);
            GameObject.Destroy(task.gameObject);
            NormalPlayerTask taskById = ShipStatus.Instance.GetTaskById(taskInfo.TypeId);
            NormalPlayerTask normalPlayerTask = Object.Instantiate(taskById, target.transform);
            normalPlayerTask.Id = taskInfo.Id;
            normalPlayerTask.Index = taskById.Index;
            normalPlayerTask.Owner = target;
            normalPlayerTask.Initialize();
            target.logger.Info($"Assigned task {normalPlayerTask.name} to {target.PlayerId}");
            target.myTasks.Add(normalPlayerTask);
        }
    }
}