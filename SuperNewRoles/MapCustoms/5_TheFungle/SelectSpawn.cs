using System.Collections;
using System.Linq;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using PowerTools;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using static Minigame;
using static Rewired.UI.ControlMapper.ControlMapper;

namespace SuperNewRoles.MapCustoms;
[HarmonyPatch]
public static class FungleSelectSpawn
{
    [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), new[] {
                typeof(StringNames),
                typeof(Il2CppReferenceArray<Il2CppSystem.Object>)
            })]
    private class StringPatch
    {
        public static bool Prefix(ref string __result, [HarmonyArgument(0)] StringNames name)
        {
            if ((int)name == 50999)
            {
                __result = "キャンプファイアー";
                return false;
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.PrespawnStep))]
    public class ShipStatisPrespawnStepPatch
    {
        public static bool Prefix(ShipStatus __instance, ref Il2CppSystem.Collections.IEnumerator __result)
        {
            if (!FungleHandler.IsFungleSpawnType(FungleHandler.FungleSpawnType.Select))
                return true;
            __result = SelectSpawn().WrapToIl2Cpp();
            return false;
        }
        static SpawnInMinigame.SpawnLocation[] Locations = new SpawnInMinigame.SpawnLocation[4]
        {
            new(){ Name = (StringNames)50999,
                Location = new(-9.81f, 0.6f),
                Image = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.FungleSelectSpawn.Campfire.png", 115f)},
            new(){ Name = StringNames.Dropship, Location = new(-8f, 10.5f),
                Image = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.FungleSelectSpawn.Dropship.png", 115f)},
            new(){ Name = StringNames.Cafeteria, Location = new(-16.16f, 7.25f),
                Image = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.FungleSelectSpawn.Cafeteria.png", 115f)},
            new(){ Name = StringNames.Kitchen, Location = new(-15.5f, -7.5f),
                Image = ModHelpers.LoadSpriteFromResources("SuperNewRoles.Resources.FungleSelectSpawn.Kitchen.png", 115f)},
        };
        public static IEnumerator SelectSpawn()
        {
            SpawnInMinigame spawnInMinigame = Object.Instantiate(Agartha.MapLoader.Airship.TryCast<AirshipStatus>().SpawnInGame);
            spawnInMinigame.transform.SetParent(Camera.main.transform, false);
            spawnInMinigame.transform.localPosition = new(0f, 0f, -600f);
            spawnInMinigame.Locations = new(Locations);
            spawnInMinigame.Begin(null);
            yield return spawnInMinigame.WaitForFinish();
        }
    }
}
public class FunglePreSpawnMinigame : Minigame
{
    public struct SpawnLocation
    {
        public string NameKey;

        public Sprite Image;

        public AnimationClip Rollover;

        public AudioClip RolloverSfx;

        public Vector3 Location;
    }

    public SpawnLocation[] Locations;

    public PassiveButton[] Buttons;

    public TextMeshPro Text;

    public AudioClip DefaultRolloverSound;

    public UiElement DefaultButtonSelected;


    private bool gotButton;
    public const int ButtonLength = 3;
    public FunglePreSpawnMinigame(System.IntPtr ptr) : base(ptr) { }
    public PassiveButton GenerateButton(Transform parent)
    {
        return Instantiate(Agartha.MapLoader.Airship.TryCast<AirshipStatus>().SpawnInGame.LocationButtons.FirstOrDefault(), parent);
    }
    public override void Begin(PlayerTask task)
    {
        base.Begin(task);
        SpawnLocation[] array = Locations.ToArray();
        array.Shuffle();
        array = (from s in array.Take(ButtonLength)
                 orderby s.Location.x, s.Location.y descending
                 select s).ToArray();
        for (int i = 0; i < ButtonLength; i++)
        {
            PassiveButton obj = GenerateButton(this.transform);
            SpawnLocation pt = array[i];
            obj.OnClick.AddListener((UnityAction)(() =>
            {
                SpawnAt(pt);
            }));
            obj.GetComponent<SpriteAnim>().Stop();
            obj.GetComponent<SpriteRenderer>().sprite = pt.Image;
            obj.GetComponentInChildren<TextMeshPro>().text = ModTranslation.GetString(pt.NameKey);
            ButtonAnimRolloverHandler component = obj.GetComponent<ButtonAnimRolloverHandler>();
            component.StaticOutImage = pt.Image;
            //component.RolloverAnim = pt.Rollover;
            //component.HoverSound = pt.RolloverSfx ? pt.RolloverSfx : DefaultRolloverSound;
        }
        if (GameManager.Instance != null && GameManager.Instance.IsNormal())
        {
            foreach (GameData.PlayerInfo allPlayer in GameData.Instance.AllPlayers)
            {
                if (allPlayer != null && allPlayer.Object != null && !allPlayer.Disconnected)
                {
                    allPlayer.Object.NetTransform.transform.position = new Vector2(-25f, 40f);
                    allPlayer.Object.NetTransform.Halt();
                }
            }
        }
        //this.StartCoroutine(RunTimer());
        //ControllerManager.Instance.OpenOverlayMenu(this.name, null, DefaultButtonSelected, ControllerSelectable);
        PlayerControl.HideCursorTemporarily();
        ConsoleJoystick.SetMode_Menu();
    }
    private void SpawnAt(SpawnLocation spawnPoint)
    {
        //IL_0056: Unknown result type (might be due to invalid IL or missing references)
        //IL_005b: Unknown result type (might be due to invalid IL or missing references)
        if (amClosing == CloseState.None)
        {
           // Logger.GlobalInstance.Info($"Player selected spawn point {spawnPoint.Name}");
            gotButton = true;
            PlayerControl.LocalPlayer.SetKinematic(b: true);
            PlayerControl.LocalPlayer.NetTransform.SetPaused(isPaused: true);
            PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(spawnPoint.Location);
            DestroyableSingleton<HudManager>.Instance.PlayerCam.SnapToTarget();
            ((MonoBehaviour)this).StopAllCoroutines();
            //((MonoBehaviour)this).StartCoroutine(CoSpawnAt(PlayerControl.LocalPlayer, spawnPoint));
        }
    }
}