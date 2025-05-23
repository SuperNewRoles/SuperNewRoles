using System;
using System.Collections.Generic;
using SuperNewRoles.Events;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace SuperNewRoles.Roles.Ability;

public class ShowPlayerUIAbility : AbilityBase
{
    private Func<List<ExPlayerControl>> _getPlayerList;
    private List<byte> _lastPlayerList = new();
    private GameObject _playerUIContainer;
    private List<(byte, GameObject)> _playerUIObjects = new();
    public ShowPlayerUIAbility(Func<List<ExPlayerControl>> getPlayerList)
    {
        _getPlayerList = getPlayerList;
    }
    private EventListener _fixedUpdateListener;
    public override void AttachToLocalPlayer()
    {
        _fixedUpdateListener = FixedUpdateEvent.Instance.AddListener(OnFixedUpdate);
        _lastPlayerList = new();
    }
    public override void DetachToLocalPlayer()
    {
        _fixedUpdateListener?.RemoveListener();
    }
    private void OnFixedUpdate()
    {
        if (FastDestroyableSingleton<HudManager>.Instance.IsIntroDisplayed || MeetingHud.Instance != null) return;
        var playerList = _getPlayerList();
        bool updated = false;
        if (playerList.Count == _lastPlayerList.Count)
        {
            for (int i = 0; i < playerList.Count; i++)
            {
                if (playerList[i] == null && _lastPlayerList[i] == byte.MaxValue) continue;
                if (playerList[i]?.PlayerId != _lastPlayerList[i])
                {
                    updated = true;
                    break;
                }
            }
        }
        else
            updated = true;

        if (!updated) return;
        if (_playerUIContainer != null)
        {
            GameObject.Destroy(_playerUIContainer);
        }

        _playerUIObjects.Clear();
        _playerUIContainer = new GameObject("PlayerUIContainer");
        _playerUIContainer.transform.SetParent(FastDestroyableSingleton<HudManager>.Instance.transform);
        _playerUIContainer.transform.localPosition = new(-4.19f, -2.4f, 0f);
        _playerUIContainer.transform.localScale = Vector3.one * 0.3f;
        var aspectPosition = _playerUIContainer.gameObject.AddComponent<AspectPosition>();
        aspectPosition.Alignment = AspectPosition.EdgeAlignments.LeftBottom;
        aspectPosition.DistanceFromEdge = new(0.35f, 0.35f);
        aspectPosition.OnEnable();
        var playerUIObjectPrefab = FastDestroyableSingleton<HudManager>.Instance.IntroPrefab.PlayerPrefab;
        int index = 0;
        foreach (var player in playerList)
        {
            if (player == null) continue;
            var playerUIObject = GameObject.Instantiate(playerUIObjectPrefab, _playerUIContainer.transform);
            _playerUIObjects.Add((player.PlayerId, playerUIObject.gameObject));
            playerUIObject.UpdateFromEitherPlayerDataOrCache(player.Data, PlayerOutfitType.Default, PlayerMaterial.MaskType.None, false);
            if (playerUIObject.cosmetics.colorBlindText != null)
                playerUIObject.cosmetics.colorBlindText.text = "";
            playerUIObject.transform.localPosition = new(index * 1.5f, 0f, -0.3f);
            playerUIObject.cosmetics.showColorBlindText = false;
            index++;
        }

        // _lastPlayerListを更新する
        _lastPlayerList.Clear();
        foreach (var player in playerList)
        {
            _lastPlayerList.Add(player?.PlayerId ?? byte.MaxValue);
        }
    }
}