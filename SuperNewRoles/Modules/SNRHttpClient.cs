// SNRHttpClient.cs
// 自前の超シンプル HTTP クライアント（Unity コルーチン用）
// 依存：System.Net.Sockets, System.Net.Security, System.Threading.Tasks
// 2025-05-16 (最終更新日)

using System;
using System.Collections;
using System.IO;
using System.IO.Compression; // GZipStream, DeflateStream のため
using System.Net; // WebHeaderCollection のため
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine; // MonoBehaviour, Coroutine のため

namespace SuperNewRoles.Modules
{
    public class SNRDownloadHandler
    {
        public byte[] data { get; internal set; }
        private string _textCache = null; // textプロパティのキャッシュ用
        public string text
        {
            get
            {
                if (_textCache == null && data != null)
                {
                    _textCache = Encoding.UTF8.GetString(data);
                }
                return _textCache;
            }
        }
    }

    // HTTPレスポンスの詳細を保持する内部クラス
    internal class SNRWebResponse
    {
        public byte[] Body { get; set; } = Array.Empty<byte>();
        public long StatusCode { get; set; } = 0;
        public string StatusDescription { get; set; } // 例: "OK", "Not Found"
        public string ErrorMessage { get; set; } // リクエスト処理中の内部エラー用
        public WebHeaderCollection Headers { get; set; } = new WebHeaderCollection();
    }

    public class SNRHttpClient
    {
        public string url { get; }
        public string method { get; private set; }

        public SNRDownloadHandler downloadHandler { get; }
        // public SNRUploadHandler uploadHandler { get; private set; } // 将来のPOST/PUTサポート用

        public bool isDone { get; private set; }
        public string error { get; private set; }
        public long responseCode => _response?.StatusCode ?? 0;
        public float timeout { get; set; } = 5f;
        public bool ignoreSslErrors { get; set; } = false;

        private SNRWebResponse _response;

        public SNRHttpClient(string url) : this(url, "GET") { }

        public SNRHttpClient(string url, string method)
        {
            this.url = url;
            this.method = method.ToUpperInvariant();
            this.downloadHandler = new SNRDownloadHandler();
            this._response = new SNRWebResponse(); // SendWebRequest前に初期化
        }

        public static SNRHttpClient Get(string url)
        {
            return new SNRHttpClient(url, "GET");
        }

        // public static SNRHttpClient Post(string url, string postData) { /* ... */ } // 将来の拡張用

        public IEnumerator SendWebRequest()
        {
            isDone = false;
            error = null;
            // _response はコンストラクタまたはここでリセットしてもよい
            // 今回はコンストラクタで初期化、SendWebRequestが複数回呼ばれることを想定しないシンプルな形

            if (method != "GET") // 現在はGETのみサポート
            {
                error = $"HTTP method '{method}' is not supported.";
                isDone = true;
                yield break;
            }

            var task = ProcessRequestAsync(this.url);
            while (!task.IsCompleted)
            {
                yield return null; // 1フレーム待つ
            }

            isDone = true; // タスク完了後、成否に関わらず完了状態とする

            if (task.Exception != null)
            {
                var aggEx = task.Exception;
                var innermostEx = aggEx.InnerException ?? aggEx;
                while (innermostEx.InnerException != null)
                {
                    innermostEx = innermostEx.InnerException;
                }
                error = $"Connection Error: {innermostEx.Message}";
                // _response.StatusCode は0のまま (接続エラーを示す)
            }
            else
            {
                _response = task.Result; // 正常完了時は結果を格納
                downloadHandler.data = _response.Body;

                if (!string.IsNullOrEmpty(_response.ErrorMessage))
                {
                    // ヘッダーパース失敗などの内部処理エラー
                    error = _response.ErrorMessage;
                }
                else if (_response.StatusCode == 0 && string.IsNullOrEmpty(error))
                {
                    // 例外も内部エラーメッセージも無いが、ステータスコードが0の場合 (通常は発生しないはず)
                    error = "Unknown error: Status code 0 without explicit error message.";
                }
                else if (_response.StatusCode >= 400)
                {
                    // HTTPエラー (4xx, 5xx)
                    error = $"HTTP Error {_response.StatusCode} ({_response.StatusDescription ?? "Status Unknown"})";
                    // エラーレスポンスのボディが短いテキストの場合、エラーメッセージに含める
                    if (downloadHandler.text != null && downloadHandler.text.Length > 0 && downloadHandler.text.Length < 512)
                    {
                        error += $": {downloadHandler.text}";
                    }
                }
                // 成功時 (2xx) は error は null のまま
            }
        }

        private async Task<SNRWebResponse> ProcessRequestAsync(string requestUrl)
        {
            var currentResponse = new SNRWebResponse();
            try
            {
                var uri = new Uri(requestUrl);
                // HTTPS でない場合、ポートが指定されていなければ 80 を使う
                int port = uri.Port != -1 ? uri.Port : (uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase) ? 443 : 80);

                using (var tcpClient = new TcpClient())
                {
                    // タイムアウト設定 (接続、SSL認証に適用)
                    var connectionTimeout = TimeSpan.FromSeconds(timeout); // プロパティを使用

                    // 接続タイムアウト
                    var connectTask = tcpClient.ConnectAsync(uri.Host, port);
                    if (await Task.WhenAny(connectTask, Task.Delay(connectionTimeout)) != connectTask)
                    {
                        throw new TimeoutException($"Connection to {uri.Host}:{port} timed out after {timeout} seconds.");
                    }
                    // connectTask.GetAwaiter().GetResult(); // Ensure any exception from ConnectAsync is thrown

                    using (Stream baseStream = tcpClient.GetStream())
                    {
                        Stream streamToUse = baseStream;
                        if (uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
                        {
                            var sslStream = new SslStream(baseStream, false, (sender, certificate, chain, sslPolicyErrors) => ignoreSslErrors);
                            // SSL認証タイムアウト
                            var authTask = sslStream.AuthenticateAsClientAsync(uri.Host);
                            if (await Task.WhenAny(authTask, Task.Delay(connectionTimeout)) != authTask)
                            {
                                throw new TimeoutException($"SSL authentication for {uri.Host} timed out after {timeout} seconds.");
                            }
                            // authTask.GetAwaiter().GetResult();
                            streamToUse = sslStream;
                        }

                        // streamToUse の読み書きにタイムアウト設定
                        // これはアイドルタイムアウトであり、全体のタイムアウトとは異なる
                        streamToUse.ReadTimeout = (int)(timeout * 1000); // float秒をintミリ秒に変換
                        streamToUse.WriteTimeout = (int)(timeout * 1000); // float秒をintミリ秒に変換

                        string httpRequest = $"GET {uri.PathAndQuery} HTTP/1.1\r\n" +
                                           $"Host: {uri.Host}\r\n" +
                                           "Connection: Close\r\n" + // レスポンス後、サーバーが接続を閉じるように要求
                                           "User-Agent: SNRHttpClient/1.0 (Unity)\r\n" +
                                           "Accept: */*\r\n" + // Acceptヘッダーを追加
                                           "Accept-Encoding: gzip, deflate\r\n" + // 圧縮サポートを通知
                                           "\r\n"; // ヘッダー終了
                        byte[] requestBytes = Encoding.ASCII.GetBytes(httpRequest);
                        await streamToUse.WriteAsync(requestBytes, 0, requestBytes.Length);
                        await streamToUse.FlushAsync();

                        using (var memoryStream = new MemoryStream())
                        {
                            byte[] buffer = new byte[8192]; // 8KB buffer
                            int bytesRead;
                            // "Connection: Close" のため、サーバーが接続を閉じるまで読み込む
                            while ((bytesRead = await streamToUse.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                memoryStream.Write(buffer, 0, bytesRead);
                            }
                            byte[] fullResponseMessage = memoryStream.ToArray();

                            // HTTPレスポンスのパース
                            int headerBodySeparatorIndex = -1;
                            for (int i = 0; i < fullResponseMessage.Length - 3; i++)
                            {
                                if (fullResponseMessage[i] == 13 && fullResponseMessage[i + 1] == 10 && //
                                    fullResponseMessage[i + 2] == 13 && fullResponseMessage[i + 3] == 10) //


                                {
                                    headerBodySeparatorIndex = i;
                                    break;
                                }
                            }

                            if (headerBodySeparatorIndex == -1)
                            {
                                currentResponse.ErrorMessage = "Invalid HTTP response: Header-body separator (\r\n\r\n) not found.";
                                currentResponse.Body = fullResponseMessage; // 区切りが見つからない場合、全てボディとして扱うか、エラーとするか。ここではエラーメッセージを設定。
                                return currentResponse;
                            }

                            // ヘッダー部分をパース
                            string headersPart = Encoding.UTF8.GetString(fullResponseMessage, 0, headerBodySeparatorIndex);
                            string[] rawHeaderLines = headersPart.Split(new string[] { "\r\n" }, StringSplitOptions.None);

                            if (rawHeaderLines.Length > 0)
                            {
                                string statusLine = rawHeaderLines[0];
                                string[] statusParts = statusLine.Split(new[] { ' ' }, 3);
                                if (statusParts.Length >= 2 && long.TryParse(statusParts[1], out long statusCodeVal))
                                {
                                    currentResponse.StatusCode = statusCodeVal;
                                    if (statusParts.Length >= 3)
                                        currentResponse.StatusDescription = statusParts[2];
                                }
                                else
                                {
                                    currentResponse.ErrorMessage = $"Could not parse HTTP status line: '{statusLine}'";
                                    // ステータスコードがパースできない場合、StatusCodeは0のまま
                                }

                                // HTTPヘッダーを WebHeaderCollection に格納
                                for (int i = 1; i < rawHeaderLines.Length; i++)
                                {
                                    string headerLine = rawHeaderLines[i];
                                    if (string.IsNullOrWhiteSpace(headerLine)) continue;
                                    int colonIndex = headerLine.IndexOf(':');
                                    if (colonIndex > 0)
                                    {
                                        string name = headerLine.Substring(0, colonIndex).Trim();
                                        string value = headerLine.Substring(colonIndex + 1).Trim();
                                        try
                                        {
                                            currentResponse.Headers.Add(name, value);
                                        }
                                        catch (ArgumentException ex)
                                        {
                                            // Log or handle duplicate/invalid headers if necessary
                                            // For now, just ignore to avoid crashing
                                            Debug.LogWarning($"Could not add header '{name}': {value}. Error: {ex.Message}");
                                        }
                                    }
                                }
                            }
                            else
                            {
                                currentResponse.ErrorMessage = "Empty headers received.";
                            }

                            // ボディ部分を抽出
                            int bodyStartIndex = headerBodySeparatorIndex + 4; // \r\n\r\n シーケンスの後
                            if (bodyStartIndex <= fullResponseMessage.Length) // bodyStartIndex == length の場合は空ボディ
                            {
                                int bodyLen = fullResponseMessage.Length - bodyStartIndex;
                                // byte[] rawBody = new byte[bodyLen]; // 削除
                                // if (bodyLen > 0) // 削除
                                // { // 削除
                                // Array.Copy(fullResponseMessage, bodyStartIndex, rawBody, 0, bodyLen); // 削除
                                // } // 削除

                                // Content-Encodingに基づいて解凍処理
                                string contentEncoding = currentResponse.Headers["Content-Encoding"]?.ToLowerInvariant();
                                if (contentEncoding == "gzip")
                                {
                                    using (var compressedStream = new MemoryStream(fullResponseMessage, bodyStartIndex, bodyLen, false))
                                    using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
                                    using (var decompressedStream = new MemoryStream())
                                    {
                                        await gzipStream.CopyToAsync(decompressedStream);
                                        currentResponse.Body = decompressedStream.ToArray();
                                    }
                                }
                                else if (contentEncoding == "deflate")
                                {
                                    using (var compressedStream = new MemoryStream(fullResponseMessage, bodyStartIndex, bodyLen, false))
                                    // DeflateStreamはヘッダーなしの生deflateデータ用。zlibヘッダー(RFC 1950)やgzipヘッダー(RFC 1952)付きの場合は注意が必要
                                    // 一般的なHTTPのdeflateはzlibヘッダーを持つことが多いが、ここではヘッダーなしと仮定
                                    // もしzlibヘッダー付きdeflateに対応するなら、ヘッダーをスキップするか、より高度なライブラリが必要
                                    using (var deflateStream = new DeflateStream(compressedStream, CompressionMode.Decompress))
                                    using (var decompressedStream = new MemoryStream())
                                    {
                                        await deflateStream.CopyToAsync(decompressedStream);
                                        currentResponse.Body = decompressedStream.ToArray();
                                    }
                                }
                                else
                                {
                                    // currentResponse.Body = rawBody; // 解凍不要。rawBodyはもうない。
                                    if (bodyLen > 0)
                                    {
                                        byte[] bodyData = new byte[bodyLen];
                                        Buffer.BlockCopy(fullResponseMessage, bodyStartIndex, bodyData, 0, bodyLen);
                                        currentResponse.Body = bodyData;
                                    }
                                    else
                                    {
                                        currentResponse.Body = Array.Empty<byte>(); // 空のボディ
                                    }
                                }
                            }
                            // else ボディがない (またはパースエラーでここまで来ない)
                        }
                    }
                }
            }
            catch (Exception ex) // SocketException, IOException, TimeoutException, UriFormatException など
            {
                // ProcessRequestAsync内で発生した例外は、currentResponse.ErrorMessage には設定せず、
                // 呼び出し元の SendWebRequest 内の task.Exception で処理されるようにスローする。
                // ただし、ここで currentResponse にエラー情報を部分的にでもセットしておくとデバッグに役立つ可能性はある。
                // currentResponse.ErrorMessage = $"Request Processing Exception: {ex.GetType().Name} - {ex.Message}";
                throw; // 例外を再スローして SendWebRequest の task.Exception で捕捉させる
            }
            return currentResponse;
        }
    }
}
