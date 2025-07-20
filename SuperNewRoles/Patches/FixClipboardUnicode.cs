using System;
using System.Reflection;
using System.Runtime.InteropServices;
using HarmonyLib;

namespace SuperNewRoles.Patches;

[HarmonyPatch]
public static class FixClipboardUnicodePatch
{
    private const uint CF_UNICODETEXT = 13;

    public static bool Prepare()
    {
        // Androidプラットフォームの場合はパッチを適用しない
        return !ModHelpers.IsAndroid();
    }

    public static MethodInfo TargetMethod()
    {
        // Androidプラットフォームの場合はnullを返してパッチを無効化
        if (ModHelpers.IsAndroid())
            return null;

        return typeof(ClipboardHelper).GetMethod(nameof(ClipboardHelper.GetClipboardString));
    }

    public static bool Prefix(ref string __result)
    {
        __result = null;

        // 1) Unicode テキスト形式かどうかチェック
        if (!ClipboardHelper.IsClipboardFormatAvailable(CF_UNICODETEXT))
        {
            return true;
        }

        // 2) クリップボードをオープン
        if (!ClipboardHelper.OpenClipboard(default))
            return false;

        IntPtr hGlobal = IntPtr.Zero;
        IntPtr pData = IntPtr.Zero;
        try
        {
            // 3) データハンドル取得
            hGlobal = ClipboardHelper.GetClipboardData(CF_UNICODETEXT);
            if (hGlobal == IntPtr.Zero)
                return false;

            // 4) 実ポインタをロック
            pData = ClipboardHelper.GlobalLock(hGlobal);
            if (pData == IntPtr.Zero)
                return false;

            // 5) ヌル終端まで長さを測る (2 バイト文字なので +2 ずつ)
            int length = 0;
            while (Marshal.ReadInt16(pData, length * 2) != 0)
                length++;

            // 6) Unicode 文字列として取得
            __result = Marshal.PtrToStringUni(pData, length);
            return false;
        }
        finally
        {
            // 必要に応じてアンロック
            if (pData != IntPtr.Zero)
                ClipboardHelper.GlobalUnlock(hGlobal);
            ClipboardHelper.CloseClipboard();
        }
    }
}