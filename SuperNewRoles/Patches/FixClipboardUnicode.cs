using System;
using System.Runtime.InteropServices;
using HarmonyLib;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(ClipboardHelper), nameof(ClipboardHelper.GetClipboardString))]
public static class FixClipboardUnicodePatch
{
    private const uint CF_UNICODETEXT = 13;
    public static bool Prefix(ref string __result)
    {
        if (ModHelpers.IsAndroid())
            return true;

        __result = null;

        // 1) Unicode テキスト形式かどうかチェック
        if (!ClipboardHelper.IsClipboardFormatAvailable(CF_UNICODETEXT))
        {
            return false;
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