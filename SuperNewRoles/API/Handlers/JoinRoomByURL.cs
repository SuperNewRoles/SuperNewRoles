using System.Net;
using System.Security.Cryptography; // 追加: AES用ライブラリ
using System.Text;                   // 追加: 文字列変換用
using System.IO;
using System;
using System.Linq;
using System.Threading;                     // 追加: 入出力操作用
using SuperNewRoles.Modules;
using HarmonyLib;
using InnerNet;
namespace SuperNewRoles.API.Handlers;

public enum ServerType
{
    Asia,
    NorthAmerica,
    Europe,
    SNRTokyo,
    Custom,
}

public class JoinRoomByURL : ServerHandlerBase
{
    private static readonly string AesKey = "SNR_JOINROOM_ENCRYPT"; // AES-128のキー (サンプル)
    private static readonly string AesIV = "12345";  // 初期化ベクトル (サンプル)

    public override string Path => "/joinGame";
    private static bool Connecting = false;
    private static HttpListenerContext ConnectingContext = null;

    public override void Handle(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;

        // DebugModeフラグ。trueの場合、クエリパラメータの復号化を行わず、そのまま使用します。
        bool DebugMode = true; // 必要に応じてtrueに切り替えてください

        // クエリパラメータを取得するローカル関数
        string DecryptQueryParameter(string key)
        {
            string value = request.QueryString[key];
            if (DebugMode)
            {
                // Debugモードでは暗号化解除せず生の値を返す
                return value ?? string.Empty;
            }
            return string.IsNullOrEmpty(value) ? string.Empty : AESDecrypt(value, AesKey, AesIV);
        }

        // 各パラメータの取得（DebugModeにより暗号化解除の有無を切り替え）
        string serverIP = DecryptQueryParameter("serverIP");
        string serverPort = DecryptQueryParameter("serverPort");
        if (!int.TryParse(DecryptQueryParameter("serverType"), out int serverType))
        {
            ApiServerManager.ReturnBadRequest(response, "serverType is not a number");
            Logger.Error("serverType is not a number");
            return;
        }

        // gameIDの取得と数値変換
        string gameIDStr = DecryptQueryParameter("gameID");
        if (string.IsNullOrEmpty(gameIDStr) || !int.TryParse(gameIDStr, out int gameID))
        {
            ApiServerManager.ReturnBadRequest(response, "gameID is not a number");
            Logger.Error("gameID is not a number");
            return;
        }

        // 必須パラメータのチェック
        if (string.IsNullOrEmpty(serverIP) || string.IsNullOrEmpty(serverPort))
        {
            ApiServerManager.ReturnBadRequest(response, "serverIP or serverPort is empty");
            Logger.Error("serverIP or serverPort is empty");
            return;
        }

        if (Connecting)
        {
            ApiServerManager.ReturnBadRequest(response, "接続中です。時間を空けてもう一度お試しください。");
            Logger.Error("Already connecting");
            return;
        }
        // メインスレッドに戻してルーム参加処理を継続
        new LateTask(() =>
        {
            if (AmongUsClient.Instance?.GameState != InnerNetClient.GameStates.NotJoined)
            {
                ApiServerManager.ReturnBadRequest(response, "すでにゲームに参加しています。切断してからもう一度お試しください。");
                Logger.Error("Already joined");
                return;
            }
            if (serverType == (int)ServerType.Asia)
            {
                FastDestroyableSingleton<ServerManager>.Instance.SetRegion(
                    FastDestroyableSingleton<ServerManager>.Instance.AvailableRegions
                        .FirstOrDefault(x => x.TranslateName == StringNames.ServerAS));
            }
            else if (serverType == (int)ServerType.NorthAmerica)
            {
                FastDestroyableSingleton<ServerManager>.Instance.SetRegion(
                    FastDestroyableSingleton<ServerManager>.Instance.AvailableRegions
                        .FirstOrDefault(x => x.TranslateName == StringNames.ServerNA));
            }
            else if (serverType == (int)ServerType.Europe)
            {
                FastDestroyableSingleton<ServerManager>.Instance.SetRegion(
                    FastDestroyableSingleton<ServerManager>.Instance.AvailableRegions
                        .FirstOrDefault(x => x.TranslateName == StringNames.ServerEU));
            }
            else if (serverType == (int)ServerType.SNRTokyo)
            {
                FastDestroyableSingleton<ServerManager>.Instance.SetRegion(GetRegion("cs.supernewroles.com", "443"));
            }
            else if (serverType == (int)ServerType.Custom)
            {
                string matchmakerIP = DecryptQueryParameter("matchmakerIP");
                string matchmakerPort = DecryptQueryParameter("matchmakerPort");
                FastDestroyableSingleton<ServerManager>.Instance.SetRegion(GetRegion(matchmakerIP, matchmakerPort));
            }
            // 以降、ルーム参加処理の実行
            Connecting = true;
            ConnectingContext = context;
            AmongUsClient.Instance.StartCoroutine(AmongUsClient.Instance.CoJoinOnlinePublicGame(
                gameID, serverIP, ushort.Parse(serverPort), AmongUsClient.MainMenuTarget.OnlineMenu));
            new LateTask(() =>
            {
                if (!Connecting) return;
                Connecting = false;
                ConnectingContext.Response.StatusCode = (int)HttpStatusCode.OK;
                ConnectingContext.Response.ContentType = "text/plain";
                using (var writer = new StreamWriter(ConnectingContext.Response.OutputStream, Encoding.UTF8))
                    writer.Write("接続しました。");
                ConnectingContext.Response.Close();
            }, 5f, "JoinRoomByURL");
        }, 0f, "JoinRoomByURL");
    }

    private static IRegionInfo GetRegion(string matchmakerIP, string matchmakerPort)
    {
        return new StaticHttpRegionInfo($"{matchmakerIP}:{matchmakerPort}", StringNames.NoTranslation,
                matchmakerIP, new(
                    [
                        new("http-1", $"http{(matchmakerPort == "443" ? "s" : "")}://{matchmakerIP}",
                        ushort.Parse(matchmakerPort), false)
                    ]
                )
            ).TryCast<IRegionInfo>();
    }
    private static string AESDecrypt(string cipherText, string key, string iv)
    {
        byte[] cipherBytes = Convert.FromBase64String(cipherText);
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Encoding.UTF8.GetBytes(key);
            aesAlg.IV = Encoding.UTF8.GetBytes(iv);
            aesAlg.Padding = PaddingMode.PKCS7;

            using (MemoryStream msDecrypt = new(cipherBytes))
            {
                using (CryptoStream csDecrypt = new(msDecrypt, aesAlg.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
        }
    }
    [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.HandleDisconnect))]
    public static class InnerNetClient_HandleDisconnect
    {
        public static void Postfix(InnerNetClient __instance, DisconnectReasons reason, string stringReason)
        {
            Logger.Error($"エラーが発生しました:{reason} {stringReason}");
            if (Connecting)
            {
                Logger.Error("エラーが発生しました");
                Connecting = false;
                string errorMessage = $"エラーが発生しました:{reason} {stringReason}";
                switch (reason)
                {
                    case DisconnectReasons.GameNotFound:
                        errorMessage = "部屋が見つかりませんでした。";
                        break;
                    case DisconnectReasons.GameFull:
                        errorMessage = "部屋が満員です。";
                        break;
                    case DisconnectReasons.GameStarted:
                        errorMessage = "ゲームが開始されています。";
                        break;
                    case DisconnectReasons.ServerNotFound:
                        errorMessage = "サーバーが見つかりませんでした。";
                        break;
                    case DisconnectReasons.ServerFull:
                        errorMessage = "サーバーが満員です。";
                        break;
                    case DisconnectReasons.Banned:
                        errorMessage = "BANされています。";
                        break;
                    case DisconnectReasons.Kicked:
                        errorMessage = "KICKされています。";
                        break;
                    case DisconnectReasons.ClientTimeout:
                        errorMessage = "タイムアウトしました。";
                        break;
                    case DisconnectReasons.Custom:
                        errorMessage = $"エラーです: {stringReason}";
                        break;
                }
                ConnectingContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                ConnectingContext.Response.ContentType = "text/plain";
                using (var writer = new StreamWriter(ConnectingContext.Response.OutputStream, Encoding.UTF8))
                    writer.Write(errorMessage);
                ConnectingContext.Response.Close();
            }
        }
    }
}