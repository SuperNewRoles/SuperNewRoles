using System;
using System.IO;
using System.Security.Cryptography;

namespace SuperNewRoles.Safety.Identity;

public static class PlayerIdentityStore
{
    public const string FileName = "player-identity.p8";

    private static readonly object Gate = new();
    private static ECDsa _cached;

    public static string FilePath => Path.Combine(SuperNewRolesPlugin.SecretDirectory, FileName);

    public static ECDsa GetOrCreate()
    {
        lock (Gate)
        {
            if (_cached != null) return _cached;

            Directory.CreateDirectory(SuperNewRolesPlugin.SecretDirectory);
            if (File.Exists(FilePath))
            {
                byte[] bytes = File.ReadAllBytes(FilePath);
                _cached = PlayerIdentityCrypto.ImportPkcs8(bytes);
                return _cached;
            }

            ECDsa created = PlayerIdentityCrypto.CreateKey();
            File.WriteAllBytes(FilePath, PlayerIdentityCrypto.ExportPkcs8(created));
            _cached = created;
            return _cached;
        }
    }

    public static byte[] GetPublicKey()
    {
        return PlayerIdentityCrypto.ExportUncompressedPublicKey(GetOrCreate());
    }

    public static PlayerIdentityProof CreateProof(string action, byte[] body)
    {
        ECDsa key = GetOrCreate();
        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        string nonce = PlayerIdentityCrypto.NewNonce();
        byte[] signature = PlayerIdentityCrypto.Sign(key, action, timestamp, nonce, body ?? Array.Empty<byte>());
        return new PlayerIdentityProof(
            Convert.ToBase64String(GetPublicKey()),
            timestamp,
            nonce,
            Convert.ToBase64String(signature),
            action);
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
