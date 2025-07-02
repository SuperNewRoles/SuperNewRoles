using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using SuperNewRoles.API.Handlers;

namespace SuperNewRoles.API;

public static class ApiServerManager
{
    // https://qiita.com/fukusin/items/61247231e0d97841ac9b
    private static HttpListener listener;
    private static Thread listenerThread;

    public static string domain = "localhost";
    public static int port = 49152;

    private static List<ServerHandlerBase> handlers = new();

    public static void Initialize()
    {
        try
        {
            listener = new HttpListener();
            listener.Prefixes.Add("http://" + domain + ":" + port + "/");
            listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            listener.Start();
        }
        catch (HttpListenerException e)
        {
            Logger.Error("HttpListenerException: " + e.Message);
            return;
        }

        listenerThread = new Thread(startListener);
        listenerThread.Start();

        handlers.Add(new JoinRoomByURL());
    }
    private static void startListener()
    {
        while (listener.IsListening)
        {
            var result = listener.BeginGetContext(ListenerCallback, listener);
            result.AsyncWaitHandle.WaitOne();
        }
    }

    private static void ListenerCallback(IAsyncResult result)
    {
        if (!listener.IsListening) return;
        HttpListenerContext context = listener.EndGetContext(result);
        Logger.Info("Method: " + context.Request.HttpMethod);
        Logger.Info("LocalUrl: " + context.Request.Url.LocalPath);

        // CORSヘッダーを追加
        context.Response.Headers.Add("Access-Control-Allow-Origin", "https://joinroom.supernewroles.com");
        context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
        context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");

        try
        {
            if (ProcessGetRequest(context)) return;
            if (ProcessPostRequest(context)) return;
        }
        catch (Exception e)
        {
            ReturnInternalError(context.Response, e);
        }
    }

    private static bool CanAccept(HttpMethod expected, string requested)
    {
        return string.Equals(expected.Method, requested, StringComparison.CurrentCultureIgnoreCase);
    }

    private static bool ProcessGetRequest(HttpListenerContext context)
    {
        if (!CanAccept(HttpMethod.Get, context.Request.HttpMethod) || context.Request.IsWebSocketRequest)
            return false;
        //メインスレッドでGetリクエストイベントを呼び出し
        foreach (var handler in handlers)
        {
            if (handler.Path == context.Request.Url.LocalPath)
            {
                handler.Handle(context);
                return true;
            }
        }
        Logger.Error("No handler found for path: " + context.Request.Url.LocalPath);
        return false;
    }

    private static bool ProcessPostRequest(HttpListenerContext context)
    {
        if (!CanAccept(HttpMethod.Post, context.Request.HttpMethod))
            return false;
        //メインスレッドでPostリクエストイベントを呼び出し
        return true;
    }

    private static void ReturnInternalError(HttpListenerResponse response, Exception cause)
    {
        Logger.Error(cause.Message);
        response.StatusCode = (int)HttpStatusCode.InternalServerError;
        response.ContentType = "text/plain";
        try
        {
            using (var writer = new StreamWriter(response.OutputStream, Encoding.UTF8))
                writer.Write(cause.ToString());
            response.Close();
        }
        catch (Exception e)
        {
            Logger.Error(e.Message);
            response.Abort();
        }
    }

    public static void ReturnBadRequest(HttpListenerResponse response, string message)
    {
        response.StatusCode = (int)HttpStatusCode.OK;
        response.ContentType = "text/plain";
        using (var writer = new StreamWriter(response.OutputStream, Encoding.UTF8))
            writer.Write(message);
    }
}