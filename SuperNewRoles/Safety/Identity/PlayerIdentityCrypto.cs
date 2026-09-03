using System;
using System.Security.Cryptography;
using System.Text;

namespace SuperNewRoles.Safety.Identity;

/// <summary>
/// ECDSA P-256 の身元証明。内部IDはサーバー専用で、ここからは返さない。
/// </summary>
public static class PlayerIdentityCrypto
{
    public const string ProtocolName = "SNR-IDENTITY-v1";
    public const int UncompressedKeyLength = 65;
    public const int SignatureLength = 64;
    public const int MaxTimestampSkewSeconds = 300;

    public static ECDsa CreateKey()
    {
        return ECDsa.Create(ECCurve.NamedCurves.nistP256);
    }

    public static byte[] ExportUncompressedPublicKey(ECDsa key)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        ECParameters parameters = key.ExportParameters(false);
        byte[] x = PadTo32(parameters.Q.X);
        byte[] y = PadTo32(parameters.Q.Y);
        byte[] result = new byte[UncompressedKeyLength];
        result[0] = 0x04;
        Buffer.BlockCopy(x, 0, result, 1, 32);
        Buffer.BlockCopy(y, 0, result, 33, 32);
        return result;
    }

    public static byte[] ExportPkcs8(ECDsa key)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        return key.ExportPkcs8PrivateKey();
    }

    public static ECDsa ImportPkcs8(byte[] pkcs8)
    {
        if (pkcs8 == null || pkcs8.Length == 0) throw new ArgumentException("PKCS8 is empty", nameof(pkcs8));
        ECDsa key = CreateKey();
        key.ImportPkcs8PrivateKey(pkcs8, out _);
        return key;
    }

    public static ECDsa ImportUncompressedPublicKey(byte[] uncompressed)
    {
        if (!IsValidUncompressedPublicKey(uncompressed))
            throw new ArgumentException("Invalid uncompressed public key", nameof(uncompressed));

        var parameters = new ECParameters
        {
            Curve = ECCurve.NamedCurves.nistP256,
            Q = new ECPoint
            {
                X = Slice(uncompressed, 1, 32),
                Y = Slice(uncompressed, 33, 32)
            }
        };
        ECDsa key = CreateKey();
        key.ImportParameters(parameters);
        return key;
    }

    public static bool IsValidUncompressedPublicKey(byte[] uncompressed)
    {
        return uncompressed != null
            && uncompressed.Length == UncompressedKeyLength
            && uncompressed[0] == 0x04;
    }

    public static string ComputeInternalIdHex(byte[] uncompressedPublicKey)
    {
        if (!IsValidUncompressedPublicKey(uncompressedPublicKey))
            throw new ArgumentException("Invalid uncompressed public key", nameof(uncompressedPublicKey));
        return Convert.ToHexString(SHA256.HashData(uncompressedPublicKey)).ToLowerInvariant();
    }

    public static byte[] BuildSignedPayload(string action, long timestampUnix, string nonce, string bodySha256Hex)
    {
        if (string.IsNullOrWhiteSpace(action)) throw new ArgumentException("action required", nameof(action));
        if (string.IsNullOrWhiteSpace(nonce)) throw new ArgumentException("nonce required", nameof(nonce));
        bodySha256Hex ??= string.Empty;
        string text = $"{ProtocolName}\n{action}\n{timestampUnix}\n{nonce}\n{bodySha256Hex}";
        return Encoding.UTF8.GetBytes(text);
    }

    public static string HashBody(byte[] body)
    {
        body ??= Array.Empty<byte>();
        return Convert.ToHexString(SHA256.HashData(body)).ToLowerInvariant();
    }

    public static byte[] Sign(ECDsa key, string action, long timestampUnix, string nonce, byte[] body)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        byte[] payload = BuildSignedPayload(action, timestampUnix, nonce, HashBody(body));
        return key.SignData(payload, HashAlgorithmName.SHA256, DSASignatureFormat.IeeeP1363FixedFieldConcatenation);
    }

    public static bool Verify(byte[] uncompressedPublicKey, string action, long timestampUnix, string nonce, byte[] body, byte[] signature, DateTimeOffset? now = null)
    {
        if (!IsValidUncompressedPublicKey(uncompressedPublicKey)) return false;
        if (signature == null || signature.Length != SignatureLength) return false;
        if (string.IsNullOrWhiteSpace(action) || string.IsNullOrWhiteSpace(nonce)) return false;

        DateTimeOffset clock = now ?? DateTimeOffset.UtcNow;
        long nowUnix = clock.ToUnixTimeSeconds();
        if (Math.Abs(nowUnix - timestampUnix) > MaxTimestampSkewSeconds) return false;

        try
        {
            using ECDsa key = ImportUncompressedPublicKey(uncompressedPublicKey);
            byte[] payload = BuildSignedPayload(action, timestampUnix, nonce, HashBody(body));
            return key.VerifyData(payload, signature, HashAlgorithmName.SHA256, DSASignatureFormat.IeeeP1363FixedFieldConcatenation);
        }
        catch (CryptographicException)
        {
            return false;
        }
    }

    public static string NewNonce()
    {
        return Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLowerInvariant();
    }

    private static byte[] PadTo32(byte[] value)
    {
        if (value == null) throw new ArgumentException("curve coordinate missing");
        if (value.Length == 32) return value;
        if (value.Length > 32)
        {
            byte[] trimmed = new byte[32];
            Buffer.BlockCopy(value, value.Length - 32, trimmed, 0, 32);
            return trimmed;
        }

        byte[] padded = new byte[32];
        Buffer.BlockCopy(value, 0, padded, 32 - value.Length, value.Length);
        return padded;
    }

    private static byte[] Slice(byte[] source, int offset, int length)
    {
        byte[] result = new byte[length];
        Buffer.BlockCopy(source, offset, result, 0, length);
        return result;
    }
}
