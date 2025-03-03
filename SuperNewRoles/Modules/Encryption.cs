using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace SuperNewRoles;

public static class Encryption
{
    private static byte[] Key;
    private static readonly string rsaPublicKey = @"
    -----BEGIN PUBLIC KEY-----
    MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEApX20mtxp3Mvx6/Oz5ETG
    m1wwnCG6GwIySbZ4WXs8xmGLO96qjVEVBBOnx7xr0YH/KbfgmC34Fx3/9vpHTda/
    /Tx1rb+Zd7JdiULEJJv/BtH5tvPxwWKJVAWfgpx3YFr26zQOe9yYAjxtb/wVQsE6
    CmF52OrC7W1MJnmWE3SZQurEqrP0wf69akQApaf6ZDvOCstB1pnNvSAPBFzX9okE
    vrwI0FsEaQriIQfm891sKNr/iYqCiRNpNrI+Egdq1p9XBJn3u3HxcRj3mtpHaFMu
    7kwL8r128eshbiDsCLyBa+x4iXL2KPYRFGhAwFn67ahkaIXpMu3WpRiOc1YHkWgv
    HQIDAQAB
    -----END PUBLIC KEY-----";

    public static void SetEncryptKey(){
        Key = RandomByte(16);
        SuperNewRolesPlugin.Instance.Log.LogInfo(EncryptKey());
    }

    //安全な乱数を生成する
    private static byte[] RandomByte(int length){
        byte[] randomBytes = new byte[length];
        for(int i = 0; i < length; i++){
            randomBytes[i] = (byte)RandomNumberGenerator.GetInt32(0,255);
        }
        return randomBytes;
    }

    private static string EncryptKey(){
        using RSA rsa = RSA.Create();
        rsa.ImportFromPem(rsaPublicKey.ToCharArray());
        byte[] encryptedKey = rsa.Encrypt(Key,RSAEncryptionPadding.OaepSHA256);
        return $"$SNRKST${Convert.ToBase64String(encryptedKey)}$SNRKET$";
    }

    public static string Encrypt(string plainText){
        using Aes aesAlg = Aes.Create();
        aesAlg.Key = Key;
        aesAlg.IV = RandomByte(16);
        ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
        using MemoryStream ms = new();
        using CryptoStream cs = new(ms, encryptor, CryptoStreamMode.Write);
        byte[] plainBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        cs.Write(plainBytes, 0, plainBytes.Length);
        cs.FlushFinalBlock();
        return $"$SNRST${Convert.ToBase64String(aesAlg.IV)}{Convert.ToBase64String(ms.ToArray())}$SNRET$";
    }
}