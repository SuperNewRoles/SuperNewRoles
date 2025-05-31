using System;
using System.Collections.Generic;
using SuperNewRoles.Events;
using SuperNewRoles.Modules;
using SuperNewRoles.Modules.Events.Bases;
using UnityEngine;

namespace SuperNewRoles.Roles.Ability;

public class ShowPlayerUIAbility : AbilityBase
{
    private Func<List<ExPlayerControl>> _getPlayerList;
    private List<byte> _lastPlayerList = new();
    private GameObject _playerUIContainer;
    private List<PoolablePlayer> _playerUIObjects = new();
    public ShowPlayerUIAbility(Func<List<ExPlayerControl>> getPlayerList)
    {
        _getPlayerList = getPlayerList;
    }
    private EventListener _fixedUpdateListener;
    public override void AttachToLocalPlayer()
    {
        _fixedUpdateListener = FixedUpdateEvent.Instance.AddListener(OnFixedUpdate);
        _lastPlayerList = new();

        _playerUIContainer = new GameObject("PlayerUIContainer");
        _playerUIContainer.transform.SetParent(FastDestroyableSingleton<HudManager>.Instance.transform);
        _playerUIContainer.transform.localPosition = new(-4.19f, -2.4f, 0f);
        _playerUIContainer.transform.localScale = Vector3.one * 0.3f;
        var aspectPosition = _playerUIContainer.gameObject.AddComponent<AspectPosition>();
        aspectPosition.Alignment = AspectPosition.EdgeAlignments.LeftBottom;
        aspectPosition.DistanceFromEdge = new(0.35f, 0.35f);
        aspectPosition.OnEnable();
    }
    public override void DetachToLocalPlayer()
    {
        _fixedUpdateListener?.RemoveListener();
        if (_playerUIContainer != null)
        {
            GameObject.Destroy(_playerUIContainer);
            _playerUIContainer = null;
        }
        _playerUIObjects.Clear();
    }
    private void OnFixedUpdate()
    {
        if (FastDestroyableSingleton<HudManager>.Instance.IsIntroDisplayed || MeetingHud.Instance != null)
        {
            if (_playerUIContainer != null && _playerUIContainer.activeSelf)
            {
                _playerUIContainer.SetActive(false);
            }
            return;
        }

        if (_playerUIContainer != null && !_playerUIContainer.activeSelf)
        {
            _playerUIContainer.SetActive(true);
        }

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
        {
            updated = true;
        }

        if (!updated) return;

        var playerUIObjectPrefab = FastDestroyableSingleton<HudManager>.Instance.IntroPrefab.PlayerPrefab;

        for (int i = 0; i < playerList.Count; i++)
        {
            if (i < _playerUIObjects.Count)
            {
                _playerUIObjects[i].gameObject.SetActive(true);
            }
            else
            {
                var playerUIObject = GameObject.Instantiate(playerUIObjectPrefab, _playerUIContainer.transform);
                _playerUIObjects.Add(playerUIObject);
                playerUIObject.gameObject.SetActive(true);
            }
            var player = playerList[i];
            var uiObject = _playerUIObjects[i];
            if (player != null)
            {
                uiObject.UpdateFromEitherPlayerDataOrCache(player.Data, PlayerOutfitType.Default, PlayerMaterial.MaskType.None, false);
                if (uiObject.cosmetics.colorBlindText != null)
                    uiObject.cosmetics.colorBlindText.text = "";
                uiObject.transform.localPosition = new(i * 1.5f, 0f, -0.3f);
                uiObject.cosmetics.showColorBlindText = false;
                uiObject.cosmetics.isNameVisible = false;
                uiObject.cosmetics.UpdateNameVisibility();
            }
            else
            {
                uiObject.gameObject.SetActive(false);
            }
        }

        for (int i = playerList.Count; i < _playerUIObjects.Count; i++)
        {
            _playerUIObjects[i].gameObject.SetActive(false);
        }

        _lastPlayerList.Clear();
        foreach (var player in playerList)
        {
            _lastPlayerList.Add(player?.PlayerId ?? byte.MaxValue);
        }
    }
}