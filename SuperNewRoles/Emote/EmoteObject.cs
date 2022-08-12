using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using SuperNewRoles;
using SuperNewRoles.Emote;
using UnityEngine;

public class EmoteObject
{
    public static List<EmoteObject> EmoteObjects = new();
    public byte PlayerId = 0;
    public PlayerControl Player = null;
    public EmoteData Data = null;
    public SpriteRenderer Renderer = null;
    public int SpriteIndex = 0;
    public bool IsBig = false;
    public float UpdateTime = 0;
    public Coroutine Coro = null;
    public GameObject gameObject;
    public Transform transform => gameObject.transform;
    public static void Reset()
    {
        EmoteObjects = new();
    }
    public static void SetEmote(byte Player, int EmoteId)
    {
        EmoteObject EmoteObj = EmoteObjects.Find(x => x.PlayerId == Player);
        if (EmoteObj == null)
        {
            SuperNewRoles.Logger.Error($"{Player}のEmoteObjectsがnullでした", "SetEmoteStatic");
            return;
        }
        EmoteData EmoteDat = EmoteData.Datas.Find(x => x.Id == EmoteId);
        EmoteDat = EmoteData.Datas[0];
        if (EmoteDat == null)
        {
            EmoteObj.SetEmote(null);
            return;
        }
        EmoteObj.SetEmote(EmoteDat);
    }
    public void SetEmote(EmoteData data)
    {
        Renderer.enabled = data != null;
        Data = data;
        IsBig = false;
        Coro = null;
        if (Data != null)
        {
            UpdateTime = Data.UpdateTime;
            if (Data.Type == EmoteType.Simple)
            {
                //Coro = Player.StartCoroutine((Il2CppSystem.Collections.IEnumerator)ViewSimpleEmote());
                Renderer.sprite = Data.Sprites[0];
            }
        }
        SpriteIndex = 0;
        transform.localScale = new(1, 1, 1);
    }
    public EmoteObject(GameObject obj)
    {
        gameObject = obj;
        SuperNewRoles.Logger.Info("a");
        Player = transform.parent.parent.GetComponent<PlayerControl>();
        EmoteObjects.Add(this);
        SuperNewRoles.Logger.Info("b");
        if ((Renderer = gameObject.GetComponent<SpriteRenderer>()) == null)
        {
            SuperNewRoles.Logger.Info("c");
            Renderer = gameObject.AddComponent<SpriteRenderer>();
            SuperNewRoles.Logger.Info("d");
        }
        SuperNewRoles.Logger.Info("e");
        transform.localPosition = new(1, 1.5f,-1);
    }
    public void Update()
    {
        SuperNewRoles.Logger.Info("f");
        if (Data == null) return;
        SuperNewRoles.Logger.Info("通過aaaaaaaaaa");
        if (Data.Type == EmoteType.Simple)
        {
            if (IsBig)
            {
                transform.localScale += new Vector3(0.01f, 0.01f, 0f);
                if (transform.localScale.x > 3f)
                {
                    IsBig = false;
                }
            }
            else
            {
                transform.localScale -= new Vector3(0.01f, 0.01f, 0f);
                if (transform.localScale.x < 2.5f)
                {
                    IsBig = true;
                }
            }
        }
        else if (Data.Type == EmoteType.Animation)
        {
            UpdateTime -= Time.deltaTime;
            if (UpdateTime <= 0)
            {
                if (SpriteIndex >= Data.Sprites.Length)
                {
                    if (!Data.IsLoop)
                    {
                        SetEmote(null);
                        return;
                    }
                    SpriteIndex = 0;
                }
                Renderer.sprite = Data.Sprites[SpriteIndex];
                UpdateTime = Data.UpdateTime;
            }
        }
    }
    public IEnumerator ViewSimpleEmote()
    {
        yield return Effects.Slide2D(transform, new(), new(0.5f, 0.5f));
    }
    public void OnDestroy()
    {
        EmoteObjects.Remove(this);
        if (CachedPlayer.LocalPlayer == null || PlayerId == CachedPlayer.LocalPlayer.PlayerId)
        {
            EmoteObjects = new();
        }
    }

}