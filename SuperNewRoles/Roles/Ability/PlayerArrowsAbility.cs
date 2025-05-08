using System;
using System.Collections.Generic;
using System.Linq;
using SuperNewRoles.Events;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using UnityEngine;

namespace SuperNewRoles.Roles.Ability;

class PlayerArrowsAbility : AbilityBase
{
    private Func<IEnumerable<ExPlayerControl>> getPlayers;
    private Func<ExPlayerControl, Color32> getColor;

    // 新しい矢印管理用の変数
    private Dictionary<ExPlayerControl, Arrow> activeArrows = new();
    private Stack<Arrow> inactiveArrows = new();
    private List<Arrow> allCreatedArrows = new(); //生成されたすべての矢印を追跡
    private readonly List<ExPlayerControl> playersToRemoveCache = new();
    private readonly HashSet<ExPlayerControl> targetPlayersSetCache = new();

    private EventListener _fixedUpdateListener;

    public PlayerArrowsAbility(Func<IEnumerable<ExPlayerControl>> getPlayers, Func<ExPlayerControl, Color32> getColor)
    {
        this.getPlayers = getPlayers;
        this.getColor = getColor;
    }

    public override void AttachToLocalPlayer()
    {
        base.AttachToLocalPlayer();
        // プールと辞書の初期化
        activeArrows.Clear();
        inactiveArrows.Clear();
        allCreatedArrows.Clear();
        _fixedUpdateListener = FixedUpdateEvent.Instance.AddListener(OnFixedUpdate);
    }

    public override void DetachToLocalPlayer()
    {
        base.DetachToLocalPlayer();
        if (_fixedUpdateListener != null)
        {
            FixedUpdateEvent.Instance.RemoveListener(_fixedUpdateListener);
            _fixedUpdateListener = null;
        }

        foreach (var arrow in allCreatedArrows)
        {
            if (arrow != null && arrow.arrow != null) // ArrowとそのGameObjectがnullでないことを確認
            {
                GameObject.Destroy(arrow.arrow);
            }
        }
        allCreatedArrows.Clear();
        activeArrows.Clear();
        inactiveArrows.Clear();
    }

    private Arrow GetArrowFromPool()
    {
        if (inactiveArrows.Count > 0)
        {
            Arrow arrow = inactiveArrows.Pop();
            arrow.arrow.SetActive(true); // 再利用する際にアクティブ化
            return arrow;
        }
        else
        {
            var newArrow = new Arrow(Color.white); // Arrowのコンストラクタは既存のものを利用
            allCreatedArrows.Add(newArrow); // 生成した矢印を追跡リストに追加
            // newArrow.arrow はデフォルトでアクティブだと仮定
            return newArrow;
        }
    }

    private void ReturnArrowToPool(Arrow arrow)
    {
        if (arrow != null && arrow.arrow != null)
        {
            arrow.arrow.SetActive(false); // プールに戻す際に非アクティブ化
            inactiveArrows.Push(arrow);
        }
    }

    private void OnFixedUpdate()
    {
        IEnumerable<ExPlayerControl> currentTargetPlayers = getPlayers?.Invoke() ?? new List<ExPlayerControl>();
        // キャッシュしたセットをクリアして再利用
        targetPlayersSetCache.Clear();
        targetPlayersSetCache.UnionWith(currentTargetPlayers);
        // キャッシュしたリストをクリリングして再利用
        playersToRemoveCache.Clear();

        // 1. 不要になった矢印を activeArrows から inactiveArrows に戻す
        foreach (KeyValuePair<ExPlayerControl, Arrow> entry in activeArrows)
        {
            // ターゲットリストにいない、またはプレイヤーが無効/死亡している場合はプールに戻す
            if (!targetPlayersSetCache.Contains(entry.Key) || entry.Key == null || !entry.Key.Player.gameObject.activeInHierarchy)
            {
                ReturnArrowToPool(entry.Value);
                playersToRemoveCache.Add(entry.Key);
            }
        }
        foreach (var playerKey in playersToRemoveCache)
        {
            activeArrows.Remove(playerKey);
        }

        // 2. 各ターゲットプレイヤーに矢印を割り当てる/更新する
        foreach (PlayerControl targetPlayer in currentTargetPlayers)
        {
            if (activeArrows.TryGetValue(targetPlayer, out Arrow arrow))
            {
                // 既に矢印が割り当てられている場合は更新
                arrow.Update(targetPlayer.transform.position, getColor?.Invoke(targetPlayer));
            }
            else
            {
                // 新しい矢印を割り当て
                Arrow newArrow = GetArrowFromPool();
                newArrow.Update(targetPlayer.transform.position, getColor?.Invoke(targetPlayer));
                activeArrows.Add(targetPlayer, newArrow);
            }
        }
    }
}
