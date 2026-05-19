using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
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
            if (!TryGetSavePath(defaultFileName, out string path))
            {
                onComplete(PresetFilePickerResult.Cancelled());
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
            if (!TryGetOpenPath(out string path))
            {
                onComplete(PresetFilePickerResult.Cancelled());
                return;
            }

            onComplete(PresetFilePickerResult.Success(File.ReadAllBytes(path)));
        }
        catch (Exception ex)
        {
            onComplete(PresetFilePickerResult.Error(ex.Message));
        }
    }

    private static bool TryGetSavePath(string defaultFileName, out string path)
    {
        using var dialogData = OpenFileNameData.Create(defaultFileName, "Export Preset", "snrpresets");
        dialogData.OpenFileName.Flags = OfnExplorer | OfnHideReadOnly | OfnNoChangeDir | OfnOverwritePrompt | OfnPathMustExist;

        bool success = GetSaveFileNameW(ref dialogData.OpenFileName);
        path = success ? dialogData.GetSelectedPath() : string.Empty;
        return success;
    }

    private static bool TryGetOpenPath(out string path)
    {
        using var dialogData = OpenFileNameData.Create(string.Empty, "Import Preset", string.Empty);
        dialogData.OpenFileName.Flags = OfnExplorer | OfnFileMustExist | OfnHideReadOnly | OfnNoChangeDir | OfnPathMustExist;

        bool success = GetOpenFileNameW(ref dialogData.OpenFileName);
        path = success ? dialogData.GetSelectedPath() : string.Empty;
        return success;
    }

    [DllImport("comdlg32.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "GetOpenFileNameW")]
    private static extern bool GetOpenFileNameW(ref OpenFileName ofn);

    [DllImport("comdlg32.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "GetSaveFileNameW")]
    private static extern bool GetSaveFileNameW(ref OpenFileName ofn);

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
    private const string BridgeResourceName = "SuperNewRoles.Resources.AndroidPresetFilePickerBridge.jar";
    private const string BridgeClassName = "com.supernewroles.preset.PresetFilePickerBridge";
    private const string ExportMethodName = "exportPreset";
    private const string ImportMethodName = "importPreset";
    private const string ReceiverObjectName = PresetFilePickerAndroidCallbackReceiver.GameObjectName;
    private static readonly object PendingRequestsLock = new();
    private static readonly Dictionary<string, PendingRequest> PendingRequests = new();
    private static IntPtr _dexClassLoader;
    private static IntPtr _bridgeClass;

    public void Export(string defaultFileName, byte[] contents, Action<PresetFilePickerResult> onComplete)
    {
        try
        {
            string requestId = Guid.NewGuid().ToString("N");
            string sourcePath = Path.Combine(Application.temporaryCachePath, $"snr-preset-export-{requestId}.snrpresets");
            Directory.CreateDirectory(Path.GetDirectoryName(sourcePath));
            File.WriteAllBytes(sourcePath, contents ?? Array.Empty<byte>());
            RegisterPendingRequest(requestId, new PendingRequest("export", sourcePath, onComplete));

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
            onComplete(PresetFilePickerResult.Error(ex.Message));
        }
    }

    public void Import(Action<PresetFilePickerResult> onComplete)
    {
        try
        {
            string requestId = Guid.NewGuid().ToString("N");
            string targetPath = Path.Combine(Application.temporaryCachePath, $"snr-preset-import-{requestId}.snrpresets");
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
            RegisterPendingRequest(requestId, new PendingRequest("import", targetPath, onComplete));

            InvokeBridge(
                ImportMethodName,
                new[] { "android/app/Activity", "java/lang/String", "java/lang/String", "java/lang/String" },
                requestId,
                targetPath,
                ReceiverObjectName);
        }
        catch (Exception ex)
        {
            onComplete(PresetFilePickerResult.Error(ex.Message));
        }
    }

    internal static void HandleAndroidResult(string payload)
    {
        try
        {
            var data = JsonParser.Parse(payload) as Dictionary<string, object>
                ?? throw new PresetImportExportException("Android file picker returned invalid JSON.");
            string requestId = ReadString(data, "requestId");
            string status = ReadString(data, "status");
            string error = data.TryGetValue("error", out var errorValue) ? errorValue as string ?? string.Empty : string.Empty;

            PendingRequest pendingRequest;
            lock (PendingRequestsLock)
            {
                if (!PendingRequests.TryGetValue(requestId, out pendingRequest))
                    return;
                PendingRequests.Remove(requestId);
            }

            if (status == "cancelled")
            {
                pendingRequest.Complete(PresetFilePickerResult.Cancelled());
                return;
            }
            if (status != "success")
            {
                pendingRequest.Complete(PresetFilePickerResult.Error(string.IsNullOrEmpty(error) ? "Android file picker failed." : error));
                return;
            }

            if (pendingRequest.Action == "import")
                pendingRequest.Complete(PresetFilePickerResult.Success(File.ReadAllBytes(pendingRequest.TempFilePath)));
            else
                pendingRequest.Complete(PresetFilePickerResult.Success());
        }
        catch (Exception ex)
        {
            Logger.Error($"Android preset file picker callback failed: {ex}");
        }
    }

    private static void RegisterPendingRequest(string requestId, PendingRequest pendingRequest)
    {
        lock (PendingRequestsLock)
        {
            PendingRequests[requestId] = pendingRequest;
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
        try
        {
            return CallObjectMethod(obj, methodName, signature);
        }
        catch
        {
            return IntPtr.Zero;
        }
    }

    private static string CallStringMethod(IntPtr obj, string methodName, string signature)
    {
        IntPtr objClass = AndroidJNI.GetObjectClass(obj);
        try
        {
            IntPtr methodId = AndroidJNI.GetMethodID(objClass, methodName, signature);
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

        public PendingRequest(string action, string tempFilePath, Action<PresetFilePickerResult> onComplete)
        {
            Action = action;
            TempFilePath = tempFilePath;
            _onComplete = onComplete;
        }

        public void Complete(PresetFilePickerResult result)
            => _onComplete?.Invoke(result);
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
