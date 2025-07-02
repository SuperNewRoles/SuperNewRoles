using UnityEngine;

namespace SuperNewRoles.Modules;

// Nebula on the Shipから!
public static class LayerExpansion
{
    private static int? _DefaultLayer;

    private static int? _ShortObjectsLayer;

    private static int? _ObjectsLayer;

    private static int? _PlayersLayer;

    private static int? _UiLayer;

    private static int? _ShipLayer;

    private static int? _ShadowLayer;

    private static int? _DrawShadowsLayer;

    public static int GetDefaultLayer() => (_DefaultLayer.HasValue ? _DefaultLayer : _DefaultLayer = LayerMask.NameToLayer("Default")).Value;

    public static int GetShortObjectsLayer() => (_ShortObjectsLayer.HasValue ? _ShortObjectsLayer : _ShortObjectsLayer = LayerMask.NameToLayer("ShortObjects")).Value;

    public static int GetObjectsLayer() => (_ObjectsLayer.HasValue ? _ObjectsLayer : _ObjectsLayer = LayerMask.NameToLayer("Objects")).Value;

    public static int GetPlayersLayer() => (_PlayersLayer.HasValue ? _PlayersLayer : _PlayersLayer = LayerMask.NameToLayer("Players")).Value;

    public static int GetUILayer() => (_UiLayer.HasValue ? _UiLayer : _UiLayer = LayerMask.NameToLayer("UI")).Value;

    public static int GetShipLayer() => (_ShipLayer.HasValue ? _ShipLayer : _ShipLayer = LayerMask.NameToLayer("Ship")).Value;

    public static int GetShadowLayer() => (_ShadowLayer.HasValue ? _ShadowLayer : _ShadowLayer = LayerMask.NameToLayer("Shadow")).Value;

    public static int GetDrawShadowsLayer() => (_DrawShadowsLayer.HasValue ? _DrawShadowsLayer : _DrawShadowsLayer = LayerMask.NameToLayer("DrawShadows")).Value;

    public static int GetShadowObjectsLayer() => 30;

    public static int GetArrowLayer() => 29;
}