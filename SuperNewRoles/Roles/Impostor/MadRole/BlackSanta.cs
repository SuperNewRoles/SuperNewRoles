
using System.Collections.Generic;
using AmongUs.GameOptions;
using Hazel;
using SuperNewRoles.Patches;
using SuperNewRoles.Roles.Crewmate;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;

namespace SuperNewRoles.Roles.Impostor.MadRole;

public class BlackSanta : RoleBase, IMadmate, ICustomButton, IRpcHandler
{
    public static new RoleInfo Roleinfo = new(
        typeof(BlackSanta),
        (p) => new BlackSanta(p),
        RoleId.BlackSanta,
        "BlackSanta",
        RoleClass.ImpostorRed,
        new(RoleId.BlackSanta, TeamTag.Crewmate,
            RoleTag.Takada),
        TeamRoleType.Crewmate,
        TeamType.Impostor
        );
    public static new OptionInfo Optioninfo =
        new(RoleId.BlackSanta, 406600, false,
            CoolTimeOption: (30f, 2.5f, 60f, 2.5f, false),
            VentOption: (false, false),
            ImpostorVisionOption: (false, false),
            optionCreator: CreateOption);
    public static new IntroInfo Introinfo =
        new(RoleId.BlackSanta, introSound: RoleTypes.Crewmate);
    //マッド設定
    public static CustomOption CanCheckImpostorOption;
    public static CustomOption IsSettingNumberOfUniqueTasks;
    public static CustomOption CommonTaskOption;
    public static CustomOption ShortTaskOption;
    public static CustomOption LongTaskOption;
    public static CustomOption IsParcentageForTaskTrigger;
    public static CustomOption ParcentageForTaskTriggerSetting;
    public static CustomOption CanUseVentOption;

    public static RoleId[] PresetRolesParam { get; } = new RoleId[]
    {
        RoleId.EvilGuesser,
        RoleId.EvilMechanic,
        RoleId.SelfBomber,
        RoleId.Slugger,
        RoleId.Penguin,
        RoleId.WaveCannon
    };
    private static CustomOption[] PresetRoleOptions { get; set; }

    private static CustomOption CanUseAbilityCount { get; set; }
    private static CustomOption TryLoversToDeath { get; set; }

    private static void CreateOption()
    {
        CanCheckImpostorOption = CustomOption.Create(Optioninfo.OptionId++, Optioninfo.SupportSHR, CustomOptionType.Crewmate, "MadmateIsCheckImpostorSetting", false, Optioninfo.RoleOption);
        IsSettingNumberOfUniqueTasks = CustomOption.Create(Optioninfo.OptionId++, true, CustomOptionType.Crewmate, "IsSettingNumberOfUniqueTasks", true, CanCheckImpostorOption);
        var taskOpt = SelectTask.TaskSetting(Optioninfo.OptionId++, Optioninfo.OptionId++, Optioninfo.OptionId++, IsSettingNumberOfUniqueTasks, CustomOptionType.Crewmate, Optioninfo.SupportSHR);
        CommonTaskOption = taskOpt.Item1;
        ShortTaskOption = taskOpt.Item2;
        LongTaskOption = taskOpt.Item3;
        IsParcentageForTaskTrigger = CustomOption.Create(Optioninfo.OptionId++, Optioninfo.SupportSHR, CustomOptionType.Crewmate, "IsParcentageForTaskTrigger", true, CanCheckImpostorOption);
        ParcentageForTaskTriggerSetting = CustomOption.Create(Optioninfo.OptionId++, Optioninfo.SupportSHR, CustomOptionType.Crewmate, "ParcentageForTaskTriggerSetting", CustomOptionHolder.rates4, IsParcentageForTaskTrigger);
        CanUseAbilityCount = CustomOption.Create(Optioninfo.OptionId++, false, Optioninfo.RoleOption.type,
            "SantaCanUseAbilityCount", 1, 1, 15, 1,
            Optioninfo.RoleOption);
        TryLoversToDeath = CustomOption.Create(Optioninfo.OptionId++, false, Optioninfo.RoleOption.type,
            string.Format(
                ModTranslation.GetString(
                    "SantaTryRoleToDeath"
                ),
                CustomOptionHolder.Cs(RoleClass.Lovers.color, "LoversName")
        ), false, Optioninfo.RoleOption);
        PresetRoleOptions = new CustomOption[PresetRolesParam.Length];
        for (int i = 0; i < PresetRolesParam.Length; i++)
        {
            PresetRoleOptions[i] = CustomOption.Create(Optioninfo.OptionId++, false, Optioninfo.RoleOption.type,
                               string.Format(
                                   ModTranslation.GetString("SantaPresentRoleOptionFormat"),
                                   CustomRoles.GetRoleNameOnColor(PresetRolesParam[i])
                               ),
                               CustomOptionHolder.ratesper5, Optioninfo.RoleOption);
        }
    }

    public bool CanUseVent => Optioninfo.CanUseVent;

    public bool HasCheckImpostorAbility { get; }
    public int CheckTask { get; }

    public bool IsImpostorLight { get; }

    public CustomButtonInfo[] CustomButtonInfos { get; }
    private CustomButtonInfo BlackSantaButtonInfo;
    private List<RoleId> RoleAssignTickets { get; }
    private void BlackSantaOnClick()
    {
        MessageWriter writer = Santa.ButtonOnClick(BlackSantaButtonInfo,
            RpcWriter, RoleAssignTickets, (target) =>
            {
                if (!target.IsImpostor())
                    return true;
                if (TryLoversToDeath.GetBool() && target.IsLovers())
                    return true;
                return false;
            });
        if (writer != null)
            SendRpc(writer);
    }
    public void RpcReader(MessageReader reader)
    {
        byte targetId = reader.ReadByte();
        if (targetId == 255)
        {
            Player.MurderPlayer(Player, MurderResultFlags.Succeeded);
            RPCProcedure.SetFinalStatus(Player.PlayerId, FinalStatus.SantaSelf);
            return;
        }
        RoleId role = (RoleId)reader.ReadInt16();
        RPCProcedure.SetRole(targetId, (byte)role);
        if (Player.PlayerId != PlayerControl.LocalPlayer.PlayerId)
            BlackSantaButtonInfo.AbilityCount--;
    }
    public BlackSanta(PlayerControl p) : base(p, Roleinfo, Optioninfo, Introinfo)
    {
        bool IsFullTask = !IsParcentageForTaskTrigger.GetBool();
        int AllTask = SelectTask.GetTotalTasks(RoleId.BlackSanta);
        CheckTask = IsFullTask ? AllTask : (AllTask * (ParcentageForTaskTriggerSetting.GetSelection() / 4));
        HasCheckImpostorAbility = CanCheckImpostorOption.GetBool();
        IsImpostorLight = Optioninfo.IsImpostorVision;
        BlackSantaButtonInfo = Santa.CreateSantaButtonInfo(this, CanUseAbilityCount.GetInt(),
            () => BlackSantaOnClick(), "BlackSantaButton", () => Optioninfo.CoolTime);
        CustomButtonInfos = new CustomButtonInfo[1] { BlackSantaButtonInfo };
        RoleAssignTickets = new();
        for (int i = 0; i < PresetRoleOptions.Length; i++)
        {
            RoleId roleId = PresetRolesParam[i];
            int ticketcount = PresetRoleOptions[i].GetSelection();
            SetTicket(roleId, ticketcount);
        }
        //もし全て0%ならすべて同じ確率で設定する
        if (RoleAssignTickets.Count <= 0)
        {
            for (int i = 0; i < PresetRoleOptions.Length; i++)
            {
                RoleId roleId = PresetRolesParam[i];
                SetTicket(roleId, 1);
            }
        }
    }

    /// <summary>
    /// ブラックサンタのプレゼント対象役の情報
    /// </summary>
    /// <param name=</param>
    /// <returns>RoleInfo : プレゼント対象役のRoleInfo, IntroData : プレゼント対象役のIntroData</returns>
    /// FIXME : IntroDataは全ての役がRoleBase対応したら削除する
    public static (List<RoleInfo> RoleInfo, List<IntroData> IntroData) PresentRoleData()
    {
        List<RoleInfo> roleInfo = new();
        List<IntroData> introData = new();

        for (int i = 0; i < PresetRoleOptions.Length; i++)
        {
            RoleId roleId = PresetRolesParam[i];
            int ticketcount = PresetRoleOptions[i].GetSelection();

            if (Optioninfo.RoleOption.GetSelection() is not 0 && PresetRoleOptions[i].GetSelection() > 0) // 設定で有効になっている役職のみ処理
            {
                IntroData intro = IntroData.GetIntrodata(roleId);

                if (intro != IntroData.CrewmateIntro && intro != IntroData.ImpostorIntro) // RoleBase化が終わったらIntroDataを削除し, RoleInfoのみ(elseの中身のみ)にする。
                {
                    introData.Add(intro);
                    Logger.Info($"プレゼント対象役 : {intro.RoleId}", "BlackSanta");
                }
                else
                {
                    RoleInfo info = RoleInfoManager.GetRoleInfo(roleId);
                    roleInfo.Add(info);
                    Logger.Info($"プレゼント対象役 : {info.Role}", "BlackSanta");
                }
            }
        }
        return (roleInfo, introData);
    }
    private void SetTicket(RoleId roleId, int count)
    {
        for (int i = 0; i < count; i++)
            RoleAssignTickets.Add(roleId);
    }

}