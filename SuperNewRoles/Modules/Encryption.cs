using System;
using System.IO;
using System.Security.Cryptography;

namespace SuperNewRoles;

public static class Encryption
{
    private static byte[] Key;

    public static void SetEncryptKey(){
        Key = RandomByte(16);
    }

    //安全な乱数を生成する
    private static byte[] RandomByte(int length){
        byte[] randomBytes = new byte[length];
        for(int i = 0; i < length; i++){
            randomBytes[i] = (byte)RandomNumberGenerator.GetInt32(0,255);
        }
        return randomBytes;
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
        return $"<E>{Convert.ToBase64String(ms.ToArray())}</E>";
    }
}