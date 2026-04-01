using HarmonyLib;

namespace SuperNewRoles.Patches;

// あまりにもクラシックモードの会議開始アニメーションのテキストが邪魔だったので消すパッチ
[HarmonyPatch(typeof(CheckClassicText), nameof(CheckClassicText.OnEnable))]
public static class CheckClassicTextPatch
{
    public static void Postfix(CheckClassicText __instance)
    {
        __instance.reportedText.gameObject.SetActive(false);
    }
}