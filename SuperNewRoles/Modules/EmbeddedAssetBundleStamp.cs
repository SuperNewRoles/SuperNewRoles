using System.Reflection;

namespace SuperNewRoles.Modules;

internal static class EmbeddedAssetBundleStamp
{
    internal static string Create(Assembly assembly, string resourceName)
    {
        // DLLの再配置・再展開では変化せず、再ビルド時には変わるIDを使う。
        return string.Join("\n", "bundle-cache-v2", resourceName, assembly.FullName,
            assembly.ManifestModule.ModuleVersionId.ToString("N"));
    }
}
