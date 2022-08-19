using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx.IL2CPP.Utils;
using UnityEngine;

namespace SuperNewRoles
{
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
        }
        public static void Start2()
        {
            UpdateTime = UpdateTimeD;
            Is2 = true;
            if (players.Count < posdatas.Count)
            {
                players = new();
                for (int i = 0; i < posdatas.Count; i++)
                {
                    players.Add(BotManager.Spawn());
                }
            }
            index = new();
            for (int i = 0; i < posdatas.Count; i++)
            {
                index[i] = -1;
            }
        }
        public static void End()
        {
            Is = false;
            var text = "new(){";
            foreach (Vector2 pos in poss)
            {
                text += $"new({pos.x}f,{pos.y}f),"; 
            }
            text += "};";
            Logger.Info(text);
            poss = new();
        }
        static Dictionary<int,int> index = new();
        static float UpdateTime;
        const float UpdateTimeD = 0.003f;
        static List<PlayerControl> players = new();
        static List<List<Vector2>> posdatas = new()
        {
            new() { new(-4.344214f, 2.840216f), new(-4.344214f, 2.840216f), new(-4.344214f, 2.840216f), new(-4.344214f, 2.840216f), new(-4.344214f, 2.745461f), new(-4.344214f, 2.639773f), new(-4.344214f, 2.531133f), new(-4.344214f, 2.428483f), new(-4.344214f, 2.324161f), new(-4.344214f, 2.216831f), new(-4.344214f, 2.112052f), new(-4.344214f, 2.060515f), new(-4.344214f, 1.951656f), new(-4.344214f, 1.844823f), new(-4.344214f, 1.738067f), new(-4.344214f, 1.630475f), new(-4.344214f, 1.526146f), new(-4.344214f, 1.419274f), new(-4.344214f, 1.314655f), new(-4.344214f, 1.209486f), new(-4.344214f, 1.106269f), new(-4.344214f, 1.002473f), new(-4.344214f, 0.8973289f), new(-4.344214f, 0.7917577f), new(-4.344214f, 0.6854982f), new(-4.344214f, 0.5791955f), new(-4.344214f, 0.4745152f), new(-4.344214f, 0.370977f), new(-4.344214f, 0.2666467f), new(-4.344214f, 0.1603489f), new(-4.344214f, 0.05531891f), new(-4.344214f, -0.04934609f), new(-4.344214f, -0.1555264f), new(-4.344214f, -0.2579039f), new(-4.344214f, -0.364772f), new(-4.344214f, -0.4715045f), new(-4.344214f, -0.5751343f), new(-4.344214f, -0.6811239f), new(-4.284565f, -0.7610957f), new(-4.209959f, -0.8357022f), new(-4.135451f, -0.9102094f), new(-4.06121f, -0.9844503f), new(-3.987f, -1.058661f), new(-3.911432f, -1.134228f), new(-3.836971f, -1.208689f), new(-3.761875f, -1.283786f), new(-3.688854f, -1.356808f), new(-3.6154f, -1.430262f), new(-3.511999f, -1.438016f), new(-3.405343f, -1.438016f), new(-3.299074f, -1.438016f), new(-3.194645f, -1.438016f), new(-3.090412f, -1.438016f), new(-2.984574f, -1.438016f), new(-2.934769f, -1.438016f), new(-2.77565f, -1.438016f), new(-2.722393f, -1.438016f), new(-2.612913f, -1.438016f), new(-2.507066f, -1.438016f), new(-2.403539f, -1.438016f), new(-2.298379f, -1.438016f), new(-2.190925f, -1.438016f), new(-2.087638f, -1.438016f), new(-1.981588f, -1.438016f), new(-1.874658f, -1.438016f), new(-1.772117f, -1.438016f), new(-1.66303f, -1.438016f), new(-1.560428f, -1.438016f), new(-1.455927f, -1.438016f), new(-1.347889f, -1.438016f), new(-1.245341f, -1.438016f), new(-1.139988f, -1.438016f), new(-1.032015f, -1.438016f), new(-0.9301834f, -1.438016f), new(-0.8229243f, -1.438016f), new(-0.7191477f, -1.438016f), new(-0.6149949f, -1.438016f), new(-0.5056677f, -1.438015f), new(-0.4593911f, -1.438015f), new(-0.3288277f, -1.438016f), new(-0.2067908f, -1.438016f), new(-0.09769958f, -1.438016f), new(0.005959788f, -1.438016f), new(0.1128732f, -1.438016f), new(0.2155082f, -1.438016f), new(0.3201879f, -1.438016f), new(0.4293892f, -1.438016f), new(0.5319779f, -1.438016f), new(0.635712f, -1.438016f), new(0.7428901f, -1.438016f), new(0.8482404f, -1.438016f), new(0.9538654f, -1.438016f), new(1.058748f, -1.438016f), new(1.164507f, -1.438016f), new(1.272803f, -1.438016f), },
            new() { new(-4.44427f, 3.305017f), new(-4.44427f, 3.305017f), new(-4.44427f, 3.305017f), new(-4.44427f, 3.221534f), new(-4.44427f, 3.116783f), new(-4.44427f, 3.011226f), new(-4.44427f, 2.904977f), new(-4.44427f, 2.798829f), new(-4.44427f, 2.692946f), new(-4.44427f, 2.589129f), new(-4.44427f, 2.483631f), new(-4.44427f, 2.434546f), new(-4.44427f, 2.277829f), new(-4.44427f, 2.169846f), new(-4.44427f, 2.065952f), new(-4.44427f, 1.961726f), new(-4.44427f, 1.856372f), new(-4.44427f, 1.751546f), new(-4.44427f, 1.647861f), new(-4.44427f, 1.543679f), new(-4.44427f, 1.439304f), new(-4.44427f, 1.334229f), new(-4.44427f, 1.229449f), new(-4.44427f, 1.124646f), new(-4.44427f, 1.016157f), new(-4.44427f, 0.9144084f), new(-4.44427f, 0.80864f), new(-4.44427f, 0.7031074f), new(-4.44427f, 0.5982735f), new(-4.44427f, 0.4938922f), new(-4.44427f, 0.4440109f), new(-4.44427f, 0.2849368f), new(-4.44427f, 0.2321069f), new(-4.44427f, 0.1247581f), new(-4.370615f, 0.002200114f), new(-4.333563f, -0.03485228f), new(-4.256318f, -0.1120968f), new(-4.180328f, -0.1880867f), new(-4.106884f, -0.2615308f), new(-4.032839f, -0.3355758f), new(-3.957324f, -0.4110904f), new(-3.882736f, -0.4856785f), new(-3.807497f, -0.5609178f), new(-3.731867f, -0.6365477f), new(-3.65705f, -0.711365f), new(-3.58205f, -0.786365f), new(-3.506883f, -0.8615324f), new(-3.431423f, -0.9369923f), new(-3.35706f, -1.011356f), new(-3.280423f, -1.087993f), new(-3.207375f, -1.161041f), new(-3.133159f, -1.235257f), new(-3.059218f, -1.309198f), new(-2.984518f, -1.383898f), new(-2.885867f, -1.397284f), new(-2.780423f, -1.397284f), new(-2.674924f, -1.397284f), new(-2.570468f, -1.397284f), new(-2.464512f, -1.397284f), new(-2.359599f, -1.397284f), new(-2.253951f, -1.397284f), new(-2.150119f, -1.397284f), new(-2.045674f, -1.397284f), new(-1.939826f, -1.397284f), new(-1.832494f, -1.397284f), new(-1.728317f, -1.397284f), new(-1.623539f, -1.397284f), new(-1.519212f, -1.397284f), new(-1.415167f, -1.397284f), new(-1.308414f, -1.397284f), new(-1.203691f, -1.397284f), new(-1.09788f, -1.397284f), new(-0.9927157f, -1.397284f), new(-0.8892629f, -1.397284f), new(-0.7849291f, -1.397284f), new(-0.735141f, -1.397284f), new(-0.577891f, -1.397284f), new(-0.5256125f, -1.397284f), new(-0.3680035f, -1.397284f), new(-0.314835f, -1.397284f), },
        };
        public static void FixedUpdate()
        {
            if (Is)
            {
                UpdateTime -= Time.fixedDeltaTime;
                if (UpdateTime <= 0)
                {
                    UpdateTime = UpdateTimeD;
                    poss.Add(CachedPlayer.LocalPlayer.transform.position);
                }
            }
            if (Is2)
            {
                UpdateTime -= Time.fixedDeltaTime;
                if (UpdateTime <= 0)
                {
                    UpdateTime = UpdateTimeD;
                    for (int i = 0; i < posdatas.Count; i++)
                    {
                        int playerindex = i;
                        index[playerindex]++;
                        if (posdatas[playerindex].Count <= index[playerindex])
                        {
                            index[playerindex] = -1;
                            Is2 = false;
                            return;
                        }
                        players[playerindex].NetTransform.RpcSnapTo(posdatas[playerindex][index[playerindex]]);
                        AmongUsClient.Instance.StartCoroutine(players[playerindex].MyPhysics.WalkPlayerTo(new()));
                        /*
                        if (index[playerindex] != 0)
                        {
                            players[playerindex].cosmetics.currentBodySprite.BodySprite.transform.localScale = posdatas[playerindex][index[playerindex]].x > posdatas[playerindex][index[playerindex] - 1].x ? new(-0.5f, 0.5f, 1) : new(0.5f, 0.5f, 1);
                            players[playerindex].cosmetics.currentBodySprite.BodySprite.transform.FindChild("Hat").localScale = posdatas[playerindex][index[playerindex]].x > posdatas[playerindex][index[playerindex] - 1].x ? new(-1f, 1f, 1) : new(1f, 1f, 1);
                            players[playerindex].cosmetics.currentBodySprite.BodySprite.transform.FindChild("Visor").localScale = posdatas[playerindex][index[playerindex]].x > posdatas[playerindex][index[playerindex] - 1].x ? new(-1f, 1f, 1) : new(1f, 1f, 1);
                            players[playerindex].cosmetics.currentBodySprite.BodySprite.transform.FindChild("NameText_TMP").localScale = posdatas[playerindex][index[playerindex]].x > posdatas[playerindex][index[playerindex] - 1].x ? new(-2.8f, 2.8f, 1) : new(2.8f, 2.8f, 1);
                        }*/
                    }
                }
                /*
                var poshreh = CachedPlayer.LocalPlayer.transform.position;
                poshreh.x = posdatas[index].x;
                poshreh.y = posdatas[index].y;
                CachedPlayer.LocalPlayer.transform.position = poshreh;*/
            }
            CustomDummyObject.Objects.All(x => { x.FixedUpdate();return false; });
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
}
