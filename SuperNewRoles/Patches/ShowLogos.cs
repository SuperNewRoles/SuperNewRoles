using HarmonyLib;
using SuperNewRoles.Modules;
using UnityEngine;

namespace SuperNewRoles.Patches;


[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
public static class ShowLogos
{
    public static void Postfix()
    {
        FastDestroyableSingleton<ModManager>.Instance.ShowModStamp();
        var logo = AssetManager.GetAsset<Sprite>("banner", AssetManager.AssetBundleType.Sprite);
        if (logo == null)
        {
            Logger.Error("logo is null");
            return;
        }
        var logoObject = new GameObject("Logo");
        logoObject.transform.localPosition = new(2.05f, 0.5f, 0);
        logoObject.transform.localScale = Vector3.one * 0.75f;
        logoObject.AddComponent<SpriteRenderer>().sprite = logo;
    }

    public static string baseCredentials => $@"<size=130%>{UIConfig.ColorModName}</size> v{VersionInfo.VersionString}";

    [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
    private static class VersionShowerPatch
    {
        public static string modColor = "#a6d289";
        static void Postfix(VersionShower __instance)
        {
            if (GameObject.FindObjectOfType<MainMenuManager>() == null)
                return;
            var credentials = UnityEngine.Object.Instantiate<TMPro.TextMeshPro>(__instance.text);
            credentials.transform.position = new Vector3(2, -0.3f, 0);
            credentials.transform.localScale = Vector3.one * 2;
            //ブランチ名表示
            string credentialsText = "";
            if (Statics.IsBeta)//masterビルド以外の時
            {
                //色+ブランチ名+コミット番号
                credentialsText = $"\r\n<color={modColor}>{ThisAssembly.Git.Branch}({ThisAssembly.Git.Commit})</color>";
            }
            credentialsText += ModTranslation.GetString("creditsMain");
            credentials.SetText(credentialsText);

            credentials.alignment = TMPro.TextAlignmentOptions.Center;
            credentials.fontSize *= 0.9f;
            /*_ = AutoUpdate.checkForUpdate(credentials);*/

            var version = UnityEngine.Object.Instantiate(credentials);
            version.transform.position = new Vector3(2, -0.65f, 0);
            version.transform.localScale = Vector3.one * 1.5f;
            version.SetText($"{Statics.ModName} v{Statics.VersionString}");

            //            credentials.transform.SetParent(amongUsLogo.transform);
            //            version.transform.SetParent(amongUsLogo.transform);
        }
    }
}
