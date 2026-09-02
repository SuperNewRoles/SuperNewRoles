using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Safety.Identity;

public static class PlayerIdentityStore
{
    public const string FileName = "player-identity.p8";
    private static readonly byte[] DpapiMagic = System.Text.Encoding.ASCII.GetBytes("SNRD1");
    private static readonly byte[] AndroidMagic = System.Text.Encoding.ASCII.GetBytes("SNRAK1");

    private static readonly object Gate = new();
    private static ECDsa _cached;

    public static string FilePath => Path.Combine(StorageDirectory, FileName);

    private static string StorageDirectory
    {
        get
        {
            if (ModHelpers.IsAndroid())
            {
                string external = AndroidIdentityBlobStore.TryExternalDirectory();
                if (!string.IsNullOrEmpty(external))
                    return external;
            }

            return SuperNewRolesPlugin.SecretDirectory;
        }
    }

    private static string LegacyAndroidFilePath => Path.Combine(SuperNewRolesPlugin.SecretDirectory, FileName);

    public static bool HasKey()
    {
        lock (Gate)
            return TryGetUnlocked() != null;
    }

    public static bool TryLoad()
    {
        lock (Gate)
            return TryGetUnlocked() != null;
    }

    public static ECDsa GetOrCreate()
    {
        lock (Gate)
        {
            ECDsa existing = TryGetUnlocked();
            if (existing != null) return existing;

            ECDsa created = PlayerIdentityCrypto.CreateKey();
            WriteStored(Protect(PlayerIdentityCrypto.ExportPkcs8(created)));
            _cached = created;
            return _cached;
        }
    }

    public static byte[] GetPublicKey()
    {
        ECDsa key;
        lock (Gate)
            key = TryGetUnlocked();
        return key == null ? null : PlayerIdentityCrypto.ExportUncompressedPublicKey(key);
    }

    public static PlayerIdentityProof CreateProof(string action, byte[] body)
    {
        ECDsa key;
        lock (Gate)
            key = TryGetUnlocked();
        if (key == null) return null;
        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        string nonce = PlayerIdentityCrypto.NewNonce();
        byte[] signature = PlayerIdentityCrypto.Sign(key, action, timestamp, nonce, body ?? Array.Empty<byte>());
        return new PlayerIdentityProof(
            Convert.ToBase64String(PlayerIdentityCrypto.ExportUncompressedPublicKey(key)),
            timestamp,
            nonce,
            Convert.ToBase64String(signature),
            action);
    }

    private static ECDsa TryGetUnlocked()
    {
        if (_cached != null) return _cached;

        try
        {
            byte[] stored = ReadStored();
            if (stored == null) return null;
            byte[] unlocked = Unprotect(stored);
            if (unlocked == null) return null;
            _cached = PlayerIdentityCrypto.ImportPkcs8(unlocked);
            WriteStored(Protect(PlayerIdentityCrypto.ExportPkcs8(_cached)));
            return _cached;
        }
        catch (Exception ex)
        {
            Logger.Warning($"Stored identity could not be restored: {ex.Message}");
            return null;
        }
    }

    private static byte[] ReadStored()
    {
        if (ModHelpers.IsAndroid())
        {
            // Keystore protect 失敗時は SecretDirectory のみに書くため、prefs/external の SNRAK1 より先に読む。
            if (File.Exists(LegacyAndroidFilePath))
            {
                byte[] fromSecret = TryReadUnlockable(LegacyAndroidFilePath);
                if (fromSecret != null)
                    return fromSecret;
            }

            byte[] fromPrefs = AndroidIdentityBlobStore.TryReadPrefs();
            if (IsUnlockable(fromPrefs))
                return fromPrefs;
        }

        if (File.Exists(FilePath))
        {
            byte[] fromFile = TryReadUnlockable(FilePath);
            if (fromFile != null)
                return fromFile;
        }

        return null;
    }

    private static byte[] TryReadUnlockable(string path)
    {
        try
        {
            byte[] stored = File.ReadAllBytes(path);
            return IsUnlockable(stored) ? stored : null;
        }
        catch (Exception ex)
        {
            Logger.Warning($"Failed to read identity file: {ex.Message}");
            return null;
        }
    }

    internal static bool IsUnlockable(byte[] stored)
    {
        if (stored == null || stored.Length == 0)
            return false;
        try
        {
            byte[] unlocked = Unprotect(stored);
            if (unlocked == null)
                return false;
            using ECDsa key = PlayerIdentityCrypto.ImportPkcs8(unlocked);
            return key != null;
        }
        catch
        {
            return false;
        }
    }

    internal static byte[] SelectUnlockableStored(params byte[][] sources)
    {
        if (sources == null)
            return null;
        foreach (byte[] stored in sources)
        {
            if (!IsUnlockable(stored))
                continue;
            return stored;
        }
        return null;
    }

    private static void WriteStored(byte[] blob)
    {
        bool androidUnprotected = ModHelpers.IsAndroid() && !HasMagic(blob, AndroidMagic);
        string path = androidUnprotected ? LegacyAndroidFilePath : FilePath;

        if (androidUnprotected)
            Logger.Warning("Android Keystore protect failed; writing identity only to SecretDirectory (not external storage or SharedPreferences)");
        else if (ModHelpers.IsAndroid())
            Logger.Info("Android Keystore protect succeeded; writing identity to external storage and SharedPreferences");

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? SuperNewRolesPlugin.SecretDirectory);
            File.WriteAllBytes(path, blob);
        }
        catch (Exception ex)
        {
            Logger.Warning($"Failed to write identity file: {ex.Message}");
        }

        if (ModHelpers.IsAndroid() && !androidUnprotected)
            AndroidIdentityBlobStore.TryWritePrefs(blob);
    }

    private static byte[] Protect(byte[] raw)
    {
        if (ModHelpers.IsAndroid())
        {
            byte[] androidWrapped = AndroidKeyProtector.TryProtect(raw);
            if (androidWrapped != null)
                return Prefix(AndroidMagic, androidWrapped);
            return raw;
        }

        byte[] protectedBytes = TryDpapi(raw, protect: true);
        if (protectedBytes == null || ReferenceEquals(protectedBytes, raw))
            return raw;
        return Prefix(DpapiMagic, protectedBytes);
    }

    internal static byte[] Unprotect(byte[] stored)
    {
        if (stored == null || stored.Length == 0)
            return null;
        if (HasMagic(stored, AndroidMagic))
        {
            byte[] payload = Strip(stored, AndroidMagic);
            return AndroidKeyProtector.TryUnprotect(payload);
        }
        if (HasMagic(stored, DpapiMagic))
        {
            byte[] payload = Strip(stored, DpapiMagic);
            return TryDpapi(payload, protect: false) ?? stored;
        }
        return stored;
    }

    private static bool HasMagic(byte[] stored, byte[] magic)
    {
        if (stored == null || stored.Length <= magic.Length)
            return false;
        for (int i = 0; i < magic.Length; i++)
        {
            if (stored[i] != magic[i])
                return false;
        }
        return true;
    }

    private static byte[] Prefix(byte[] magic, byte[] payload)
    {
        byte[] wrapped = new byte[magic.Length + payload.Length];
        Buffer.BlockCopy(magic, 0, wrapped, 0, magic.Length);
        Buffer.BlockCopy(payload, 0, wrapped, magic.Length, payload.Length);
        return wrapped;
    }

    private static byte[] Strip(byte[] stored, byte[] magic)
    {
        byte[] payload = new byte[stored.Length - magic.Length];
        Buffer.BlockCopy(stored, magic.Length, payload, 0, payload.Length);
        return payload;
    }

    private static byte[] TryDpapi(byte[] data, bool protect)
    {
        try
        {
            Type type = Type.GetType("System.Security.Cryptography.ProtectedData, System.Security.Cryptography.ProtectedData")
                ?? Type.GetType("System.Security.Cryptography.ProtectedData, System.Security");
            if (type == null) return data;

            Type scopeType = type.GetNestedType("DataProtectionScope")
                ?? Type.GetType("System.Security.Cryptography.DataProtectionScope, System.Security.Cryptography.ProtectedData")
                ?? Type.GetType("System.Security.Cryptography.DataProtectionScope, System.Security");
            if (scopeType == null) return data;

            MethodInfo method = type.GetMethod(protect ? "Protect" : "Unprotect", BindingFlags.Public | BindingFlags.Static);
            if (method == null) return data;
            object scope = Enum.Parse(scopeType, "CurrentUser");
            return method.Invoke(null, new object[] { data, null, scope }) as byte[] ?? data;
        }
        catch
        {
            return data;
        }
    }
}

public sealed class PlayerIdentityProof
{
    public PlayerIdentityProof(string publicKeyBase64, long timestampUnix, string nonce, string signatureBase64, string action)
    {
        PublicKeyBase64 = publicKeyBase64;
        TimestampUnix = timestampUnix;
        Nonce = nonce;
        SignatureBase64 = signatureBase64;
        Action = action;
    }

    public string PublicKeyBase64 { get; }
    public long TimestampUnix { get; }
    public string Nonce { get; }
    public string SignatureBase64 { get; }
    public string Action { get; }
}
