using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.SuperNewRolesWeb
{
    public static class WebConstants
    {
        public const string WebUrlDefault = "https://web.supernewroles.com/";
        public const string ApiUrlPrefix = "api/";
        public static string ApiUrl => WebUrl + ApiUrlPrefix;
        public const string CApiUrlPrefix = "capi/";
        public static string CApiUrl => WebUrl + CApiUrlPrefix;
        public static string WebUrlDebug;
        public static string WebUrl { get { return IsDebugUrl ? WebUrlDebug : WebUrlDefault; } }
        public static bool IsDebugUrl = false;

        /// <summary>
        /// SNR Webの動作確認中か
        /// </summary>
        public const bool IsWebDebug = false; // プルリク時にtrueなら指摘してください

        public static void Load()
        {
            var url = IsWebDebug ? Environment.GetEnvironmentVariable("SNRWebUrl") : null;
            IsDebugUrl = url is not null;
            WebUrlDebug = url;
        }
    }
}