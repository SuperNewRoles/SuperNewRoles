using System.Collections.Generic;
using AmongUs.GameOptions;
using Hazel;
using SuperNewRoles.Roles.Role;
using SuperNewRoles.Roles.RoleBases;
using SuperNewRoles.Roles.RoleBases.Interfaces;
using UnityEngine;

namespace SuperNewRoles.Roles.Impostor;

public class Mushroomer : RoleBase, IImpostor, ICustomButton, IRpcHandler, IWrapUpHandler
{
    public static new RoleInfo Roleinfo = new(
        typeof(Mushroomer),
        (p) => new Mushroomer(p),
        RoleId.Mushroomer,
        "Mushroomer",
        RoleClass.ImpostorRed,
        new(RoleId.Mushroomer, TeamTag.Impostor,
            RoleTag.Killer, RoleTag.Information),
        TeamRoleType.Impostor,
        TeamType.Impostor
        );
    public static new OptionInfo Optioninfo =
        new(RoleId.Mushroomer, 205900, false,
            optionCreator: CreateOption);
    public static new IntroInfo Introinfo =
        new(RoleId.Mushroomer, introSound: RoleTypes.Impostor);

    public static CustomOption HasGasMask;
    public static CustomOption MushroomPlantCoolTime;
    public static CustomOption MushroomPlantCount;
    public static CustomOption MushroomExplosionCoolTime;
    public static CustomOption MushroomExplosionCount;
    public static CustomOption MushroomExplosionRange;
    public static CustomOption MushroomExplosionDurationTime;

    public CustomButtonInfo[] CustomButtonInfos { get; }
    public List<Mushroom> CreatedMushrooms { get; }
    public List<Mushroom> NextturnActivateMushrooms { get; private set; }

    private static void CreateOption()
    {
        HasGasMask = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Impostor, "MushroomerHasGasMask", true, Optioninfo.RoleOption);
        MushroomPlantCoolTime = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Impostor, "MushroomerMushroomPlantCoolTime", 20f, 2.5f, 60f, 2.5f, Optioninfo.RoleOption);
        MushroomPlantCount = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Impostor, "MushroomerMushroomPlantCount", 3, 1, 10, 1, Optioninfo.RoleOption);
        MushroomExplosionCoolTime = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Impostor, "MushroomerMushroomExplosionCoolTime", 30f, 2.5f, 60f, 2.5f, Optioninfo.RoleOption);
        MushroomExplosionCount = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Impostor, "MushroomerMushroomExplosionCount", 2, 1, 10, 1, Optioninfo.RoleOption);
        MushroomExplosionRange = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Impostor, "MushroomerMushroomExplosionRange", 0.5f, 0.25f, 3f, 0.25f, Optioninfo.RoleOption);
        MushroomExplosionDurationTime = CustomOption.Create(Optioninfo.OptionId++, false, CustomOptionType.Impostor, "MushroomerMushroomExplosionDurationTime", 2.5f, 0.5f, 10f, 0.5f, Optioninfo.RoleOption);
    }
    public CustomButtonInfo PlanetButtonInfo { get; }
    public CustomButtonInfo ExplosionButtonInfo { get; }
    public Mushroomer(PlayerControl p) : base(p, Roleinfo, Optioninfo, Introinfo)
    {
        PlanetButtonInfo = new(MushroomPlantCount.GetInt(), this, PlanetButtonOnClick,
            (isAlive) => isAlive && (PlanetButtonInfo.AbilityCount > 0 || NextturnActivateMushrooms.Count > 0), CustomButtonCouldType.CanMove, null,
            ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.MushroomerPlanetButton.png", 115f),
            MushroomPlantCoolTime.GetFloat,
            new(-2f, 1, 0), "MushroomerPlanetButtonName", KeyCode.F,
            49, HasAbilityCountText: true, AbilityCountTextFormat: "MushroomerPlanetAbilityCountText");

        ExplosionButtonInfo = new(MushroomExplosionCount.GetInt(), this, ExplosionButtonOnClick,
            (isAlive) => isAlive && PlanetButtonInfo.AbilityCount <= 0 && NextturnActivateMushrooms.Count <= 0,
            CustomButtonCouldType.CanMove, null,
            ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.MushroomerExplosionButton.png", 115f),
            MushroomExplosionCoolTime.GetFloat,
            new(-2f, 1, 0), "MushroomerExplosionButtonName", KeyCode.F, 49,
            HasAbilityCountText: true, AbilityCountTextFormat: "MushroomerExplosionAbilityCountText");
        CustomButtonInfos = new CustomButtonInfo[2]
        {
            PlanetButtonInfo, ExplosionButtonInfo
        };
        CreatedMushrooms = new();
        NextturnActivateMushrooms = new();
    }
    public void RpcReader(MessageReader reader)
    {
        //設置かを判定
        if (reader.ReadBoolean())
        {
            Vector2 position = new(reader.ReadSingle(), reader.ReadSingle());
            Mushroom mushRoom = CustomSpores.AddMushroom(position);
            mushRoom.gameObject.SetActive(false);
            CreatedMushrooms.Add(mushRoom);
            NextturnActivateMushrooms.Add(mushRoom);
            //有効じゃない時に見えるかを設定
            if (PlayerControl.LocalPlayer.IsImpostor())
                SetActiveMushroom(mushRoom, false);
            else
                mushRoom.gameObject.SetActive(false);
        }
        else
        {
            if (ModHelpers.IsMap(MapNames.Fungle))
            {
                foreach (var mushrooms in ShipStatus.Instance.TryCast<FungleShipStatus>().sporeMushrooms)
                    if (mushrooms.Value.isActiveAndEnabled)
                        CustomTriggerSpores(mushrooms.Value);
            }
            else
            {
                foreach (var mushrooms in CustomSpores.mushRooms)
                    if (mushrooms.Value.isActiveAndEnabled)
                        CustomTriggerSpores(mushrooms.Value);
            }
        }
    }
    public void OnWrapUp()
    {
        foreach (Mushroom mushRoom in NextturnActivateMushrooms.AsSpan())
        {
            SetActiveMushroom(mushRoom, true);
        }
        NextturnActivateMushrooms = new();
    }
    private void SetActiveMushroom(Mushroom mushroom, bool Active)
    {
        mushroom.gameObject.SetActive(true);
        mushroom.enabled = Active;
        mushroom.GetComponent<SpriteRenderer>().color = new(1, 1, 1, Active ? 1 : 0.6f);
    }
    private void PlanetButtonOnClick()
    {
        MessageWriter writer = RpcWriter;
        Vector2 position = PlayerControl.LocalPlayer.transform.position;
        writer.Write(true);
        writer.Write(position.x);
        writer.Write(position.y);
        SendRpc(writer);
    }
    private void ExplosionButtonOnClick()
    {
        MessageWriter writer = RpcWriter;
        writer.Write(false);
        SendRpc(writer);
    }
    private void CustomTriggerSpores(Mushroom mushroom)
    {
        mushroom.secondsSporeIsActive = MushroomExplosionDurationTime.GetFloat();
        mushroom.transform.localScale = Vector3.one * MushroomExplosionRange.GetFloat();
        mushroom.TriggerSpores();
    }
    public static void MushroomResetStatePatch(Mushroom __instance)
    {
        //デフォルトに戻す
        __instance.transform.localScale = Vector3.one;
        __instance.secondsSporeIsActive = 5f;
        __instance.sporeMask.transform.localScale = new(2.4f, 2.4f, 1.2f);
    }
}