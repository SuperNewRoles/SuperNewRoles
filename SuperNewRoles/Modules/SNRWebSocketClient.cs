using System;
using System.Collections;
using System.Net.Security;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SuperNewRoles.Modules;
/// <summary>
/// シンプルな WebSocket クライアント（Unity コルーチン対応）
/// </summary>
public class SNRWebSocketClient
{
    public Uri Uri { get; }
    public ClientWebSocket WebSocket { get; private set; }
    public bool IsConnected => WebSocket?.State == WebSocketState.Open;
    public float timeout { get; set; } = 5f;
    public bool ignoreSslErrors { get; set; } = false;

    // イベント
    public event Action OnOpen;
    public event Action<string> OnMessage;
    public event Action<WebSocketCloseStatus?, string> OnClose;
    public event Action<Exception> OnError;

    public SNRWebSocketClient(string url)
    {
        Uri = new Uri(url);
    }

    /// <summary>
    /// WebSocket サーバーへ接続します。
    /// </summary>
    public IEnumerator Connect()
    {
        WebSocket = new ClientWebSocket();
        if (ignoreSslErrors)
        {
            WebSocket.Options.RemoteCertificateValidationCallback += (sender, cert, chain, errors) => true;
        }

        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeout));
        var connectTask = WebSocket.ConnectAsync(Uri, cts.Token);
        while (!connectTask.IsCompleted)
        {
            yield return null;
        }
        if (connectTask.Exception != null)
        {
            var ex = connectTask.Exception.InnerException ?? connectTask.Exception;
            OnError?.Invoke(ex);
            yield break;
        }

        OnOpen?.Invoke();
    }

    /// <summary>
    /// テキストメッセージを送信します。
    /// </summary>
    public IEnumerator Send(string message)
    {
        if (!IsConnected)
        {
            OnError?.Invoke(new InvalidOperationException("WebSocket is not connected."));
            yield break;
        }

        var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
        var sendTask = WebSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        while (!sendTask.IsCompleted)
        {
            yield return null;
        }
        if (sendTask.Exception != null)
        {
            var ex = sendTask.Exception.InnerException ?? sendTask.Exception;
            OnError?.Invoke(ex);
        }
    }

    /// <summary>
    /// メッセージ受信ループを開始します。
    /// </summary>
    public IEnumerator ReceiveLoop()
    {
        if (WebSocket == null)
            yield break;

        var buffer = new ArraySegment<byte>(new byte[8192]);
        while (WebSocket.State == WebSocketState.Open)
        {
            var recvTask = WebSocket.ReceiveAsync(buffer, CancellationToken.None);
            while (!recvTask.IsCompleted)
            {
                yield return null;
            }
            if (recvTask.Exception != null)
            {
                var ex = recvTask.Exception.InnerException ?? recvTask.Exception;
                OnError?.Invoke(ex);
                yield break;
            }

            var result = recvTask.Result;
            if (result.MessageType == WebSocketMessageType.Close)
            {
                yield return Close();
                yield break;
            }

            var message = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
            OnMessage?.Invoke(message);
        }
    }

    /// <summary>
    /// WebSocket を正常クローズします。
    /// </summary>
    public IEnumerator Close()
    {
        if (WebSocket == null)
            yield break;

        var closeTask = WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
        while (!closeTask.IsCompleted)
        {
            yield return null;
        }
        if (closeTask.Exception != null)
        {
            var ex = closeTask.Exception.InnerException ?? closeTask.Exception;
            OnError?.Invoke(ex);
        }

        OnClose?.Invoke(WebSocket.CloseStatus, WebSocket.CloseStatusDescription);
    }
}