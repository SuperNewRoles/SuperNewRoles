using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using InnerNet;
using TMPro;
using UnityEngine;

namespace SuperNewRoles.Modules;

public enum PresetFilePickerStatus
{
    Success,
    Cancelled,
    Error
}

public sealed class PresetFilePickerResult
{
    public PresetFilePickerStatus Status { get; }
    public byte[] Content { get; }
    public string ErrorMessage { get; }

    private PresetFilePickerResult(PresetFilePickerStatus status, byte[] content = null, string errorMessage = "")
    {
        Status = status;
        Content = content ?? Array.Empty<byte>();
        ErrorMessage = errorMessage ?? string.Empty;
    }

    public static PresetFilePickerResult Success(byte[] content = null)
        => new(PresetFilePickerStatus.Success, content);

    public static PresetFilePickerResult Cancelled()
        => new(PresetFilePickerStatus.Cancelled);

    public static PresetFilePickerResult Error(string errorMessage)
        => new(PresetFilePickerStatus.Error, errorMessage: errorMessage);
}

public interface IPresetFilePicker
{
    void Export(string defaultFileName, byte[] contents, Action<PresetFilePickerResult> onComplete);
    void Import(Action<PresetFilePickerResult> onComplete);
}

public static class PresetFilePickerWorkflow
{
    public static void Export(
        IPresetFilePicker filePicker,
        string defaultFileName,
        Func<byte[]> createContents,
        Action onSuccess,
        Action<string> onNotCompleted,
        Action<Exception> onFailed)
    {
        try
        {
            byte[] contents = createContents();
            filePicker.Export(defaultFileName, contents, result =>
            {
                if (result.Status == PresetFilePickerStatus.Cancelled)
                    return;
                if (result.Status == PresetFilePickerStatus.Error)
                {
                    onNotCompleted(result.ErrorMessage);
                    return;
                }

                onSuccess();
            });
        }
        catch (Exception ex)
        {
            onFailed(ex);
        }
    }

    public static void Import(
        IPresetFilePicker filePicker,
        Func<byte[], PresetImportResult> importContents,
        Action<PresetImportResult> onSuccess,
        Action<string> onNotCompleted,
        Action<Exception> onFailed)
    {
        try
        {
            filePicker.Import(result =>
            {
                if (result.Status == PresetFilePickerStatus.Cancelled)
                    return;
                if (result.Status == PresetFilePickerStatus.Error)
                {
                    onNotCompleted(result.ErrorMessage);
                    return;
                }

                try
                {
                    onSuccess(importContents(result.Content));
                }
                catch (Exception ex)
                {
                    onFailed(ex);
                }
            });
        }
        catch (Exception ex)
        {
            onFailed(ex);
        }
    }
}

public static class PresetFilePickerFactory
{
    public static IPresetFilePicker Create()
    {
        if (ModHelpers.IsAndroid())
            return new AndroidPresetFilePicker();
        if (OperatingSystem.IsWindows())
            return new WindowsPresetFilePicker();
        return new UnsupportedPresetFilePicker();
    }
}

internal sealed class UnsupportedPresetFilePicker : IPresetFilePicker
{
    public void Export(string defaultFileName, byte[] contents, Action<PresetFilePickerResult> onComplete)
        => onComplete(PresetFilePickerResult.Error("Preset file picker is not supported on this platform."));

    public void Import(Action<PresetFilePickerResult> onComplete)
        => onComplete(PresetFilePickerResult.Error("Preset file picker is not supported on this platform."));
}

internal sealed class WindowsPresetFilePicker : IPresetFilePicker
{
    private const int MaxPathBuffer = 4096;
    private const int OfnExplorer = 0x00080000;
    private const int OfnFileMustExist = 0x00001000;
    private const int OfnHideReadOnly = 0x00000004;
    private const int OfnNoChangeDir = 0x00000008;
    private const int OfnOverwritePrompt = 0x00000002;
    private const int OfnPathMustExist = 0x00000800;

    private const string PresetFilter =
        "SuperNewRoles Presets (*.snrpresets)\0*.snrpresets\0ZIP Archive (*.zip)\0*.zip\0All Files (*.*)\0*.*\0\0";

    public void Export(string defaultFileName, byte[] contents, Action<PresetFilePickerResult> onComplete)
    {
        try
        {
            if (!TryGetSavePath(defaultFileName, out string path, out string dialogError))
            {
                onComplete(string.IsNullOrEmpty(dialogError)
                    ? PresetFilePickerResult.Cancelled()
                    : PresetFilePickerResult.Error(dialogError));
                return;
            }

            if (string.IsNullOrWhiteSpace(Path.GetExtension(path)))
                path += ".snrpresets";
            File.WriteAllBytes(path, contents ?? Array.Empty<byte>());
            onComplete(PresetFilePickerResult.Success());
        }
        catch (Exception ex)
        {
            onComplete(PresetFilePickerResult.Error(ex.Message));
        }
    }

    public void Import(Action<PresetFilePickerResult> onComplete)
    {
        try
        {
            if (!TryGetOpenPath(out string path, out string dialogError))
            {
                onComplete(string.IsNullOrEmpty(dialogError)
                    ? PresetFilePickerResult.Cancelled()
                    : PresetFilePickerResult.Error(dialogError));
                return;
            }

            onComplete(PresetFilePickerResult.Success(File.ReadAllBytes(path)));
        }
        catch (Exception ex)
        {
            onComplete(PresetFilePickerResult.Error(ex.Message));
        }
    }

    private static bool TryGetSavePath(string defaultFileName, out string path, out string errorMessage)
    {
        using var dialogData = OpenFileNameData.Create(defaultFileName, "Export Preset", "snrpresets");
        dialogData.OpenFileName.Flags = OfnExplorer | OfnHideReadOnly | OfnNoChangeDir | OfnOverwritePrompt | OfnPathMustExist;

        bool success = GetSaveFileNameW(ref dialogData.OpenFileName);
        path = success ? dialogData.GetSelectedPath() : string.Empty;
        errorMessage = success ? string.Empty : GetCommonDialogErrorMessage();
        return success;
    }

    private static bool TryGetOpenPath(out string path, out string errorMessage)
    {
        using var dialogData = OpenFileNameData.Create(string.Empty, "Import Preset", string.Empty);
        dialogData.OpenFileName.Flags = OfnExplorer | OfnFileMustExist | OfnHideReadOnly | OfnNoChangeDir | OfnPathMustExist;

        bool success = GetOpenFileNameW(ref dialogData.OpenFileName);
        path = success ? dialogData.GetSelectedPath() : string.Empty;
        errorMessage = success ? string.Empty : GetCommonDialogErrorMessage();
        return success;
    }

    private static string GetCommonDialogErrorMessage()
    {
        int errorCode = CommDlgExtendedError();
        return errorCode == 0
            ? string.Empty
            : $"Windows file dialog failed with error 0x{errorCode:X}.";
    }

    [DllImport("comdlg32.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "GetOpenFileNameW")]
    private static extern bool GetOpenFileNameW(ref OpenFileName ofn);

    [DllImport("comdlg32.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "GetSaveFileNameW")]
    private static extern bool GetSaveFileNameW(ref OpenFileName ofn);

    [DllImport("comdlg32.dll", SetLastError = true, EntryPoint = "CommDlgExtendedError")]
    private static extern int CommDlgExtendedError();

    private sealed class OpenFileNameData : IDisposable
    {
        private readonly IntPtr _filterBuffer;
        private readonly IntPtr _fileBuffer;
        private readonly IntPtr _titleBuffer;
        private readonly IntPtr _defaultExtensionBuffer;

        public OpenFileName OpenFileName;

        private OpenFileNameData(IntPtr filterBuffer, IntPtr fileBuffer, IntPtr titleBuffer, IntPtr defaultExtensionBuffer)
        {
            _filterBuffer = filterBuffer;
            _fileBuffer = fileBuffer;
            _titleBuffer = titleBuffer;
            _defaultExtensionBuffer = defaultExtensionBuffer;
        }

        public static OpenFileNameData Create(string fileName, string title, string defaultExtension)
        {
            IntPtr filterBuffer = Marshal.StringToHGlobalUni(PresetFilter);
            IntPtr fileBuffer = Marshal.AllocHGlobal(MaxPathBuffer * sizeof(char));
            IntPtr titleBuffer = Marshal.StringToHGlobalUni(title);
            IntPtr defaultExtensionBuffer = string.IsNullOrEmpty(defaultExtension)
                ? IntPtr.Zero
                : Marshal.StringToHGlobalUni(defaultExtension);
            ZeroMemory(fileBuffer, MaxPathBuffer * sizeof(char));

            if (!string.IsNullOrEmpty(fileName))
            {
                string clippedFileName = fileName.Length >= MaxPathBuffer ? fileName[..(MaxPathBuffer - 1)] : fileName;
                byte[] fileNameBytes = Encoding.Unicode.GetBytes(clippedFileName);
                Marshal.Copy(fileNameBytes, 0, fileBuffer, fileNameBytes.Length);
            }

            var data = new OpenFileNameData(filterBuffer, fileBuffer, titleBuffer, defaultExtensionBuffer);
            data.OpenFileName = new OpenFileName
            {
                lStructSize = Marshal.SizeOf<OpenFileName>(),
                lpstrFilter = filterBuffer,
                lpstrFile = fileBuffer,
                nMaxFile = MaxPathBuffer,
                lpstrTitle = titleBuffer,
                nFilterIndex = 1,
                lpstrDefExt = defaultExtensionBuffer
            };
            return data;
        }

        public string GetSelectedPath()
            => Marshal.PtrToStringUni(_fileBuffer) ?? string.Empty;

        public void Dispose()
        {
            FreeHGlobal(_filterBuffer);
            FreeHGlobal(_fileBuffer);
            FreeHGlobal(_titleBuffer);
            FreeHGlobal(_defaultExtensionBuffer);
        }

        private static void ZeroMemory(IntPtr target, int bytes)
        {
            byte[] zeroes = new byte[bytes];
            Marshal.Copy(zeroes, 0, target, bytes);
        }

        private static void FreeHGlobal(IntPtr value)
        {
            if (value != IntPtr.Zero)
                Marshal.FreeHGlobal(value);
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct OpenFileName
    {
        public int lStructSize;
        public IntPtr hwndOwner;
        public IntPtr hInstance;
        public IntPtr lpstrFilter;
        public IntPtr lpstrCustomFilter;
        public int nMaxCustFilter;
        public int nFilterIndex;
        public IntPtr lpstrFile;
        public int nMaxFile;
        public IntPtr lpstrFileTitle;
        public int nMaxFileTitle;
        public IntPtr lpstrInitialDir;
        public IntPtr lpstrTitle;
        public int Flags;
        public short nFileOffset;
        public short nFileExtension;
        public IntPtr lpstrDefExt;
        public IntPtr lCustData;
        public IntPtr lpfnHook;
        public IntPtr lpTemplateName;
        public IntPtr pvReserved;
        public int dwReserved;
        public int FlagsEx;
    }
}

internal sealed class AndroidPresetFilePicker : IPresetFilePicker
{
    private const int AndroidSafWarningStringName = 100000 + 0x534e52;
    private const string BridgeResourceName = "SuperNewRoles.Resources.AndroidPresetFilePickerBridge.jar";
    private const string BridgeClassName = "com.supernewroles.preset.PresetFilePickerBridge";
    private const string ExportMethodName = "exportPreset";
    private const string ImportMethodName = "importPreset";
    private const string ReceiverObjectName = PresetFilePickerAndroidCallbackReceiver.GameObjectName;
    private static readonly object PendingRequestsLock = new();
    private static readonly Dictionary<string, PendingRequest> PendingRequests = new();
    // SAFの選択画面にいる間だけ、Among Usのサスペンド切断猶予を延ばして戻す。
    private static readonly AndroidSafSuspensionGuard SafSuspensionGuard = new(
        () => InnerNetClient.SecondsSuspendedBeforeDisconnect,
        value => InnerNetClient.SecondsSuspendedBeforeDisconnect = value,
        () => Application.runInBackground,
        value => Application.runInBackground = value,
        message => Logger.Warning(message));
    private static IntPtr _dexClassLoader;
    private static IntPtr _bridgeClass;

    public void Export(string defaultFileName, byte[] contents, Action<PresetFilePickerResult> onComplete)
    {
        string requestId = string.Empty;
        IDisposable suspensionScope = null;
        try
        {
            requestId = Guid.NewGuid().ToString("N");
            string sourcePath = Path.Combine(Application.temporaryCachePath, $"snr-preset-export-{requestId}.snrpresets");
            Directory.CreateDirectory(Path.GetDirectoryName(sourcePath));
            File.WriteAllBytes(sourcePath, contents ?? Array.Empty<byte>());
            suspensionScope = BeginAndroidSafSession();
            // ここでscopeの所有権をPendingRequestへ移す。コールバック未到達時も失敗経路でDisposeする。
            if (!TryRegisterPendingRequest(requestId, new PendingRequest("export", sourcePath, onComplete, suspensionScope), out string registerError))
            {
                suspensionScope.Dispose();
                suspensionScope = null;
                onComplete?.Invoke(PresetFilePickerResult.Error(registerError));
                return;
            }
            suspensionScope = null;

            InvokeBridge(
                ExportMethodName,
                new[] { "android/app/Activity", "java/lang/String", "java/lang/String", "java/lang/String", "java/lang/String" },
                requestId,
                sourcePath,
                defaultFileName,
                ReceiverObjectName);
        }
        catch (Exception ex)
        {
            CompleteFailedStart(requestId, suspensionScope, onComplete, ex);
        }
    }

    public void Import(Action<PresetFilePickerResult> onComplete)
    {
        string requestId = string.Empty;
        IDisposable suspensionScope = null;
        try
        {
            requestId = Guid.NewGuid().ToString("N");
            string targetPath = Path.Combine(Application.temporaryCachePath, $"snr-preset-import-{requestId}.snrpresets");
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
            suspensionScope = BeginAndroidSafSession();
            // ここでscopeの所有権をPendingRequestへ移す。コールバック未到達時も失敗経路でDisposeする。
            if (!TryRegisterPendingRequest(requestId, new PendingRequest("import", targetPath, onComplete, suspensionScope), out string registerError))
            {
                suspensionScope.Dispose();
                suspensionScope = null;
                onComplete?.Invoke(PresetFilePickerResult.Error(registerError));
                return;
            }
            suspensionScope = null;

            InvokeBridge(
                ImportMethodName,
                new[] { "android/app/Activity", "java/lang/String", "java/lang/String", "java/lang/String" },
                requestId,
                targetPath,
                ReceiverObjectName);
        }
        catch (Exception ex)
        {
            CompleteFailedStart(requestId, suspensionScope, onComplete, ex);
        }
    }

    internal static void HandleAndroidResult(string payload)
    {
        PendingRequest pendingRequest = null;
        bool completed = false;
        try
        {
            var data = JsonParser.Parse(payload) as Dictionary<string, object>
                ?? throw new PresetImportExportException("Android file picker returned invalid JSON.");
            string requestId = ReadString(data, "requestId");
            string status = ReadString(data, "status");
            string error = data.TryGetValue("error", out var errorValue) ? errorValue as string ?? string.Empty : string.Empty;

            pendingRequest = RemovePendingRequest(requestId);
            if (pendingRequest == null)
                return;

            if (status == "cancelled")
            {
                completed = true;
                pendingRequest.Complete(PresetFilePickerResult.Cancelled());
                return;
            }
            if (status != "success")
            {
                completed = true;
                pendingRequest.Complete(PresetFilePickerResult.Error(string.IsNullOrEmpty(error) ? "Android file picker failed." : error));
                return;
            }

            var result = pendingRequest.Action == "import"
                ? PresetFilePickerResult.Success(File.ReadAllBytes(pendingRequest.TempFilePath))
                : PresetFilePickerResult.Success();
            completed = true;
            pendingRequest.Complete(result);
        }
        catch (Exception ex)
        {
            Logger.Error($"Android preset file picker callback failed: {ex}");
            if (pendingRequest != null && !completed)
                pendingRequest.Complete(PresetFilePickerResult.Error(ex.Message));
            else if (pendingRequest == null)
                CompleteAllPendingRequests(PresetFilePickerResult.Error("Android file picker callback failed."));
        }
    }

    private static bool TryRegisterPendingRequest(string requestId, PendingRequest pendingRequest, out string errorMessage)
    {
        lock (PendingRequestsLock)
        {
            if (PendingRequests.Count > 0)
            {
                errorMessage = "Android file picker is already open.";
                return false;
            }

            PendingRequests[requestId] = pendingRequest;
            errorMessage = string.Empty;
            return true;
        }
    }

    private static PendingRequest RemovePendingRequest(string requestId)
    {
        if (string.IsNullOrEmpty(requestId))
            return null;

        lock (PendingRequestsLock)
        {
            if (!PendingRequests.TryGetValue(requestId, out var pendingRequest))
                return null;
            PendingRequests.Remove(requestId);
            return pendingRequest;
        }
    }

    private static void CompleteAllPendingRequests(PresetFilePickerResult result)
    {
        List<PendingRequest> pendingRequests;
        lock (PendingRequestsLock)
        {
            // コールバック内容が壊れてrequestIdを特定できない場合は、残っているSAF待ちをまとめて閉じる。
            pendingRequests = PendingRequests.Values.ToList();
            PendingRequests.Clear();
        }

        foreach (var pendingRequest in pendingRequests)
            pendingRequest.Complete(result);
    }

    private static void CompleteFailedStart(
        string requestId,
        IDisposable suspensionScope,
        Action<PresetFilePickerResult> onComplete,
        Exception ex)
    {
        // RegisterPendingRequest後にDexClassLoaderやJava呼び出しで失敗した場合も、必ずguardを解除する。
        var pendingRequest = RemovePendingRequest(requestId);
        if (pendingRequest != null)
        {
            pendingRequest.Complete(PresetFilePickerResult.Error(ex.Message));
            return;
        }

        suspensionScope?.Dispose();
        onComplete?.Invoke(PresetFilePickerResult.Error(ex.Message));
    }

    private static IDisposable BeginAndroidSafSession()
    {
        var scope = SafSuspensionGuard.Begin();
        try
        {
            // 完全なKeepAliveではないため、オンライン中はユーザーに切断リスクを知らせる。
            ShowConnectedSafWarningIfNeeded();
        }
        catch (Exception ex)
        {
            Logger.Warning($"Could not prepare Android SAF file picker warning: {ex.Message}");
        }
        return scope;
    }

    private static void ShowConnectedSafWarningIfNeeded()
    {
        if (!IsConnected())
            return;

        string message = ModTranslation.GetString("PresetAndroidFilePickerDisconnectWarning");
        try
        {
            if (DestroyableSingleton<HudManager>.InstanceExists &&
                DestroyableSingleton<HudManager>.Instance.Notifier != null)
            {
                // NotifierはStringNames前提なので、一旦ユニークな文言を出してから実メッセージへ差し替える。
                string placeholder = $"SNRPresetFilePicker:{Environment.TickCount}";
                DestroyableSingleton<HudManager>.Instance.Notifier.AddSettingsChangeMessage(
                    (StringNames)AndroidSafWarningStringName,
                    placeholder,
                    false);
                ReplaceNotificationPlaceholder(placeholder, message);
                return;
            }
        }
        catch (Exception ex)
        {
            Logger.Warning($"Could not show Android preset file picker warning notification: {ex.Message}");
        }

        Logger.Warning(message);
    }

    private static bool IsConnected()
    {
        try
        {
            return AmongUsClient.Instance != null && AmongUsClient.Instance.AmConnected;
        }
        catch
        {
            return false;
        }
    }

    private static void ReplaceNotificationPlaceholder(string placeholder, string message)
    {
        foreach (var text in UnityEngine.Object.FindObjectsOfType<TextMeshPro>(true))
        {
            if (text == null || string.IsNullOrEmpty(text.text))
                continue;
            if (text.text.Contains(placeholder))
                text.text = message;
        }
    }

    private static string ReadString(Dictionary<string, object> data, string key)
        => data.TryGetValue(key, out var value) && value is string text
            ? text
            : throw new PresetImportExportException($"Android file picker callback is missing {key}.");

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
            if (unityPlayerClass != IntPtr.Zero)
                AndroidJNI.DeleteLocalRef(unityPlayerClass);
        }
    }

    private static void InvokeBridge(string methodName, string[] parameterClassNames, params object[] stringArgs)
    {
        PresetFilePickerAndroidCallbackReceiver.EnsureExists();
        IntPtr activity = GetCurrentActivity();
        try
        {
            IntPtr bridgeClass = EnsureBridgeClass(activity);
            var args = new object[stringArgs.Length + 1];
            args[0] = activity;
            Array.Copy(stringArgs, 0, args, 1, stringArgs.Length);
            AndroidJavaReflectionInvoker.InvokeStatic(bridgeClass, methodName, parameterClassNames, args);
        }
        finally
        {
            AndroidJNI.DeleteLocalRef(activity);
        }
    }

    private static IntPtr EnsureBridgeClass(IntPtr activity)
    {
        if (_bridgeClass != IntPtr.Zero)
            return _bridgeClass;
        if (activity == IntPtr.Zero)
            throw new InvalidOperationException("Android currentActivity is null.");

        IntPtr context = IntPtr.Zero;
        IntPtr codeCacheDir = IntPtr.Zero;
        IntPtr parentClassLoader = IntPtr.Zero;
        IntPtr dexClassLoaderLocal = IntPtr.Zero;
        IntPtr bridgeClassLocal = IntPtr.Zero;
        IntPtr jarPathString = IntPtr.Zero;
        IntPtr optimizedDirectoryString = IntPtr.Zero;
        IntPtr bridgeClassNameString = IntPtr.Zero;

        try
        {
            context = CallObjectMethod(activity, "getApplicationContext", "()Landroid/content/Context;");
            codeCacheDir = TryCallObjectMethod(context, "getCodeCacheDir", "()Ljava/io/File;");
            if (codeCacheDir == IntPtr.Zero)
                codeCacheDir = CallObjectMethod(context, "getCacheDir", "()Ljava/io/File;");
            string optimizedDirectory = CallStringMethod(codeCacheDir, "getAbsolutePath", "()Ljava/lang/String;");
            string jarPath = ExtractBridgeJar(optimizedDirectory);
            parentClassLoader = CallObjectMethod(activity, "getClassLoader", "()Ljava/lang/ClassLoader;");

            IntPtr dexClassLoaderClass = AndroidJNI.FindClass("dalvik/system/DexClassLoader");
            try
            {
                IntPtr constructorId = AndroidJNI.GetMethodID(
                    dexClassLoaderClass,
                    "<init>",
                    "(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Ljava/lang/ClassLoader;)V");
                jarPathString = AndroidJNI.NewStringUTF(jarPath);
                optimizedDirectoryString = AndroidJNI.NewStringUTF(optimizedDirectory);
                dexClassLoaderLocal = AndroidJNI.NewObject(
                    dexClassLoaderClass,
                    constructorId,
                    new[]
                    {
                        new jvalue { l = jarPathString },
                        new jvalue { l = optimizedDirectoryString },
                        new jvalue { l = IntPtr.Zero },
                        new jvalue { l = parentClassLoader }
                    });
                ThrowIfJavaException("DexClassLoader.<init>");
            }
            finally
            {
                if (dexClassLoaderClass != IntPtr.Zero)
                    AndroidJNI.DeleteLocalRef(dexClassLoaderClass);
            }

            _dexClassLoader = AndroidJNI.NewGlobalRef(dexClassLoaderLocal);
            bridgeClassNameString = AndroidJNI.NewStringUTF(BridgeClassName);
            bridgeClassLocal = CallObjectMethod(_dexClassLoader, "loadClass", "(Ljava/lang/String;)Ljava/lang/Class;", bridgeClassNameString);
            _bridgeClass = AndroidJNI.NewGlobalRef(bridgeClassLocal);
        }
        finally
        {
            DeleteLocalRef(context);
            DeleteLocalRef(codeCacheDir);
            DeleteLocalRef(parentClassLoader);
            DeleteLocalRef(dexClassLoaderLocal);
            DeleteLocalRef(bridgeClassLocal);
            DeleteLocalRef(jarPathString);
            DeleteLocalRef(optimizedDirectoryString);
            DeleteLocalRef(bridgeClassNameString);
        }

        return _bridgeClass;
    }

    private static IntPtr CallObjectMethod(IntPtr obj, string methodName, string signature, params IntPtr[] objectArgs)
    {
        IntPtr objClass = AndroidJNI.GetObjectClass(obj);
        try
        {
            IntPtr methodId = AndroidJNI.GetMethodID(objClass, methodName, signature);
            ThrowIfJavaException(methodName);
            var args = objectArgs.Select(arg => new jvalue { l = arg }).ToArray();
            IntPtr result = AndroidJNI.CallObjectMethod(obj, methodId, args);
            ThrowIfJavaException(methodName);
            return result;
        }
        finally
        {
            DeleteLocalRef(objClass);
        }
    }

    private static IntPtr TryCallObjectMethod(IntPtr obj, string methodName, string signature)
    {
        IntPtr objClass = IntPtr.Zero;
        try
        {
            objClass = AndroidJNI.GetObjectClass(obj);
            IntPtr methodId = AndroidJNI.GetMethodID(objClass, methodName, signature);
            if (AndroidJNI.ExceptionOccurred() != IntPtr.Zero)
            {
                AndroidJNI.ExceptionClear();
                return IntPtr.Zero;
            }

            IntPtr result = AndroidJNI.CallObjectMethod(obj, methodId, Array.Empty<jvalue>());
            ThrowIfJavaException(methodName);
            return result;
        }
        catch
        {
            if (AndroidJNI.ExceptionOccurred() != IntPtr.Zero)
                AndroidJNI.ExceptionClear();
            return IntPtr.Zero;
        }
        finally
        {
            DeleteLocalRef(objClass);
        }
    }

    private static string CallStringMethod(IntPtr obj, string methodName, string signature)
    {
        IntPtr objClass = AndroidJNI.GetObjectClass(obj);
        try
        {
            IntPtr methodId = AndroidJNI.GetMethodID(objClass, methodName, signature);
            ThrowIfJavaException(methodName);
            string result = AndroidJNI.CallStringMethod(obj, methodId, Array.Empty<jvalue>());
            ThrowIfJavaException(methodName);
            return result;
        }
        finally
        {
            DeleteLocalRef(objClass);
        }
    }

    private static void DeleteLocalRef(IntPtr localRef)
    {
        if (localRef != IntPtr.Zero)
            AndroidJNI.DeleteLocalRef(localRef);
    }

    private static void ThrowIfJavaException(string methodName)
    {
        IntPtr exception = AndroidJNI.ExceptionOccurred();
        if (exception == IntPtr.Zero)
            return;

        AndroidJNI.ExceptionClear();
        string javaMessage = GetJavaThrowableString(exception);
        AndroidJNI.DeleteLocalRef(exception);
        throw new InvalidOperationException($"Android preset file picker bridge failed while invoking {methodName}: {javaMessage}");
    }

    private static string GetJavaThrowableString(IntPtr exception)
    {
        IntPtr exceptionClass = IntPtr.Zero;
        try
        {
            exceptionClass = AndroidJNI.GetObjectClass(exception);
            IntPtr toStringMethodId = AndroidJNI.GetMethodID(exceptionClass, "toString", "()Ljava/lang/String;");
            return AndroidJNI.CallStringMethod(exception, toStringMethodId, Array.Empty<jvalue>()) ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
        finally
        {
            DeleteLocalRef(exceptionClass);
        }
    }

    private static string ExtractBridgeJar(string codeCacheDirectory)
    {
        string directory = Path.Combine(codeCacheDirectory, "snr-preset-picker");
        Directory.CreateDirectory(directory);
        string jarPath = Path.Combine(directory, "AndroidPresetFilePickerBridge.jar");
        if (File.Exists(jarPath))
            File.SetAttributes(jarPath, File.GetAttributes(jarPath) & ~FileAttributes.ReadOnly);

        using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(BridgeResourceName)
            ?? throw new FileNotFoundException($"Embedded Android preset file picker bridge was not found: {BridgeResourceName}");
        using var fileStream = new FileStream(jarPath, FileMode.Create, FileAccess.Write);
        stream.CopyTo(fileStream);
        fileStream.Close();
        MarkBridgeJarReadOnly(jarPath);
        return jarPath;
    }

    private static void MarkBridgeJarReadOnly(string jarPath)
    {
        File.SetAttributes(jarPath, File.GetAttributes(jarPath) | FileAttributes.ReadOnly);

        IntPtr fileClass = IntPtr.Zero;
        IntPtr pathString = IntPtr.Zero;
        IntPtr fileObject = IntPtr.Zero;
        try
        {
            fileClass = AndroidJNI.FindClass("java/io/File");
            IntPtr constructorId = AndroidJNI.GetMethodID(fileClass, "<init>", "(Ljava/lang/String;)V");
            pathString = AndroidJNI.NewStringUTF(jarPath);
            fileObject = AndroidJNI.NewObject(fileClass, constructorId, new[] { new jvalue { l = pathString } });
            ThrowIfJavaException("File.<init>");

            IntPtr setReadOnlyMethodId = AndroidJNI.GetMethodID(fileClass, "setReadOnly", "()Z");
            bool success = AndroidJNI.CallBooleanMethod(fileObject, setReadOnlyMethodId, Array.Empty<jvalue>());
            ThrowIfJavaException("File.setReadOnly");
            if (!success)
                Logger.Warning("Android preset file picker bridge jar could not be marked read-only.");
        }
        finally
        {
            DeleteLocalRef(fileObject);
            DeleteLocalRef(pathString);
            DeleteLocalRef(fileClass);
        }
    }

    private sealed class PendingRequest
    {
        public string Action { get; }
        public string TempFilePath { get; }
        private readonly Action<PresetFilePickerResult> _onComplete;
        private readonly IDisposable _suspensionScope;

        public PendingRequest(string action, string tempFilePath, Action<PresetFilePickerResult> onComplete, IDisposable suspensionScope)
        {
            Action = action;
            TempFilePath = tempFilePath;
            _onComplete = onComplete;
            _suspensionScope = suspensionScope;
        }

        public void Complete(PresetFilePickerResult result)
        {
            // 成功・キャンセル・エラーのどれでもSAF用の一時変更はここで戻す。
            _suspensionScope?.Dispose();
            _onComplete?.Invoke(result);
        }
    }
}

// AndroidのSAF画面へ遷移するとUnityがサスペンド扱いになり、Among Us側が一定秒数で切断する。
// その間だけ切断猶予とバックグラウンド実行を広げ、最後のSAF要求が終わった時に元へ戻す。
internal sealed class AndroidSafSuspensionGuard
{
    public const int MinimumSuspendedSeconds = 300;

    private readonly object _lock = new();
    private readonly Func<int> _getSuspendedSeconds;
    private readonly Action<int> _setSuspendedSeconds;
    private readonly Func<bool> _getRunInBackground;
    private readonly Action<bool> _setRunInBackground;
    private readonly Action<string> _logWarning;
    private int _activeCount;
    private int _originalSuspendedSeconds;
    private bool _originalRunInBackground;
    private bool _hasOriginalSuspendedSeconds;
    private bool _hasOriginalRunInBackground;

    public AndroidSafSuspensionGuard(
        Func<int> getSuspendedSeconds,
        Action<int> setSuspendedSeconds,
        Func<bool> getRunInBackground,
        Action<bool> setRunInBackground,
        Action<string> logWarning = null)
    {
        _getSuspendedSeconds = getSuspendedSeconds ?? throw new ArgumentNullException(nameof(getSuspendedSeconds));
        _setSuspendedSeconds = setSuspendedSeconds ?? throw new ArgumentNullException(nameof(setSuspendedSeconds));
        _getRunInBackground = getRunInBackground ?? throw new ArgumentNullException(nameof(getRunInBackground));
        _setRunInBackground = setRunInBackground ?? throw new ArgumentNullException(nameof(setRunInBackground));
        _logWarning = logWarning;
    }

    public int ActiveCount
    {
        get
        {
            lock (_lock)
                return _activeCount;
        }
    }

    public IDisposable Begin()
    {
        lock (_lock)
        {
            if (_activeCount == 0)
            {
                // 複数リクエストが重なっても、復元元は最初の1回だけ保持する。
                CaptureOriginalValues();
                ApplyTemporaryValues();
            }

            _activeCount++;
        }

        return new Scope(this);
    }

    private void End()
    {
        lock (_lock)
        {
            if (_activeCount <= 0)
                return;

            _activeCount--;
            // 最後のSAF要求が閉じた時だけ元値へ戻す。
            if (_activeCount == 0)
                RestoreOriginalValues();
        }
    }

    private void CaptureOriginalValues()
    {
        _hasOriginalSuspendedSeconds = TryGet(_getSuspendedSeconds, out _originalSuspendedSeconds, "SecondsSuspendedBeforeDisconnect");
        _hasOriginalRunInBackground = TryGet(_getRunInBackground, out _originalRunInBackground, "Application.runInBackground");
    }

    private void ApplyTemporaryValues()
    {
        // 既に長い猶予が設定されている環境では短くしない。
        int suspendedSeconds = _hasOriginalSuspendedSeconds
            ? Math.Max(_originalSuspendedSeconds, MinimumSuspendedSeconds)
            : MinimumSuspendedSeconds;
        TrySet(() => _setSuspendedSeconds(suspendedSeconds), "SecondsSuspendedBeforeDisconnect");
        TrySet(() => _setRunInBackground(true), "Application.runInBackground");
    }

    private void RestoreOriginalValues()
    {
        if (_hasOriginalSuspendedSeconds)
            TrySet(() => _setSuspendedSeconds(_originalSuspendedSeconds), "SecondsSuspendedBeforeDisconnect");
        if (_hasOriginalRunInBackground)
            TrySet(() => _setRunInBackground(_originalRunInBackground), "Application.runInBackground");
    }

    private bool TryGet<T>(Func<T> getter, out T value, string targetName)
    {
        try
        {
            value = getter();
            return true;
        }
        catch (Exception ex)
        {
            // Android/Among Usのバージョン差で対象APIが取れなくても、SAF自体は続行する。
            value = default;
            _logWarning?.Invoke($"Could not read {targetName} before Android SAF file picker: {ex.Message}");
            return false;
        }
    }

    private void TrySet(Action setter, string targetName)
    {
        try
        {
            setter();
        }
        catch (Exception ex)
        {
            // 復元処理で例外が出てもコールバック完了処理を止めない。
            _logWarning?.Invoke($"Could not update {targetName} for Android SAF file picker: {ex.Message}");
        }
    }

    private sealed class Scope : IDisposable
    {
        private readonly AndroidSafSuspensionGuard _owner;
        private bool _disposed;

        public Scope(AndroidSafSuspensionGuard owner)
        {
            _owner = owner;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _owner.End();
        }
    }
}

internal static class AndroidJavaReflectionInvoker
{
    public static void InvokeStatic(IntPtr targetClass, string methodName, string[] parameterClassNames, object[] args)
    {
        IntPtr classClass = IntPtr.Zero;
        IntPtr methodClass = IntPtr.Zero;
        IntPtr objectClass = IntPtr.Zero;
        IntPtr methodNameString = IntPtr.Zero;
        IntPtr parameterTypes = IntPtr.Zero;
        IntPtr methodObject = IntPtr.Zero;
        IntPtr invokeArgs = IntPtr.Zero;
        var localRefs = new List<IntPtr>();

        try
        {
            classClass = AndroidJNI.FindClass("java/lang/Class");
            methodClass = AndroidJNI.FindClass("java/lang/reflect/Method");
            objectClass = AndroidJNI.FindClass("java/lang/Object");
            methodNameString = AndroidJNI.NewStringUTF(methodName);
            localRefs.Add(methodNameString);

            parameterTypes = AndroidJNI.NewObjectArray(parameterClassNames.Length, classClass, IntPtr.Zero);
            localRefs.Add(parameterTypes);
            for (int i = 0; i < parameterClassNames.Length; i++)
            {
                IntPtr parameterClass = AndroidJNI.FindClass(parameterClassNames[i]);
                localRefs.Add(parameterClass);
                AndroidJNI.SetObjectArrayElement(parameterTypes, i, parameterClass);
            }

            IntPtr getMethodId = AndroidJNI.GetMethodID(
                classClass,
                "getMethod",
                "(Ljava/lang/String;[Ljava/lang/Class;)Ljava/lang/reflect/Method;");
            methodObject = AndroidJNI.CallObjectMethod(
                targetClass,
                getMethodId,
                new[]
                {
                    new jvalue { l = methodNameString },
                    new jvalue { l = parameterTypes }
                });
            localRefs.Add(methodObject);
            ThrowIfJavaException(methodName);

            invokeArgs = AndroidJNI.NewObjectArray(args.Length, objectClass, IntPtr.Zero);
            localRefs.Add(invokeArgs);
            for (int i = 0; i < args.Length; i++)
            {
                IntPtr argRef = ToJavaObjectRef(args[i], localRefs);
                AndroidJNI.SetObjectArrayElement(invokeArgs, i, argRef);
            }

            IntPtr invokeMethodId = AndroidJNI.GetMethodID(
                methodClass,
                "invoke",
                "(Ljava/lang/Object;[Ljava/lang/Object;)Ljava/lang/Object;");
            IntPtr result = AndroidJNI.CallObjectMethod(
                methodObject,
                invokeMethodId,
                new[]
                {
                    new jvalue { l = IntPtr.Zero },
                    new jvalue { l = invokeArgs }
                });
            if (result != IntPtr.Zero)
                localRefs.Add(result);
            ThrowIfJavaException(methodName);
        }
        finally
        {
            foreach (IntPtr localRef in localRefs)
            {
                if (localRef != IntPtr.Zero)
                    AndroidJNI.DeleteLocalRef(localRef);
            }
            if (classClass != IntPtr.Zero)
                AndroidJNI.DeleteLocalRef(classClass);
            if (methodClass != IntPtr.Zero)
                AndroidJNI.DeleteLocalRef(methodClass);
            if (objectClass != IntPtr.Zero)
                AndroidJNI.DeleteLocalRef(objectClass);
        }
    }

    private static IntPtr ToJavaObjectRef(object value, List<IntPtr> localRefs)
    {
        switch (value)
        {
            case null:
                return IntPtr.Zero;
            case IntPtr objectRef:
                return objectRef;
            case string text:
                IntPtr textRef = AndroidJNI.NewStringUTF(text);
                localRefs.Add(textRef);
                return textRef;
            default:
                throw new ArgumentException($"Unsupported Android bridge argument type: {value.GetType().FullName}");
        }
    }

    private static void ThrowIfJavaException(string methodName)
    {
        IntPtr exception = AndroidJNI.ExceptionOccurred();
        if (exception == IntPtr.Zero)
            return;

        AndroidJNI.ExceptionClear();
        string javaMessage = GetJavaThrowableString(exception);
        AndroidJNI.DeleteLocalRef(exception);
        throw new InvalidOperationException($"Android preset file picker bridge failed while invoking {methodName}: {javaMessage}");
    }

    private static string GetJavaThrowableString(IntPtr exception)
    {
        IntPtr exceptionClass = IntPtr.Zero;
        try
        {
            exceptionClass = AndroidJNI.GetObjectClass(exception);
            IntPtr toStringMethodId = AndroidJNI.GetMethodID(exceptionClass, "toString", "()Ljava/lang/String;");
            return AndroidJNI.CallStringMethod(exception, toStringMethodId, Array.Empty<jvalue>()) ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
        finally
        {
            if (exceptionClass != IntPtr.Zero)
                AndroidJNI.DeleteLocalRef(exceptionClass);
        }
    }
}

public class PresetFilePickerAndroidCallbackReceiver : MonoBehaviour
{
    public const string GameObjectName = "SuperNewRolesPresetFilePicker";

    public static void EnsureExists()
    {
        var gameObject = GameObject.Find(GameObjectName);
        if (gameObject == null)
        {
            gameObject = new GameObject(GameObjectName);
            GameObject.DontDestroyOnLoad(gameObject);
        }

        if (gameObject.GetComponent<PresetFilePickerAndroidCallbackReceiver>() == null)
            gameObject.AddComponent<PresetFilePickerAndroidCallbackReceiver>();
    }

    public void OnPresetFilePickerResult(string payload)
        => AndroidPresetFilePicker.HandleAndroidResult(payload);
}
