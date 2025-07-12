using SuperNewRoles.Modules;
using SuperNewRoles.Roles.Ability.CustomButton;
using UnityEngine;

namespace SuperNewRoles.Roles.Ability
{
    internal class SilverBulletRepairAbility : CustomButtonBase
    {
        public override string buttonText => ModTranslation.GetString("SilverBulletRepairButtonName");
        public override Sprite Sprite => AssetManager.GetAsset<Sprite>("SilverBulletRepairButton.png");
        protected override KeyType keytype => KeyType.None;
        public override float DefaultTimer => cooltime;
        private float cooltime;
        private bool canUseRepair;

        public override ShowTextType showTextType => ShowTextType.ShowWithCount;

        public SilverBulletRepairAbility(bool canUseRepair, int count, float cooltime)
        {
            this.canUseRepair = canUseRepair;
            Count = count;
            this.cooltime = cooltime;
        }

        public override void OnClick()
        {
            if (Count <= 0 || !ModHelpers.IsSabotageAvailable()) return;

            Count--;

            // Placeholder for RPC call
            RpcRepair();
        }

        public override bool CheckIsAvailable()
        {
            return Count > 0 && Player.Player.CanMove && ModHelpers.IsSabotageAvailable();
        }

        public override bool CheckHasButton()
        {
            return canUseRepair && Player.IsAlive();
        }

        [CustomRPC]
        public void RpcRepair()
        {
            if (Player.Player.IsMushroomMixupActive())
            {
                ModHelpers.RpcFixingSabotage(TaskTypes.MushroomMixupSabotage);
                return;
            }
            foreach (PlayerTask task in PlayerControl.LocalPlayer.myTasks)
            {
                if (!ModHelpers.IsSabotage(task.TaskType))
                    continue;
                ModHelpers.RpcFixingSabotage(task.TaskType);
            }
        }
    }
}