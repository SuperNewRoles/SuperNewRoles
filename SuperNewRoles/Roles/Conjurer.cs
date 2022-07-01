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
    public class Vecs
    {
        public static Vector3 pos1;
        public static Vector3 pos2;
        public static Vector3 pos3;
    }

    public class Conjurer
    {
        //ベクトル内積
        public double dot_product(Vector3 vl, Vector3 vr)
        {
            return vl.x * vr.x + vl.y * vr.y + vl.z * vr.z;
        }
        public bool TriangleArea(Vector3 A, Vector3 B, Vector3 C, Vector3 P)
        {
            Vector3 sub_vector(Vector3 a, Vector3 b)
            {
                Vector3 ret;
                ret.x = a.x - b.x;
                ret.y = a.y - b.y;
                ret.z = a.z - b.z;
                return ret;
            }

            //ベクトル外積( vl × vr )
            Vector3 cross_product(Vector3 vl, Vector3 vr)
            {
                Vector3 ret;
                ret.x = vl.y * vr.z - vl.z * vr.y;
                ret.y = vl.z * vr.x - vl.x * vr.z;
                ret.z = vl.x * vr.y - vl.y * vr.x;

                return ret;
            }

            Vector3 A2 = Vecs.pos1;
            Vector3 B2 = Vecs.pos2;
            Vector3 C2 = Vecs.pos3;

            Vector3 AB = sub_vector(B2, A2);
            Vector3 BP = sub_vector(C2, B2);

            Vector3 BC = sub_vector(C2, B2);
            Vector3 CP = sub_vector(P, C);

            Vector3 CA = sub_vector(A2, C);
            Vector3 AP = sub_vector(P, A2);

            Vector3 c1 = cross_product(AB, BP);
            Vector3 c2 = cross_product(BC, CP);
            Vector3 c3 = cross_product(CA, AP);

            //内積で順方向か逆方向か調べる
            double dot_12 = dot_product(c1, c2);
            double dot_13 = dot_product(c1, c3);

            if (dot_12 > 0 && dot_13 > 0)
            {
                //三角形の内側に点がある
                SuperNewRolesPlugin.Logger.LogInfo("TriangleAreaがtrue");
                return true;
            }
            else
            {
                SuperNewRolesPlugin.Logger.LogInfo("TriangleAreaがfalse");
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
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                if (PlayerControl.LocalPlayer.isRole(RoleId.Conjurer) || p.isDead())//魔術師と死人のとき
                {
                    //boxAnimationSprites[index] = ModHelpers.loadSpriteFromResources($"SuperNewRoles.Resources.Animation.Conjurer_Maker_00{index + 1:00}.png", 175f);
                    Transform Conjurer_Marker = GameObject.Instantiate(GameObject.Find("JackInTheBox").transform);
                    CustomAnimation.Animation Conjurer_Marker_Animation = new CustomAnimation.Animation();
                    Conjurer_Marker_Animation.Start(10, Conjurer_Marker);
                    Conjurer_Marker_Animation.Sprites = CustomAnimation.LoadSprites.GetSpritesAgartha("SuperNewRoles.Resources.Animation.Conjurer_Maker", 20);
                }
                else//それ以外の時
                {
                    boxAnimationSprites[index] = ModHelpers.loadSpriteFromResources("", 175f);
                }
            }
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
