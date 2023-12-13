
using AmongUs.GameOptions;
using Hazel;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;

namespace SuperNewRoles.Roles.Crewmate.Santa;

public class Santa : RoleBase, ICrewmate, ICustomButton, IRpcHandler
{
    public static new RoleInfo Roleinfo = new(
        typeof(Santa),
        (p) => new Santa(p),
        RoleId.Santa,
        "Santa",
        new(255, 178, 178, byte.MaxValue),
        new(RoleId.Santa, TeamTag.Crewmate,
            RoleTag.Takada),
        TeamRoleType.Crewmate,
        TeamType.Crewmate
        );
    public static new OptionInfo Optioninfo =
        new(RoleId.Santa, 406500, false,
            CoolTimeOption: (30f, 2.5f, 60f, 2.5f, false),
            optionCreator: CreateOption);
    public static new IntroInfo Introinfo =
        new(RoleId.Santa, introSound: RoleTypes.Crewmate);

    private static CustomOption CanUseAbilityCount;

    public CustomButtonInfo[] CustomButtonInfos { get; }

    public static RoleId[] PresetRolesParam { get; } = new RoleId[]
    {
        RoleId.Sheriff,
        RoleId.Bait,
        RoleId.ToiletFan,
    };
    private static CustomOption[] PresetRoleOptions { get; set; }
    
    private static void CreateOption()
    {
        CanUseAbilityCount = CustomOption.Create(Optioninfo.OptionId++, false, Optioninfo.RoleOption.type,
            "SantaCanUseAbilityCount", 1, 1, 15, 1,
            Optioninfo.RoleOption);
        PresetRoleOptions = new CustomOption[PresetRolesParam.Length];
        for (int i = 0; i < PresetRolesParam.Length; i++)
        {
            PresetRoleOptions[i] = CustomOption.Create(Optioninfo.OptionId++, false, Optioninfo.RoleOption.type,
                               string.Format(
                                   ModTranslation.GetString("SantaPresentRoleOptionFormat"),
                                   CustomRoles.GetRoleNameOnColor(PresetRolesParam[i])
                               ),
                               true, Optioninfo.RoleOption);
        }
    }

    public void RpcReader(MessageReader reader)
    {

    }

    public Santa(PlayerControl p) : base(p, Roleinfo, Optioninfo, Introinfo)
    {
    }

}