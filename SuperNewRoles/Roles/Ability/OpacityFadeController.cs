using System;
using System.Collections.Generic;
using UnityEngine;
using SuperNewRoles.Modules;

namespace SuperNewRoles.Roles.Ability;

internal sealed class OpacityFadeController
{
    public const float DefaultFadeSeconds = 0.2f;

    private readonly float _duration;
    private readonly Dictionary<byte, Coroutine> _fades = new();
    private readonly Dictionary<byte, float> _targets = new();

    public OpacityFadeController(float duration = DefaultFadeSeconds)
    {
        _duration = duration;
    }

    public void StopAll()
    {
        var hud = FastDestroyableSingleton<HudManager>.Instance;
        if (hud != null)
        {
            foreach (var fade in _fades.Values)
            {
                if (fade != null) hud.StopCoroutine(fade);
            }
        }
        _fades.Clear();
        _targets.Clear();
    }

    public void Apply(ExPlayerControl target, float targetOpacity, bool forceSnap = false)
    {
        if (target?.Player == null) return;
        byte playerId = target.PlayerId;

        if (_targets.TryGetValue(playerId, out var prevTarget) && Mathf.Abs(prevTarget - targetOpacity) < 0.001f)
        {
            if (forceSnap && (!_fades.TryGetValue(playerId, out var existing) || existing == null))
            {
                ModHelpers.SetOpacity(target.Player, targetOpacity);
            }
            return;
        }

        _targets[playerId] = targetOpacity;
        var hud = FastDestroyableSingleton<HudManager>.Instance;
        if (_fades.TryGetValue(playerId, out var active) && active != null && hud != null)
        {
            hud.StopCoroutine(active);
        }
        _fades.Remove(playerId);

        float startOpacity = GetCurrentOpacity(target.Player);
        if (hud == null || _duration <= 0f || Mathf.Abs(startOpacity - targetOpacity) < 0.01f)
        {
            ModHelpers.SetOpacity(target.Player, targetOpacity);
            return;
        }

        var fade = hud.StartCoroutine(Effects.Lerp(_duration, new Action<float>((p) =>
        {
            ModHelpers.SetOpacity(target.Player, Mathf.Lerp(startOpacity, targetOpacity, p));
            if (p >= 1f) _fades.Remove(playerId);
        })));
        _fades[playerId] = fade;
    }

    private static float GetCurrentOpacity(PlayerControl player)
    {
        try
        {
            var body = player?.cosmetics?.currentBodySprite?.BodySprite;
            if (body != null) return body.color.a;
        }
        catch { }
        return 1f;
    }
}
