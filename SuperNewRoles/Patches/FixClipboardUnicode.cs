using System;
using System.Reflection;
using System.Runtime.InteropServices;
using HarmonyLib;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Patches;

[HarmonyPatch(typeof(ClipboardHelper), nameof(ClipboardHelper.GetClipboardString))]
public static class FixClipboardUnicodePatch
{
    private const uint CF_UNICODETEXT = 13;
    
    public static bool Prepare()
    {
        // Androidプラットフォームの場合はパッチを適用しない
        return !ModHelpers.IsAndroid();
    }
    
    public static bool Prefix(ref string __result)
    {
        __result = null;

        try 
        {
            var clipboardHelperType = typeof(ClipboardHelper);
            
            // リフレクションでprivateメソッドを呼び出し
            var isClipboardFormatAvailableMethod = clipboardHelperType.GetMethod("IsClipboardFormatAvailable", BindingFlags.NonPublic | BindingFlags.Static);
            var openClipboardMethod = clipboardHelperType.GetMethod("OpenClipboard", BindingFlags.NonPublic | BindingFlags.Static);
            var closeClipboardMethod = clipboardHelperType.GetMethod("CloseClipboard", BindingFlags.NonPublic | BindingFlags.Static);
            var getClipboardDataMethod = clipboardHelperType.GetMethod("GetClipboardData", BindingFlags.NonPublic | BindingFlags.Static);
            var globalLockMethod = clipboardHelperType.GetMethod("GlobalLock", BindingFlags.NonPublic | BindingFlags.Static);
            var globalUnlockMethod = clipboardHelperType.GetMethod("GlobalUnlock", BindingFlags.NonPublic | BindingFlags.Static);

            // 1) Unicode テキスト形式かどうかチェック
            if (!(bool)isClipboardFormatAvailableMethod.Invoke(null, new object[] { CF_UNICODETEXT }))
            {
                return true;
            }

            // 2) クリップボードをオープン
            if (!(bool)openClipboardMethod.Invoke(null, new object[] { IntPtr.Zero }))
                return false;

            IntPtr hGlobal = IntPtr.Zero;
            IntPtr pData = IntPtr.Zero;
            try
            {
                // 3) データハンドル取得
                hGlobal = (IntPtr)getClipboardDataMethod.Invoke(null, new object[] { CF_UNICODETEXT });
                if (hGlobal == IntPtr.Zero)
                    return false;

                // 4) 実ポインタをロック
                pData = (IntPtr)globalLockMethod.Invoke(null, new object[] { hGlobal });
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
                    globalUnlockMethod.Invoke(null, new object[] { hGlobal });
                closeClipboardMethod.Invoke(null, null);
            }
        }
        catch
        {
            // エラーが発生した場合は元のメソッドを実行
            return true;
        }
    }
}