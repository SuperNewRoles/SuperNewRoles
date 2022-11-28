using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SuperNewRoles.Modules;

public static class PVCreator
{
    public static List<Vector2> poss = new();
    public static bool Is;
    public static bool Is2;
    public static void Start()
    {
        UpdateTime = UpdateTimeD;
        Is = true;
        poss = new();
        OldIsVent = false;
        starttime = DateTime.UtcNow;
        ventData = new();
    }
    public static void Start2()
    {
        UpdateTime = UpdateTimeD;
        Is2 = true;
        Logger.Info(posData.Count.ToString());
        if (players.Count < posData.Count || players.TrueForAll(x => x == null))
        {
            players = new();
            for (int i = 0; i < posData.Count; i++)
            {
                players.Add(BotManager.Spawn());
            }
        }
        Logger.Info(players.Count.ToString());
        index = new();
        ventindex = new();
        for (int i = 0; i < posData.Count; i++)
        {
            index[i] = -1;
            Logger.Info($"{i} : {VentPos[i] != null}");
            ventindex[i] = VentPos[i].Count > 0 ? 0 : -1;
        }
        Logger.Info("a");
        coros = new();
        Logger.Info("b");
        NextVentTime = new();
        Logger.Info("c");
        for (int i = 0; i < players.Count; i++)
        {
            Logger.Info("d");
            PlayerControl player = players[i];
            Logger.Info("e");
            player.SetColor(Colors[i]);
            Logger.Info("f");
            player.SetName(Names[i]);
            Logger.Info("g");
            coros[i] = null;
            Logger.Info("h");
            NextVentTime.Add(VentPos[i].Count > 0 ? (float)VentPos[i][0].Item1 : -1);
            Logger.Info("i");
        }
    }

    public static List<string> Names = new()
        {
            "茎",
            "ぴーつー",
            "一般マトリョーシカ",
            "おかしいおかし",
            "ベント超大好きマン",
            "あもんぐうーす",
            "クキッキー",
        };

    public static List<int> Colors
    {
        get
        {
            List<int> lists = new();
            var Color = Palette.PlayerColors.ToList();
            Color.RemoveAt(CachedPlayer.LocalPlayer.Data.DefaultOutfit.ColorId);
            for (int i = 0; i < posData.Count; i++)
            {
                bool Is = true;
                int index = -1;
                while (Is)
                {
                    index = ModHelpers.GetRandomIndex(Color);
                    if (!lists.Contains(index))
                    {
                        Is = false;
                    }
                }
                lists.Add(index);
                Color.RemoveAt(index);
            }
            return lists;
        }
    }
    public static void End()
    {
        Is = false;
        var text = "\nnew(){";
        foreach (Vector2 pos in poss)
        {
            text += $"new({pos.x}f,{pos.y}f),";
        }
        text += "},";
        GUIUtility.systemCopyBuffer = text;
        Logger.Info(text);
        Logger.Info("");
        Logger.Info("");
        text = "new(){";
        foreach (var data in ventData)
        {
            text += $"({data.Item1}f,{data.Item2}),";
        }
        text += "},";
        Logger.Info(text);
        poss = new();
    }
    static Dictionary<int, int> index = new();
    static Dictionary<int, int> ventindex = new();
    static Dictionary<int, Coroutine> coros = new();
    static float UpdateTime;
    const float UpdateTimeD = 0.003f;
    static List<PlayerControl> players = new();
    static readonly List<List<Vector2>> posData = new()
        {
            new() { new(999, 999), new(999, 999), new(999, 999), new(999, 999), new(999, 999), new(999, 999), new(999, 999), new(999, 999), new(999, 999), new(999, 999), new(999, 999), },
            new() { new(-17.83858f, -0.7692544f), new(-17.73985f, -0.7692544f), new(-17.68814f, -0.7692544f), new(-17.52902f, -0.7692544f), new(-17.47519f, -0.7692544f), new(-17.36343f, -0.7692544f), new(-17.25801f, -0.7692544f), new(-17.15269f, -0.7692544f), new(-17.04646f, -0.7692544f), new(-16.93991f, -0.7692544f), new(-16.83258f, -0.7692544f), new(-16.72733f, -0.7692544f), new(-16.62273f, -0.7692544f), new(-16.51651f, -0.7692544f), new(-16.41391f, -0.7692544f), new(-16.36756f, -0.7692544f), new(-16.1754f, -0.7692544f), new(-16.10338f, -0.7692544f), new(-15.99899f, -0.7692544f), new(-15.89186f, -0.7692544f), new(-15.78843f, -0.7692544f), new(-15.68425f, -0.7692544f), new(-15.5776f, -0.7692544f), new(-15.47262f, -0.7692544f), new(-15.36438f, -0.7692544f), new(-15.26075f, -0.7692544f), new(-15.15393f, -0.7692544f), new(-15.04957f, -0.7692544f), new(-14.94427f, -0.7692544f), new(-14.83457f, -0.7692544f), new(-14.73378f, -0.7692544f), new(-14.63067f, -0.7692544f), new(-14.52562f, -0.7692544f), new(-14.41863f, -0.7692544f), new(-14.31262f, -0.7692544f), new(-14.20595f, -0.7692544f), new(-14.10061f, -0.7692544f), new(-13.99673f, -0.7692544f), new(-13.89112f, -0.7692544f), new(-13.78352f, -0.7692544f), new(-13.68066f, -0.7692544f), new(-13.57696f, -0.7692544f), new(-13.4698f, -0.7692544f), new(-13.36475f, -0.7692544f), new(-13.2601f, -0.7692544f), new(-13.15588f, -0.7692544f), new(-13.04917f, -0.7692544f), new(-12.94553f, -0.7692544f), new(-12.83948f, -0.7692544f), new(-12.79428f, -0.7692544f), new(-12.63613f, -0.7692544f), new(-12.58113f, -0.7692544f), new(-12.46924f, -0.7692544f), new(-12.31866f, -0.7692544f), new(-12.26778f, -0.7692544f), new(-12.15413f, -0.7692544f), new(-12.08031f, -0.7692545f), new(-11.93735f, -0.7692544f), new(-11.8251f, -0.7692544f), new(-11.72099f, -0.7692544f), new(-11.6154f, -0.7692544f), new(-11.53395f, -0.8238863f), new(-11.4603f, -0.8975364f), new(-11.38653f, -0.9712974f), new(-11.3126f, -1.04523f), new(-11.29395f, -1.141629f), new(-11.29395f, -1.245807f), new(-11.29395f, -1.350032f), new(-11.29395f, -1.454303f), new(-11.29395f, -1.559359f), new(-11.29395f, -1.661758f), new(-11.29395f, -1.768876f), new(-11.29395f, -1.876258f), new(-11.29395f, -1.979575f), new(-11.29395f, -2.0837f), new(-11.29395f, -2.190134f), new(-11.23439f, -2.269274f), new(-11.16033f, -2.343338f), new(-11.08519f, -2.418474f), new(-11.00712f, -2.49654f), new(-10.93743f, -2.566228f), new(-10.86303f, -2.640629f), new(-10.78742f, -2.716238f), new(-10.71251f, -2.791155f), new(-10.63713f, -2.866537f), new(-10.56223f, -2.941428f), new(-10.48777f, -3.015886f), new(-10.41162f, -3.092044f), new(-10.37973f, -3.12393f), new(-10.2667f, -3.236958f), new(-10.22885f, -3.274814f), new(-10.11611f, -3.387551f), new(-10.06667f, -3.388222f), new(-9.908051f, -3.388222f), new(-9.870446f, -3.42488f), new(-9.788818f, -3.484231f), new(-9.686021f, -3.57067f), new(-9.636395f, -3.620295f), new(-9.560088f, -3.696601f), new(-9.485559f, -3.771131f), new(-9.412174f, -3.844516f), new(-9.336353f, -3.920337f), new(-9.260134f, -3.996556f), new(-9.240444f, -4.092438f), new(-9.240444f, -4.197047f), new(-9.240444f, -4.30082f), new(-9.240444f, -4.409595f), new(-9.240444f, -4.512941f), new(-9.240444f, -4.62105f), new(-9.240444f, -4.722967f), new(-9.240444f, -4.829381f), new(-9.300671f, -4.909804f), new(-9.376542f, -4.985676f), new(-9.451127f, -5.06026f), new(-9.524355f, -5.133488f), new(-9.599475f, -5.208609f), new(-9.67197f, -5.281105f), new(-9.747055f, -5.356189f), new(-9.822382f, -5.431516f), new(-9.897756f, -5.506889f), new(-9.903358f, -5.610878f), new(-9.903358f, -5.714098f), new(-9.903358f, -5.819915f), new(-9.903358f, -5.923483f), new(-9.903358f, -6.027727f), new(-9.83149f, -6.105192f), new(-9.798437f, -6.138244f), new(-9.686296f, -6.250385f), new(-9.648441f, -6.28824f), new(-9.535793f, -6.400887f), new(-9.498194f, -6.438488f), new(-9.417422f, -6.519258f), new(-9.343165f, -6.593516f), new(-9.267784f, -6.668897f), new(-9.193329f, -6.743352f), new(-9.16683f, -6.799369f), new(-9.166874f, -6.844606f), new(-9.166889f, -6.890541f), new(-9.166889f, -6.909925f), new(-9.165863f, -6.957907f), new(-9.109095f, -7.053862f), new(-9.031242f, -7.131715f), new(-8.957521f, -7.205434f), new(-8.882231f, -7.280726f), new(-8.80759f, -7.355366f), new(-8.734669f, -7.428288f), new(-8.65979f, -7.503166f), new(-8.560279f, -7.516953f), new(-8.456124f, -7.516953f), new(-8.350001f, -7.516953f), new(-8.24292f, -7.516953f), new(-8.138195f, -7.516953f), new(-8.033526f, -7.516953f), new(-7.927155f, -7.516953f), new(-7.820292f, -7.516953f), new(-7.812669f, -7.516953f), new(-7.812669f, -7.516953f), },
            new() { new(-17.42826f, -1.132976f), new(-17.42826f, -1.132976f), new(-17.42826f, -1.132976f), new(-17.42826f, -1.132976f), new(-17.42826f, -1.132976f), new(-17.42826f, -1.132976f), new(-17.42826f, -1.132976f), new(-17.42826f, -1.132976f), new(-17.42826f, -1.132976f), new(-17.42826f, -1.132976f), new(-17.42826f, -1.132976f), new(-17.36489f, -1.132976f), new(-17.25726f, -1.132976f), new(-17.15271f, -1.132976f), new(-17.04922f, -1.132976f), new(-16.94311f, -1.132976f), new(-16.83743f, -1.132976f), new(-16.79293f, -1.132976f), new(-16.60723f, -1.132976f), new(-16.50679f, -1.132976f), new(-16.3982f, -1.132976f), new(-16.29441f, -1.132976f), new(-16.18896f, -1.132976f), new(-16.08263f, -1.132976f), new(-15.97481f, -1.132976f), new(-15.87213f, -1.132976f), new(-15.76316f, -1.132976f), new(-15.66053f, -1.132976f), new(-15.55416f, -1.132976f), new(-15.45142f, -1.132976f), new(-15.34556f, -1.132976f), new(-15.24102f, -1.132976f), new(-15.1374f, -1.132976f), new(-15.03306f, -1.132976f), new(-14.92846f, -1.132976f), new(-14.88002f, -1.132976f), new(-14.72338f, -1.132976f), new(-14.67035f, -1.132976f), new(-14.51401f, -1.132976f), new(-14.46292f, -1.132976f), new(-14.34921f, -1.132976f), new(-14.24325f, -1.132976f), new(-14.13754f, -1.132976f), new(-14.03508f, -1.132976f), new(-13.92996f, -1.132976f), new(-13.82578f, -1.132976f), new(-13.71956f, -1.132976f), new(-13.61596f, -1.132976f), new(-13.51053f, -1.132976f), new(-13.46455f, -1.132976f), new(-13.28805f, -1.132976f), new(-13.17532f, -1.132976f), new(-13.0724f, -1.132976f), new(-12.96857f, -1.132976f), new(-12.86514f, -1.132976f), new(-12.76046f, -1.132976f), new(-12.65611f, -1.132976f), new(-12.5514f, -1.132976f), new(-12.48123f, -1.132976f), new(-12.35001f, -1.132976f), new(-12.2463f, -1.132976f), new(-12.14339f, -1.132976f), new(-12.03581f, -1.132976f), new(-11.93138f, -1.132976f), new(-11.82388f, -1.132976f), new(-11.71637f, -1.132976f), new(-11.60822f, -1.132976f), new(-11.5333f, -1.194599f), new(-11.45953f, -1.268371f), new(-11.38544f, -1.342457f), new(-11.31146f, -1.416435f), new(-11.23723f, -1.490667f), new(-11.16151f, -1.56639f), new(-11.08785f, -1.640044f), new(-11.01431f, -1.713583f), new(-10.93871f, -1.78919f), new(-10.86367f, -1.864225f), new(-10.85835f, -1.967191f), new(-10.85835f, -2.071661f), new(-10.85835f, -2.176247f), new(-10.85835f, -2.279283f), new(-10.85835f, -2.385085f), new(-10.85835f, -2.487697f), new(-10.78735f, -2.565546f), new(-10.71449f, -2.638401f), new(-10.63978f, -2.713119f), new(-10.5662f, -2.786694f), new(-10.49222f, -2.860675f), new(-10.45962f, -2.893278f), new(-10.31595f, -2.936485f), new(-10.26417f, -2.936485f), new(-10.10901f, -2.936485f), new(-10.05512f, -2.936485f), new(-9.896371f, -2.936485f), new(-9.859379f, -2.97268f), new(-9.748981f, -3.083077f), new(-9.711387f, -3.120672f), new(-9.632511f, -3.199548f), new(-9.556785f, -3.275273f), new(-9.527289f, -3.30879f), new(-9.527289f, -3.463752f), new(-9.527289f, -3.57562f), new(-9.527289f, -3.682166f), new(-9.527289f, -3.787088f), new(-9.527289f, -3.892235f), new(-9.527289f, -4.001619f), new(-9.527289f, -4.106593f), new(-9.527289f, -4.209701f), new(-9.527289f, -4.316138f), new(-9.527289f, -4.422761f), new(-9.527289f, -4.526606f), new(-9.527289f, -4.634845f), new(-9.527289f, -4.739234f), new(-9.527289f, -4.843379f), new(-9.527289f, -4.946979f), new(-9.527289f, -5.053632f), new(-9.527289f, -5.157795f), new(-9.527289f, -5.262819f), new(-9.527289f, -5.367993f), new(-9.527289f, -5.473044f), new(-9.527289f, -5.579688f), new(-9.527289f, -5.684021f), new(-9.527289f, -5.789813f), new(-9.527289f, -5.893166f), new(-9.527289f, -5.998998f), new(-9.527289f, -6.100682f), new(-9.527288f, -6.209098f), new(-9.527289f, -6.312809f), new(-9.462441f, -6.390446f), new(-9.388336f, -6.464551f), new(-9.314243f, -6.538643f), new(-9.240288f, -6.612599f), new(-9.165948f, -6.686939f), new(-9.166332f, -6.734038f), new(-9.167126f, -6.779039f), new(-9.167118f, -6.824251f), new(-9.167093f, -6.868833f), new(-9.167054f, -6.913048f), new(-9.167004f, -6.957908f), new(-9.156617f, -7.018845f), new(-9.085839f, -7.091401f), new(-9.051259f, -7.125981f), new(-8.951914f, -7.225327f), new(-8.868078f, -7.309162f), new(-8.796015f, -7.381225f), new(-8.722525f, -7.454715f), new(-8.647204f, -7.530036f), new(-8.580387f, -7.577268f), new(-8.533995f, -7.581356f), new(-8.435979f, -7.581369f), new(-8.331306f, -7.581369f), new(-8.22713f, -7.581369f), new(-8.123574f, -7.581369f), new(-8.018767f, -7.581369f), new(-7.912819f, -7.581369f), new(-7.806799f, -7.581369f), new(-7.700333f, -7.581369f), new(-7.593781f, -7.581369f), new(-7.489979f, -7.581369f), },
            new() { new(-14.946f, -1.205668f), new(-14.946f, -1.205668f), new(-14.946f, -1.205668f), new(-14.87708f, -1.205668f), new(-14.77271f, -1.205668f), new(-14.66912f, -1.205668f), new(-14.56495f, -1.205668f), new(-14.4591f, -1.205668f), new(-14.3286f, -1.205668f), new(-14.24676f, -1.205668f), new(-14.13914f, -1.205668f), new(-14.03352f, -1.205668f), new(-13.9265f, -1.205668f), new(-13.82208f, -1.205668f), new(-13.71796f, -1.205668f), new(-13.61221f, -1.205668f), new(-13.50845f, -1.205668f), new(-13.40292f, -1.205668f), new(-13.29625f, -1.205668f), new(-13.18705f, -1.205668f), new(-13.08445f, -1.205668f), new(-12.97742f, -1.205668f), new(-12.87104f, -1.205668f), new(-12.76721f, -1.205668f), new(-12.66007f, -1.205668f), new(-12.5566f, -1.205668f), new(-12.45292f, -1.205668f), new(-12.347f, -1.205668f), new(-12.24257f, -1.205668f), new(-12.13879f, -1.205668f), new(-12.03472f, -1.205668f), new(-11.93044f, -1.205668f), new(-11.82525f, -1.205668f), new(-11.71974f, -1.205668f), new(-11.61386f, -1.205668f), new(-11.50934f, -1.205668f), new(-11.43548f, -1.278684f), new(-11.36186f, -1.352304f), new(-11.28802f, -1.426139f), new(-11.21458f, -1.499578f), new(-11.18146f, -1.532702f), new(-11.07084f, -1.643321f), new(-11.03392f, -1.680243f), new(-10.92371f, -1.790453f), new(-10.88664f, -1.827521f), new(-10.84558f, -1.967803f), new(-10.84558f, -2.019283f), new(-10.84558f, -2.176722f), new(-10.84558f, -2.229075f), new(-10.84558f, -2.373567f), new(-10.84558f, -2.461821f), new(-10.84558f, -2.566137f), new(-10.84558f, -2.671632f), new(-10.84558f, -2.778636f), new(-10.78976f, -2.861897f), new(-10.71501f, -2.936647f), new(-10.64012f, -3.011536f), new(-10.56429f, -3.087373f), new(-10.4901f, -3.161565f), new(-10.41724f, -3.234419f), new(-10.34135f, -3.310305f), new(-10.26885f, -3.38281f), new(-10.19447f, -3.457186f), new(-10.14397f, -3.46899f), new(-10.09862f, -3.46899f), new(-10.00131f, -3.46899f), new(-9.896928f, -3.46899f), new(-9.789434f, -3.46899f), new(-9.685565f, -3.46899f), new(-9.607971f, -3.53863f), new(-9.533669f, -3.612931f), new(-9.457672f, -3.688929f), new(-9.45664f, -3.790641f), new(-9.45664f, -3.897463f), new(-9.45664f, -3.941642f), new(-9.45664f, -4.101608f), new(-9.45664f, -4.154388f), new(-9.45664f, -4.312367f), new(-9.45664f, -4.364416f), new(-9.45664f, -4.52268f), new(-9.45664f, -4.573693f), new(-9.45664f, -4.687973f), new(-9.45664f, -4.791049f), new(-9.45664f, -4.89784f), new(-9.45664f, -5.002144f), new(-9.45664f, -5.107413f), new(-9.45664f, -5.210218f), new(-9.45664f, -5.316636f), new(-9.45664f, -5.4199f), new(-9.45664f, -5.525105f), new(-9.45664f, -5.571862f), new(-9.45664f, -5.701637f), },

        };
    static List<(double, int)> ventData = new();
    static List<List<(double, int)>> VentPos
    {
        get
        {
            List<List<(double, int)>> lists = new();
            for (int i = 0; i < posData.Count; i++)
            {
                lists.Add(new());
            }
            return lists;
        }
    }

    static DateTime starttime;
    static bool OldIsVent;
    static List<float> NextVentTime;
    public static void FixedUpdate()
    {
        if (!ConfigRoles.DebugMode.Value) return;
        if (Is)
        {
            UpdateTime -= Time.fixedDeltaTime;
            if (UpdateTime <= 0)
            {
                UpdateTime = UpdateTimeD;
                poss.Add(CachedPlayer.LocalPlayer.transform.position);
                if (OldIsVent != CachedPlayer.LocalPlayer.PlayerControl.inVent)
                {
                    ventData.Add(((DateTime.UtcNow - starttime).TotalSeconds, Mode.BattleRoyal.Main.VentData[CachedPlayer.LocalPlayer.PlayerId] != null ? (int)Mode.BattleRoyal.Main.VentData[CachedPlayer.LocalPlayer.PlayerId] : MapUtilities.CachedShipStatus.AllVents.FirstOrDefault(x => Vector2.Distance(CachedPlayer.LocalPlayer.transform.position, x.transform.position) <= 0.5f).Id));
                    starttime = DateTime.UtcNow;
                }
                OldIsVent = CachedPlayer.LocalPlayer.PlayerControl.inVent;
            }
        }
        if (Is2)
        {
            UpdateTime -= Time.fixedDeltaTime;
            if (UpdateTime <= 0)
            {
                UpdateTime = UpdateTimeD;
                for (int i = 0; i < posData.Count; i++)
                {
                    int playerindex = i;
                    index[playerindex]++;
                    if (posData[playerindex].Count <= index[playerindex])
                    {
                        index[playerindex] = -1;
                        Is2 = false;
                        if (coros[playerindex] != null) AmongUsClient.Instance.StopCoroutine(coros[playerindex]);
                        return;
                    }
                    //ベント
                    if (ventindex[playerindex] != -1)
                    {
                        Logger.Info(NextVentTime[playerindex].ToString());
                        Logger.Info(ventindex[playerindex].ToString());
                        NextVentTime[playerindex] -= Time.fixedDeltaTime;
                        if (NextVentTime[playerindex] <= 0)
                        {
                            if (players[playerindex].inVent)
                            {
                                Logger.Info("Exit");
                                players[playerindex].MyPhysics.RpcExitVent(VentPos[playerindex][ventindex[playerindex]].Item2);
                                MapUtilities.CachedShipStatus.AllVents.FirstOrDefault(x => x.Id == VentPos[playerindex][ventindex[playerindex]].Item2).ExitVent(players[playerindex]);
                            }
                            else
                            {
                                Logger.Info("In");
                                players[playerindex].MyPhysics.RpcEnterVent(VentPos[playerindex][ventindex[playerindex]].Item2);
                                MapUtilities.CachedShipStatus.AllVents.FirstOrDefault(x => x.Id == VentPos[playerindex][ventindex[playerindex]].Item2).EnterVent(players[playerindex]);
                            }
                            ventindex[playerindex]++;
                            if (VentPos[playerindex].Count > ventindex[playerindex])
                            {
                                NextVentTime[playerindex] = (float)VentPos[playerindex][ventindex[playerindex]].Item1;
                            }
                            else
                            {
                                ventindex[playerindex] = -1;
                            }
                        }
                    }
                    players[playerindex].NetTransform.RpcSnapTo(posData[playerindex][index[playerindex]]);

                    if (coros[playerindex] != null)
                    {
                        AmongUsClient.Instance.StopCoroutine(coros[playerindex]);
                        coros[playerindex] = null;
                    }
                    if (posData[playerindex][index[playerindex]].x == posData[playerindex][index[playerindex] - 1].x && posData[playerindex][index[playerindex]].y == posData[playerindex][index[playerindex] - 1].y)
                    {

                    }
                    else
                    {
                        coros[playerindex] = AmongUsClient.Instance.StartCoroutine(players[playerindex].MyPhysics.WalkPlayerTo(new()));
                    }

                    if (index[playerindex] > 0)
                    {
                        players[playerindex].cosmetics.currentBodySprite.BodySprite.transform.localScale = posData[playerindex][index[playerindex]].x > posData[playerindex][index[playerindex] - 1].x ? new(0.5f, 0.5f, 1) : new(-0.5f, 0.5f, 1);
                        players[playerindex].cosmetics.currentBodySprite.BodySprite.transform.FindChild("Hat").localScale = posData[playerindex][index[playerindex]].x > posData[playerindex][index[playerindex] - 1].x ? new(1f, 1f, 1) : new(-1f, 1f, 1);
                        players[playerindex].cosmetics.currentBodySprite.BodySprite.transform.FindChild("Visor").localScale = posData[playerindex][index[playerindex]].x > posData[playerindex][index[playerindex] - 1].x ? new(1f, 1f, 1) : new(-1f, 1f, 1);
                        players[playerindex].cosmetics.currentBodySprite.BodySprite.transform.FindChild("NameText_TMP").localScale = posData[playerindex][index[playerindex]].x > posData[playerindex][index[playerindex] - 1].x ? new(2.8f, 2.8f, 1) : new(-2.8f, 2.8f, 1);
                        players[playerindex].cosmetics.currentBodySprite.BodySprite.transform.FindChild("ColorblindName_TMP").localScale = posData[playerindex][index[playerindex]].x > posData[playerindex][index[playerindex] - 1].x ? new(2.8f, 2.8f, 1) : new(-2.8f, 2.8f, 1);
                        if (players[playerindex].cosmetics.currentBodySprite.BodySprite.transform.FindChild("Info") != null) players[playerindex].cosmetics.currentBodySprite.BodySprite.transform.FindChild("Info").localScale = posData[playerindex][index[playerindex]].x > posData[playerindex][index[playerindex] - 1].x ? new(2.8f, 2.8f, 1) : new(-2.8f, 2.8f, 1);
                    }
                }
            }
        }
        CustomDummyObject.Objects.All(x => { x.FixedUpdate(); return false; });
    }
    public class CustomDummyObject
    {
        public static List<CustomDummyObject> Objects = new();
        public List<Vector2> pos;
        public int index = -1;
        public PlayerControl Player;
        public CustomNetworkTransform Netrans;
        public void FixedUpdate()
        {
            index++;
            if (pos.Count <= index)
            {
                End();
                return;
            }
            Netrans.RpcSnapTo(pos[index]);
        }
        public void End()
        {
            Objects.Remove(this);
        }
    }
}