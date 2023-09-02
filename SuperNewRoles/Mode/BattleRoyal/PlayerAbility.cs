using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AmongUs.GameOptions;

namespace SuperNewRoles.Mode.BattleRoyal
{
    public class PlayerAbility
    {
        public static List<PlayerAbility> PlayerAbilitys = new();
        public PlayerControl CurrentPlayer;
        public int KillDistance
        {
            get
            {
                int kds = MyKillDistance;
                foreach (int akd in AbilityKillDistance.Values) kds += akd;
                if (kds > 2) kds = 2;
                if (kds < 0) kds = 0;
                return kds;
            }
        }
        public int MyKillDistance;
        public Dictionary<string, int> AbilityKillDistance;
        public float Light
        {
            get
            {
                float lt = MyLight;
                foreach (float alt in AbilityLight.Values) lt += alt;
                return lt;
            }
        }
        public float MyLight;
        public Dictionary<string, float> AbilityLight;
        public float KillCoolTime
        {
            get
            {
                float kt = MyKillCoolTime;
                foreach (float akt in AbilityKillCoolTime.Values) kt += akt;
                if (kt <= 0) kt = 0.000001f;
                return kt;
            }
        }
        public float MyKillCoolTime;
        public Dictionary<string, float> AbilityKillCoolTime;
        public PlayerAbility(PlayerControl player)
        {
            CurrentPlayer = player;
            PlayerAbilitys.Add(this);
            MyKillDistance = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.KillDistance);
            AbilityKillDistance = new();
            MyLight = GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.ImpostorLightMod);
            AbilityLight = new();
            MyKillCoolTime = GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown);
            AbilityKillCoolTime = new();
        }
        public bool CanUseKill = true;
        public bool CanKill = true;
        public bool CanMove = true;
        public bool CanRevive = false;
        public static PlayerAbility GetPlayerAbility(PlayerControl player)
        {
            return PlayerAbilitys.FirstOrDefault(x => x.CurrentPlayer == player) ?? new(player);
        }
        public static void ClearAll()
        {
            PlayerAbilitys = new();
        }
    }
}