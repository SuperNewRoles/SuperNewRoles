using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AmongUs.GameOptions;
using Newtonsoft.Json.Linq;
using SuperNewRoles.Helpers;
using UnityEngine;
using UnityEngine.Networking;


namespace SuperNewRoles.Modules;

public static class CustomRegulation
{
    static bool Loaded = false;

    // CustomRegulation.jsonのテストをする時にtrueに変える
    const bool IsTest = false;
    public static IEnumerator FetchRegulation()
    {
        if (Loaded) yield break;
        Logger.Info("フェチ開始いいいい");
        JObject json;

#pragma warning disable 0162 // 「到達できないコードが検出されました」の表示を無効
        // CustomRegulation.jsonの読み込み
        if (!IsTest)
        {
            var request = UnityWebRequest.Get("https://raw.githubusercontent.com/SuperNewRoles/SuperNewRegulations/main/Regulations.json");
            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError)
            {
                Logger.Info("むりやった");
                yield break;
            }
            Logger.Info("通過");
            json = JObject.Parse(request.downloadHandler.text);
        }
        else
        {
            try
            {
                var filePath = Path.GetDirectoryName(Application.dataPath) + @"\SuperNewRoles\Regulations.json";
                using StreamReader sr = new(filePath);
                var text = sr.ReadToEnd();
                json = JObject.Parse(text);
                Logger.Info("カスタムレギュレーションのテストファイルの読み込みに成功しました。", "ReadingRegistration");
            }
            catch (Exception e)
            {
                Logger.Error($"カスタムレギュレーションのテストファイルの読み込みに失敗しました。 : {e}", "ReadingRegistration");
                yield break;
            }
        }
#pragma warning restore 0162

        RegulationData CustomData = new()
        {
            id = 0,
            title = "カスタム"
        };
        RegulationData.Regulations.Add(CustomData);
        for (var regulation = json["regulations"].First; regulation != null; regulation = regulation.Next)
        {
            RegulationData data = new()
            {
                title = regulation["title"]?.ToString()
            };
            RegulationData.MaxId++;
            data.id = RegulationData.MaxId;
            data.NumImpostors = int.Parse(regulation["NumImpostors"]?.ToString());
            data.MeetingButtonNum = int.Parse(regulation["MeetingButtonNum"]?.ToString());
            data.MeetingButtonCooldown = int.Parse(regulation["MeetingButtonCooldown"]?.ToString());
            data.DiscussionTime = int.Parse(regulation["DiscussionTime"]?.ToString());
            data.VoteTime = int.Parse(regulation["VoteTime"]?.ToString());
            data.PlayerSpeed = float.Parse(regulation["PlayerSpeed"]?.ToString());
            data.CrewVision = float.Parse(regulation["CrewVision"]?.ToString());
            data.ImpostorVision = float.Parse(regulation["ImpostorVision"]?.ToString());
            data.KillCoolTime = float.Parse(regulation["KillCoolTime"]?.ToString());
            data.CommonTask = int.Parse(regulation["CommonTask"]?.ToString());
            data.LongTask = int.Parse(regulation["LongTask"]?.ToString());
            data.ShortTask = int.Parse(regulation["ShortTask"]?.ToString());
            data.VisualTasks = bool.Parse(regulation["VisualTasks"]?.ToString());
            data.ConfirmImpostor = bool.Parse(regulation["ConfirmImpostor"]?.ToString());
            data.AnonymousVotes = bool.Parse(regulation["AnonymousVotes"]?.ToString());
            for (var option = regulation["ModOptions"].First; option != null; option = option.Next)
            {
                data.ChangeOptions.Add(int.Parse(option["id"]?.ToString()), int.Parse(option["selection"]?.ToString()));
            }
            RegulationData.Regulations.Add(data);
        }
        /* foreach (RegulationData data in RegulationData.Regulations)
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
        }*/
        Loaded = true;
    }
    public static void Select(int id)
    {
        if (RegulationData.Selected == id) return;
        RegulationData.Selected = id;
        if (id == 0)
        {
            foreach (CustomOption options in CustomOption.options)
            {
                options.selection = options.ClientSelection;
            }
            CustomOption.ShareOptionSelections();
            return;
        }
        RegulationData data = RegulationData.Regulations.FirstOrDefault(rd => rd.id == id);
        GameManager.Instance.LogicOptions.currentGameOptions.SetInt(Int32OptionNames.NumImpostors, data.NumImpostors);
        GameManager.Instance.LogicOptions.currentGameOptions.SetInt(Int32OptionNames.NumEmergencyMeetings, data.MeetingButtonNum);
        GameManager.Instance.LogicOptions.currentGameOptions.SetInt(Int32OptionNames.EmergencyCooldown, data.MeetingButtonCooldown);
        GameManager.Instance.LogicOptions.currentGameOptions.SetInt(Int32OptionNames.DiscussionTime, data.DiscussionTime);
        GameManager.Instance.LogicOptions.currentGameOptions.SetInt(Int32OptionNames.VotingTime, data.VoteTime);
        GameManager.Instance.LogicOptions.currentGameOptions.SetFloat(FloatOptionNames.PlayerSpeedMod, data.PlayerSpeed);
        GameManager.Instance.LogicOptions.currentGameOptions.SetFloat(FloatOptionNames.CrewLightMod, data.CrewVision);
        GameManager.Instance.LogicOptions.currentGameOptions.SetFloat(FloatOptionNames.ImpostorLightMod, data.ImpostorVision);
        GameManager.Instance.LogicOptions.currentGameOptions.SetFloat(FloatOptionNames.KillCooldown, data.KillCoolTime);
        GameManager.Instance.LogicOptions.currentGameOptions.SetInt(Int32OptionNames.NumCommonTasks, data.CommonTask);
        GameManager.Instance.LogicOptions.currentGameOptions.SetInt(Int32OptionNames.NumLongTasks, data.LongTask);
        GameManager.Instance.LogicOptions.currentGameOptions.SetInt(Int32OptionNames.NumShortTasks, data.ShortTask);
        GameManager.Instance.LogicOptions.currentGameOptions.SetBool(BoolOptionNames.VisualTasks, data.VisualTasks);
        GameManager.Instance.LogicOptions.currentGameOptions.SetBool(BoolOptionNames.ConfirmImpostor, data.ConfirmImpostor);
        GameManager.Instance.LogicOptions.currentGameOptions.SetBool(BoolOptionNames.AnonymousVotes, data.AnonymousVotes);
        foreach (CustomOption options in CustomOption.options)
        {
            options.selection = options.defaultSelection;
        }
        foreach (var option in data.ChangeOptions)
        {
            var opt = CustomOption.options.FirstOrDefault((CustomOption optiondata) => optiondata.id == option.Key);
            if (opt != null)
            {
                opt.selection = option.Value;
            }
            else
            {
                Logger.Info(option.Key + "がnullでした");
            }
        }
        CustomOptionHolder.DisconnectNotPCOption.selection = 0;

        RPCHelper.RpcSyncOption(GameManager.Instance.LogicOptions.currentGameOptions);
    }
    public class RegulationData
    {
        public static List<RegulationData> Regulations = new();
        public static int MaxId = 0;
        public static int Selected = 0;
        public string title;
        public int id;

        //[ゲーム設定]
        public int NumImpostors;
        public int MeetingButtonNum;
        public int MeetingButtonCooldown;
        public int DiscussionTime;
        public int VoteTime;
        public float PlayerSpeed;
        public float CrewVision;
        public float ImpostorVision;
        public float KillCoolTime;
        public int CommonTask;
        public int LongTask;
        public int ShortTask;
        public bool VisualTasks;
        public bool ConfirmImpostor;
        public bool AnonymousVotes;

        public OptionBehaviour optionBehaviour;

        //オプションID:セレクション
        public Dictionary<int, int> ChangeOptions = new();
    }
}