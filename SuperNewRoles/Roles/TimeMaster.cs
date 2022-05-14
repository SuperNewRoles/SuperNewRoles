

/*

// Time Master Rewind Time
timeMasterShieldButton = new CustomButton(
    () => {
        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.TimeMasterShield, Hazel.SendOption.Reliable, -1);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
        RPCProcedure.timeMasterShield();
    },
    () => { return TimeMaster.timeMaster != null && TimeMaster.timeMaster == PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.Data.IsDead; },
    () => { return PlayerControl.LocalPlayer.CanMove; },
    () => {
        timeMasterShieldButton.Timer = timeMasterShieldButton.MaxTimer;
        timeMasterShieldButton.isEffectActive = false;
        timeMasterShieldButton.actionButton.cooldownTimerText.color = Palette.EnabledColor;
    },
    TimeMaster.getButtonSprite(),
    new Vector3(-1.8f, -0.06f, 0),
    __instance,
    KeyCode.F,
    true,
    TimeMaster.shieldDuration,
    () => { timeMasterShieldButton.Timer = timeMasterShieldButton.MaxTimer; }
);

*/