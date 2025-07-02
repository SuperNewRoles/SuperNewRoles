using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace SuperNewRoles.Modules;

public static class LogCompression
{
    // 固定キー（実際の実装では、より安全なキー管理を検討してください）
    private static readonly byte[] EncryptionKey = GetFixedKey();

    private static byte[] GetFixedKey()
    {
        // AESキーは16, 24, または32バイトである必要があります（AES-128, AES-192, AES-256）
        byte[] key = new byte[32]; // AES-256用の32バイト
        byte[] sourceKey = Encoding.UTF8.GetBytes("SNRLogKey2024!@#");
        Array.Copy(sourceKey, key, Math.Min(sourceKey.Length, key.Length));
        return key;
    }

    /// <summary>
    /// ログをBrotliで圧縮し、AESで暗号化します
    /// </summary>
    /// <param name="logText">圧縮・暗号化するログテキスト</param>
    /// <returns>Base64エンコードされた圧縮・暗号化データ</returns>
    public static string CompressAndEncryptLog(string logText)
    {
        if (string.IsNullOrEmpty(logText))
        {
            Logger.Info("LogCompression: Input log text is null or empty");
            return string.Empty;
        }

        try
        {
            Logger.Info($"LogCompression: Starting compression and encryption. Original log size: {logText.Length} characters");
            Logger.Info($"LogCompression: Encryption key size: {EncryptionKey.Length} bytes");

            // 1. テキストをUTF-8バイト配列に変換
            byte[] textBytes = Encoding.UTF8.GetBytes(logText);
            Logger.Info($"LogCompression: UTF-8 bytes size: {textBytes.Length}");

            // 2. Brotli圧縮
            byte[] compressedBytes = CompressBytes(textBytes);
            Logger.Info($"LogCompression: Brotli compressed size: {compressedBytes.Length} bytes (compression ratio: {(float)compressedBytes.Length / textBytes.Length:P1})");

            // 3. AES暗号化
            byte[] encryptedBytes = EncryptBytes(compressedBytes);
            Logger.Info($"LogCompression: Encrypted size: {encryptedBytes.Length} bytes");

            // 4. Base64エンコード
            string result = Convert.ToBase64String(encryptedBytes);
            Logger.Info($"LogCompression: Base64 encoded size: {result.Length} characters");

            return result;
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to compress and encrypt log: {ex.Message}");
            Logger.Error($"Stack trace: {ex.StackTrace}");
            return string.Empty;
        }
    }

    /// <summary>
    /// 暗号化・圧縮されたログを復号・展開します（デバッグ用）
    /// </summary>
    /// <param name="encryptedCompressedData">Base64エンコードされた暗号化・圧縮データ</param>
    /// <returns>元のログテキスト</returns>
    public static string DecryptAndDecompressLog(string encryptedCompressedData)
    {
        if (string.IsNullOrEmpty(encryptedCompressedData))
            return string.Empty;

        try
        {
            // 1. Base64デコード
            byte[] encryptedBytes = Convert.FromBase64String(encryptedCompressedData);

            // 2. AES復号
            byte[] compressedBytes = DecryptBytes(encryptedBytes);

            // 3. Brotli展開
            byte[] textBytes = DecompressBytes(compressedBytes);

            // 4. UTF-8テキストに変換
            return Encoding.UTF8.GetString(textBytes);
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to decrypt and decompress log: {ex.Message}");
            return string.Empty;
        }
    }

    private static byte[] CompressBytes(byte[] data)
    {
        using (var memoryStream = new MemoryStream())
        {
            using (var brotliStream = new BrotliStream(memoryStream, CompressionMode.Compress))
            {
                brotliStream.Write(data, 0, data.Length);
            }
            return memoryStream.ToArray();
        }
    }

    private static byte[] DecompressBytes(byte[] compressedData)
    {
        using (var compressedStream = new MemoryStream(compressedData))
        using (var brotliStream = new BrotliStream(compressedStream, CompressionMode.Decompress))
        using (var resultStream = new MemoryStream())
        {
            brotliStream.CopyTo(resultStream);
            return resultStream.ToArray();
        }
    }

    private static byte[] EncryptBytes(byte[] data)
    {
        using (var aes = Aes.Create())
        {
            aes.Key = EncryptionKey;
            // IVは自動生成される
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            byte[] iv = aes.IV;

            using (var encryptor = aes.CreateEncryptor())
            using (var memoryStream = new MemoryStream())
            {
                // ストリームの先頭にIVを書き込む
                memoryStream.Write(iv, 0, iv.Length);

                using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(data, 0, data.Length);
                    cryptoStream.FlushFinalBlock();
                }
                return memoryStream.ToArray();
            }
        }
    }

    private static byte[] DecryptBytes(byte[] encryptedData)
    {
        using (var aes = Aes.Create())
        {
            aes.Key = EncryptionKey;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            // データからIVを読み取る (先頭16バイト)
            byte[] iv = new byte[16];
            Array.Copy(encryptedData, 0, iv, 0, iv.Length);
            aes.IV = iv;

            using (var decryptor = aes.CreateDecryptor())
            using (var memoryStream = new MemoryStream(encryptedData, iv.Length, encryptedData.Length - iv.Length))
            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
            using (var resultStream = new MemoryStream())
            {
                cryptoStream.CopyTo(resultStream);
                return resultStream.ToArray();
            }
        }
    }
}