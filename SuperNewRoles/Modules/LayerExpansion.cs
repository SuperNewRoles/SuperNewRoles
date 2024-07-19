using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SuperNewRoles.Modules;

// Nebula on the Shipから!
public static class LayerExpansion
{
    private static int? DefaultLayer;

    private static int? ShortObjectsLayer;

    private static int? ObjectsLayer;

    private static int? PlayersLayer;

    private static int? UiLayer;

    private static int? ShipLayer;

    private static int? ShadowLayer;

    private static int? DrawShadowsLayer;

    public static int GetDefaultLayer() => (DefaultLayer.HasValue ? DefaultLayer : DefaultLayer = LayerMask.NameToLayer("Default")).Value;

    public static int GetShortObjectsLayer() => (ShortObjectsLayer.HasValue ? ShortObjectsLayer : ShortObjectsLayer = LayerMask.NameToLayer("ShortObjects")).Value;

    public static int GetObjectsLayer() => (ObjectsLayer.HasValue ? ObjectsLayer : ObjectsLayer = LayerMask.NameToLayer("Objects")).Value;

    public static int GetPlayersLayer() => (PlayersLayer.HasValue ? PlayersLayer : PlayersLayer = LayerMask.NameToLayer("Players")).Value;

    public static int GetUILayer() => (UiLayer.HasValue ? UiLayer : UiLayer = LayerMask.NameToLayer("UI")).Value;

    public static int GetShipLayer() => (ShipLayer.HasValue ? ShipLayer : ShipLayer = LayerMask.NameToLayer("Ship")).Value;

    public static int GetShadowLayer() => (ShadowLayer.HasValue ? ShadowLayer : ShadowLayer = LayerMask.NameToLayer("Shadow")).Value;

    public static int GetDrawShadowsLayer() => (DrawShadowsLayer.HasValue ? DrawShadowsLayer : DrawShadowsLayer = LayerMask.NameToLayer("DrawShadows")).Value;

    public static int GetShadowObjectsLayer() => 30;

    public static int GetArrowLayer() => 29;
}
