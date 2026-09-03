using System;
using SuperNewRoles.Modules;
using UnityEngine;

namespace SuperNewRoles.Safety.Identity;

/// <summary>
/// Android Keystore の AES 鍵で PKCS8 を包む。Windows の DPAPI 相当。
/// 失敗したら null を返し、呼び出し側は平文ファイルへ落とす。
/// </summary>
internal static class AndroidKeyProtector
{
    private const string StoreName = "AndroidKeyStore";
    private const string Alias = "jp.ykundesu.snr.player-identity";
    private const string AesTransform = "AES/GCM/NoPadding";
    private const int GcmTagBits = 128;
    private const int PurposeEncryptDecrypt = 3;

    public static byte[] TryProtect(byte[] plain)
    {
        if (!ModHelpers.IsAndroid() || plain == null || plain.Length == 0)
            return null;
        try
        {
            return Transform(plain, encrypt: true);
        }
        catch (Exception ex)
        {
            Logger.Warning($"Android Keystore protect failed: {ex.Message}");
            return null;
        }
    }

    public static byte[] TryUnprotect(byte[] payload)
    {
        if (!ModHelpers.IsAndroid() || payload == null || payload.Length < 13)
            return null;
        try
        {
            return Transform(payload, encrypt: false);
        }
        catch (Exception ex)
        {
            Logger.Warning($"Android Keystore unprotect failed: {ex.Message}");
            return null;
        }
    }

    private static byte[] Transform(byte[] data, bool encrypt)
    {
        IntPtr keyStoreClass = IntPtr.Zero;
        IntPtr keyGeneratorClass = IntPtr.Zero;
        IntPtr builderClass = IntPtr.Zero;
        IntPtr cipherClass = IntPtr.Zero;
        IntPtr gcmSpecClass = IntPtr.Zero;
        IntPtr stringClass = IntPtr.Zero;
        IntPtr storeName = IntPtr.Zero;
        IntPtr alias = IntPtr.Zero;
        IntPtr aesName = IntPtr.Zero;
        IntPtr transformName = IntPtr.Zero;
        IntPtr gcmName = IntPtr.Zero;
        IntPtr paddingName = IntPtr.Zero;
        IntPtr keyStore = IntPtr.Zero;
        IntPtr keyGenerator = IntPtr.Zero;
        IntPtr builder = IntPtr.Zero;
        IntPtr spec = IntPtr.Zero;
        IntPtr secretKey = IntPtr.Zero;
        IntPtr cipher = IntPtr.Zero;
        IntPtr gcmSpec = IntPtr.Zero;
        IntPtr ivJava = IntPtr.Zero;
        IntPtr inputJava = IntPtr.Zero;
        IntPtr outputJava = IntPtr.Zero;
        IntPtr modes = IntPtr.Zero;
        IntPtr paddings = IntPtr.Zero;

        try
        {
            keyStoreClass = AndroidJNI.FindClass("java/security/KeyStore");
            ThrowIfJava("KeyStore");
            storeName = AndroidJNI.NewStringUTF(StoreName);
            alias = AndroidJNI.NewStringUTF(Alias);
            keyStore = CallStaticObject(keyStoreClass, "getInstance", "(Ljava/lang/String;)Ljava/security/KeyStore;", storeName);
            CallVoid(keyStore, "load", "(Ljava/security/KeyStore$LoadStoreParameter;)V", IntPtr.Zero);

            bool exists = CallBoolean(keyStore, "containsAlias", "(Ljava/lang/String;)Z", alias);
            if (!exists)
            {
                keyGeneratorClass = AndroidJNI.FindClass("javax/crypto/KeyGenerator");
                builderClass = AndroidJNI.FindClass("android/security/keystore/KeyGenParameterSpec$Builder");
                aesName = AndroidJNI.NewStringUTF("AES");
                keyGenerator = CallStaticObject(
                    keyGeneratorClass,
                    "getInstance",
                    "(Ljava/lang/String;Ljava/lang/String;)Ljavax/crypto/KeyGenerator;",
                    aesName,
                    storeName);

                IntPtr builderCtor = AndroidJNI.GetMethodID(builderClass, "<init>", "(Ljava/lang/String;I)V");
                builder = AndroidJNI.NewObject(builderClass, builderCtor, new[]
                {
                    new jvalue { l = alias },
                    new jvalue { i = PurposeEncryptDecrypt }
                });
                ThrowIfJava("KeyGenParameterSpec.Builder");

                stringClass = AndroidJNI.FindClass("java/lang/String");
                gcmName = AndroidJNI.NewStringUTF("GCM");
                paddingName = AndroidJNI.NewStringUTF("NoPadding");
                modes = AndroidJNI.NewObjectArray(1, stringClass, IntPtr.Zero);
                paddings = AndroidJNI.NewObjectArray(1, stringClass, IntPtr.Zero);
                AndroidJNI.SetObjectArrayElement(modes, 0, gcmName);
                AndroidJNI.SetObjectArrayElement(paddings, 0, paddingName);

                CallObject(builder, "setBlockModes", "([Ljava/lang/String;)Landroid/security/keystore/KeyGenParameterSpec$Builder;", modes);
                CallObject(builder, "setEncryptionPaddings", "([Ljava/lang/String;)Landroid/security/keystore/KeyGenParameterSpec$Builder;", paddings);
                spec = CallObject(builder, "build", "()Landroid/security/keystore/KeyGenParameterSpec;");
                CallVoid(keyGenerator, "init", "(Ljava/security/spec/AlgorithmParameterSpec;)V", spec);
                CallObject(keyGenerator, "generateKey", "()Ljavax/crypto/SecretKey;");
            }

            secretKey = CallObject(keyStore, "getKey", "(Ljava/lang/String;[C)Ljava/security/Key;", alias, IntPtr.Zero);
            cipherClass = AndroidJNI.FindClass("javax/crypto/Cipher");
            transformName = AndroidJNI.NewStringUTF(AesTransform);
            cipher = CallStaticObject(cipherClass, "getInstance", "(Ljava/lang/String;)Ljavax/crypto/Cipher;", transformName);

            if (encrypt)
            {
                CallVoidIntObject(cipher, "init", "(ILjava/security/Key;)V", 1, secretKey);
                ivJava = CallObject(cipher, "getIV", "()[B");
                byte[] iv = AndroidJNI.FromByteArray(ivJava);
                inputJava = AndroidJNI.ToByteArray(data);
                outputJava = CallObject(cipher, "doFinal", "([B)[B", inputJava);
                byte[] cipherText = AndroidJNI.FromByteArray(outputJava);
                if (iv == null || cipherText == null || iv.Length == 0 || iv.Length > 255)
                    return null;
                byte[] packed = new byte[1 + iv.Length + cipherText.Length];
                packed[0] = (byte)iv.Length;
                Buffer.BlockCopy(iv, 0, packed, 1, iv.Length);
                Buffer.BlockCopy(cipherText, 0, packed, 1 + iv.Length, cipherText.Length);
                return packed;
            }

            int ivLength = data[0];
            if (ivLength < 8 || 1 + ivLength >= data.Length)
                return null;
            byte[] ivBytes = new byte[ivLength];
            byte[] cipherBytes = new byte[data.Length - 1 - ivLength];
            Buffer.BlockCopy(data, 1, ivBytes, 0, ivLength);
            Buffer.BlockCopy(data, 1 + ivLength, cipherBytes, 0, cipherBytes.Length);

            gcmSpecClass = AndroidJNI.FindClass("javax/crypto/spec/GCMParameterSpec");
            IntPtr gcmCtor = AndroidJNI.GetMethodID(gcmSpecClass, "<init>", "(I[B)V");
            ivJava = AndroidJNI.ToByteArray(ivBytes);
            gcmSpec = AndroidJNI.NewObject(gcmSpecClass, gcmCtor, new[]
            {
                new jvalue { i = GcmTagBits },
                new jvalue { l = ivJava }
            });
            ThrowIfJava("GCMParameterSpec");
            CallVoidIntObjectObject(cipher, "init", "(ILjava/security/Key;Ljava/security/spec/AlgorithmParameterSpec;)V", 2, secretKey, gcmSpec);
            inputJava = AndroidJNI.ToByteArray(cipherBytes);
            outputJava = CallObject(cipher, "doFinal", "([B)[B", inputJava);
            return AndroidJNI.FromByteArray(outputJava);
        }
        finally
        {
            Delete(outputJava);
            Delete(inputJava);
            Delete(ivJava);
            Delete(gcmSpec);
            Delete(cipher);
            Delete(secretKey);
            Delete(spec);
            Delete(builder);
            Delete(keyGenerator);
            Delete(keyStore);
            Delete(modes);
            Delete(paddings);
            Delete(paddingName);
            Delete(gcmName);
            Delete(transformName);
            Delete(aesName);
            Delete(alias);
            Delete(storeName);
            Delete(stringClass);
            Delete(gcmSpecClass);
            Delete(cipherClass);
            Delete(builderClass);
            Delete(keyGeneratorClass);
            Delete(keyStoreClass);
        }
    }

    private static IntPtr CallStaticObject(IntPtr clazz, string name, string signature, params IntPtr[] args)
    {
        IntPtr id = AndroidJNI.GetStaticMethodID(clazz, name, signature);
        ThrowIfJava(name);
        IntPtr result = AndroidJNI.CallStaticObjectMethod(clazz, id, ToJValues(args));
        ThrowIfJava(name);
        return result;
    }

    private static IntPtr CallObject(IntPtr obj, string name, string signature, params IntPtr[] args)
    {
        IntPtr clazz = AndroidJNI.GetObjectClass(obj);
        try
        {
            IntPtr id = AndroidJNI.GetMethodID(clazz, name, signature);
            ThrowIfJava(name);
            IntPtr result = AndroidJNI.CallObjectMethod(obj, id, ToJValues(args));
            ThrowIfJava(name);
            return result;
        }
        finally
        {
            Delete(clazz);
        }
    }

    private static void CallVoid(IntPtr obj, string name, string signature, params IntPtr[] args)
    {
        IntPtr clazz = AndroidJNI.GetObjectClass(obj);
        try
        {
            IntPtr id = AndroidJNI.GetMethodID(clazz, name, signature);
            ThrowIfJava(name);
            AndroidJNI.CallVoidMethod(obj, id, ToJValues(args));
            ThrowIfJava(name);
        }
        finally
        {
            Delete(clazz);
        }
    }

    private static void CallVoidIntObject(IntPtr obj, string name, string signature, int first, IntPtr second)
    {
        IntPtr clazz = AndroidJNI.GetObjectClass(obj);
        try
        {
            IntPtr id = AndroidJNI.GetMethodID(clazz, name, signature);
            ThrowIfJava(name);
            AndroidJNI.CallVoidMethod(obj, id, new[] { new jvalue { i = first }, new jvalue { l = second } });
            ThrowIfJava(name);
        }
        finally
        {
            Delete(clazz);
        }
    }

    private static void CallVoidIntObjectObject(IntPtr obj, string name, string signature, int first, IntPtr second, IntPtr third)
    {
        IntPtr clazz = AndroidJNI.GetObjectClass(obj);
        try
        {
            IntPtr id = AndroidJNI.GetMethodID(clazz, name, signature);
            ThrowIfJava(name);
            AndroidJNI.CallVoidMethod(obj, id, new[]
            {
                new jvalue { i = first },
                new jvalue { l = second },
                new jvalue { l = third }
            });
            ThrowIfJava(name);
        }
        finally
        {
            Delete(clazz);
        }
    }

    private static bool CallBoolean(IntPtr obj, string name, string signature, params IntPtr[] args)
    {
        IntPtr clazz = AndroidJNI.GetObjectClass(obj);
        try
        {
            IntPtr id = AndroidJNI.GetMethodID(clazz, name, signature);
            ThrowIfJava(name);
            bool result = AndroidJNI.CallBooleanMethod(obj, id, ToJValues(args));
            ThrowIfJava(name);
            return result;
        }
        finally
        {
            Delete(clazz);
        }
    }

    private static jvalue[] ToJValues(IntPtr[] args)
    {
        if (args == null || args.Length == 0)
            return Array.Empty<jvalue>();
        var values = new jvalue[args.Length];
        for (int i = 0; i < args.Length; i++)
            values[i] = new jvalue { l = args[i] };
        return values;
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
