/* ◯作り方◯
    1.ICrewmateかINeutralかIImpostorのどれかを継承する // [x]
    2.必要なインターフェースを実装する // [ ]
    3.Roleinfo,Optioninfo,Introinfoを設定する // [x]
    4.設定を作成する(CreateOptionが必要なければOptioninfoのoptionCreatorをnullにする) // [x]
    5.インターフェースの内容を実装していく // [ ]
*/

using System.ComponentModel;
using AmongUs.GameOptions;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;

namespace SuperNewRoles.Roles.Crewmate;

public class ClinicalTechnologist : RoleBase, ICrewmate, ISupportSHR
{
    public static new RoleInfo Roleinfo = new(
        typeof(ClinicalTechnologist),
        (p) => new ClinicalTechnologist(p),
        RoleId.ClinicalTechnologist,
        "ClinicalTechnologist",
        new(37, 159, 148, byte.MaxValue),
        new(RoleId.ClinicalTechnologist, TeamTag.Crewmate, RoleTag.Information),
        TeamRoleType.Crewmate,
        TeamType.Crewmate
        );

    public static new OptionInfo Optioninfo =
        new(RoleId.ClinicalTechnologist, 406700, true,
            CoolTimeOption: (15f, 2.5f, 60f, 2.5f, true),
            AbilityCountOption: (1f, 1f, 15f, 1f, true),
            optionCreator: null);

    public RoleTypes RealRole => RoleTypes.Crewmate;
    public RoleTypes DesyncRole => RoleTypes.Impostor;

    public static new IntroInfo Introinfo =
        new(RoleId.ClinicalTechnologist, introSound: RoleTypes.Scientist);
    public ClinicalTechnologist(PlayerControl p) : base(p, Roleinfo, Optioninfo, Introinfo)
    {
    }
}