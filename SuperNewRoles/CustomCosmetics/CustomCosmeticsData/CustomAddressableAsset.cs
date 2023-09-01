using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Innersloth.Assets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine;
using Sentry;
using System.Collections;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;

namespace SuperNewRoles.CustomCosmetics.CustomCosmeticsData;

[Serializable]
public class CustomAddressableAsset<T> : AddressableAsset<T> where T : UnityEngine.Object
{
    public HatViewData currentViewData;
    public static List<CustomAddressableAsset<T>> CustomAddressableAssets = new();
    public CustomAddressableAsset(HatViewData hat)
    {
        currentViewData = hat;
        CustomAddressableAssets.Add(this);
    }
    public override T GetAsset()
    {
        Logger.Info((currentViewData == null).ToString()+":"+(currentViewData.TryCast<T>() == null).ToString());
        if (currentViewData != null)
            return currentViewData.TryCast<T>();
        return null;
    }
    public IEnumerator coloadasync(Il2CppSystem.Action onFinished)
    {
        onFinished?.Invoke();
        yield return null;
    }
    public override Il2CppSystem.Collections.IEnumerator CoLoadAsync(Il2CppSystem.Action onFinished = null)
    {
        return coloadasync(onFinished).WrapToIl2Cpp();
    }
    public override void Unload()
    {
        Logger.Info("UNLOAD");
    }
    public override void LoadAsync(Il2CppSystem.Action onSuccessCb = null, Il2CppSystem.Action onErrorcb = null, Il2CppSystem.Action onFinishedcb = null)
    {
        onSuccessCb?.Invoke();
        return;
    }
}
