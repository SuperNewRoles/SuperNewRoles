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
        public static void Load()
        {
            var url = Environment.GetEnvironmentVariable("SNRWebUrl");
            IsDebugUrl = url is not null;
            WebUrlDebug = url;
        }
    }
}