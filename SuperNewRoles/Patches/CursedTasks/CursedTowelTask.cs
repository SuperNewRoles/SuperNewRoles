using System.Collections.Generic;
using System.Linq;
using Agartha;
using HarmonyLib;
using Il2CppSystem.Text;
using UnityEngine;

namespace SuperNewRoles.Patches.CursedTasks;

public class CursedTowelTask
{
    [HarmonyPatch(typeof(TowelMinigame))]
    public static class TowelMinigamePatch
    {
        [HarmonyPatch(nameof(TowelMinigame.Begin)), HarmonyPostfix]
        public static void BeginPostfix(TowelMinigame __instance)
        {
            if (!Main.IsCursed) return;
            Collider2D copytowel = Object.Instantiate(__instance.Towels[0]);
            copytowel.gameObject.SetActive(false);
            Vector3 scale = copytowel.transform.localScale;
            scale.x -= 0.1f;
            scale.y -= 0.1f;
            copytowel.transform.localScale = scale;
            foreach (Collider2D towel in __instance.Towels)
                Object.Destroy(towel.gameObject);
            __instance.Towels = new Collider2D[25];
            for (int i = 0; i < 25;  i++)
            {
                Collider2D towel = Object.Instantiate(copytowel);
                towel.transform.SetParent(__instance.transform);
                towel.transform.position = __instance.transform.position + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f));
                towel.name = $"towel_towel(Clone) {i}";
                __instance.Towels[i] = towel;
                towel.gameObject.SetActive(true);
            }
            Object.Destroy(copytowel.gameObject);
        }
    }

    [HarmonyPatch(typeof(TowelTaskConsole))]
    public static class TowelTaskConsolePatch
    {
        [HarmonyPatch(nameof(TowelTaskConsole.AfterUse)), HarmonyPrefix]
        public static bool AfterUsePrefix(TowelTaskConsole __instance, NormalPlayerTask task)
        {
            if (!Main.IsCursed) return true;
            if (__instance.useSound && Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(__instance.useSound, false, 1f, null);
            task.Data[task.Data.IndexOf((byte)__instance.ConsoleId)] = 254;
            return false;
        }
    }

    [HarmonyPatch(typeof(TowelTask))]
    public static class TowelTaskPatch
    {
        [HarmonyPatch(nameof(TowelTask.ValidConsole)), HarmonyPrefix]
        public static bool ValidConsolePrefix(TowelTask __instance, ref bool __result, Console console)
        {
            if (!Main.IsCursed) return true;
            if (__instance.TaskType == TaskTypes.PickUpTowels && console.TaskTypes.Contains(TaskTypes.PickUpTowels) &&
                (__instance.Data.IndexOf((byte)console.ConsoleId) != -1 ||
                (__instance.Data.All((byte b) => b == 254) && console.ConsoleId == 255)))
            {
                __result = true;
                return false;
            }
            __result = false;
            return false;
        }

        [HarmonyPatch(nameof(TowelTask.AppendTaskText)), HarmonyPrefix]
        public static bool AppendTaskTextPrefix(TowelTask __instance, StringBuilder sb)
        {
            if (!Main.IsCursed) return true;
            int num = __instance.Data.Count((byte b) => b == 254);
            if (num > 0)
            {
                if (__instance.IsComplete) sb.Append("<color=#00DD00FF>");
                else sb.Append("<color=#FFFF00FF>");
            }
            sb.Append(DestroyableSingleton<TranslationController>.Instance.GetString(__instance.StartAt));
            sb.Append(": ");
            sb.Append(DestroyableSingleton<TranslationController>.Instance.GetString(__instance.TaskType));
            if (num < __instance.Data.Length) sb.Append($" ({num}/{__instance.Data.Length})");
            if (num > 0) sb.Append("</color>");
            sb.AppendLine();
            return false;
        }
    }

    [HarmonyPatch(typeof(NormalPlayerTask))]
    public static class NormalPlayerTaskPatch
    {
        [HarmonyPatch(nameof(NormalPlayerTask.Initialize)), HarmonyPostfix]
        public static void InitializePostfix(NormalPlayerTask __instance)
        {
            if (!Main.IsCursed) return;
            if (__instance.TaskType != TaskTypes.PickUpTowels) return;
            __instance.Data = new byte[25];
            List<int> number = Enumerable.Range(0, 100).ToList();
            number.Shuffle(0);
            for (int i = 0; i < __instance.Data.Length; i++)
                __instance.Data[i] = (byte)number[i];
        }
    }

    [HarmonyPatch(typeof(ShipStatus))]
    public static class ShipStatusPatch
    {
        public static List<Sprite> Sprites;

        public static readonly List<TowelData> Data = new()
        {
            // コックピット : 5
            new(SystemTypes.Cockpit, new(-18.47035f, 0.5832333f, 0.1f), TowelType.flat),
            new(SystemTypes.Cockpit, new(-19.0786f, -0.757f, -0.001f), TowelType.bunched),
            new(SystemTypes.Cockpit, new(-23.59438f, 0.5832333f, 0.01f), TowelType.bunched),
            new(SystemTypes.Cockpit, new(-16.86567f, -2.572975f, -0.0029f), TowelType.bunched),
            new(SystemTypes.Cockpit, new(-24.38003f, -2.161964f, -0.0025f), TowelType.bunched),

            // 展望デッキ : 5
            new(SystemTypes.ViewingDeck, new(-16.0932f, -12.71112f, 0.1f), TowelType.flat),
            new(SystemTypes.ViewingDeck, new(-10.80271f, -12.48994f, 0.1f), TowelType.flat, flipX : true),
            new(SystemTypes.ViewingDeck, new(-14.17303f, -15.88182f, 0.1f), TowelType.hang),
            new(SystemTypes.ViewingDeck, new(-13.61654f, -16.62689f, 0.1f), TowelType.hang, flipX : true),
            new(SystemTypes.ViewingDeck, new(-12.91979f, -11.00276f, 0.1f), TowelType.bunched),

            // 武器庫 : 6
            new(SystemTypes.Armory, new(-9.642434f, -7.265387f, 0.1f), TowelType.flat),
            new(SystemTypes.Armory, new(-13.70116f, -9.720105f, 0.1f), TowelType.wet),
            new(SystemTypes.Armory, new(-10.878f, -3.962051f, -0.00355f), TowelType.wet),
            new(SystemTypes.Armory, new(-13.81397f, -4.858333f, 0.1f), TowelType.wet),
            new(SystemTypes.Armory, new(-11.02336f, -8.918399f, -0.0085f), TowelType.wet, flipX : true),
            new(SystemTypes.Armory, new(-10.01964f, -8.987483f, -0.0085f), TowelType.hang, flipX : true),

            // 通信室 : 3
            new(SystemTypes.Comms, new(-14.27815f, 1.072299f, 0.1f), TowelType.flat),
            new(SystemTypes.Comms, new(-11.96049f, 2.050555f, 0.1f), TowelType.wet),
            new(SystemTypes.Comms, new(-14.05836f, 2.399576f, 0.1f), TowelType.bunched),

            // 金庫室 : 10
            new(SystemTypes.VaultRoom, new(-12.56722f, 9.645317f, 0.1f), TowelType.flat),
            new(SystemTypes.VaultRoom, new(-5.44001f, 8.358071f, 0.1f), TowelType.flat),
            new(SystemTypes.VaultRoom, new(-6.57983f, 6.634166f, 0.1f), TowelType.flat),
            new(SystemTypes.VaultRoom, new(-8.156692f, 8.538751f, 0.1f), TowelType.flat, flipX : true),
            new(SystemTypes.VaultRoom, new(-6.167562f, 11.83135f, 0.1f), TowelType.wet),
            new(SystemTypes.VaultRoom, new(-9.215044f, 7.077537f, 0.1f), TowelType.hang),
            new(SystemTypes.VaultRoom, new(-8.367708f, 6.07024f, 0.1f), TowelType.hang, flipX : true),
            new(SystemTypes.VaultRoom, new(-8.367708f, 10.85551f, 0.1f), TowelType.hang, flipX : true),
            new(SystemTypes.VaultRoom, new(-9.60568f, 12.81671f, 0.1f), TowelType.bunched),
            new(SystemTypes.VaultRoom, new(-10.74735f, 6.492662f, 0.1f), TowelType.bunched),

            // キッチン : 5
            new(SystemTypes.Kitchen, new(-3.618798f, -8.210999f, 0.1f), TowelType.hang),
            new(SystemTypes.Kitchen, new(-4.664008f, -11.32344f, 0.1f), TowelType.wet),
            new(SystemTypes.Kitchen, new(-5.903908f, -12.80518f, 0.1f), TowelType.wet),
            new(SystemTypes.Kitchen, new(-7.614682f, -5.50508f, 0.1f), TowelType.bunched),
            new(SystemTypes.Kitchen, new(-6.251021f, -9.014095f, 0.1f), TowelType.bunched),

            // エンジンルーム : 6
            new(SystemTypes.Engine, new(-0.9302869f, -2.632946f, 0.1f), TowelType.flat),
            new(SystemTypes.Engine, new(-4.941173f, -1.163853f, 0.1f), TowelType.flat),
            new(SystemTypes.Engine, new(-8.084006f, 0.956023f, 0.1f), TowelType.hang, flipX : true),
            new(SystemTypes.Engine, new(-8.084849f, -2.28388f, 0.1f), TowelType.hang, flipX : true),
            new(SystemTypes.Engine, new(-1.829834f, 2.949617f, 0.1f), TowelType.hang, flipX : true),
            new(SystemTypes.Engine, new(0.2460737f, -0.003876925f, 0.1f), TowelType.bunched),

            // 宿舎前通路 : 3
            new(SystemTypes.Brig, new(1.435634f, 8.913982f, 0.1f), TowelType.flat),
            new(SystemTypes.Brig, new(-2.777073f, 8.248223f, 0.1f), TowelType.flat),
            new(SystemTypes.Brig, new(-0.7207161f, 9.037292f, 0.1f), TowelType.flat),

            // 昇降機 : 4
            new(SystemTypes.GapRoom, new(9.620799f, 9.157299f, 1f), TowelType.hang),
            new(SystemTypes.GapRoom, new(4.56f, 6.8568f, 0.1f), TowelType.hang, flipX : true),
            new(SystemTypes.GapRoom, new(5.8752f, 9.167671f, 1.01f), TowelType.hang, flipX : true),
            new(SystemTypes.GapRoom, new(13.95504f, 5.720777f, 0.1f), TowelType.bunched),

            // ミーティングルーム : 5
            new(SystemTypes.MeetingRoom, new(12.83581f, 15.19516f, 0.0157f), TowelType.flat),
            new(SystemTypes.MeetingRoom, new(3.970275f, 14.80952f, 0.1f), TowelType.flat),
            new(SystemTypes.MeetingRoom, new(8.717099f, 15.07041f, 0.0155f), TowelType.hang),
            new(SystemTypes.MeetingRoom, new(15.59604f, 16.11175f, 0.1f), TowelType.bunched),
            new(SystemTypes.MeetingRoom, new(6.8664f, 16.7065f, 0.1f), TowelType.bunched, usableDistance : 1.35f),

            // メインホール : 6
            new(SystemTypes.MainHall, new(6.716944f, -2.536576f, 0.1f), TowelType.flat),
            new(SystemTypes.MainHall, new(14.81363f, 2.370545f, 0.1f), TowelType.wet),
            new(SystemTypes.MainHall, new(13.7208f, -3.683899f, 0.1f), TowelType.hang),
            new(SystemTypes.MainHall, new(5.326502f, 3.299651f, 0.1f), TowelType.hang, flipX : true),
            new(SystemTypes.MainHall, new(10.37586f, 1.607015f, 0.1f), TowelType.bunched),
            new(SystemTypes.MainHall, new(13.45004f, 2.877884f, 0.1f), TowelType.bunched),

            // セキュリティルーム : 4
            new(SystemTypes.Security, new(5.643566f, -11.04392f, 0.1f), TowelType.flat),
            new(SystemTypes.Security, new(5.128053f, -15.04819f, -0.002f), TowelType.hang),
            new(SystemTypes.Security, new(10.84729f, -16.1029f, -0.002f), TowelType.hang, flipX : true),
            new(SystemTypes.Security, new(7.681743f, -10.02235f, 0.1f), TowelType.bunched),

            // 電気室 : 10
            new(SystemTypes.Electrical, new(19.55181f, -8.813416f, 0.1f), TowelType.flat),
            new(SystemTypes.Electrical, new(12.51974f, -6.20963f, 0.1f), TowelType.flat),
            new(SystemTypes.Electrical, new(16.84863f, -8.396872f, 0.1f), TowelType.flat),
            new(SystemTypes.Electrical, new(15.51704f, -10.88247f, 0.1f), TowelType.wet),
            new(SystemTypes.Electrical, new(14.1331f, -7.663693f, -0.001f), TowelType.hang),
            new(SystemTypes.Electrical, new(18.41132f, -10.07528f, -0.001f), TowelType.hang, flipX : true),
            new(SystemTypes.Electrical, new(20.26785f, -6.701759f, 0.1f), TowelType.bunched),
            new(SystemTypes.Electrical, new(12.94056f, -10.74243f, 0.1f), TowelType.bunched),
            new(SystemTypes.Electrical, new(15.19987f, -5.878736f, 0.1f), TowelType.bunched),
            new(SystemTypes.Electrical, new(17.22105f, -3.371418f, 0.1f), TowelType.bunched, flipX : true, usableDistance : 1.25f),

            // アーカイブ : 5
            new(SystemTypes.Records, new(18.94719f, 11.66066f, 0.1f), TowelType.flat),
            new(SystemTypes.Records, new(20.91998f, 6.490541f, 0.1f), TowelType.flat),
            new(SystemTypes.Records, new(17.4174f, 7.9311f, 0.1f), TowelType.flat, flipX : true),
            new(SystemTypes.Records, new(22.25137f, 10.64356f, 0.1f), TowelType.flat, flipX : true),
            new(SystemTypes.Records, new(19.87958f, 9.000803f, 0.0095f), TowelType.bunched),

            // シャワー室 : 6
            new(SystemTypes.Showers, new(18.26736f, -0.8086417f, 0.1f), TowelType.flat),
            new(SystemTypes.Showers, new(23.9988f, 2.595201f, 0.1f), TowelType.wet),
            new(SystemTypes.Showers, new(19.17997f, 3.003997f, 0.1f), TowelType.hang),
            new(SystemTypes.Showers, new(20.53838f, -1.847569f, 0.0048f), TowelType.hang, flipX : true),
            new(SystemTypes.Showers, new(17.9012f, 4.5888f, 0.0045f), TowelType.bunched),
            new(SystemTypes.Showers, new(23.05849f, -1.132234f, -0.001f), TowelType.bunched),

            // 診察室 : 5
            new(SystemTypes.Medical, new(32.47348f, -6.037143f, -0.0068f), TowelType.flat),
            new(SystemTypes.Medical, new(25.9241f, -8.671277f, 0.1f), TowelType.hang),
            new(SystemTypes.Medical, new(22.28066f, -5.556706f, -0.001f), TowelType.bunched),
            new(SystemTypes.Medical, new(29.32224f, -4.949036f, 0.1f), TowelType.bunched),
            new(SystemTypes.Medical, new(29.26855f, -7.262999f, -0.0068f), TowelType.bunched),

            // ラウンジ : 6
            new(SystemTypes.Lounge, new(25.99549f, 8.167336f, 0.0093f), TowelType.flat),
            new(SystemTypes.Lounge, new(24.96334f, 5.076278f, 0.1f), TowelType.flat),
            new(SystemTypes.Lounge, new(30.90265f, 5.101399f, 0.1f), TowelType.flat),
            new(SystemTypes.Lounge, new(29.25394f, 6.881144f, 1.0001f), TowelType.flat),
            new(SystemTypes.Lounge, new(33.7757f, 7.076f, 1.0001f), TowelType.wet),
            new(SystemTypes.Lounge, new(25.96569f, 6.604475f, 0.0068f), TowelType.bunched),

            // 貨物室 : 6
            new(SystemTypes.CargoBay, new(32.23074f, 1.318f, 0.1f), TowelType.flat),
            new(SystemTypes.CargoBay, new(30.48857f, -1.543814f, 0.1f), TowelType.flat),
            new(SystemTypes.CargoBay, new(38.8645f, -0.3838112f, 0.1f), TowelType.flat),
            new(SystemTypes.CargoBay, new(38.0135f, -3.741991f, 0.1f), TowelType.flat, flipX : true),
            new(SystemTypes.CargoBay, new(34.11892f, -3.954755f, 0.1f), TowelType.bunched),
            new(SystemTypes.CargoBay, new(36.91407f, 2.336189f, 0.1f), TowelType.hang, usableDistance : 1.15f),
        };

        [HarmonyPatch(nameof(ShipStatus.Start)), HarmonyPostfix]
        public static void StartPostfix(ShipStatus __instance)
        {
            if (!Main.IsCursed) return;
            if (!MapData.IsMap(CustomMapNames.Airship)) return;
            Transform towels = __instance.transform.Find("Showers/TaskTowels");
            GameObject copytowel = Object.Instantiate(towels.GetChild(0).gameObject);

            Sprites = new();
            for (int i = 0; i < towels.childCount; i++)
            {
                Transform towel = towels.GetChild(i);
                towel.position = new(9999f, 9999f, 0.1f);
                if (i is 0 or 2 or 5 or 7) Sprites.Add(towel.GetComponent<SpriteRenderer>().sprite);
            }

            List<Console> consoles = new(__instance.AllConsoles);
            Dictionary<SystemTypes, uint> count = new();
            for (int i = 0; i < Data.Count; i++)
            {
                TowelData data = Data[i];
                if (!count.ContainsKey(data.Type)) count.Add(data.Type, 1);

                GameObject towel = Object.Instantiate(copytowel);
                towel.name = $"TaskTowel {count[data.Type]++}";
                towel.transform.SetParent(__instance.AllRooms.ToList().Find(n => n.RoomId == data.Type).transform);
                towel.transform.position = data.Position;
                towel.transform.localScale = Vector3.one;
                SpriteRenderer sprite = towel.GetComponent<SpriteRenderer>();
                sprite.sprite = Sprites[(int)data.SpriteNumber];
                sprite.flipX = data.FlipX;
                sprite.flipY = data.FlipY;
                TowelTaskConsole console = towel.GetComponent<TowelTaskConsole>();
                int id = i;
                console.ConsoleId = id;
                console.usableDistance = data.UsableDistance;

                consoles.Add(console);
            }
            __instance.AllConsoles = consoles.ToArray();
            Object.Destroy(copytowel);
            towels.name = "Destroy TaskTowels";
            towels.SetParent(__instance.transform);
        }
    }

    public class TowelData
    {
        public SystemTypes Type;
        public Vector3 Position;
        public TowelType SpriteNumber;
        public bool FlipX;
        public bool FlipY;
        public float UsableDistance;
        public TowelData(SystemTypes type, Vector3 position, TowelType spriteNumber, bool flipX = false, bool flipY = false, float usableDistance = 1f)
        {
            this.Type = type;
            this.Position = position;
            this.SpriteNumber = spriteNumber;
            this.FlipX = flipX;
            this.FlipY = flipY;
            this.UsableDistance = usableDistance;
        }
    }

    public enum TowelType
    {
        flat = 0,
        wet = 1,
        hang = 2,
        bunched = 3,
    }
}