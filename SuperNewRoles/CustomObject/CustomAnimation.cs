using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SuperNewRoles.CustomObject;
public struct CustomAnimationOptions
{
    public bool PlayEndDestory { get; private set; }
    public Sprite[] Sprites { get; private set; }
    public float frameRate { get; private set; }
    public bool IsLoop { get; private set; }
    public CustomAnimationOptions()
    {
        Sprites = new Sprite[] { };
    }
    public CustomAnimationOptions(Sprite[] sprites, float framerate, bool loop = false, bool playenddestroy = false)
    {
        Sprites = sprites;
        frameRate = framerate;
        IsLoop = loop;
        PlayEndDestory = playenddestroy;
    }

}
public class CustomAnimation : MonoBehaviour
{
    public static List<CustomAnimation> CustomAnimations = new();

    public SpriteRenderer spriteRenderer;
    public bool Playing;
    public CustomAnimationOptions Options;
    private float UpdateTimer;
    private float updatetime;
    private int Index;
    public static Sprite[] GetSprites(string path, int Count)
    {
        List<Sprite> Sprites = new();
        for (int i = 1; i < Count + 1; i++)
        {
            string countdata = "000" + i.ToString();
            if (i >= 10)
            {
                countdata = i >= 100 ? "0" + i.ToString() : "00" + i.ToString();
            }
            Sprites.Add(ModHelpers.LoadSpriteFromResources(path + "_" + countdata + ".png", 110f));
        }
        return Sprites.ToArray();
    }
    public virtual void Awake()
    {
        spriteRenderer = gameObject.GetOrAddComponent<SpriteRenderer>();
        Playing = false;
        CustomAnimations.Add(this);
    }
    public void Init(CustomAnimationOptions option)
    {
        Options = option;
        spriteRenderer.sprite = option.Sprites[0];
        Index = 1;
        updatetime = 1 / Options.frameRate;
        UpdateTimer = updatetime;
        Playing = true;
    }
    public void Play()
    {
        Playing = true;
    }
    public void Pause()
    {
        Playing = false;
    }
    public void Stop()
    {
        Playing = false;
        spriteRenderer.sprite = Options.Sprites[0];
        Index = 1;
        UpdateTimer = updatetime;
    }
    public virtual void Update()
    {
        if (Playing && Options.Sprites.Length > 1)
        {
            UpdateTimer -= Time.deltaTime;
            if (UpdateTimer <= 0)
            {
                spriteRenderer.sprite = Options.Sprites[Index];
                Index++;
                if (Options.Sprites.Length <= Index)
                {
                    if (Options.IsLoop)
                        Index = 0;
                    else
                    {
                        if (Options.PlayEndDestory)
                            Destroy(this);
                        else
                            Playing = false;
                        return;
                    }
                }
                UpdateTimer = updatetime;
            }
        }
    }
    public virtual void OnDestroy()
    {
        CustomAnimations.Remove(this);
    }
}
