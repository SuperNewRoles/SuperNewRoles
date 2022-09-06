using System.Collections.Generic;
using SuperNewRoles.Patch;
using UnityEngine;
using static SuperNewRoles.Modules.CustomOptions;
using static SuperNewRoles.Roles.RoleClass;

namespace SuperNewRoles.Roles.Impostor
{
    public static class ShiftActor
    {
        public const int OptionId = 894;// 設定のId
        // CustomOptionDate
        public static CustomRoleOption ShiftActorOption;
        public static CustomOption ShiftActorPlayerCount;
        public static CustomOption ShiftActorKillCool;
        public static CustomOption ShiftActorShiftLimit;
        public static CustomOption ShiftActorRightChance;
        public static CustomOption ShiftActorCanWatchAttribute;
        public static void SetupCustomOptions()
        {
            ShiftActorOption = new(OptionId, false, CustomOptionType.Impostor, "ShiftActorName", color, 1);
            ShiftActorPlayerCount = CustomOption.Create(OptionId + 1, false, CustomOptionType.Impostor, "SettingPlayerCountName", ImpostorPlayers[0], ImpostorPlayers[1], ImpostorPlayers[2], ImpostorPlayers[3], ShiftActorOption);
            ShiftActorKillCool = CustomOption.Create(OptionId + 2, false, CustomOptionType.Impostor, "SheriffCoolDownSetting", 30f, 2.5f, 60f, 2.5f, ShiftActorOption, format: "unitSeconds");
            ShiftActorShiftLimit = CustomOption.Create(OptionId + 3, false, CustomOptionType.Impostor, "SettingLimitName", 1f, 0f, 5f, 1f, ShiftActorOption);
            ShiftActorRightChance = CustomOption.Create(OptionId + 4, false, CustomOptionType.Impostor, "RightChance", rates[1..], ShiftActorOption);
            ShiftActorCanWatchAttribute = CustomOption.Create(OptionId + 5, false, CustomOptionType.Impostor, "CanWatchAttribute", false, ShiftActorOption);
        }

        // RoleClass
        public static List<PlayerControl> Player;
        public static Color32 color = ImpostorRed;
        public static float KillCool;
        public static int Limit;
        public static int Count;
        public static bool IsWatchAttribute;
        public static void ClearAndReload()
        {
            Player = new();
            KillCool = ShiftActorKillCool.GetFloat();
            Limit = ShiftActorShiftLimit.GetInt();
            Count = 0;
            IsWatchAttribute = ShiftActorCanWatchAttribute.GetBool(); // 重複を見れるか
        }
        public static bool CanShow = Count >= Limit;// シェイプカウントが上限より少ないか

        public static void Shapeshift(PlayerControl shapeshifter, PlayerControl target)
        {
            Logger.Info($"現在のカウント{Count}", "ShiftActor");
            if (!CanShow) return;
            var TargetRoleText = "";

            // 役職名
            if (!ModHelpers.IsSucsessChance(ShiftActorRightChance.GetSelection() + 1))
            { // 確率を判定し、失敗なら「クルーメイト」のみ表示。
                Logger.Info("失敗", "ShiftActor");
                TargetRoleText = ModTranslation.GetString("CrewMateName");
            }
            else
            {
                TargetRoleText = target.IsRole(RoleId.DefaultRole)
                    ? target.IsImpostor() ? ModTranslation.GetString("ImpostorName") : ModTranslation.GetString("CrewMateName")
                    : target.IsRole(RoleId.Marine) ? ModTranslation.GetString("CrewMateName") : ModTranslation.GetString($"{target.GetRole()}Name");

                // 重複役職
                if (IsWatchAttribute)
                {
                    if (target.IsLovers())
                    {
                        TargetRoleText += ModHelpers.Cs(RoleClass.Lovers.color, " ♥"); // ラバーズ
                    }
                    else if (target.IsQuarreled())
                    {
                        TargetRoleText += ModHelpers.Cs(RoleClass.Quarreled.color, "○"); //　クラード
                    }
                }
            }
            Logger.Info($"テキスト名は{TargetRoleText}", "ShiftActor");

            // ここからが表示関連
            var text1 = ModTranslation.GetString("ShiftActorText1"); // の役職は
            var text2 = ModTranslation.GetString("ShiftActorText2"); // です
            var showtext = $"{target.name}{text1}{TargetRoleText}{text2}"; // ex)たろうの役職はパン屋 ♥です

            new CustomMessage(showtext, 10);

            Count++;
        }
        public static void ShapeshifterSet()
        {
            foreach (PlayerControl p in Player)
            {
                Logger.Info("シェイプシフター割り当て", "ShiftActor");
                DestroyableSingleton<RoleManager>.Instance.SetRole(p, RoleTypes.Shapeshifter);
            }
        }
    }
}