using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static System.Math;
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
    public static class Vecs
    {
        public static Vector3D pos1;
        public static Vector3D pos2;
        public static Vector3D pos3;
    }
    public struct Vector3D
    {
        public double x;
        public double y;
        public double z;
    }
    public class Conjurer
    {
        //ベクトル内積
        public double dot_product(Vector3D vl, Vector3D vr)
        {
            return vl.x * vr.x + vl.y * vr.y + vl.z * vr.z;
        }
        public bool TriangleArea(Vector3D A, Vector3D B, Vector3D C, Vector3D P)
        {
            Vector3D sub_vector(Vector3D a, Vector3D b)
            {
                Vector3D ret;
                ret.x = a.x - b.x;
                ret.y = a.y - b.y;
                ret.z = a.z - b.z;
                return ret;
            }

            //ベクトル外積( vl × vr )
            Vector3D cross_product(Vector3D vl, Vector3D vr)
            {
                Vector3D ret;
                ret.x = vl.y * vr.z - vl.z * vr.y;
                ret.y = vl.z * vr.x - vl.x * vr.z;
                ret.z = vl.x * vr.y - vl.y * vr.x;

                return ret;
            }

            Vector3D A2 = Vecs.pos1;
            Vector3D B2 = Vecs.pos2;
            Vector3D C2 = Vecs.pos3;

            Vector3D AB = sub_vector(B2, A2);
            Vector3D BP = sub_vector(C2, B2);

            Vector3D BC = sub_vector(C2, B2);
            Vector3D CP = sub_vector(P, C);

            Vector3D CA = sub_vector(A2, C);
            Vector3D AP = sub_vector(P, A2);

            Vector3D c1 = cross_product(AB, BP);
            Vector3D c2 = cross_product(BC, CP);
            Vector3D c3 = cross_product(CA, AP);

            //内積で順方向か逆方向か調べる
            double dot_12 = dot_product(c1, c2);
            double dot_13 = dot_product(c1, c3);

            if (dot_12 > 0 && dot_13 > 0)
            {
                //三角形の内側に点がある
                return true;
            }
            else
            {
                return false;
            }
        }


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

        public static Sprite getBoxAnimationSprite(int index)
        {
            if (boxAnimationSprites == null || boxAnimationSprites.Length == 0) return null;
            index = Mathf.Clamp(index, 0, boxAnimationSprites.Length - 1);
            //if (boxAnimationSprites[index] == null)
            //SuperNewRolesPlugin.Logger.LogInfo("nullnull");
            boxAnimationSprites[index] = ModHelpers.loadSpriteFromResources($"SuperNewRoles.Resources.Animation.Conjurer_Maker_00{index + 1:00}.png", 175f);
            return boxAnimationSprites[index];
        }


        public static void startAnimation(int ventId)
        {
            JackInTheBox box = AllJackInTheBoxes.FirstOrDefault((x) => x?.vent != null && x.vent.Id == ventId);
            if (box == null) return;

            HudManager.Instance.StartCoroutine(Effects.Lerp(0.6f, new Action<float>((p) =>
            {
                // if (box.boxRenderer != null)
                //{
                box.boxRenderer.sprite = getBoxAnimationSprite((int)(p * boxAnimationSprites.Length));
                /*if (p == 1f)*/
                box.boxRenderer.sprite = getBoxAnimationSprite(0);
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
            boxRenderer.sprite = getBoxAnimationSprite(0);
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
