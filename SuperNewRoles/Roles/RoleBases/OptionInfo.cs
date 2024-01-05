using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperNewRoles.Roles.RoleBases;
public class OptionInfo
{
    public static Dictionary<RoleId, OptionInfo> OptionInfos = new();
    public RoleId Role { get; }
    public bool SupportSHR { get; }
    public int OptionId { get; set; }
    public bool IsHidden => isHidden || (RoleOpenTimeUTC != null && RoleOpenTimeUTC.Value > DateTime.UtcNow);
    public DateTime? RoleOpenTimeUTC { get; }

    // 外から参照する郡
    public int AssignSelection => RoleOption?.GetSelection() ?? 0;
    public int PlayerCount => PlayerCountOption?.GetInt() ?? 0;
    public bool CanUseVent => CanUseVentOption != null && CanUseVentOption.GetBool();
    public bool CanUseSabo => CanUseSaboOption != null && CanUseSaboOption.GetBool();
    public bool IsImpostorVision => IsImpostorVisionOption != null && IsImpostorVisionOption.GetBool();
    public float CoolTime => CoolTimeOption?.GetFloat() ?? 0;
    public float DurationTime => DurationTimeOption?.GetFloat() ?? 0;
    public int AbilityMaxCount => AbilityCountOption?.GetInt() ?? -1;

    // 設定郡
    public CustomOption RoleOption { get; private set; }
    private CustomOption PlayerCountOption;
    private CustomOption CanUseVentOption;
    private CustomOption CanUseSaboOption;
    private CustomOption IsImpostorVisionOption;
    private CustomOption CoolTimeOption;
    private CustomOption DurationTimeOption;
    private CustomOption AbilityCountOption;

    // 設定生成のために保存しておくやつ
    private (bool CanUseVentDefault, bool SupportSHR)? VentOption { get; }
    private (bool CanUseSaboDefault, bool SupportSHR)? SaboOption { get; }
    private (bool IsImpostorVisionDefault, bool SupportSHR)? ImpostorVisionOption { get; }
    private (float CoolTimeDefault, float CoolTimeMin, float CoolTimeMax, float CoolTimeStep, bool SupportSHR)? CoolTimeOpt { get; }
    private (float DurationTimeDefault, float DurationTimeMin, float DurationTimeMax, float DurationTimeStep, bool SupportSHR)? DurationTimeOpt { get; }
    private (int AbilityCountDefault, int AbilityCountMin, int AbilityCountMax, int AbilityCountStep, bool SupportSHR)? AbilityCountOpt { get; }
    private bool isHidden { get; }
    private float MaxPlayer { get; }
    private Action OptionCreater;
    public OptionInfo(RoleId role, int OptionId, bool SupportSHR,
        (bool CanUseVentDefault, bool SupportSHR)? VentOption = null,
        (bool CanUseSaboDefault, bool SupportSHR)? SaboOption = null,
        (bool IsImpostorVisionDefault, bool SupportSHR)? ImpostorVisionOption = null,
        (float CoolTimeDefault, float CoolTimeMin, float CoolTimeMax, float CoolTimeStep, bool SupportSHR)? CoolTimeOption = null,
        (float DurationTimeDefault, float DurationTimeMin, float DurationTimeMax, float DurationTimeStep, bool SupportSHR)? DurationTimeOption = null,
        (int AbilityCountDefault, int AbilityCountMin, int AbilityCountMax, int AbilityCountStep, bool SupportSHR)? AbilityCountOption = null,
        Action optionCreator = null,
        bool isHidden = false,
        float MaxPlayer = -1,
        DateTime? RoleOpenTimeUTC = null
        )
    {
        this.Role = role;
        this.SupportSHR = SupportSHR;
        this.VentOption = VentOption;
        this.SaboOption = SaboOption;
        this.ImpostorVisionOption = ImpostorVisionOption;
        this.CoolTimeOpt = CoolTimeOption;
        this.DurationTimeOpt = DurationTimeOption;
        this.AbilityCountOpt = AbilityCountOption;
        this.OptionId = OptionId;
        this.RoleOpenTimeUTC = RoleOpenTimeUTC;
        this.MaxPlayer = MaxPlayer;
        this.OptionCreater = optionCreator;
        this.isHidden = isHidden;
        OptionInfos.TryAdd(Role, this);
    }
    public void CreateOption()
    {
        // もう設定があるのでリターン
        if (RoleOption != null)
            return;
        // 設定を作成
        RoleOption = CustomOption.SetupCustomRoleOption(
            OptionId++, SupportSHR, Role, isHidden: isHidden);
        List<float> PlayerCount = CustomOptionHolder.CrewPlayers;
        if (MaxPlayer != -1f)
            PlayerCount = new(4) { 1, 1, MaxPlayer, 1 };
        else if (RoleOption.type == CustomOptionType.Impostor)
            PlayerCount = CustomOptionHolder.ImpostorPlayers;
        PlayerCountOption = CustomOption.Create(
            OptionId++,
            SupportSHR, RoleOption.type,
            "SettingPlayerCountName",
            PlayerCount[0], PlayerCount[1], PlayerCount[2],
            PlayerCount[3], RoleOption);
        if (VentOption.HasValue)
            CanUseVentOption = CustomOption.Create(OptionId++, VentOption.Value.SupportSHR, RoleOption.type, "CanUseVentOption", VentOption.Value.CanUseVentDefault, RoleOption, isHidden: isHidden);
        if (SaboOption.HasValue)
            CanUseSaboOption = CustomOption.Create(OptionId++, SaboOption.Value.SupportSHR, RoleOption.type, "CanUseSaboOption", SaboOption.Value.CanUseSaboDefault, RoleOption, isHidden: isHidden);
        if (ImpostorVisionOption.HasValue)
            IsImpostorVisionOption = CustomOption.Create(OptionId++, ImpostorVisionOption.Value.SupportSHR, RoleOption.type, "IsImpostorVisionOption", ImpostorVisionOption.Value.IsImpostorVisionDefault, RoleOption, isHidden: isHidden);
        if (CoolTimeOpt.HasValue)
            CoolTimeOption = CustomOption.Create(OptionId++, CoolTimeOpt.Value.SupportSHR, RoleOption.type, "CoolTimeOption", CoolTimeOpt.Value.CoolTimeDefault, CoolTimeOpt.Value.CoolTimeMin, CoolTimeOpt.Value.CoolTimeMax, CoolTimeOpt.Value.CoolTimeStep, RoleOption, isHidden: isHidden);
        if (DurationTimeOpt.HasValue)
            DurationTimeOption = CustomOption.Create(OptionId++, DurationTimeOpt.Value.SupportSHR, RoleOption.type, "DurationTimeOption", DurationTimeOpt.Value.DurationTimeDefault, DurationTimeOpt.Value.DurationTimeMin, DurationTimeOpt.Value.DurationTimeMax, DurationTimeOpt.Value.DurationTimeStep, RoleOption, isHidden: isHidden);
        if (AbilityCountOpt.HasValue)
            AbilityCountOption = CustomOption.Create(OptionId++, AbilityCountOpt.Value.SupportSHR, RoleOption.type, "AbilityCountOption", AbilityCountOpt.Value.AbilityCountDefault, AbilityCountOpt.Value.AbilityCountMin, AbilityCountOpt.Value.AbilityCountMax, AbilityCountOpt.Value.AbilityCountStep, RoleOption, isHidden: isHidden);
        // 個別設定を生成する
        if (OptionCreater != null)
            OptionCreater.Invoke();
    }
    //RoleIdからOptionInfoを取得する
    public static OptionInfo GetOptionInfo(RoleId role)
    {
        // TryGetValutを使う
        if (!OptionInfos.TryGetValue(role, out var optionInfo))
        {
            Logger.Error($"OptionInfoが見つかりませんでした。Role:{role}", "GetOptionInfo");
            return null;
        }
        return optionInfo;
    }
    //Todo:後で並び順を設定する関数を書く
    public static void CreateOptions()
    {
        foreach (var optionInfo in OptionInfos.Values)
        {
            optionInfo.CreateOption();
        }
    }
}