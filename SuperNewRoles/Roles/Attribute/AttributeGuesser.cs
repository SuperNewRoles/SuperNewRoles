using System;
using System.Collections.Generic;
using System.Linq;
using SuperNewRoles.Mode;
using UnityEngine;

namespace SuperNewRoles.Roles.Attribute;

public static class AttributeGuesser
{
    public static int OptionId = 500500;
    public static CustomOption AttributeGuesserOption;
    public static CustomOption AttributeGuesserMaximumAllocationImpostor;
    public static CustomOption AttributeGuesserEmissionProbabilityImpostor;
    public static CustomOption AttributeGuesserMaximumAllocationNeutral;
    public static CustomOption AttributeGuesserEmissionProbabilityNeutral;
    public static CustomOption AttributeGuesserMaximumAllocationCrew;
    public static CustomOption AttributeGuesserEmissionProbabilityCrew;
    public static CustomOption MadmateCannotAttributeGuesser;
    public static CustomOption MadmateSelfDestructive;

    public static CustomOption ShotOneMeetingCount;
    public static CustomOption ShotMaxCount;
    public static CustomOption CannotShotCrewOption;
    public static CustomOption CannotShotCelebrityOption;
    public static CustomOption BecomeShotCelebrityOption;
    public static CustomOption BecomeShotCelebrityTurn;

    public static void SetupCustomOption()
    {
        AttributeGuesserOption = CustomOption.Create(OptionId++, true, CustomOptionType.Modifier, "AttributeGuesserOption", false, isHeader: true);
        AttributeGuesserMaximumAllocationImpostor = CustomOption.Create(OptionId++, true, CustomOptionType.Modifier, "AttributeGuesserMaximumAllocationImpostor", 0, 0, 15, 1, AttributeGuesserOption);
        AttributeGuesserEmissionProbabilityImpostor = CustomOption.Create(OptionId++, true, CustomOptionType.Modifier, "AttributeGuesserEmissionProbabilityImpostor", 100, 5, 100, 5, AttributeGuesserMaximumAllocationImpostor, format: "%");

        AttributeGuesserMaximumAllocationNeutral = CustomOption.Create(OptionId++, true, CustomOptionType.Modifier, "AttributeGuesserMaximumAllocationNeutral", 0, 0, 15, 1, AttributeGuesserOption);
        AttributeGuesserEmissionProbabilityNeutral = CustomOption.Create(OptionId++, true, CustomOptionType.Modifier, "AttributeGuesserEmissionProbabilityNeutral", 100, 5, 100, 5, AttributeGuesserMaximumAllocationNeutral, format: "%");

        AttributeGuesserMaximumAllocationCrew = CustomOption.Create(OptionId++, true, CustomOptionType.Modifier, "AttributeGuesserMaximumAllocationCrew", 0, 0, 15, 1, AttributeGuesserOption);
        AttributeGuesserEmissionProbabilityCrew = CustomOption.Create(OptionId++, true, CustomOptionType.Modifier, "AttributeGuesserEmissionProbabilityCrew", 100, 5, 100, 5, AttributeGuesserMaximumAllocationCrew, format: "%");

        MadmateCannotAttributeGuesser = CustomOption.Create(OptionId++, true, CustomOptionType.Modifier, "MadmateCannotAttributeGuesser", true, AttributeGuesserOption);
        MadmateSelfDestructive = CustomOption.Create(OptionId++, true, CustomOptionType.Modifier, "MadmateSelfDestructive", true, AttributeGuesserOption);

        ShotMaxCount = CustomOption.Create(OptionId++, true, CustomOptionType.Modifier, "EvilGuesserShortMaxCountSetting", 2f, 1f, 15f, 1f, AttributeGuesserOption);
        ShotOneMeetingCount = CustomOption.Create(OptionId++, true, CustomOptionType.Modifier, "EvilGuesserOneMeetingShortSetting", true, AttributeGuesserOption);
        CannotShotCrewOption = CustomOption.Create(OptionId++, true, CustomOptionType.Modifier, "EvilGuesserCannotCrewShotSetting", false, AttributeGuesserOption);
        CannotShotCelebrityOption = CustomOption.Create(OptionId++, true, CustomOptionType.Modifier, "EvilGuesserCannotCelebrityShotSetting", false, AttributeGuesserOption);
        BecomeShotCelebrityOption = CustomOption.Create(OptionId++, true, CustomOptionType.Modifier, "EvilGuesserBecomeShotCelebritySetting", true, CannotShotCelebrityOption);
        BecomeShotCelebrityTurn = CustomOption.Create(OptionId++, true, CustomOptionType.Modifier, "EvilGuesserBecomeShotCelebrityTurnSetting", 3f, 1f, 15f, 1f, BecomeShotCelebrityOption);
    }

    public static List<PlayerControl> AttributeGuesserPlayer;
    public static PlayerData<int> GuesCount;
    public static void ClearAndReload()
    {
        AttributeGuesserPlayer = new();
        GuesCount = new(true, ShotMaxCount.GetInt(), true);
    }

    public static void RandomSelect()
    {
        if (!AttributeGuesserOption.GetBool()) return;
        Select(AttributeGuesserMaximumAllocationImpostor, AttributeGuesserEmissionProbabilityImpostor.GetFloat() / 100, x => x.IsImpostor());
        Select(AttributeGuesserMaximumAllocationNeutral, AttributeGuesserEmissionProbabilityNeutral.GetFloat() / 100, x => x.IsNeutral());
        Select(AttributeGuesserMaximumAllocationCrew, AttributeGuesserEmissionProbabilityCrew.GetFloat() / 100, x => x.IsCrew() && (!MadmateCannotAttributeGuesser.GetBool() || (!x.IsMadRoles() && !x.IsFriendRoles())));

        void Select(CustomOption maximum, float probability, Func<PlayerControl, bool> func)
        {
            if (!maximum.GetBool()) return;
            if (probability <= 0f) return;
            List<PlayerControl> players = PlayerControl.AllPlayerControls.FindAll(x => func(x) && !IsNonAssignableRole(x)).ToList();
            for (int i = 0; i < maximum.GetInt(); i++)
            {
                if (probability >= 1f)
                {
                    int index = players.GetRandomIndex();
                    RoleHelpers.SetAttributeGuesserRPC(players[index]);
                    players.RemoveAt(index);
                }
                else
                {
                    if (!BoolRange.Next(probability)) continue;
                    int index = players.GetRandomIndex();
                    RoleHelpers.SetAttributeGuesserRPC(players[index]);
                    players.RemoveAt(index);
                }
            }
        }

    }

    public static bool IsNonAssignableRole(this PlayerControl player) =>
        player.IsRole(
            RoleId.EvilGuesser,
            RoleId.NiceGuesser,
            RoleId.God,
            RoleId.OrientalShaman,
            RoleId.SoothSayer,
            RoleId.PartTimer,
            RoleId.Moira,
            RoleId.Crook,
            RoleId.MeetingSheriff,
            RoleId.Balancer,
            RoleId.PoliceSurgeon,
            RoleId.Assassin,
            RoleId.Marlin,
            RoleId.Safecracker,
            RoleId.TheFirstLittlePig,
            RoleId.TheSecondLittlePig,
            RoleId.TheThirdLittlePig,
            RoleId.Pokerface
        );

    public static bool IsAttributeGuesser(this PlayerControl player) =>
        AttributeGuesserPlayer.Contains(player);

    public static void StartMeetingPostfix(MeetingHud __instance)
    {
        if (ModeHandler.IsMode(ModeId.Default) && GuesCount.Local is > 0 or (-1))
            CreateGuesserButton(__instance);
    }

    public static void CreateGuesserButton(MeetingHud __instance)
    {
        for (int i = 0; i < __instance.playerStates.Length; i++)
        {
            PlayerVoteArea playerVoteArea = __instance.playerStates[i];
            if (playerVoteArea.AmDead || playerVoteArea.TargetPlayerId == PlayerControl.LocalPlayer.PlayerId) continue;

            GameObject template = playerVoteArea.Buttons.transform.Find("CancelButton").gameObject;
            GameObject targetBox = UnityEngine.Object.Instantiate(template, playerVoteArea.transform);
            targetBox.name = "ShootButton";
            targetBox.transform.localPosition = new(-0.95f, 0.03f, -1.3f);
            SpriteRenderer renderer = targetBox.GetComponent<SpriteRenderer>();
            renderer.sprite = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.TargetIcon.png", 115f);
            PassiveButton button = targetBox.GetComponent<PassiveButton>();
            button.OnClick.RemoveAllListeners();
            int copiedIndex = i;
            // button.OnClick.AddListener((Action)(() => guesserOnClick(copiedIndex, __instance)));
        }
    }
}
