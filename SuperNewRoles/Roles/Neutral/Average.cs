using System.Collections.Generic;
using UnityEngine;

namespace SuperNewRoles.Roles.Neutral;

public static class Average
{
    private const int OptionId = 1268;
    public static CustomRoleOption AverageOption;
    public static CustomOption AveragePlayerCount;
    public static CustomOption AverageVoteTime;
    public static void SetupCustomOptions()
    {
        AverageOption = CustomOption.SetupCustomRoleOption(OptionId, false, RoleId.Average);
        AveragePlayerCount = CustomOption.Create(OptionId + 1, false, CustomOptionType.Neutral, "SettingPlayerCountName", CustomOptionHolder.CrewPlayers[0], CustomOptionHolder.CrewPlayers[1], CustomOptionHolder.CrewPlayers[2], CustomOptionHolder.CrewPlayers[3], AverageOption);
        AverageVoteTime = CustomOption.Create(OptionId + 2, false, CustomOptionType.Neutral, "AverageVoteTime", 30f, 0f, 180f, 2.5f, AverageOption);
    }
    
    public static List<PlayerControl> AveragePlayer;
    public static Color32 color = new(255, 128, 0, byte.MaxValue);
    public static void ClearAndReload()
    {
        AveragePlayer = new();
        currentAbilityUser = null;
    }
    public static PlayerControl currentAbilityUser;
    public static SpriteRenderer BackObject;
    public static Sprite BackSprite => ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.Average.MeetingBack.png", 115f);
    public static void StartAbility() {
        BackObject = new GameObject("BackObject").AddComponent<SpriteRenderer>();
        BackObject.transform.parent = MeetingHud.Instance.transform;
        BackObject.sprite = BackSprite;
        // UIレイヤーに移動
        BackObject.gameObject.layer = 5;
        // 位置移動
        BackObject.transform.localPosition = new(0, 0, -11);
        BackObject.transform.localScale = new(1.4f, 1.4f, 1.4f);
    }
    
    // ここにコードを書きこんでください
}