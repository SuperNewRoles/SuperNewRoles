using System.Collections.Generic;
using System.Linq;
using System.Text;
using AmongUs.GameOptions;
using Hazel;
using SuperNewRoles.CustomObject;
using SuperNewRoles.Mode;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;

namespace SuperNewRoles.Roles.Neutral;

public class PavlovsOwner : RoleBase, INeutral, INameHandler, IRpcHandler, IFixedUpdaterMe, ICustomButton, ISupportSHR, ICheckMurderHandler, IFixedUpdaterAll
{
    public static new RoleInfo Roleinfo = new(
        typeof(PavlovsOwner),
        (p) => new PavlovsOwner(p),
        RoleId.Pavlovsowner,
        "Pavlovsowner",
        PavlovsDogs.PavlovsColor,
        new(RoleId.Pavlovsowner, TeamTag.Neutral),
        TeamRoleType.Neutral,
        TeamType.Neutral
        );

    public static new OptionInfo Optioninfo =
        new(RoleId.Pavlovsowner, 300400, true,
            CoolTimeOption: (30f, 2.5f, 60f, 2.5f, true),
            optionCreator: CreateOption);
    public static new IntroInfo Introinfo =
        new(RoleId.Pavlovsowner, introSound: RoleTypes.Phantom);

    /*PavlovsownerCreateDogLimit = Create(300403, false, CustomOptionType.Neutral, "PavlovsownerCreateDogLimit", 1f, 1f, 15f, 1f, PavlovsownerOption);
        PavlovsownerIsTargetImpostorDeath = Create(300404, false, CustomOptionType.Neutral, "PavlovsownerIsTargetImpostorDeath", true, PavlovsownerOption);
        PavlovsdogIsImpostorView = Create(300405, false, CustomOptionType.Neutral, "PavlovsdogIsImpostorView", true, PavlovsownerOption);
        PavlovsdogKillCoolTime = Create(300406, false, CustomOptionType.Neutral, "SheriffCooldownSetting", 30f, 2.5f, 120f, 2.5f, PavlovsownerOption);
        PavlovsdogCanVent = Create(300407, false, CustomOptionType.Neutral, "MadmateUseVentSetting", true, PavlovsownerOption);
        PavlovsdogRunAwayKillCoolTime = Create(300408, false, CustomOptionType.Neutral, "PavlovsdogRunAwayKillCoolTime", 20f, 2.5f, 60f, 2.5f, PavlovsownerOption);
        PavlovsdogRunAwayDeathTime = Create(300409, false, CustomOptionType.Neutral, "PavlovsdogRunAwayDeathTime", 60f, 2.5f, 180f, 2.5f, PavlovsownerOption);
        PavlovsdogRunAwayDeathTimeIsMeetingReset = Create(300410, false, CustomOptionType.Neutral, "PavlovsdogRunAwayDeathTimeIsMeetingReset", true, PavlovsownerOption);
*/
    public static CustomOption OwnerDogLimit;
    public static CustomOption IsTargetImpostorDeath;
    public static CustomOption IsImpostorView;
    public static CustomOption KillCoolTime;
    public static CustomOption CanVent;
    public static CustomOption RunAwayKillCoolTime;
    public static CustomOption RunAwayDeathTime;
    public static CustomOption RunAwayDeathTimeIsMeetingReset;

    private static void CreateOption()
    {
        OwnerDogLimit = CustomOption.Create(Optioninfo.OptionId++, Optioninfo.SupportSHR, Optioninfo.RoleOption.type, "PavlovsownerCreateDogLimit", 1f, 1f, 15f, 1f, Optioninfo.RoleOption);
        IsTargetImpostorDeath = CustomOption.Create(Optioninfo.OptionId++, Optioninfo.SupportSHR, Optioninfo.RoleOption.type, "PavlovsownerIsTargetImpostorDeath", true, Optioninfo.RoleOption);
        IsImpostorView = CustomOption.Create(Optioninfo.OptionId++, Optioninfo.SupportSHR, Optioninfo.RoleOption.type, "PavlovsdogIsImpostorView", true, Optioninfo.RoleOption);
        KillCoolTime = CustomOption.Create(Optioninfo.OptionId++, Optioninfo.SupportSHR, Optioninfo.RoleOption.type, "SheriffCooldownSetting", 30f, 2.5f, 120f, 2.5f, Optioninfo.RoleOption);
        CanVent = CustomOption.Create(Optioninfo.OptionId++, Optioninfo.SupportSHR, Optioninfo.RoleOption.type, "MadmateUseVentSetting", true, Optioninfo.RoleOption);
        RunAwayKillCoolTime = CustomOption.Create(Optioninfo.OptionId++, Optioninfo.SupportSHR, Optioninfo.RoleOption.type, "PavlovsdogRunAwayKillCoolTime", 20f, 2.5f, 60f, 2.5f, Optioninfo.RoleOption);
        RunAwayDeathTime = CustomOption.Create(Optioninfo.OptionId++, Optioninfo.SupportSHR, Optioninfo.RoleOption.type, "PavlovsdogRunAwayDeathTime", 60f, 2.5f, 180f, 2.5f, Optioninfo.RoleOption);
        RunAwayDeathTimeIsMeetingReset = CustomOption.Create(Optioninfo.OptionId++, Optioninfo.SupportSHR, Optioninfo.RoleOption.type, "PavlovsdogRunAwayDeathTimeIsMeetingReset", true, Optioninfo.RoleOption);
    }

    private Arrow DogArrow;
    public List<PavlovsDogs> CreatedDogs { get; } = new();
    private PavlovsDogs _aliveCreatedDog;
    private bool aliveChecked = false;
    private int lastCreatedDogs = -1;
    private PavlovsDogs? AliveCreatedDog
    {
        get
        {
            // 生きている犬がいないかつ、生存確認済みかつ、前回の犬の数と現在の犬の数が同じ場合はスキップ
            // でない場合、生存しているCached犬がいないか死んでいる場合に処理する
            if (_aliveCreatedDog == null && aliveChecked && lastCreatedDogs == CreatedDogs.Count)
                return null;
            if (_aliveCreatedDog?.Player == null || _aliveCreatedDog.Player.IsDead())
            {
                aliveChecked = true;
                if (CreatedDogs.Count == 0)
                    return null;
                if (CreatedDogs.Count == 1)
                {
                    PavlovsDogs firstDog = CreatedDogs.First();
                    if (firstDog?.Player == null || firstDog.Player.IsDead())
                        return null;
                }
                _aliveCreatedDog = CreatedDogs.FirstOrDefault(d => d.Player.IsAlive());
                lastCreatedDogs = CreatedDogs.Count;
                if (_aliveCreatedDog != null)
                    aliveChecked = false;
            }
            return _aliveCreatedDog;
        }
    }
    private bool CanCreateDog = false;

    private CustomButtonInfo CreateDogButtonInfo;
    public CustomButtonInfo[] CustomButtonInfos { get; }

    public RoleTypes RealRole => RoleTypes.Crewmate;
    public RoleTypes DesyncRole =>  RoleTypes.Impostor;

    public int CreateCountLimit = OwnerDogLimit.GetInt();

    public void OnHandleName()
        => PavlovsDogs.SeePavlovsTeam();

    public void RpcReader(MessageReader reader)
    {
        CanCreateDog = false;
        PlayerControl source = Player;
        PlayerControl target = ModHelpers.PlayerById(reader.ReadByte());
        if (source == null || target == null) return;
        if (ModeHandler.IsMode(ModeId.SuperHostRoles))
        {
            if (reader.ReadBoolean())
            {
                if (!AmongUsClient.Instance.AmHost)
                    return;
                source.RpcMurderPlayer(source, true);
            }
            else
            {
                CreateCountLimit--;
                CreateDogButtonInfo.AbilityCount = CreateCountLimit;
                if (AmongUsClient.Instance.AmHost)
                {
                    target.RpcSetRole(target.IsMod() ? RoleTypes.Crewmate : RoleTypes.Engineer, true);
                    if (!target.IsMod())
                    {
                        new LateTask(() =>
                        {
                            target.RpcSetRoleDesync(RoleTypes.Impostor, true);
                            Player.RpcSetRoleDesync(RoleTypes.Phantom, true, target);
                            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                            {
                                if (player.PlayerId == target.PlayerId
                                    || player.PlayerId == Player.PlayerId)
                                    continue;
                                player.RpcSetRoleDesync(RoleTypes.Engineer, true, target);
                            }
                            target.SetRole(RoleTypes.Crewmate);
                        }, 0.15f);
                    }
                    if (!Player.IsMod())
                        Player.RpcSetRoleDesync(RoleTypes.Crewmate, true);
                    target.SetRole(RoleTypes.Crewmate);
                }
                RPCProcedure.SetRole(target.PlayerId, (byte)RoleId.Pavlovsdogs);
                if (!target.TryGetRoleBase(out PavlovsDogs dog))
                    throw new System.NotImplementedException("Target is not PavlovsDogs. Why!?");
                CreatedDogs.Add(dog);
                dog.UpdateOwner(this);
                ChangeName.UpdateRoleName(target, ChangeNameType.AllPlayers);
                ChangeName.UpdateRoleName(Player, ChangeNameType.AllPlayers);
            }
            return;
        }

        if (reader.ReadBoolean())
        {
            source.MurderPlayer(source, MurderResultFlags.Succeeded | MurderResultFlags.DecisionByHost);
        }
        else
        {
            FastDestroyableSingleton<RoleManager>.Instance.SetRole(target, RoleTypes.Crewmate);
            RPCProcedure.SetRole(target.PlayerId, (byte)RoleId.Pavlovsdogs);
            CreateCountLimit--;
            CreateDogButtonInfo.AbilityCount = CreateCountLimit;
            if (!target.TryGetRoleBase(out PavlovsDogs dog))
                throw new System.NotImplementedException("Target is not PavlovsDogs. Why!?");
            CreatedDogs.Add(dog);
            dog.UpdateOwner(this);
        }
    }

    public bool OnCheckMurderPlayerAmKiller(PlayerControl target)
    {
        Logger.Info("Pavlovs Checking");
        if (AliveCreatedDog != null)
            return false;
        bool isSelfDeath = target.IsImpostor() && IsTargetImpostorDeath.GetBool();
        MessageWriter writer = RpcWriter;
        writer.Write(target.PlayerId);
        writer.Write(isSelfDeath);
        SendRpc(writer);
        return false;
    }

    public void FixedUpdateMeDefaultAlive()
    {
        if (DogArrow == null)
            return;
        PavlovsDogs aliveDog = AliveCreatedDog;
        if (aliveDog?.Player != null)
            DogArrow.Update(aliveDog.Player.transform.position);
        DogArrow.arrow.gameObject.SetActive(aliveDog?.Player != null && aliveDog.Player.IsAlive());
    }

    public PavlovsOwner(PlayerControl p) : base(p, Roleinfo, Optioninfo, Introinfo)
    {
        DogArrow = new(Roleinfo.RoleColor);
        DogArrow.arrow.SetActive(false);
        CreateDogButtonInfo = new(CreateCountLimit, this, CreateDogOnClick,
            (isAlive) => isAlive && AliveCreatedDog == null, CustomButtonCouldType.CanMove | CustomButtonCouldType.SetTarget,
            null, ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.PavlovsownerCreatedogButton.png", 115f),
            () => Optioninfo.CoolTime, new(), "PavlovsownerCreatedogButtonName",
            UnityEngine.KeyCode.F, 49, SetTargetFunc: () => PavlovsDogs.SetTarget());
        CustomButtonInfos = [CreateDogButtonInfo];
        CanCreateDog = true;
    }
    private void CreateDogOnClick()
    {
        PlayerControl target = CreateDogButtonInfo.CurrentTarget;
        if (Frankenstein.IsMonster(target))
            return;
        bool isSelfDeath = target.IsImpostor() && IsTargetImpostorDeath.GetBool();
        MessageWriter writer = RpcWriter;
        writer.Write(target.PlayerId);
        writer.Write(isSelfDeath);
        SendRpc(writer);
    }
    public void BuildName(StringBuilder Suffix, StringBuilder RoleNameText, PlayerData<string> ChangePlayers)
    {
        foreach(PlayerControl player in PlayerControl.AllPlayerControls)
        {
            if (player.PlayerId == Player.PlayerId || !player.IsPavlovsTeam())
                continue;
            ChangePlayers[player.PlayerId] = ModHelpers.Cs(PavlovsDogs.PavlovsColor, ChangeName.GetNowName(ChangePlayers, player));
        }
    }
    public void BuildSetting(IGameOptions gameOptions)
    {
        gameOptions.SetFloat(FloatOptionNames.KillCooldown, Optioninfo.CoolTime);
        gameOptions.SetFloat(FloatOptionNames.ProtectionDurationSeconds, 0.1f);
    }

    public void FixedUpdateAllSHR()
    {
        if (!AmongUsClient.Instance.AmHost)
            return;
        if (!CanCreateDog && AliveCreatedDog == null)
        {
            if (!Player.IsMod())
                Player.RpcSetRoleDesync(RoleTypes.Impostor, true);
            CanCreateDog = true;
        }
    }

    public void FixedUpdateAllDefault()
    {
    }
}