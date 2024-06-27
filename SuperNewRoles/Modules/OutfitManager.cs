namespace SuperNewRoles.Modules;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

public static class OutfitManager
{
    public static void setOutfit(this PlayerControl pc, NetworkedPlayerInfo.PlayerOutfit outfit, bool visible = true)
    {
        pc.Data.Outfits[PlayerOutfitType.Shapeshifted] = outfit;
        if (IsSameOutfit(pc.Data.DefaultOutfit, outfit)) pc.CurrentOutfitType = PlayerOutfitType.Shapeshifted;
        pc.RawSetName(outfit.PlayerName);
        pc.RawSetHat(outfit.HatId, outfit.ColorId);
        pc.RawSetVisor(outfit.VisorId, outfit.ColorId);
        pc.RawSetColor(outfit.ColorId);
        pc.RawSetPet(outfit.PetId, outfit.ColorId);
        pc.RawSetSkin(outfit.SkinId, outfit.ColorId);
        ModHelpers.SetSkinWithAnim(pc.MyPhysics, outfit.SkinId);

        var caller = new System.Diagnostics.StackFrame(1, false);
        var callerMethod = caller.GetMethod();
        string callerMethodName = callerMethod.Name;
        string callerClassName = callerMethod.DeclaringType.FullName;
        Logger.Info($"{pc.name} : CurrentOutfitType = {pc.CurrentOutfitType}, 呼び出し元 : {callerClassName}.{callerMethodName}", "OutfitManager");
    }
    public static void changeToPlayer(this PlayerControl pc, PlayerControl target)
    {
        SuperNewRolesPlugin.Logger.LogInfo($"Change Outfit : {pc.name} => {target.name}");
        setOutfit(pc, target.Data.DefaultOutfit, target.Visible);
    }
    public static void resetChange(this PlayerControl pc)
    {
        changeToPlayer(pc, pc);
        pc.CurrentOutfitType = PlayerOutfitType.Default;

        var caller = new System.Diagnostics.StackFrame(1, false);
        var callerMethod = caller.GetMethod();
        string callerMethodName = callerMethod.Name;
        string callerClassName = callerMethod.DeclaringType.FullName;
        SuperNewRolesPlugin.Logger.LogInfo($"シェイプリセット : {pc.name}, 呼び出し元 : {callerClassName}.{callerMethodName}");
    }


    // クルーカラーの翻訳を取得する。
    // 参考=>https://github.com/tugaru1975/TownOfPlus/blob/main/Helpers.cs
    internal static string GetColorTranslation(StringNames name) =>
        DestroyableSingleton<TranslationController>.Instance.GetString(name, new Il2CppReferenceArray<Il2CppSystem.Object>(0));

    /// <summary>
    /// 別の姿にシェイプするか, 自分自身にシェイプする(シェイプ解除)かを判定する
    /// </summary>
    /// <param name="shifterOutfit">シェイプ元の姿</param>
    /// <param name="targetOutfit">シェイプ先の姿</param>
    /// <returns>true : シェイプ / false : シェイプ解除</returns>
    private static bool IsSameOutfit(NetworkedPlayerInfo.PlayerOutfit shifterOutfit, NetworkedPlayerInfo.PlayerOutfit targetOutfit) =>
        !(shifterOutfit.ColorId == targetOutfit.ColorId
        && shifterOutfit.HatId == targetOutfit.HatId
        && shifterOutfit.PetId == targetOutfit.PetId
        && shifterOutfit.SkinId == targetOutfit.SkinId
        && shifterOutfit.VisorId == targetOutfit.VisorId
        && shifterOutfit.PlayerName == targetOutfit.PlayerName);
}