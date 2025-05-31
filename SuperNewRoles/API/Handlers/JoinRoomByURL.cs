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
using System.Web;
using UnityEngine;
using UnityEngine.Events; // HttpUtility.UrlEncode を使用するために追加
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
    private static readonly string AesKey = JoinRoomURLGenerator.AesKey; // AES-128のキー (サンプル)
    private static readonly string AesIV = JoinRoomURLGenerator.AesIV;  // 初期化ベクトル (サンプル)

    public override string Path => "/joinGame";
    private static bool Connecting = false;
    private static HttpListenerContext ConnectingContext = null;

    public override void Handle(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;

        // DebugModeフラグ。trueの場合、クエリパラメータの復号化を行わず、そのまま使用します。
        bool DebugMode = false; // 必要に応じてtrueに切り替えてください

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
        string prefix = "";
        if (!matchmakerIP.StartsWith("http"))
            prefix = $"http{(matchmakerPort == "443" ? "s" : "")}://";
        return new StaticHttpRegionInfo($"{matchmakerIP}:{matchmakerPort}", StringNames.NoTranslation,
                matchmakerIP, new(
                    [
                        new("http-1", $"{prefix}{matchmakerIP}",
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
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    public static class MainMenuManager_Start
    {
        public static void Postfix(MainMenuManager __instance)
        {
            var servermanager = FastDestroyableSingleton<ServerManager>.Instance;
            if (!servermanager.AvailableRegions.Contains(servermanager.CurrentRegion))
            {
                servermanager.SetRegion(CustomServer.SNRRegion);
            }
        }
    }
}

public class JoinRoomURLGenerator // または既存のクラスに追加
{
    // JoinRoomByURL.cs から AesKey と AesIV をコピーまたは参照
    public static readonly string AesKey = "ThisIsA16ByteKey"; // AES-128用に16バイトに変更
    public static readonly string AesIV = "ThisIsA16ByteIV!";  // AES用に16バイトに変更
    private static readonly string ApiPath = "/joinGame"; // JoinRoomByURL.cs の Path プロパティ

    // AES暗号化処理 (JoinRoomByURL.cs の AESDecrypt を参考に作成)
    private static string AESEncrypt(string plainText, string key, string iv)
    {
        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Encoding.UTF8.GetBytes(key);
            aesAlg.IV = Encoding.UTF8.GetBytes(iv);
            aesAlg.Padding = PaddingMode.PKCS7; // 暗号化時もPKCS7を使用

            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, aesAlg.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    csEncrypt.Write(plainBytes, 0, plainBytes.Length);
                    csEncrypt.FlushFinalBlock(); // Ensure all data is written
                }
                return Convert.ToBase64String(msEncrypt.ToArray());
            }
        }
    }

    public static string GenerateJoinRoomURL(
        string serverIP,
        string serverPort,
        ServerType serverType,
        int gameID,
        string matchmakerIP = null, // Customの場合のみ
        string matchmakerPort = null, // Customの場合のみ
        bool debugMode = false // デバッグモードフラグ
    )
    {
        // クエリパラメータを準備
        var queryParameters = new System.Collections.Specialized.NameValueCollection();

        // 暗号化またはそのままの値を追加するヘルパー関数
        void AddParameter(string paramKey, string value)
        {
            if (debugMode)
            {
                queryParameters[paramKey] = value;
            }
            else
            {
                queryParameters[paramKey] = AESEncrypt(value, AesKey, AesIV);
            }
        }

        AddParameter("serverIP", serverIP);
        AddParameter("serverPort", serverPort);
        AddParameter("serverType", ((int)serverType).ToString());
        AddParameter("gameID", gameID.ToString());

        if (serverType == ServerType.Custom)
        {
            if (string.IsNullOrEmpty(matchmakerIP) || string.IsNullOrEmpty(matchmakerPort))
            {
                throw new ArgumentException("Custom server type requires matchmakerIP and matchmakerPort.");
            }
            AddParameter("matchmakerIP", matchmakerIP);
            AddParameter("matchmakerPort", matchmakerPort);
        }

        // クエリ文字列を構築
        StringBuilder queryStringBuilder = new StringBuilder();
        foreach (string key in queryParameters)
        {
            if (queryStringBuilder.Length > 0)
            {
                queryStringBuilder.Append("&");
            }
            // HttpUtility.UrlEncode を使用してパラメータ値をエンコード
            queryStringBuilder.AppendFormat("{0}={1}", key, HttpUtility.UrlEncode(queryParameters[key]));
        }

        // 完全なURLを構築
        UriBuilder uriBuilder = new("https", SNRURLs.JoinRoomHost, 443, ApiPath)
        {
            Query = queryStringBuilder.ToString()
        };

        return uriBuilder.ToString();
    }

    // ServerType enum (JoinRoomByURL.cs と同じものを定義または参照)
    public enum ServerType
    {
        Asia,
        NorthAmerica,
        Europe,
        SNRTokyo,
        Custom,
    }
    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Start))]
    public static class GameStartManager_Start
    {
        private static ServerType GetRegionType(IRegionInfo region, ServerInfo server)
        {
            Logger.Info($"Server: {server.Ip} {server.Port}");
            switch (region.TranslateName)
            {
                case StringNames.ServerAS:
                    return ServerType.Asia;
                case StringNames.ServerNA:
                    return ServerType.NorthAmerica;
                case StringNames.ServerEU:
                    return ServerType.Europe;

            }
            if (server.Ip == "cs.supernewroles.com") return ServerType.SNRTokyo;
            return ServerType.Custom;
        }
        public static void Postfix(GameStartManager __instance)
        {
            if (AmongUsClient.Instance.NetworkMode != NetworkModes.OnlineGame) return;
            var obj = new GameObject("JoinRoomURLGenerator");
            obj.layer = 5;
            var selected = new GameObject("Selected");
            selected.transform.SetParent(obj.transform);
            selected.transform.localPosition = Vector3.zero;
            selected.transform.localScale = Vector3.one * 4.65f;
            SpriteRenderer selectedRenderer = selected.AddComponent<SpriteRenderer>();
            selectedRenderer.sprite = AssetManager.GetAsset<Sprite>("processed_white2.png");
            selectedRenderer.color = new Color(1, 1, 1, 0.6f);
            selected.layer = 5;
            SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
            renderer.sprite = AssetManager.GetAsset<Sprite>("SNRDirectCopy.png");
            obj.transform.SetParent(__instance.LobbyInfoPane.CopyCodeText.transform.parent);
            obj.transform.localPosition = new(1.965f, -0.248f, -2f);
            obj.transform.localScale = Vector3.one * 0.405f;
            PassiveButton button = obj.AddComponent<PassiveButton>();
            BoxCollider2D boxCollider = obj.AddComponent<BoxCollider2D>();
            button.Colliders = new Collider2D[] { boxCollider };
            button.OnClick = new();
            button.OnClick.AddListener((UnityAction)(() =>
            {
                var server = FastDestroyableSingleton<ServerManager>.Instance.CurrentRegion.Servers.FirstOrDefault();
                var url = GenerateJoinRoomURL(
                    AmongUsClient.Instance.networkAddress,
                    AmongUsClient.Instance.networkPort.ToString(),
                    GetRegionType(FastDestroyableSingleton<ServerManager>.Instance.CurrentRegion, server),
                    AmongUsClient.Instance.GameId,
                    server.Ip,
                    server.Port.ToString()
                );
                GUIUtility.systemCopyBuffer = url;
            }));
            button.ClickSound = __instance.LobbyInfoPane.CopyCodeSound;
            button.OnMouseOut = new();
            button.OnMouseOut.AddListener((UnityAction)(() =>
            {
                selected.SetActive(false);
            }));
            button.OnMouseOver = new();
            button.OnMouseOver.AddListener((UnityAction)(() =>
            {
                selected.SetActive(true);
            }));
        }
    }
}