using System.Collections;
using System.Collections.Generic;
using Agartha;
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using SuperNewRoles.Mode.SuperHostRoles;
using SuperNewRoles.Roles.Impostor;
using UnityEngine;

namespace SuperNewRoles.Patches.CursedTasks;

public class CursedDressUpTask
{
    public static Transform PanelDummy;
    public static float PlayerSpeed => SyncSetting.OptionData.GetFloat(FloatOptionNames.PlayerSpeedMod);
    public static float MoveSpeed => PlayerSpeed * PlayerControl.LocalPlayer.MyPhysics.Speed - 0.25f * PlayerSpeed * 1.5f;
    public static bool IsDisabledPlatform;

    [HarmonyPatch(typeof(DressUpMinigame))]
    public static class DressUpMinigamePatch
    {
        [HarmonyPatch(nameof(DressUpMinigame.Begin)), HarmonyPostfix]
        public static void BeginPostfix(DressUpMinigame __instance)
        {
            if (!Main.IsCursed) return;
            int num = __instance.MyNormTask.Data[0];
            __instance.DummyHat.transform.SetLocalZ(num == 1 ? 1 : -1);
            __instance.DummyHat.sprite = __instance.Hats[num];
            __instance.DummyAccessory.sprite = __instance.Accessories[__instance.MyNormTask.Data[1]];
            __instance.DummyClothes.sprite = __instance.Clothes[__instance.MyNormTask.Data[2]];
            __instance.SetHat(__instance.MyNormTask.Data[3]);
            __instance.SetAccessory(__instance.MyNormTask.Data[4]);
            __instance.SetClothes(__instance.MyNormTask.Data[5]);
        }

        [HarmonyPatch(nameof(DressUpMinigame.Update)), HarmonyPostfix]
        public static void UpdatePostfix(DressUpMinigame __instance)
        {
            if (!Main.IsCursed) return;
            float num = 0.75f + 0.25f * (PlayerSpeed / 0.25f - 1);
            if (IsDisabledPlatform) num = num >= 1f ? 1f : 0.75f;
            if (Vector2.Distance(PlayerControl.LocalPlayer.transform.position, PanelDummy.position) > num)
                __instance.Close();
        }

        [HarmonyPatch(nameof(DressUpMinigame.SetHat)), HarmonyPrefix]
        public static bool SetHatPrefix(DressUpMinigame __instance, int i)
        {
            if (!Main.IsCursed) return true;
            if (__instance.amClosing != Minigame.CloseState.None || i < 0 || i >= __instance.Hats.Count) return false;
            __instance.ActualHat.transform.SetLocalZ(i == 1 ? 1 : -1);
            __instance.ActualHat.sprite = __instance.Hats[i];
            __instance.MyNormTask.Data[3] = (byte)i;
            if (Constants.ShouldPlaySfx())
            {
                if (__instance.DummyHat.sprite == __instance.ActualHat.sprite) SoundManager.Instance.PlaySound(__instance.correctSound, false, 1f, null);
                else SoundManager.Instance.PlaySound(__instance.incorrectSound, false, 1f, null);
            }
            __instance.CheckOutfit();
            return false;
        }

        [HarmonyPatch(nameof(DressUpMinigame.SetAccessory)), HarmonyPrefix]
        public static bool SetAccessoryPrefix(DressUpMinigame __instance, int i)
        {
            if (!Main.IsCursed) return true;
            if (__instance.amClosing != Minigame.CloseState.None || i < 0 || i >= __instance.Accessories.Count) return false;
            __instance.ActualAccessory.sprite = __instance.Accessories[i];
            __instance.MyNormTask.Data[4] = (byte)i;
            if (Constants.ShouldPlaySfx())
            {
                if (__instance.DummyAccessory.sprite == __instance.ActualAccessory.sprite) SoundManager.Instance.PlaySound(__instance.correctSound, false, 1f, null);
                else SoundManager.Instance.PlaySound(__instance.incorrectSound, false, 1f, null);
            }
            __instance.CheckOutfit();
            return false;
        }

        [HarmonyPatch(nameof(DressUpMinigame.SetClothes)), HarmonyPrefix]
        public static bool SetClothesPrefix(DressUpMinigame __instance, int i)
        {
            if (!Main.IsCursed) return true;
            if (__instance.amClosing != Minigame.CloseState.None || i < 0 || i >= __instance.Clothes.Count) return false;
            __instance.ActualClothes.sprite = __instance.Clothes[i];
            __instance.MyNormTask.Data[5] = (byte)i;
            if (Constants.ShouldPlaySfx())
            {
                if (__instance.DummyClothes.sprite == __instance.ActualClothes.sprite) SoundManager.Instance.PlaySound(__instance.correctSound, false, 1f, null);
                else SoundManager.Instance.PlaySound(__instance.incorrectSound, false, 1f, null);
            }
            __instance.CheckOutfit();
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
            if (__instance.TaskType != TaskTypes.DressMannequin) return;
            __instance.Data = new byte[6];
            for (int i = 0; i < 3; i++) __instance.Data[i] = IntRange.NextByte(0, 3);
            for (int i = 3; i < __instance.Data.Count; i++) __instance.Data[i] = 255;
        }
    }

    [HarmonyPatch(typeof(ShipStatus))]
    public static class ShipStatusPatch
    {
        [HarmonyPatch(nameof(ShipStatus.Start)), HarmonyPostfix]
        public static void StartPostfix(ShipStatus __instance)
        {
            if (!Main.IsCursed) return;
            if (!MapData.IsMap(CustomMapNames.Airship)) return;
            IsDisabledPlatform = false;

            PanelDummy = __instance.transform.Find("Vault/panel_dummy");
            if (!PanelDummy) return;
            __instance.StartCoroutine(MovePanelDummy(__instance, true));
        }

        public static readonly List<Vector2> MovePositions = new()
        {
            // 金庫
            new(-7.9841f,  8.6265f),
            // 向き変更

            // 宿舎
            new(-0.9036f,  8.6265f),

            // エンジン
            new(-0.9036f,  -1.1772f),
            // 向き変更

            // コミュ前通路
            new(-11.0026f, -1.1772f),

            // 武器庫
            new(-11.0026f, -3.0572f),
            // 向き変更
            new(-9.5769f,  -3.0572f),
            new(-9.5769f,  -7.4494f),

            // キッチン
            new(-7.4998f,  -7.4494f),
            new(-7.4998f,  -9.4661f),
            new(-5.7942f,  -11.2703f),
            new(-3.0778f,  -11.2703f),
            new(-1.4814f,  -12.2884f),

            // セキュ
            new(9.0294f,   -12.2884f),
            new(9.0294f,   -10.357f),

            // エレキ
            new(10.134f,   -10.357f),
            new(10.134f,   -8.5628f),

            // 医務室
            new(21.9018f,  -8.5628f),
            new(23.2181f,  -5.7589f),
            new(32.5278f,  -5.7589f),

            // 貨物
            new(32.5278f,  -3.0715f),
            new(33.8368f,  0f),

            // ラウンジ
            new(33.8368f,  5.3207f),
            // 向き変更
            new(27.0311f,  5.3202f),
            new(27.0311f,  7.2808f),
            new(24.7547f,  7.2808f),
            new(24.7547f,  9.1638f),

            // アーカイブ
            new(21.6401f,  9.1638f),
            new(19.8963f,  8.331f),
            new(18.1525f,  9.1638f),

            // 昇降機
            new(15.3158f,  9.1638f),
            new(13.5011f,  8.9163f),
            new(10.0832f,  8.9163f),

            // 宿舎
            new(-0.9036f,  8.6265f),
        };

        public static IEnumerator MovePanelDummy(ShipStatus __instance, bool start = false)
        {
            for (int i = start ? 0 : 2; i < MovePositions.Count; i++)
            {
                if (!PanelDummy) yield break;
                Vector2 pos = PanelDummy.position;
                if (i == 16)
                {
                    ElectricalDoors electrical = __instance.transform.Find("Electrical").GetComponent<ElectricalDoors>();

                    StaticDoor door = electrical.Doors[11];
                    bool open = door.IsOpen;
                    pos = new(11.2175f, -8.5628f);
                    yield return Slide2DWorld(PanelDummy, PanelDummy.position, pos, Vector2.Distance(PanelDummy.position, pos) / MoveSpeed);

                    door.SetOpen(true);
                    pos = new(12.2502f, -8.5628f);
                    yield return Slide2DWorld(PanelDummy, PanelDummy.position, pos, Vector2.Distance(PanelDummy.position, pos) / MoveSpeed);
                    door.SetOpen(open);


                    door = electrical.Doors[5];
                    open = door.IsOpen;
                    pos = new(14.1956f, -8.5628f);
                    yield return Slide2DWorld(PanelDummy, PanelDummy.position, pos, Vector2.Distance(PanelDummy.position, pos) / MoveSpeed);

                    door.SetOpen(true);
                    pos = new(15.2713f, -8.5628f);
                    yield return Slide2DWorld(PanelDummy, PanelDummy.position, pos, Vector2.Distance(PanelDummy.position, pos) / MoveSpeed);
                    door.SetOpen(open);


                    door = electrical.Doors[6];
                    open = door.IsOpen;
                    pos = new(17.2893f, -8.5628f);
                    yield return Slide2DWorld(PanelDummy, PanelDummy.position, pos, Vector2.Distance(PanelDummy.position, pos) / MoveSpeed);

                    door.SetOpen(true);
                    pos = new(18.3382f, -8.5628f);
                    yield return Slide2DWorld(PanelDummy, PanelDummy.position, pos, Vector2.Distance(PanelDummy.position, pos) / MoveSpeed);
                    door.SetOpen(open);
                }
                else if (i == 32)
                {
                    Transform room = __instance.transform.Find("GapRoom");
                    IsDisabledPlatform = true;
                    List<string> finds = new() { "PlatformLeftClick", "PlatformLeft", "PlatformRight", "PlatformRightClick" };
                    foreach (string find in finds) room.Find(find).gameObject.SetActive(false);
                    Vector3 correction = new(0f, -0.3636f);
                    MovingPlatformBehaviour platform = room.Find("Platform").GetComponent<MovingPlatformBehaviour>();
                    bool left = platform.IsLeft;
                    if (platform.IsLeft) yield return Nun.NotMoveUsePlatform(platform);

                    pos = platform.transform.parent.TransformPoint(platform.RightPosition) - correction;
                    yield return Effects.Slide2DWorld(PanelDummy, PanelDummy.position, pos, Vector2.Distance(PanelDummy.position, pos) / MoveSpeed);
                    yield return Effects.All(Nun.NotMoveUsePlatform(platform).WrapToIl2Cpp(),
                                             Effects.Slide2DWorld(PanelDummy, platform.transform.position - correction, platform.transform.parent.TransformPoint(platform.LeftPosition) - correction, PlayerControl.LocalPlayer.MyPhysics.Speed));
                    yield return Effects.Slide2DWorld(PanelDummy, PanelDummy.position, platform.transform.parent.TransformPoint(platform.LeftUsePosition), PlayerControl.LocalPlayer.MyPhysics.Speed);
                    if (!left) __instance.StartCoroutine(Nun.NotMoveUsePlatform(platform));

                    foreach (string find in finds) room.Find(find).gameObject.SetActive(true);
                    IsDisabledPlatform = false;
                }

                if ((i is 1 or 3 && start) || i is 5 or 22)
                {
                    Vector3 scale = PanelDummy.localScale;
                    scale.x *= -1;
                    PanelDummy.localScale = scale;
                }
                pos = MovePositions[i];
                yield return Slide2DWorld(PanelDummy, PanelDummy.position, pos, Vector2.Distance(PanelDummy.position, pos) / MoveSpeed);
            }
            yield return MovePanelDummy(__instance);
            yield break;
        }

        public static IEnumerator Slide2DWorld(Transform target, Vector2 source, Vector2 dest, float duration = 0.75f)
        {
            if (!target) yield break;
            Vector3 temp;
            for (float time = 0f; time < duration; time += Time.deltaTime)
            {
                float num = time / duration;
                temp.x = Mathf.Lerp(source.x, dest.x, num);
                temp.y = Mathf.Lerp(source.y, dest.y, num);
                temp.z = temp.y / 1000;
                target.position = temp;
                yield return null;
            }
            temp.x = dest.x;
            temp.y = dest.y;
            temp.z = temp.y / 1000;
            target.position = temp;
            yield break;
        }
    }
}
