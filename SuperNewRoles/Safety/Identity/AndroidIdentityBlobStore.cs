using System;
using System.IO;
using SuperNewRoles.Modules;
using UnityEngine;

namespace SuperNewRoles.Safety.Identity;

/// <summary>
/// ランチャーのプロファイル（Unity persistentDataPath / BepInEx）の外に身元を置く。
/// SharedPreferences と外部ストレージの SNR フォルダへ二重に残す。
/// </summary>
internal static class AndroidIdentityBlobStore
{
    private const string PrefsName = "jp.ykundesu.snr.identity";
    private const string PrefsKey = "player-identity";

    public static string TryExternalDirectory()
    {
        if (!ModHelpers.IsAndroid())
            return null;
        try
        {
            string root = GetExternalStorageRoot();
            if (string.IsNullOrEmpty(root))
                return null;
            return Path.Combine(root, "SuperNewRoles", "identity");
        }
        catch (Exception ex)
        {
            Logger.Warning($"Android external identity directory failed: {ex.Message}");
            return null;
        }
    }

    public static byte[] TryReadPrefs()
    {
        if (!ModHelpers.IsAndroid())
            return null;
        IntPtr activity = IntPtr.Zero;
        IntPtr prefsName = IntPtr.Zero;
        IntPtr prefsKey = IntPtr.Zero;
        IntPtr prefs = IntPtr.Zero;
        try
        {
            activity = GetCurrentActivity();
            prefsName = AndroidJNI.NewStringUTF(PrefsName);
            prefsKey = AndroidJNI.NewStringUTF(PrefsKey);
            prefs = CallObject(activity, "getSharedPreferences", "(Ljava/lang/String;I)Landroid/content/SharedPreferences;", prefsName, 0);
            string text = CallString(prefs, "getString", "(Ljava/lang/String;Ljava/lang/String;)Ljava/lang/String;", prefsKey, IntPtr.Zero);
            if (string.IsNullOrEmpty(text))
                return null;
            return Convert.FromBase64String(text);
        }
        catch (Exception ex)
        {
            Logger.Warning($"Android identity prefs read failed: {ex.Message}");
            return null;
        }
        finally
        {
            Delete(prefs);
            Delete(prefsKey);
            Delete(prefsName);
            Delete(activity);
        }
    }

    public static void TryWritePrefs(byte[] blob)
    {
        if (!ModHelpers.IsAndroid() || blob == null || blob.Length == 0)
            return;
        IntPtr activity = IntPtr.Zero;
        IntPtr prefsName = IntPtr.Zero;
        IntPtr prefsKey = IntPtr.Zero;
        IntPtr prefs = IntPtr.Zero;
        IntPtr editor = IntPtr.Zero;
        IntPtr encoded = IntPtr.Zero;
        try
        {
            activity = GetCurrentActivity();
            prefsName = AndroidJNI.NewStringUTF(PrefsName);
            prefsKey = AndroidJNI.NewStringUTF(PrefsKey);
            encoded = AndroidJNI.NewStringUTF(Convert.ToBase64String(blob));
            prefs = CallObject(activity, "getSharedPreferences", "(Ljava/lang/String;I)Landroid/content/SharedPreferences;", prefsName, 0);
            editor = CallObject(prefs, "edit", "()Landroid/content/SharedPreferences$Editor;");
            CallObject(editor, "putString", "(Ljava/lang/String;Ljava/lang/String;)Landroid/content/SharedPreferences$Editor;", prefsKey, encoded);
            CallVoid(editor, "apply", "()V");
        }
        catch (Exception ex)
        {
            Logger.Warning($"Android identity prefs write failed: {ex.Message}");
        }
        finally
        {
            Delete(encoded);
            Delete(editor);
            Delete(prefs);
            Delete(prefsKey);
            Delete(prefsName);
            Delete(activity);
        }
    }

    private static string GetExternalStorageRoot()
    {
        IntPtr envClass = IntPtr.Zero;
        IntPtr file = IntPtr.Zero;
        try
        {
            envClass = AndroidJNI.FindClass("android/os/Environment");
            ThrowIfJava("Environment");
            IntPtr id = AndroidJNI.GetStaticMethodID(envClass, "getExternalStorageDirectory", "()Ljava/io/File;");
            ThrowIfJava("getExternalStorageDirectory");
            file = AndroidJNI.CallStaticObjectMethod(envClass, id, Array.Empty<jvalue>());
            ThrowIfJava("getExternalStorageDirectory");
            return CallString(file, "getAbsolutePath", "()Ljava/lang/String;");
        }
        finally
        {
            Delete(file);
            Delete(envClass);
        }
    }

    private static IntPtr GetCurrentActivity()
    {
        IntPtr unityPlayerClass = AndroidJNI.FindClass("com/unity3d/player/UnityPlayer");
        try
        {
            IntPtr fieldId = AndroidJNI.GetStaticFieldID(unityPlayerClass, "currentActivity", "Landroid/app/Activity;");
            IntPtr activity = AndroidJNI.GetStaticObjectField(unityPlayerClass, fieldId);
            if (activity == IntPtr.Zero)
                throw new InvalidOperationException("Android currentActivity is null.");
            return activity;
        }
        finally
        {
            Delete(unityPlayerClass);
        }
    }

    private static IntPtr CallObject(IntPtr obj, string name, string signature, IntPtr arg1, int arg2)
    {
        IntPtr clazz = AndroidJNI.GetObjectClass(obj);
        try
        {
            IntPtr id = AndroidJNI.GetMethodID(clazz, name, signature);
            ThrowIfJava(name);
            IntPtr result = AndroidJNI.CallObjectMethod(obj, id, new[]
            {
                new jvalue { l = arg1 },
                new jvalue { i = arg2 }
            });
            ThrowIfJava(name);
            return result;
        }
        finally
        {
            Delete(clazz);
        }
    }

    private static IntPtr CallObject(IntPtr obj, string name, string signature, params IntPtr[] args)
    {
        IntPtr clazz = AndroidJNI.GetObjectClass(obj);
        try
        {
            IntPtr id = AndroidJNI.GetMethodID(clazz, name, signature);
            ThrowIfJava(name);
            var values = new jvalue[args.Length];
            for (int i = 0; i < args.Length; i++)
                values[i] = new jvalue { l = args[i] };
            IntPtr result = AndroidJNI.CallObjectMethod(obj, id, values);
            ThrowIfJava(name);
            return result;
        }
        finally
        {
            Delete(clazz);
        }
    }

    private static void CallVoid(IntPtr obj, string name, string signature)
    {
        IntPtr clazz = AndroidJNI.GetObjectClass(obj);
        try
        {
            IntPtr id = AndroidJNI.GetMethodID(clazz, name, signature);
            ThrowIfJava(name);
            AndroidJNI.CallVoidMethod(obj, id, Array.Empty<jvalue>());
            ThrowIfJava(name);
        }
        finally
        {
            Delete(clazz);
        }
    }

    private static string CallString(IntPtr obj, string name, string signature)
    {
        return CallString(obj, name, signature, Array.Empty<IntPtr>());
    }

    private static string CallString(IntPtr obj, string name, string signature, params IntPtr[] args)
    {
        IntPtr clazz = AndroidJNI.GetObjectClass(obj);
        try
        {
            IntPtr id = AndroidJNI.GetMethodID(clazz, name, signature);
            ThrowIfJava(name);
            var values = new jvalue[args.Length];
            for (int i = 0; i < args.Length; i++)
                values[i] = new jvalue { l = args[i] };
            return AndroidJNI.CallStringMethod(obj, id, values);
        }
        finally
        {
            Delete(clazz);
        }
    }

    private static void ThrowIfJava(string where)
    {
        IntPtr exception = AndroidJNI.ExceptionOccurred();
        if (exception == IntPtr.Zero)
            return;
        AndroidJNI.ExceptionClear();
        string message = "Java exception";
        try
        {
            IntPtr exceptionClass = AndroidJNI.GetObjectClass(exception);
            IntPtr toString = AndroidJNI.GetMethodID(exceptionClass, "toString", "()Ljava/lang/String;");
            message = AndroidJNI.CallStringMethod(exception, toString, Array.Empty<jvalue>()) ?? message;
            Delete(exceptionClass);
        }
        catch
        {
            // 例外文字列が取れなくても、発生したこと自体は返す
        }
        finally
        {
            Delete(exception);
        }
        throw new InvalidOperationException($"{where}: {message}");
    }

    private static void Delete(IntPtr localRef)
    {
        if (localRef != IntPtr.Zero)
            AndroidJNI.DeleteLocalRef(localRef);
    }
}
