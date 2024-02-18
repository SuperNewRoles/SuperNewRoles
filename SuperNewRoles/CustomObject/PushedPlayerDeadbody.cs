using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public void Awake()
    {
    }
    public void Init(PlayerControl player, Pusher.PushTarget pushTarget)
    {
        Player = player;
        transform.position = Player.transform.position;
        if (!MapOption.MapOption.playerIcons.TryGetValue(player.PlayerId, out PoolablePlayer poolableBehaviour))
            throw new Exception("Failed to get poolableBehavior Icon");
        currentPoolableBehaviour = Instantiate(poolableBehaviour);
        currentPoolableBehaviour.gameObject.layer = 8;
        currentPoolableBehaviour.transform.SetParent(transform);
        currentPoolableBehaviour.transform.localPosition = Vector3.zero;
        currentPoolableBehaviour.transform.localScale = Vector3.one * 0.4f;
        currentPoolableBehaviour.gameObject.SetActive(true);
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
                if (AnimationTimer >= 0.75f)
                {
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
            Pusher.PushTarget.Right => new Vector3(13.5f, 0, 0),
            Pusher.PushTarget.Left => new Vector3(-13.5f, 0, 0),
            Pusher.PushTarget.Down => new Vector3(0, -9f, 0),
            _ => throw new Exception("PushedPlayerDeadbody: Invalid PushTarget")
        } * Time.deltaTime;
        transform.position += addposition;
    }
    private void HandleDown()
    {
        transform.position += new Vector3(0, -1.5f, 0) * Time.deltaTime;
        transform.localScale -= Vector3.one * 0.001f;
        rotate += 0.05f;
        if (rotate >= 360)
            rotate = 0;
        transform.Rotate(new(0, 0, rotate + (pushTarget == Pusher.PushTarget.Left ? 0 : -360)));
    }
    private void HandleDownAndFadeout()
    {
        HandleDown();
        transform.localScale -= Vector3.one * 0.01f;
        currentPoolableBehaviour.cosmetics.currentBodySprite.BodySprite.color = new(1, 1, 1, (0.75f - AnimationTimer) * 1.34f);
    }
}
