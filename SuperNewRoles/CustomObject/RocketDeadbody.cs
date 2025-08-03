using System;
using UnityEngine;
using System.Linq;
using SuperNewRoles.Modules;

namespace SuperNewRoles.CustomObject;
public class RocketDeadbody : MonoBehaviour
{
    public RocketDeadbody(IntPtr intPtr) : base(intPtr)
    {
    }
    private Vector3 BasePos;
    private static Vector3 movepos = new(0, 0.1f, 0);
    private static float FireworksSize = 2;
    private bool IsFirework;
    private SpriteRenderer spriteRenderer;
    private float animationTimer;
    private float animationFrameRate = 30f;
    private int currentFrameIndex;
    private Sprite[] fireworkSprites;
    private AudioClip fireworkSound;
    private bool isPlayingFirework = false;
    private Sprite[] rocketSprites;
    private int currentRocketFrameIndex;

    public void Awake()
    {
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sharedMaterial = FastDestroyableSingleton<HatManager>.Instance.PlayerMaterial;
        spriteRenderer.maskInteraction = SpriteMaskInteraction.None;
        transform.localScale = Vector3.one * 0.45f;
        fireworkSprites = CustomPlayerAnimationSimple.GetSprites("fireworks__{0}.png", 1, 20, 1);
        fireworkSound = AssetManager.GetAsset<AudioClip>("RocketSound.wav");
        rocketSprites = CustomPlayerAnimationSimple.GetSprites("RocketPlayer_{0}.png", 1, 2, 3);
    }
    public void Init(PlayerControl Player, int index, int maxcount)
    {
        PlayerMaterial.SetMaskLayerBasedOnLocalPlayer(spriteRenderer, false);
        PlayerMaterial.SetColors(Player.Data.DefaultOutfit.ColorId, spriteRenderer);

        if (maxcount <= 1)
        {
            transform.position = new(Player.transform.position.x, Player.transform.position.y, -10);
        }
        else
        {
            float offset = 0.5f * ((index + 1) / 2);
            if (index % 2 == 0)
            {
                transform.position = new(Player.transform.position.x - offset, Player.transform.position.y, -10);
            }
            else
            {
                transform.position = new(Player.transform.position.x + offset, Player.transform.position.y, -10);
            }
        }
        BasePos = transform.position;
        IsFirework = false;
        isPlayingFirework = false;
        currentFrameIndex = 0;
        animationTimer = 0f;
        spriteRenderer.color = Color.white;
        currentRocketFrameIndex = 0;
        spriteRenderer.sprite = rocketSprites.FirstOrDefault();
    }
    public void Update()
    {
        if (!IsFirework)
        {
            if (rocketSprites != null && rocketSprites.Length > 1)
            {
                currentRocketFrameIndex = 1 - currentRocketFrameIndex;
                spriteRenderer.sprite = rocketSprites[currentRocketFrameIndex];
            }
            transform.position += movepos * Time.deltaTime * 60f;
            if ((transform.position - BasePos).y > 6f)
            {
                IsFirework = true;
                transform.localScale = Vector3.one * FireworksSize;
                spriteRenderer.sprite = fireworkSprites.FirstOrDefault();
                if (fireworkSound != null)
                {
                    AssetManager.PlaySoundFromBundle("RocketSound.wav", false);
                }
                isPlayingFirework = true;
                animationTimer = 0f;
                currentFrameIndex = 0;
            }
        }
        else if (isPlayingFirework)
        {
            animationTimer += Time.deltaTime;
            if (animationTimer >= 1f / animationFrameRate)
            {
                animationTimer -= 1f / animationFrameRate;
                currentFrameIndex++;
                if (currentFrameIndex >= fireworkSprites.Length)
                {
                    isPlayingFirework = false;
                    Destroy(gameObject);
                    return;
                }
                spriteRenderer.sprite = fireworkSprites[currentFrameIndex];
            }
        }
    }
}