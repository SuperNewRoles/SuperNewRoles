using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using BepInEx.Unity.IL2CPP.Utils;
using UnityEngine.Networking;

namespace SuperNewRoles.SuperNewRolesWeb
{
    public static class Requests
    {
        public static void Get(string url, Action<long, DownloadHandler> callback)
        {
            AmongUsClient.Instance.StartCoroutine(GetIE(url, callback));
        }
        public static void Post(string url, string data, Action<long, DownloadHandler> callback)
        {
            AmongUsClient.Instance.StartCoroutine(PostIE(url, data, callback));
        }
        static IEnumerator PostIE(string url, string data, Action<long, DownloadHandler> callback)
        {
            var request = new UnityWebRequest(url, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(data);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

            yield return request.Send();

            callback(request.responseCode, request.downloadHandler);

            //Logger.Info($"Status Code: {request.responseCode}", "Analytics");
        }

        static IEnumerator GetIE(string url, Action<long, DownloadHandler> callback)
        {
            var request = new UnityWebRequest(url, "GET")
            {
                downloadHandler = new DownloadHandlerBuffer()
            };
            request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

            yield return request.Send();

            callback(request.responseCode, request.downloadHandler);

            //Logger.Info($"Status Code: {request.responseCode}", "Analytics");
        }
    }
}