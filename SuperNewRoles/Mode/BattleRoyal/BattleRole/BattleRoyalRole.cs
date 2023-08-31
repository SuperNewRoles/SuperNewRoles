using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AmongUs.GameOptions;

namespace SuperNewRoles.Mode.BattleRoyal.BattleRole
{
    public class BattleRoyalRole
    {
        public static List<BattleRoyalRole> BattleRoyalRoles = new();
        public PlayerControl CurrentPlayer;
        public int KillDistance = -1;
        public float Light = -1f;
        public float KillCoolTime = -1f;
        public virtual void FixedUpdate() { }
        public virtual void UseAbility(PlayerControl target) { }
        public static BattleRoyalRole GetObject(PlayerControl player)
        {
            return BattleRoyalRoles.FirstOrDefault(x => x.CurrentPlayer == player);
        }
        public BattleRoyalRole(PlayerControl player)
        {
            BattleRoyalRoles.Add(this);
            CurrentPlayer = player;
            if (KillDistance == -1) KillDistance = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.KillDistance);
            if (Light == -1f) Light = GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.ImpostorLightMod);
            if (KillCoolTime == -1f) KillCoolTime = GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown);
        }
        public BattleRoyalRole()
        {
            BattleRoyalRoles.Add(this);
            if (KillDistance == -1) KillDistance = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.KillDistance);
            if (Light == -1f) Light = GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.ImpostorLightMod);
            if (KillCoolTime == -1f) KillCoolTime = GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown);
        }
        public static void ClearAll()
        {
            BattleRoyalRoles = new();
        }
    }
}
