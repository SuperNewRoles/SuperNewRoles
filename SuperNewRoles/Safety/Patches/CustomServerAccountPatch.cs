using System;
using System.Collections.Generic;
using HarmonyLib;
using InnerNet;
using UnityEngine;
using UnityEngine.Events;

namespace SuperNewRoles.Safety.Patches;

internal static class CustomServerFriendsListDisplay
{
    private sealed class OriginalState
    {
        public string FriendCodeText;
        public bool ToggleActive;
        public readonly List<TabState> Tabs = new();
        public GameObject ModeToggleObject;
        public GameObject AddFriendObjects;
        public bool AddFriendObjectsActive;
        public GameObject AddFriendArea;
        public bool AddFriendAreaActive;
    }

    private sealed class TabState
    {
        public GameObject Object;
        public Vector3 Position;
        public bool Active;
    }

    private static readonly Dictionary<IntPtr, OriginalState> OriginalStates = new();
    private static readonly string CustomLabel = UIConfig.ColorModName + " Custom";
    private static bool _outsideSnrMode = true;
    private static bool _changingMode;

    public static bool ShouldUseSnrMode()
    {
        if (!OfficialSnrServer.IsIdentityEnabled()) return false;
        return IsInLobby() || _outsideSnrMode;
    }

    private static bool IsInLobby()
    {
        return AmongUsClient.Instance != null
            && (AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Joined
                || AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started);
    }

    public static void Apply(FriendsListUI ui)
    {
        if (ui == null || ui.Pointer == IntPtr.Zero) return;
        GameObject friendsTab = FindDescendant(ui.transform, "AUFriendsTab")?.gameObject;
        GameObject blockedTab = FindDescendant(ui.transform, "BlockedPlayersTab")?.gameObject;
        if (ui.Tabs != null)
        {
            foreach (FriendsListUI.FriendsListTabButton tab in ui.Tabs)
            {
                if (tab == null) continue;
                if (tab.tab == FriendsListUI.FriendsListTab.AmongUsFriends && friendsTab == null)
                    friendsTab = tab.tabObject;
                if (tab.tab == FriendsListUI.FriendsListTab.Blocked && blockedTab == null)
                    blockedTab = tab.tabObject;
            }
        }

        bool officialServer = OfficialSnrServer.IsIdentityEnabled();
        bool snrMode = officialServer && ShouldUseSnrMode();
        if (officialServer)
        {
            if (!OriginalStates.TryGetValue(ui.Pointer, out OriginalState state))
            {
                state = new OriginalState
                {
                    FriendCodeText = ui.FriendCodeText?.text ?? string.Empty,
                    ToggleActive = ui.FriendCodeHideToggleObject != null && ui.FriendCodeHideToggleObject.activeSelf,
                    AddFriendObjects = ui.AddFriendObjects,
                    AddFriendObjectsActive = ui.AddFriendObjects != null && ui.AddFriendObjects.activeSelf,
                    AddFriendArea = ui.AddFriendArea?.gameObject,
                    AddFriendAreaActive = ui.AddFriendArea != null && ui.AddFriendArea.gameObject.activeSelf,
                };
                if (ui.Tabs != null)
                {
                    foreach (FriendsListUI.FriendsListTabButton tab in ui.Tabs)
                    {
                        if (tab?.tabObject == null) continue;
                        state.Tabs.Add(new TabState
                        {
                            Object = tab.tabObject,
                            Position = tab.tabObject.transform.localPosition,
                            Active = tab.tabObject.activeSelf,
                        });
                    }
                }
                AddFallbackTabState(state, friendsTab);
                AddFallbackTabState(state, blockedTab);
                OriginalStates[ui.Pointer] = state;
            }

            if (snrMode && ui.FriendCodeText != null)
            {
                ui.FriendCodeText.richText = true;
                ui.FriendCodeText.text = CustomLabel;
            }
            if (snrMode)
            {
                ui.FriendCodeHideToggleObject?.SetActive(false);
                if (state.AddFriendObjects != null)
                    state.AddFriendObjects.SetActive(false);
                if (state.AddFriendArea != null)
                    state.AddFriendArea.SetActive(false);
                HideAddFriendControls(ui);
                GameObject addFriendTab = FindTabObject(ui, FriendsListUI.FriendsListTab.AddFriend);
                // FriendsListTabButton.tabObject points to the tab's content root,
                // not the clickable tab header.  Restoring its active flag here
                // would undo OpenTab's selection (and leave every content pane
                // disabled after the redirect to Recent Players).  Keep the
                // original positions in SNR mode, but let vanilla OpenTab own
                // content activation.
                RestoreTabLayout(state, restoreActive: false);
                if (friendsTab != null)
                {
                    friendsTab.SetActive(false);
                }
                if (addFriendTab != null)
                    addFriendTab.SetActive(false);
                if (blockedTab != null && friendsTab != null)
                    blockedTab.transform.localPosition = friendsTab.transform.localPosition;

                // FriendsListUI opens on AU Friends by default.  That tab is hidden in SNR
                // mode, so move the content selection to a real SNR-backed tab as well.
                if (!_changingMode && !IsInLobby()
                    && (ui.CurrentTab == FriendsListUI.FriendsListTab.AmongUsFriends
                        || ui.CurrentTab == FriendsListUI.FriendsListTab.AddFriend))
                {
                    ui.OpenTab((int)FriendsListUI.FriendsListTab.RecentPlayers);
                }
            }
            else
            {
                RestoreVanillaLayout(ui, state);
            }

            ConfigureModeToggle(ui, state, !IsInLobby());
            return;
        }

        if (!OriginalStates.TryGetValue(ui.Pointer, out OriginalState original)) return;
        RestoreVanillaLayout(ui, original);
        RemoveModeToggle(original);
        OriginalStates.Remove(ui.Pointer);
    }

    private static void RestoreVanillaLayout(FriendsListUI ui, OriginalState state)
    {
        if (ui.FriendCodeText != null)
            ui.FriendCodeText.text = state.FriendCodeText;
        ui.FriendCodeHideToggleObject?.SetActive(state.ToggleActive);
        if (state.AddFriendObjects != null)
            state.AddFriendObjects.SetActive(state.AddFriendObjectsActive);
        if (state.AddFriendArea != null)
            state.AddFriendArea.SetActive(state.AddFriendAreaActive);
        RestoreTabLayout(state);
    }

    private static void ConfigureModeToggle(FriendsListUI ui, OriginalState state, bool visible)
    {
        if (ui.FriendCodeText == null) return;
        if (state.ModeToggleObject == null)
        {
            var toggle = new GameObject("SNRModeToggle");
            toggle.transform.SetParent(ui.FriendCodeText.transform, false);
            // PassiveButton uses the world-space 2D raycaster used by the game;
            // an object left on the default layer is not hit by the UI camera.
            // Keep the hitbox over the friend-code label and in front of its text.
            toggle.layer = ui.FriendCodeText.gameObject.layer == 0
                ? 5
                : ui.FriendCodeText.gameObject.layer;
            toggle.transform.localPosition = new Vector3(0f, 0f, -1f);
            toggle.transform.localScale = Vector3.one;
            var collider = toggle.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(4.2f, 0.7f);
            var button = toggle.AddComponent<PassiveButton>();
            button.Colliders = new Collider2D[] { collider };
            button.OnClick = new();
            button.OnMouseOver = new();
            button.OnMouseOut = new();
            button.OnClick.AddListener((UnityAction)(() =>
            {
                ToggleOutsideMode(ui);
            }));
            state.ModeToggleObject = toggle;
        }

        state.ModeToggleObject.SetActive(visible);
    }

    private static void ToggleOutsideMode(FriendsListUI ui)
    {
        if (_changingMode || ui == null || IsInLobby())
            return;

        _changingMode = true;
        try
        {
            _outsideSnrMode = !_outsideSnrMode;

            // Restore/hide the correct content roots first, then explicitly open
            // a pane that exists in the selected mode.  Relying on the previous
            // selected tab leaves a hidden AddFriend/AU Friends pane active and
            // makes the next click appear to do nothing.
            Apply(ui);
            FriendsListUI.FriendsListTab target = _outsideSnrMode
                ? FriendsListUI.FriendsListTab.RecentPlayers
                : FriendsListUI.FriendsListTab.AmongUsFriends;
            ui.OpenTab((int)target);

            if (_outsideSnrMode)
            {
                ui.RefreshRecentlyPlayed();
                ui.RefreshBlockedPlayers();
            }
            else
            {
                ui.RefreshFriends();
                ui.RefreshPlatformFriends();
                ui.RefreshBlockedPlayers();
            }

            Apply(ui);
        }
        catch (Exception error)
        {
            Logger.Error($"Failed to switch Friends List mode: {error}");
        }
        finally
        {
            _changingMode = false;
        }
    }

    private static void RemoveModeToggle(OriginalState state)
    {
        if (state.ModeToggleObject != null)
            UnityEngine.Object.Destroy(state.ModeToggleObject);
        state.ModeToggleObject = null;
    }

    private static void AddFallbackTabState(OriginalState state, GameObject tabObject)
    {
        if (tabObject == null || state.Tabs.Exists(tab => tab.Object == tabObject)) return;
        state.Tabs.Add(new TabState
        {
            Object = tabObject,
            Position = tabObject.transform.localPosition,
            Active = tabObject.activeSelf,
        });
    }

    private static void RestoreTabLayout(OriginalState state, bool restoreActive = true)
    {
        foreach (TabState tab in state.Tabs)
        {
            if (tab.Object == null) continue;
            tab.Object.transform.localPosition = tab.Position;
            if (restoreActive)
                tab.Object.SetActive(tab.Active);
        }
    }

    private static GameObject FindTabObject(FriendsListUI ui, FriendsListUI.FriendsListTab target)
    {
        if (ui?.Tabs == null) return null;
        foreach (FriendsListUI.FriendsListTabButton tab in ui.Tabs)
        {
            if (tab != null && tab.tab == target)
                return tab.tabObject;
        }
        return null;
    }

    private static void HideAddFriendControls(FriendsListUI ui)
    {
        if (ui == null) return;
        foreach (LobbyPlayerBar bar in ui.GetComponentsInChildren<LobbyPlayerBar>(true))
            HideAddFriendControl(bar);
    }

    // LobbyPlayerBar.AddFriendButton is the vanilla per-row add-friend control.
    // Apply() hides it on already-built bars; SetUp postfix covers pooled/refreshed rows.
    public static void HideAddFriendControl(LobbyPlayerBar bar)
    {
        if (bar == null || !ShouldUseSnrMode()) return;
        if (bar.AddFriendButton != null)
            bar.AddFriendButton.gameObject.SetActive(false);

        Transform named = FindDescendant(bar.transform, "AddFriendButton")
            ?? FindDescendant(bar.transform, "AddFriend");
        if (named != null)
            named.gameObject.SetActive(false);
    }

    private static Transform FindDescendant(Transform parent, string name)
    {
        if (parent == null) return null;
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (string.Equals(child.name, name, StringComparison.Ordinal))
                return child;
            Transform nested = FindDescendant(child, name);
            if (nested != null) return nested;
        }
        return null;
    }
}

[HarmonyPatch(typeof(FriendsListUI), nameof(FriendsListUI.Open))]
public static class FriendsListCustomServerOpenPatch
{
    public static void Postfix(FriendsListUI __instance)
    {
        CustomServerFriendsListDisplay.Apply(__instance);
        if (__instance?.CurrentTab == FriendsListUI.FriendsListTab.Blocked)
            FriendsListBlockedPlayersOverridePatch.OnBlockedTabOpened(__instance);
    }
}

[HarmonyPatch(typeof(FriendsListUI), nameof(FriendsListUI.UpdateFriendCodeUI))]
public static class FriendsListCustomServerFriendCodePatch
{
    public static void Postfix(FriendsListUI __instance) => CustomServerFriendsListDisplay.Apply(__instance);
}

[HarmonyPatch(typeof(FriendsListUI), nameof(FriendsListUI.OpenTab))]
public static class FriendsListCustomServerTabPatch
{
    public static void Postfix(FriendsListUI __instance)
    {
        CustomServerFriendsListDisplay.Apply(__instance);
        if (__instance?.CurrentTab == FriendsListUI.FriendsListTab.Blocked)
            FriendsListBlockedPlayersOverridePatch.OnBlockedTabOpened(__instance);
    }
}
