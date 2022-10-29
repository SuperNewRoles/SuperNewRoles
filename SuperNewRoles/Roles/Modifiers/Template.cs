// Modifier.csのModifierData.allModTypes及びModifierTypeにも追加すること。

/*using HarmonyLib;
using UnityEngine;

namespace SuperNewRoles.Roles
{
    [HarmonyPatch]
    public class Template : ModifierBase<Template>
    {
        public Color32 color = new();

        public Template()
        {
            ModType = modId = ModifierType.Template;
        }

        public override void OnMeetingStart() { }
        public override void OnMeetingEnd() { }
        public override void FixedUpdate() { }
        public override void OnKill(PlayerControl target) { }
        public override void OnDeath(PlayerControl killer) { }
        public override void HandleDisconnect(PlayerControl player, DisconnectReasons reason) { }

        public override string ModifyNameText(string nameText)
        {
            return nameText + ModHelpers.Cs(color, " ♥");
        }

        public  override void ClearAndReload()
        {
            players = new();
        }
    }
}
*/