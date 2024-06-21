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
    /// <summary>
    /// 役職を解放する日時
    /// これを使用して封印する際 [ 定数 : HasSealingOption (CustomOptionHolder.cs)] をtrueにする。
    /// </summary>
    /// <value></value>
    public DateTime? RoleOpenTimeUTC { get; }

    // 外から参照する郡
    public int AssignSelection => RoleOption?.GetSelection() ?? 0;
    public int PlayerCount => PlayerCountOption?.GetInt() ?? 0;
    public CustomOption GetPlayerCountOption => PlayerCountOption;
    public float KillCoolTime => KillCoolTimeOption?.GetFloat() ?? 0;
    public bool CanUseVent => CanUseVentOption != null && CanUseVentOption.GetBool();
    public bool CanUseSabo => CanUseSaboOption != null && CanUseSaboOption.GetBool();
    public bool IsImpostorVision => IsImpostorVisionOption != null && IsImpostorVisionOption.GetBool();
    public float CoolTime => CoolTimeOption?.GetFloat() ?? 0;
    public float DurationTime => DurationTimeOption?.GetFloat() ?? 0;
    public int AbilityMaxCount => AbilityCountOption?.GetInt() ?? -1;
    public bool IsHidden => isHidden || (HasSealingCondition && RoleOpenTimeUTC.Value > DateTime.UtcNow);
    /// <summary>日時による封印条件を有するか</summary>
    public bool HasSealingCondition => RoleOpenTimeUTC != null;

    // 設定郡
    public CustomOption RoleOption { get; private set; }
    private CustomOption PlayerCountOption;
    private CustomOption KillCoolTimeOption;
    private CustomOption CanUseVentOption;
    private CustomOption CanUseSaboOption;
    private CustomOption IsImpostorVisionOption;
    private CustomOption CoolTimeOption;
    private CustomOption DurationTimeOption;
    private CustomOption AbilityCountOption;

    // 設定生成のために保存しておくやつ
    private (float CoolTimeDefault, float CoolTimeMin, float CoolTimeMax, float CoolTimeStep, bool SupportSHR)? KillCoolTimeOpt { get; }
    private (bool CanUseVentDefault, bool SupportSHR)? VentOption { get; }
    private (bool CanUseSaboDefault, bool SupportSHR)? SaboOption { get; }
    private (bool IsImpostorVisionDefault, bool SupportSHR)? ImpostorVisionOption { get; }
    private (float CoolTimeDefault, float CoolTimeMin, float CoolTimeMax, float CoolTimeStep, bool SupportSHR)? CoolTimeOpt { get; }
    private (float DurationTimeDefault, float DurationTimeMin, float DurationTimeMax, float DurationTimeStep, bool SupportSHR)? DurationTimeOpt { get; }
    private (int AbilityCountDefault, int AbilityCountMin, int AbilityCountMax, int AbilityCountStep, bool SupportSHR)? AbilityCountOpt { get; }
    private bool isHidden { get; }
    private float MaxPlayer { get; }
    private Action OptionCreater;

    /// <summary>
    /// OptionInfo
    /// </summary>
    /// <param name="role">作成対象のRoleId</param>
    /// <param name="OptionId">CustomRoleOptionの設定Id (Topのオプションの設定Id)</param>
    /// <param name="SupportSHR">役職がSHR対応しているか</param>
    /// <param name="VentOption">通気口使用を設定可能にするか</param>
    /// <param name="SaboOption">サボタージュのを設定可能にするか</param>
    /// <param name="ImpostorVisionOption">インポスター視野を設定可能にするか</param>
    /// <param name="CoolTimeOption">クールタイムの設定</param>
    /// <param name="DurationTimeOption">効果時間の設定</param>
    /// <param name="AbilityCountOption">アビリティ最大使用回数の設定</param>
    /// <param name="optionCreator">その他CustomOptionを定義する関数の呼び出し(不必要ならnull)</param>
    /// <param name="isHidden">設定を非表示にするか</param>
    /// <param name="MaxPlayer">最大アサイン可能人数(省略時:[インポ最大15人, クルー＆第三最大15人])</param>
    /// <param name="RoleOpenTimeUTC">役職を解放する日時 (これを使用する際は, [ 定数 : HasSealingOption (CustomOptionHolder.cs)] をtrueにして下さい。)</param>
    public OptionInfo(RoleId role, int OptionId, bool SupportSHR,
        (float CoolTimeDefault, float CoolTimeMin, float CoolTimeMax, float CoolTimeStep, bool SupportSHR)? KillCoolTimeOption = null,
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
        this.KillCoolTimeOpt = KillCoolTimeOption;
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
        if (KillCoolTimeOpt.HasValue)
            KillCoolTimeOption = CustomOption.Create(OptionId++, KillCoolTimeOpt.Value.SupportSHR, RoleOption.type, "KillCoolTimeOption", KillCoolTimeOpt.Value.CoolTimeDefault, KillCoolTimeOpt.Value.CoolTimeMin, KillCoolTimeOpt.Value.CoolTimeMax, KillCoolTimeOpt.Value.CoolTimeStep, RoleOption, isHidden: isHidden);
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
    public static OptionInfo GetOptionInfo(RoleId role, bool error = true)
    {
        // TryGetValutを使う
        if (!OptionInfos.TryGetValue(role, out var optionInfo))
        {
            if (error) Logger.Error($"OptionInfoが見つかりませんでした。Role:{role}", "GetOptionInfo");
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