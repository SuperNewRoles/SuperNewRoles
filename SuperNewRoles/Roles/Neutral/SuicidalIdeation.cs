using System;
using SuperNewRoles.Buttons;
using SuperNewRoles.Patches;


namespace SuperNewRoles.Roles
{
    public class SuicidalIdeation
    {
        public static void Postfix()
        {
            if (!CachedPlayer.LocalPlayer.PlayerControl.IsAlive()) return;
            //ボタンのカウントが0になったら自殺する
            if (HudManagerStartPatch.SuicidalIdeationButton.Timer <= 0f) CachedPlayer.LocalPlayer.PlayerControl.RpcMurderPlayer(CachedPlayer.LocalPlayer.PlayerControl);
            //タスクを完了したかを検知
            var (playerCompleted, playerTotal) = TaskCount.TaskDate(CachedPlayer.LocalPlayer.PlayerControl.Data);
            if (RoleClass.SuicidalIdeation.CompletedTask <= playerCompleted)
            {
                RoleClass.SuicidalIdeation.CompletedTask += 1;
                HudManagerStartPatch.SuicidalIdeationButton.Timer += CustomOptionHolder.SuicidalIdeationAddTimeLeft.GetFloat();
            }
        }
    }
}