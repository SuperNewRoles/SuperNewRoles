using System.Collections.Generic;
using SuperNewRoles.Modules;
using UnityEngine;

namespace SuperNewRoles.CustomObject;

public class SluggerDeadbody : MonoBehaviour
{
    public static List<SluggerDeadbody> DeadBodies = new();
    public PlayerControl Source { get; private set; }
    public PlayerControl Target { get; private set; }
    public Rigidbody2D Body { get; private set; }
    public Vector2 Velocity { get; private set; }
    public int SpriteType { get; private set; }
    public SpriteRenderer SpriteRenderer { get; private set; }

    private Sprite[] sprites;
    private int frame;
    private float timer;
    private const float frameRate = 10f;

    public static void Spawn(PlayerControl source, PlayerControl target)
    {
        var go = new GameObject("SluggerDeadbody");
        var deadbody = go.AddComponent<SluggerDeadbody>();
        deadbody.Init(source, target);
    }

    public void Init(PlayerControl source, PlayerControl target)
    {
        Source = source;
        Target = target;
        SpriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        Body = gameObject.AddComponent<Rigidbody2D>();
        Body.gravityScale = 0;
        Sprite[] sprites = GetSprites();
        SpriteRenderer.sprite = sprites[0];
        Velocity = (Source.transform.position - Target.transform.position) * -10f;
        Body.velocity = Velocity;
        transform.position = Target.transform.position;
        transform.localScale = new(0.1f, 0.1f, 0);
        DeadBodies.Add(this);
        this.sprites = sprites;
        this.frame = 0;
        this.timer = 0f;
    }

    public Sprite[] GetSprites()
    {
        // 0,1,2: 8枚アニメ、3,4: 1枚絵
        int type = UnityEngine.Random.Range(0, 5);
        SpriteType = type;
        if (type <= 2)
        {
            Sprite[] arr = new Sprite[8];
            for (int i = 1; i <= 8; i++)
                arr[i - 1] = AssetManager.GetAsset<Sprite>($"deadbody_{type}_{i:000}.png");
            return arr;
        }
        else
        {
            return new Sprite[] { AssetManager.GetAsset<Sprite>($"deadbody_{type}.png") };
        }
    }

    void Update()
    {
        // 画面外で自動消去
        if (Vector2.Distance(PlayerControl.LocalPlayer.transform.position, transform.position) > 30)
            Destroy(gameObject);
        // アニメーション再生
        if (sprites != null && sprites.Length > 1)
        {
            timer += Time.deltaTime;
            if (timer >= 1f / frameRate)
            {
                timer = 0f;
                frame++;
                if (frame < sprites.Length)
                {
                    SpriteRenderer.sprite = sprites[frame];
                }
            }
        }
    }

    void OnDestroy()
    {
        DeadBodies.Remove(this);
    }
}