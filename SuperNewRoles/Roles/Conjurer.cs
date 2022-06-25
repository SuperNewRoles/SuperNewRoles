using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.EndGame;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using UnityEngine;

namespace SuperNewRoles.Roles
{
    public static class Conjurer
    {
        //Buttonのクールリセット
        public static void AllCoolReset()
        {
            HudManagerStartPatch.ConjurerFirstAddButton.MaxTimer = RoleClass.Conjurer.CoolTime;
            HudManagerStartPatch.ConjurerFirstAddButton.Timer = RoleClass.Conjurer.CoolTime;

            HudManagerStartPatch.ConjurerSecondAddButton.MaxTimer = RoleClass.Conjurer.CoolTime;
            HudManagerStartPatch.ConjurerSecondAddButton.Timer = RoleClass.Conjurer.CoolTime;

            HudManagerStartPatch.ConjurerThirdAddButton.MaxTimer = RoleClass.Conjurer.CoolTime;
            HudManagerStartPatch.ConjurerThirdAddButton.Timer = RoleClass.Conjurer.CoolTime;
        }

        //FirstAddをtrueに
        public static void FirstAddAdd()
        {
            RoleClass.Conjurer.FirstAdd = true;
        }

        //SecondAddをtrueに
        public static void SecondAddAdd()
        {
            RoleClass.Conjurer.SecondAdd = true;
        }

        //ThirdAddをtrueに
        public static void ThirdAddAdd()
        {
            RoleClass.Conjurer.ThirdAdd = true;
        }

        //全部falseに
        public static void AllClear()
        {
            RoleClass.Conjurer.FirstAdd = false;
            RoleClass.Conjurer.SecondAdd = false;
            RoleClass.Conjurer.ThirdAdd = false;
        }

        //一回追加されたかを判定する
        public static bool IsFirstAdded()
        {
            if (RoleClass.Conjurer.FirstAdd)
            {
                SuperNewRolesPlugin.Logger.LogInfo("IsFirstAddedがtrueeee");
                return true;
            }
            return false;
        }

        //二回追加されたかを判定する
        public static bool IsSecondAdded()
        {
            if (RoleClass.Conjurer.SecondAdd)
            {
                SuperNewRolesPlugin.Logger.LogInfo("IsSecondAddedがtrueeee");
                return true;
            }
            return false;
        }

        //三回追加されたかを判定する
        public static bool IsThirdAdded()
        {
            if (RoleClass.Conjurer.ThirdAdd)
            {
                SuperNewRolesPlugin.Logger.LogInfo("IsThirdAddedがtrueeee");
                return true;
            }
            return false;
        }


        public static void SetConjurerButton()
        {
            if (PlayerControl.LocalPlayer.isRole(CustomRPC.RoleId.Conjurer))
            {
                FastDestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(false);
            }
        }
        //キルボタンを消す

        public class FixedUpdate
        {
            public static void Postfix()
            {
                SetConjurerButton();
            }
        }
        //Fixedddddd

        public static void ShowFlash(Color color, float duration = 2f)
        //Seerで使用している画面を光らせるコード
        {
            if (FastDestroyableSingleton<HudManager>.Instance == null || FastDestroyableSingleton<HudManager>.Instance.FullScreen == null) return;
            FastDestroyableSingleton<HudManager>.Instance.FullScreen.gameObject.SetActive(true);
            FastDestroyableSingleton<HudManager>.Instance.FullScreen.enabled = true;
            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(duration, new Action<float>((p) =>
            {
                if (RoleClass.Conjurer.ScreenFrash)//ScreenFrashが有効なら
                {
                    var renderer = FastDestroyableSingleton<HudManager>.Instance.FullScreen;

                    if (p < 0.5)
                    {
                        if (renderer != null)
                            renderer.color = new Color(color.r, color.g, color.b, Mathf.Clamp01(p * 2 * 0.75f));
                    }
                    else
                    {
                        if (renderer != null)
                            renderer.color = new Color(color.r, color.g, color.b, Mathf.Clamp01((1 - p) * 2 * 0.75f));
                    }
                    if (p == 1f && renderer != null) renderer.enabled = false;
                }
            })));
        }

    }
    public class JackInTheBox
    {
        public static System.Collections.Generic.List<JackInTheBox> AllJackInTheBoxes = new();
        public static int JackInTheBoxLimit = 3;
        public static bool boxesConvertedToVents = false;
        public static Sprite[] boxAnimationSprites = new Sprite[3];
        /*
        public static Sprite getBoxAnimationSprite(int index)
        {
            if (boxAnimationSprites == null || boxAnimationSprites.Length == 0) return null;
            index = Mathf.Clamp(index, 0, boxAnimationSprites.Length - 1);
            if (boxAnimationSprites[index] == null)
                boxAnimationSprites[index] = ModHelpers.loadSpriteFromResources($"SuperNewRoles.Resources.Animation.Conjurer_Maker__{index + 1:00}.png", 175f);
            return boxAnimationSprites[index];
        }*/
        public static Transform miraship;
        public static Sprite SetObject(int index)
        {
            Transform Object_Projecter = GameObject.Instantiate(Template, MiraShip);
            //Object_Projecter.position = new Vector3(10.6f, 18.1f, 0.1f);
            GameObject.Destroy(Object_Projecter.GetComponent<PolygonCollider2D>());
            Object_Projecter.GetComponent<SpriteRenderer>().sprite = AgarthagetSprite("Animation.pro_polygon");
            Object_Projecter.gameObject.AddComponent<PolygonCollider2D>();
            Object_Projecter.name = "Object_Projecter";
            CustomAnimation.Animation Object_Projecter_Animation = new CustomAnimation.Animation();
            Object_Projecter_Animation.Start(8, Object_Projecter);
            Object_Projecter_Animation.Sprites = CustomAnimation.LoadSprites.GetSpritesAgartha("SuperNewRoles.Resources.Animation.Conjurer_Marker", 32);
            Object_Projecter.localScale *= 2.5f;

            if (boxAnimationSprites == null || boxAnimationSprites.Length == 0) return null;
            index = Mathf.Clamp(index, 0, boxAnimationSprites.Length - 1);
            if (boxAnimationSprites[index] == null)
                boxAnimationSprites[index] = ModHelpers.loadSpriteFromResources($"SuperNewRoles.Resources.Animation.Conjurer_Maker_{index + 1:00}.png", 175f);
            return boxAnimationSprites[index];
        }

        private static Dictionary<string, Sprite> Datas = new Dictionary<string, Sprite>();
        public static Sprite AgarthagetSprite(string id)
        {
            //if (Datas.ContainsKey(id)) return Datas[id];
            Datas[id] = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.Animation." + id + ".png", 115f);
            return Datas[id];
        }

        public static void startAnimation(int ventId)
        {
            JackInTheBox box = AllJackInTheBoxes.FirstOrDefault((x) => x?.vent != null && x.vent.Id == ventId);
            if (box == null) return;

            HudManager.Instance.StartCoroutine(Effects.Lerp(0.6f, new Action<float>((p) =>
            {
                // if (box.boxRenderer != null)
                //{
                box.boxRenderer.sprite = SetObject((int)(p * boxAnimationSprites.Length));
                if (p == 1f) box.boxRenderer.sprite = SetObject(0);
                //}
            })));
        }

        private GameObject gameObject;
        public Vent vent;
        private SpriteRenderer boxRenderer;

        public JackInTheBox(Vector2 p)
        {
            gameObject = new GameObject("JackInTheBox") { layer = 11 };
            gameObject.AddSubmergedComponent(SubmergedCompatibility.Classes.ElevatorMover);
            Vector3 position = new(p.x, p.y, p.y / 1000f + 0.01f);
            position += (Vector3)PlayerControl.LocalPlayer.Collider.offset; // Add collider offset that DoMove moves the player up at a valid position
                                                                            // Create the marker
            gameObject.transform.position = position;
            boxRenderer = gameObject.AddComponent<SpriteRenderer>();
            boxRenderer.sprite = SetObject(0);
            /*
                        // Create the vent
                        var referenceVent = UnityEngine.Object.FindObjectOfType<Vent>();
                        vent = UnityEngine.Object.Instantiate<Vent>(referenceVent);
                        vent.gameObject.AddSubmergedComponent(SubmergedCompatibility.Classes.ElevatorMover);
                        vent.transform.position = gameObject.transform.position;
                        vent.Left = null;
                        vent.Right = null;
                        vent.Center = null;
                        vent.EnterVentAnim = null;
                        vent.ExitVentAnim = null;
                        vent.Offset = new Vector3(0f, 0.25f, 0f);
                        vent.GetComponent<PowerTools.SpriteAnim>()?.Stop();
                        vent.Id = ShipStatus.Instance.AllVents.Select(x => x.Id).Max() + 1; // Make sure we have a unique id
                        var ventRenderer = vent.GetComponent<SpriteRenderer>();
                        ventRenderer.sprite = null;
                        vent.myRend = ventRenderer;
                        var allVentsList = ShipStatus.Instance.AllVents.ToList();
                        allVentsList.Add(vent);
                        ShipStatus.Instance.AllVents = allVentsList.ToArray();
                        vent.gameObject.SetActive(false);
                        vent.name = "JackInTheBoxVent_" + vent.Id;
            */
            // Only render the box for the Trickster
            var playerIsTrickster = PlayerControl.LocalPlayer;
            gameObject.SetActive(playerIsTrickster);

            AllJackInTheBoxes.Add(this);
        }

        public static void UpdateStates()
        {
            if (boxesConvertedToVents == true) return;
            foreach (var box in AllJackInTheBoxes)
            {
                var playerIsTrickster = PlayerControl.LocalPlayer;
                box.gameObject.SetActive(playerIsTrickster);
            }
        }

        public void convertToVent()
        {
            gameObject.SetActive(true);
            vent.gameObject.SetActive(true);
            return;
        }

        public static void convertToVents()
        {
            foreach (var box in AllJackInTheBoxes)
            {
                box.convertToVent();
            }
            connectVents();
            boxesConvertedToVents = true;
            return;
        }

        public static bool hasJackInTheBoxLimitReached()
        {
            return AllJackInTheBoxes.Count >= JackInTheBoxLimit;
        }

        private static void connectVents()
        {
            for (var i = 0; i < AllJackInTheBoxes.Count - 1; i++)
            {
                var a = AllJackInTheBoxes[i];
                var b = AllJackInTheBoxes[i + 1];
                a.vent.Right = b.vent;
                b.vent.Left = a.vent;
            }
            // Connect first with last
            AllJackInTheBoxes.First().vent.Left = AllJackInTheBoxes.Last().vent;
            AllJackInTheBoxes.Last().vent.Right = AllJackInTheBoxes.First().vent;
        }

        public static void clearJackInTheBoxes()
        {
            foreach (var box in AllJackInTheBoxes)
            {
                GameObject.Destroy(box.gameObject);
            }
            boxesConvertedToVents = false;
            AllJackInTheBoxes = new List<JackInTheBox>();
        }
    }
}
