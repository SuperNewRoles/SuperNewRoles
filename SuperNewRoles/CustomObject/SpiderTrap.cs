using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hazel;
using SuperNewRoles.Helpers;
using SuperNewRoles.Roles;
using SuperNewRoles.Roles.Crewmate;
using UnityEngine;

namespace SuperNewRoles.CustomObject;
public class SpiderTrap : MonoBehaviour
{
    public static Dictionary<ushort, SpiderTrap> SpiderTraps;
    //キャッチされてる人、キャッチしてる人
    public static Dictionary<byte, byte> CatchingPlayers;
    public static ushort MaxId;
    private ushort Id;
    private PlayerControl Source;
    private byte SourceId;
    public PlayerControl CatchingPlayer { get; private set; }
    private byte CatchingPlayerId;
    private float ActivateTimer;
    public float DestroyTimer;
    private const float TrapCatchDistance = 0.9f;
    private Arrow arrow;
    private SpriteRenderer renderer;
    private bool Activated => ActivateTimer <= 0;
    public static void ClearAndReloads()
    {
        SpiderTraps = new();
        CatchingPlayers = new();
        MaxId = 0;
    }
    public static void Create(PlayerControl player, Vector2 pos, ushort id)
    {
        if (player == null)
            return;
        SpiderTraps[id] = new GameObject("SpiderTrap").AddComponent<SpiderTrap>();
        SpiderTraps[id].Init(player, pos, id);
    }
    public void Init(PlayerControl player, Vector2 pos, ushort id)
    {
        //設置者を設定
        Source = player;
        SourceId = player.PlayerId;
        //キャッチ中のプレイヤーを初期化
        CatchingPlayer = null;
        CatchingPlayerId = byte.MaxValue;
        //有効になるまでのタイマーをセット
        ActivateTimer = Roles.Impostor.Spider.CustomOptionData.SpiderButtonActivate.GetFloat();
        //自分がインポスターか判定して表示非表示を切り替える
        UpdateRendererSprite(PlayerControl.LocalPlayer.IsImpostor());
        //位置を設定
        transform.position = new(pos.x, pos.y, 0.095f);
        //サイズを設定
        transform.localScale = Vector3.one * 0.55f;
        //まだ無効なら半透明にする
        if (!Activated)
            renderer.color = new Color(1, 1, 1, 0.5f);
        //個別でIdをセット
        Id = id;
        if (Id > MaxId)
            MaxId = Id;
    }
    public void Awake()
    {
        renderer = gameObject.GetOrAddComponent<SpriteRenderer>();
    }
    private void UpdateRendererSprite()
        => UpdateRendererSprite(PlayerControl.LocalPlayer.IsImpostor() || CatchingPlayer != null);
    private void UpdateRendererSprite(bool active)
    {
        renderer.sprite = active ? ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.SpiderTrap.png", 115f) : null;
    }
    public void Update()
    {
        if (!Activated)
        {
            //まだ無効なら有効までの時間を減らしていく
            ActivateTimer -= Time.deltaTime;
            //今回のフレームで有効になった場合
            if (Activated)
            {
                renderer.color = new Color(1,1,1,1);
            }
        }
        else
        {
            //キャッチ済みか判定する
            if (CatchingPlayer == null) {
                //各視点で判定する
                if (PlayerControl.LocalPlayer.IsAlive() && !PlayerControl.LocalPlayer.IsImpostor())
                {
                    //自分視点でキャッチされるか判定する
                    if (Vector2.Distance(PlayerControl.LocalPlayer.transform.position, transform.position) <= TrapCatchDistance)
                    {
                        if (AmongUsClient.Instance.AmHost)
                        {
                            RPCProcedure.CheckSpiderTrapCatch(Id, PlayerControl.LocalPlayer.PlayerId);
                        }
                        else
                        {
                            MessageWriter writer = RPCHelper.StartRPC(CustomRPC.CheckSpiderTrapCatch);
                            writer.Write(Id);
                            writer.Write(PlayerControl.LocalPlayer.PlayerId);
                            writer.EndRPC();
                        }
                    }
                }
            }
            else
            {
                DestroyTimer -= Time.deltaTime;
                if (RoleClass.IsMeeting || DestroyTimer <= 0 || CatchingPlayer.IsDead())
                    Destroy(gameObject);
                //矢印のアップデート処理
                else if (arrow != null)
                    arrow.Update(CatchingPlayer.transform.position);
            }
        }
    }
    public void CatchPlayer(PlayerControl target)
    {
        CatchingPlayer = target;
        CatchingPlayerId = target.PlayerId;
        if (SourceId == PlayerControl.LocalPlayer.PlayerId)
        {
            //発光させる
            SeerHandler.ShowFlash(RoleClass.ImpostorRed);
            //矢印を出す処理を書く
            arrow = new(RoleClass.ImpostorRed);
            arrow.Update(CatchingPlayer.transform.position);
        }
        CatchingPlayers.Add(CatchingPlayerId, SourceId);
        UpdateRendererSprite(true);
        DestroyTimer = Roles.Impostor.Spider.CustomOptionData.SpiderSpentSetting.GetFloat();
        CatchingPlayer.transform.position = this.transform.position;
    }
    public void OnDestroy()
    {
        Logger.Info("SpiderTrapObjectDeleted");
        SpiderTraps.Remove(Id);
        CatchingPlayers.Remove(CatchingPlayerId);
        //矢印を破棄
        if (arrow != null)
        {
            Destroy(arrow.arrow);
            arrow = null;
        }
    }
}