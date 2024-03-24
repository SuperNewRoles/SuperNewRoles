
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AmongUs.GameOptions;
using Hazel;
using SuperNewRoles.CustomCosmetics;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;

namespace SuperNewRoles.Roles.CrewMate;

public class SilverBullet : RoleBase, IImpostor, ISupportSHR, ICustomButton, IRpcHandler, IVentAvailable, IMeetingHandler
{
    public static new RoleInfo Roleinfo = new(
        typeof(SilverBullet),
        (p) => new SilverBullet(p),
        RoleId.SilverBullet,
        "SilverBullet",
        new(204, 204, 204, byte.MaxValue),
        new(RoleId.SilverBullet, TeamTag.Crewmate,
            RoleTag.Information),
        TeamRoleType.Crewmate,
        TeamType.Crewmate
        );
    public static new OptionInfo Optioninfo =
        new(RoleId.SilverBullet, 206800, true,
            CoolTimeOption: (30,2.5f,60,2.5f, true),
            optionCreator: CreateOption);
    public static new IntroInfo Introinfo =
        new(RoleId.SilverBullet, introSound: RoleTypes.Engineer);

    /*
    解析の使用可能回数
        (初期値:1回, 最低値:1, 最大値:15, 間隔:1)
    解析で使用者の明暗までわかるか
        初期値:OFF (ON/OFF)
    リペアを使用出来る
        初期値:OFF (ON/OFF)
    リペアの使用可能回数　//「リペアを使用出来る」が有効のときのみ有効
        (初期値:1回, 最低値:1, 最大値:15, 間隔:1)
*/
    public static CustomOption AnalysisCountOption;
    public static CustomOption AnalysisLightOption;
    public static CustomOption CanUseRepairOption;
    public static CustomOption RepairCountOption;

    private static void CreateOption()
    {
        AnalysisCountOption = CustomOption.Create(Optioninfo.OptionId++, Optioninfo.SupportSHR, Optioninfo.RoleOption.type, "SilverBulletAnalyzeCanUseCountOption", 1, 1, 15, 1, Optioninfo.RoleOption);
        AnalysisLightOption = CustomOption.Create(Optioninfo.OptionId++, Optioninfo.SupportSHR, Optioninfo.RoleOption.type, "SilverBulletAnalyzeCanCheckSentimentOption", false, Optioninfo.RoleOption);
        CanUseRepairOption = CustomOption.Create(Optioninfo.OptionId++, Optioninfo.SupportSHR, Optioninfo.RoleOption.type, "SilverBulletCanUseRepairOption", false, Optioninfo.RoleOption);
        RepairCountOption = CustomOption.Create(Optioninfo.OptionId++, Optioninfo.SupportSHR, Optioninfo.RoleOption.type, "SilverBulletCanRepairUseCountOption", 1, 1, 15, 1, CanUseRepairOption);
    }

    /*
     boolean: IsRepair
     */

    void IRpcHandler.RpcReader(MessageReader reader)
    {
        if (reader.ReadBoolean())
        {
            CanRepairCount--;
            if (Player.IsMushroomMixupActive())
            {
                Sabotage.FixSabotage.RepairProcsee.ReceiptOfSabotageFixing(TaskTypes.MushroomMixupSabotage);
                return;
            }
            foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks)
            {
                if (!(task.TaskType is TaskTypes.FixLights or TaskTypes.RestoreOxy or TaskTypes.ResetReactor or TaskTypes.ResetSeismic or TaskTypes.FixComms or TaskTypes.StopCharles))
                    continue;
                Sabotage.FixSabotage.RepairProcsee.ReceiptOfSabotageFixing(task.TaskType);
            }
            return;
        }
        AnalysisCount--;
    }

    public void StartMeeting()
    {
        AddAnalyzeChats();
        if (LastUsedVentData == null)
            return;
        LastUsedVentData = new();
        foreach (VentInfo ventInfo in VentInfo.VentInfos.Values)
        {
            LastUsedVentData[ventInfo.VentId] = [.. ventInfo.UsedPlayersCurrentTurn];
        }
    }

    public void CloseMeeting()
    {
    }

    public List<int> WillSendChat = new();

    public int AnalysisCount { get; private set; }
    public int CanRepairCount { get; private set; }

    public CustomButtonInfo[] CustomButtonInfos { get; }
    public CustomButtonInfo AnalyzeButtonInfo;
    public CustomButtonInfo RepairButtonInfo;

    public RoleTypes RealRole => RoleTypes.Shapeshifter;

    public SilverBullet(PlayerControl p) : base(p, Roleinfo, Optioninfo, Introinfo)
    {
        AnalysisCount = AnalysisCountOption.GetInt();
        CanRepairCount = RepairCountOption.GetInt();

        AnalyzeButtonInfo = new(AnalysisCount, this, AnalyzeOnClick,
            (isAlive) => isAlive, CustomButtonCouldType.CanMove, null,
            ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.SilverBulletAnalyzeButton.png",115f),
            () => 0f, new(), "SilverBulletAnalyzeButtonName",
            UnityEngine.KeyCode.F, 49, CouldUse:AnalyzeCouldUse,
            HasAbilityCountText:true);
        RepairButtonInfo = new(CanRepairCount, this, RepairOnClick,
            (isAlive) => CanUseRepairOption.GetBool() && isAlive, CustomButtonCouldType.CanMove, null,
            ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.SilverBulletRepairButton.png",115f),
            () => 0f, new(), "SilverBulletRepairButtonName",
            UnityEngine.KeyCode.R, 50, CouldUse:() => RoleHelpers.IsSabotage(),
            HasAbilityCountText:true);

        CustomButtonInfos = [AnalyzeButtonInfo, RepairButtonInfo];

        LastUsedVentData = new();
    }
    private void RepairOnClick()
    {
        if (!CanUseRepairOption.GetBool())
            return;
        MessageWriter writer = RpcWriter;
        writer.Write(true);
        SendRpc(writer);
    }
    private void AnalyzeOnClick()
    {
        if (AnalyzeTargetVent == null)
            return;
        WillSendChat.Add(AnalyzeTargetVent.Id);
        MessageWriter writer = RpcWriter;
        writer.Write(false);
        SendRpc(writer);
    }
    private bool AnalyzeCouldUse()
    {
        return AnalysisCount > 0 && (AnalyzeTargetVent = ModHelpers.SetTargetVent()) != null;
    }
    private Vent AnalyzeTargetVent;
    public const string TextLine = "|-------------------------------------------------------------|";

    public static Dictionary<int, List<byte>> LastUsedVentData;

    public void AddAnalyzeChats()
    {
        if (WillSendChat.Count <= 0)
            return;
        string BaseText = ModHelpers.Cs(Roleinfo.RoleColor, TextLine + "\n" +
            $"|{ModTranslation.GetString(Roleinfo.NameKey + "Name")}|\n" +
            TextLine) + "\n\n";
        foreach (int ventId in WillSendChat)
        {
            StringBuilder text = new(BaseText);
            if (!LastUsedVentData.TryGetValue(ventId, out List<byte> usedPlayers) ||
                usedPlayers.Count <= 0)
                text.Append(ModTranslation.GetString("SilverBulletVentNotUsed"));
            else
            {
                text.Append(string.Format(ModTranslation.GetString("SilverBulletVentUsed"), usedPlayers.Count)+"\n\n");
                text.AppendLine("|-----------------------");
                if (AnalysisLightOption.GetBool())
                {
                    int index = 1;
                    foreach (byte playerId in usedPlayers)
                    {
                        PlayerControl player = ModHelpers.PlayerById(playerId);
                        text.AppendLine(string.Format(
                            ModTranslation.GetString(
                                $"SilverBulletVentUsedColor{
                                    (CustomColors.LighterColors.Contains(
                                        player.Data.DefaultOutfit.ColorId
                                    ) ? "Light" : "Dark")
                                }"
                            ), index)
                        );
                        index++;
                    }
                }
                text.AppendLine("|-----------------------");
            }

            FastDestroyableSingleton<HudManager>.Instance.Chat.AddChat(
                PlayerControl.LocalPlayer,
                text.ToString(),
                censor: false
            );
        }
        if (AnalysisCount <= 0)
        {
            LastUsedVentData = null;
            Logger.Warn($"残り使用可能回数が0になったのでLastUsedVentDataをnullにしました。","SilverBullet");
        }
    }
}