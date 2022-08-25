using System.Collections.Generic;

namespace SuperNewRoles.MapOptions
{
    public static class RandomMap
    {
        public static void Prefix()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            if (MapOption.IsRandomMap)
            {
                var rand = new System.Random();
                List<byte> RandomMaps = new();
                if (MapOption.ValidationSkeld) RandomMaps.Add(0);
                if (MapOption.ValidationMira) RandomMaps.Add(1);
                if (MapOption.ValidationPolus) RandomMaps.Add(2);
                if (MapOption.ValidationAirship) RandomMaps.Add(4);
                if (MapOption.ValidationSubmerged && SubmergedCompatibility.Loaded) RandomMaps.Add(5);
                if (RandomMaps.Count <= 0)
                {
                    return;
                }
                var MapsId = RandomMaps[rand.Next(RandomMaps.Count)];
                PlayerControl.GameOptions.MapId = MapsId;
                CachedPlayer.LocalPlayer.PlayerControl.RpcSyncSettings(PlayerControl.GameOptions);
            }
            return;
        }
    }
}