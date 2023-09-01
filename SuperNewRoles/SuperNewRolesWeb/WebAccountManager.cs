using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;

namespace SuperNewRoles.SuperNewRolesWeb
{
    public static class WebAccountManager
    {
        public static string Token = "";
        public static bool IsLogined => Token != "";

        public static string MyPlayerName = "";
        public static string MyUserId = "";

        public static bool CanOtherPlayerSendData = false;

        static string tokenfilepath = "";
        public static void SetToken(string token, Action<bool> OnEnd = null)
        {
            Token = token;
            BinaryWriter writer = new(new FileStream(tokenfilepath, FileMode.Create));
            writer.Write(Encoding.Unicode.GetBytes(token));
            writer.Dispose();
            if (AmongUsClient.Instance != null)
            {
                CheckToken(OnEnd);
            }
        }
        public static void LogOut()
        {
            Token = "";
            BinaryWriter writer = new(new FileStream(tokenfilepath, FileMode.Create));
            writer.Write(Encoding.Unicode.GetBytes(""));
            writer.Dispose();
        }
        public static void CheckToken(Action<bool> OnEnd = null)
        {
            if (Token != "")
            {
                WebApi.CheckToken(Token, (code, handler) => {
                    if (code != 200)
                    {
                        Token = "";
                        if (OnEnd != null)
                        {
                            OnEnd(false);
                        }
                    }
                    else
                        WebApi.GetMyAccountByTokenFirst(Token, (codea, handler) =>
                        {
                            if (codea != 200)
                            {
                                if (OnEnd != null)
                                {
                                    OnEnd(false);
                                }
                                return;
                            }
                            JToken jobj = JObject.Parse(handler.text);
                            if (!jobj.HasValues) return;
                            MyPlayerName = jobj["name"]?.ToString();
                            MyUserId = jobj["userid"]?.ToString();
                            CanOtherPlayerSendData = jobj["CanOtherPlayerSendData"]?.ToString() == "a";
                            if (OnEnd != null)
                            {
                                OnEnd(true);
                            }
                        });
                    
                });
            }
        }
        [HarmonyPatch(typeof(AmongUsClient),nameof(AmongUsClient.Awake))]
        class AmongUsClientAwakePatch
        {
            public static void Postfix(AmongUsClient __instance)
            {
                CheckToken();
            }
        }
        public static void Load()
        {
            string supernewrolesfolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)+"\\SuperNewRoles";
            if (!Directory.Exists(supernewrolesfolder)){
                Directory.CreateDirectory(supernewrolesfolder);
            }
            string tokenfile = supernewrolesfolder + "\\WebToken.token";
            tokenfilepath = tokenfile;
            if (!File.Exists(tokenfile))
            {
                File.Create(tokenfile);
            }
            else
            {
                FileStream fs = new(
                  tokenfile, FileMode.Open, FileAccess.Read);

                int fileSize = (int)fs.Length; // ファイルのサイズ
                byte[] buf = new byte[fileSize]; // データ格納用配列

                int readSize; // Readメソッドで読み込んだバイト数
                int remain = fileSize; // 読み込むべき残りのバイト数
                int bufPos = 0; // データ格納用配列内の追加位置

                while (remain > 0)
                {
                    // 1024Bytesずつ読み込む
                    readSize = fs.Read(buf, bufPos, Math.Min(1024, remain));

                    bufPos += readSize;
                    remain -= readSize;
                }
                fs.Dispose();

                Token = Encoding.Unicode.GetString(buf);
            }
        }
    }
}
