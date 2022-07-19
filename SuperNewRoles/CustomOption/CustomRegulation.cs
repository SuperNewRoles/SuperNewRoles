using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;

namespace SuperNewRoles.CustomOption
{
    public static class CustomRegulation
    {
        public static IEnumerator FetchRegulation()
        {
            Logger.Info("フェチ開始いいいい");
            var request = UnityWebRequest.Get("https://raw.githubusercontent.com/ykundesu/SuperNewRegulations/main/Regulations.json");
            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError)
            {
                Logger.Info("むりやった");
                yield break;
            }
            Logger.Info("通過");
            var json = JObject.Parse(request.downloadHandler.text);
            for (var regulation = json["regulations"].First; regulation != null; regulation = regulation.Next)
            {
                RegulationData data = new();
                data.title = regulation["title"]?.ToString();
                data.id = RegulationData.MaxId++;
                data.MeetingButtonNum = int.Parse(regulation["MeetingButtonNum"]?.ToString());
                data.MeetingButtonCooldown = int.Parse(regulation["MeetingButtonCooldown"]?.ToString());
                data.VoteTime = int.Parse(regulation["VoteTime"]?.ToString());
                data.PlayerSpeed = float.Parse(regulation["PlayerSpeed"]?.ToString());
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
            foreach (RegulationData data in RegulationData.Regulations)
            {
                SuperNewRolesPlugin.Logger.LogInfo
                    ("～～～～\n"
                    + data.title + "\n"
                    + data.id + "\n"
                    + data.MeetingButtonNum + "\n"
                + data.MeetingButtonCooldown + "\n"
                + data.VoteTime + "\n"
                + data.PlayerSpeed + "\n"
                + data.CrewVision + "\n"
                + data.ImpostorVision + "\n"
                + data.KillCoolTime + "\n"
                + data.CommonTask + "\n"
                + data.LongTask + "\n"
                + data.ShortTask + "\n");
                foreach (var datas in data.ChangeOptions)
                {
                    Logger.Info(CustomOption.options.FirstOrDefault((CustomOption option) => option.id == datas.Key).GetName() +" => "+datas.Value);
                }
            }
        }
        public class RegulationData
        {
            public static List<RegulationData> Regulations = new();
            public static int MaxId = 0;
            public static int Selected = 0;
            public string title;
            public int id;

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

            public OptionBehaviour optionBehaviour;

            //オプションID:セレクション
            public Dictionary<int, int> ChangeOptions = new();
        }
    }
}
