using System;
using SuperNewRoles.Buttons;
using SuperNewRoles.Patches;


namespace SuperNewRoles.Roles
{
    public class SuicidalIdeation
    {
        public static void Postfix()
        {
            if (!PlayerControl.LocalPlayer.IsAlive()) return;
            //ボタンのカウントが0になったら自殺する
            if (HudManagerStartPatch.SuicidalIdeationButton.Timer <= 0f) PlayerControl.LocalPlayer.RpcMurderPlayer(PlayerControl.LocalPlayer);
            //タスクを完了したかを検知
            var (playerCompleted, playerTotal) = TaskCount.TaskDate(PlayerControl.LocalPlayer.Data);
            if (RoleClass.SuicidalIdeation.CompletedTask <= playerCompleted)
            {
                RoleClass.SuicidalIdeation.CompletedTask += 1;
                HudManagerStartPatch.SuicidalIdeationButton.Timer += CustomOptions.SuicidalIdeationAddTimeLeft.GetFloat();
            }
        }
    }
}