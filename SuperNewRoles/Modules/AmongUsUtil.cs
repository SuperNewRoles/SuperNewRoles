using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SuperNewRoles.Modules;

public static class AmongUsUtil
{
    public static MonoBehaviour CurrentCamTarget => DestroyableSingleton<HudManager>.Instance.PlayerCam.Target;
    public static bool IsCamTargetIsPlayer => CurrentCamTarget == PlayerControl.LocalPlayer;

    public static void SetCamTarget(MonoBehaviour target = null)
    {
        if (IsCamTargetIsPlayer) PlayerControl.LocalPlayer.NetTransform.Halt();
        DestroyableSingleton<HudManager>.Instance.PlayerCam.Target = target ?? PlayerControl.LocalPlayer;
    }

    public static void ToggleCamTarget(MonoBehaviour target1 = null, MonoBehaviour target2 = null)
    {
        if (target1 == null) target1 = PlayerControl.LocalPlayer;
        if (target2 == null) target2 = PlayerControl.LocalPlayer;
        SetCamTarget(CurrentCamTarget != target1 ? target1 : target2);
    }
}
