using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;

namespace SuperNewRoles.CustomOption
{
    public static class CustomRegulation
    {
        public static IEnumerator FetchRegulation()
        {
            // config.json を GoogleDriveなどに上げる
            var request = UnityWebRequest.Get("https://raw.githubusercontent.com/ykundesu/AmongUs_Blacklist/main/Blacklist.json");
            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError)
            {
                yield break;
            }
            var json = JObject.Parse(request.downloadHandler.text);
            for (var regulation = json["regulations"].First; regulation != null; regulation = regulation.Next)
            {
                RegulationData data = new();
                data.title = regulation["title"]?.ToString();
                data.MeetingButtonNum = int.Parse(regulation["MeetingButtonNum"]?.ToString());
                data.MeetingButtonCooldown = int.Parse(regulation["MeetingButtonCooldown"]?.ToString());
                data.VoteTime = int.Parse(regulation["VoteTime"]?.ToString());
                data.PlayerSpeed = int.Parse(regulation["PlayerSpeed"]?.ToString());
                data.CrewVision = float.Parse(regulation["CrewVision"]?.ToString());
                data.ImpostorVision = float.Parse(regulation["ImpostorVision"]?.ToString());
                data.KillCoolTime = float.Parse(regulation["KillCoolTime"]?.ToString());
                data.CommonTask = int.Parse(regulation["CommonTask"]?.ToString());
                data.LongTask = int.Parse(regulation["LongTask"]?.ToString());
                data.ShortTask = int.Parse(regulation["ShortTask"]?.ToString());
                for (var option = regulation["ModOptions"].First; option != null; option = option.Next)
                {
                    data.ChangeOptions.Add(int.Parse(option["id"]?.ToString()), int.Parse(option["selection"]?.ToString()));
                }
                RegulationData.Regulations.Add(data);
            }
        }
        public class RegulationData
        {
            public static List<RegulationData> Regulations = new();
            public string title;

            //[ゲーム設定]
            public int MeetingButtonNum;
            public int MeetingButtonCooldown;
            public int VoteTime;
            public float PlayerSpeed;
            public float CrewVision;
            public float ImpostorVision;
            public float KillCoolTime;
            public int CommonTask;
            public int LongTask;
            public int ShortTask;

            //オプションID:セレクション
            public Dictionary<int, int> ChangeOptions = new();
        }
        public class RegulationObject
        {

        }
    }
}
