using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agartha;
using SuperNewRoles.Roles.Impostor.Pusher;
using UnityEngine;

namespace SuperNewRoles.CustomObject;
public class PushedPlayerDeadbody : MonoBehaviour
{
    public PushedPlayerDeadbody(IntPtr intPtr) : base(intPtr)
    {
    }
    public enum PushAnimation
    {
        Push,
        Down,
        DownAndFadeout
    }
    public PoolablePlayer currentPoolableBehaviour { get; private set; }
    public PlayerControl Player { get; private set; }
    private float AnimationTimer;
    private PushAnimation pushAnimation;
    private Pusher.PushTarget pushTarget;
    private SpriteRenderer HandSlot;
    private DeadBody DeadBody;
    private Vector3 DeadBodyPosition;
    private float Speed = 1;
    public void Awake()
    {
    }
    public void Init(PlayerControl player, Pusher.PushTarget pushTarget, DeadBody deadBody, Vector3 deadbodyPosition)
    {
        Player = player;
        DeadBody = deadBody;
        DeadBodyPosition = deadbodyPosition;
        if (DeadBody != null)
            Speed = 1.5f;
        transform.position = Player.transform.position;
        if (!MapOption.MapOption.playerIcons.TryGetValue(player.PlayerId, out PoolablePlayer poolableBehaviour))
            throw new Exception("Failed to get poolableBehavior Icon");
        currentPoolableBehaviour = Instantiate(MapLoader.Airship.ExileCutscenePrefab.Player);//poolableBehaviour);
        currentPoolableBehaviour.gameObject.layer = 8;
        currentPoolableBehaviour.transform.SetParent(transform);
        currentPoolableBehaviour.SetBodyColor(player.CurrentOutfit.ColorId);
        currentPoolableBehaviour.transform.localPosition = Vector3.zero;
        currentPoolableBehaviour.transform.localScale = Vector3.one * 0.4f;
        currentPoolableBehaviour.gameObject.SetActive(true);
        HandSlot = currentPoolableBehaviour.transform.FindChild("HandSlot").GetComponent<SpriteRenderer>();
        PlayerMaterial.SetColors(player.CurrentOutfit.ColorId, HandSlot);
        //currentPoolableBehaviour.cosmetics.currentBodySprite.BodySprite.sprite = MapLoader.Airship.ExileCutscenePrefab.Player.transform.FindChild("BodyForms/Normal").GetComponent<SpriteRenderer>().sprite;
        AnimationTimer = 0f;
        pushAnimation = PushAnimation.Push;
        this.pushTarget = pushTarget;
    }
    public void Update()
    {
        if (currentPoolableBehaviour == null)
            return;
        switch (pushAnimation)
        {
            case PushAnimation.Push:
                HandlePush();
                AnimationTimer += Time.deltaTime;
                if (AnimationTimer > 0.03f)
                {
                    pushAnimation = PushAnimation.Down;
                    AnimationTimer = 0f;
                }
                break;
            case PushAnimation.Down:
                HandleDown();
                AnimationTimer += Time.deltaTime;
                if (AnimationTimer >= 0.2f)
                {
                    pushAnimation = PushAnimation.DownAndFadeout;
                    AnimationTimer = 0f;
                }
                break;
            case PushAnimation.DownAndFadeout:
                HandleDownAndFadeout();
                AnimationTimer += Time.deltaTime;
                if (DeadBody != null ? Vector2.Distance(transform.position, DeadBodyPosition) <= 0.25f : AnimationTimer >= 0.75f)
                {
                    if (DeadBody != null)
                    {
                        DeadBody.enabled = false;
                        DeadBody.enabled = true;
                        SpriteRenderer rend = DeadBody.transform.FindChild("Sprite").GetComponent<SpriteRenderer>();
                        rend.gameObject.SetActive(false);
                        rend.gameObject.SetActive(true);
                        DeadBody.transform.position = DeadBodyPosition;
                    }
                    Destroy(gameObject);
                }
                break;
            default:
                throw new Exception("PushedPlayerDeadbody: Invalid PushAnimation");
        }
    }
    private float rotate;
    private void HandlePush()
    {
        Vector3 addposition = pushTarget switch
        {
            Pusher.PushTarget.Right => new Vector3(16f, 0, 0),
            Pusher.PushTarget.Left => new Vector3(-16f, 0, 0),
            Pusher.PushTarget.Down => new Vector3(0, -9f, 0),
            _ => throw new Exception("PushedPlayerDeadbody: Invalid PushTarget")
        } * Time.deltaTime;
        transform.position += addposition;
    }
    private void HandleDown()
    {
        transform.position += new Vector3(0, -1.5f, 0) * Time.deltaTime * Speed;
        transform.localScale -= Vector3.one * 0.001f;
        rotate += 0.05f;
        if (rotate >= 360)
            rotate = 0;
        float rotated = 360 - rotate;
        if (pushTarget == Pusher.PushTarget.Left)
            rotated = rotate;
        transform.Rotate(new(0, 0, rotated));
    }
    private void HandleDownAndFadeout()
    {
        // 回しながら落とす
        HandleDown();
        if (DeadBody != null)
            return;
        transform.localScale -= Vector3.one * 0.01f;
        Color color = new(1, 1, 1, (0.75f - AnimationTimer) * 1.34f);
        // フェードアウト
        currentPoolableBehaviour.cosmetics.currentBodySprite.BodySprite.color = color;
        HandSlot.color = color;
    }
}