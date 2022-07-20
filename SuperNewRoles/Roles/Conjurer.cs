using System;
using System.Collections.Generic;
using System.Linq;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomRPC;
using UnityEngine;

namespace SuperNewRoles.Roles
{
    public class Conjurer
    {
        //ベクトル内積
        public static double dot_product(Vector3 vl, Vector3 vr)
        {
            return vl.x * vr.x + vl.y * vr.y + vl.z * vr.z;
        }
        public static bool TriangleArea(Vector3 A, Vector3 B, Vector3 C, Vector3 P)
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


            Vector3 AB = sub_vector(B, A);
            Vector3 BP = sub_vector(C, B);

            Vector3 BC = sub_vector(C, B);
            Vector3 CP = sub_vector(P, C);

            Vector3 CA = sub_vector(A, C);
            Vector3 AP = sub_vector(P, A);

            Vector3 c1 = cross_product(AB, BP);
            Vector3 c2 = cross_product(BC, CP);
            Vector3 c3 = cross_product(CA, AP);

            //内積で順方向か逆方向か調べる
            double dot_12 = dot_product(c1, c2);
            double dot_13 = dot_product(c1, c3);

            if (dot_12 > 0 && dot_13 > 0)
            {
                //三角形の内側に点がある
                //SuperNewRolesPlugin.Logger.LogInfo("TriangleAreaがtrue");
                return true;
            }
            return false;
        }



        //Buttonのクールリセット
        public static void AllCoolReset()
        {
            HudManagerStartPatch.ConjurerAddButton.MaxTimer = RoleClass.Conjurer.CoolTime;
            HudManagerStartPatch.ConjurerAddButton.Timer = RoleClass.Conjurer.CoolTime;
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
                // if (PlayerControl.LocalPlayer.isRole(RoleId.Conjurer) || p.isDead())//魔術師と死人のとき
                {
                    /*アニメーション*/
                    CustomAnimation.Animation Conjurer_Marker_Animation = new();
                    Conjurer_Marker_Animation.Sprites = CustomAnimation.LoadSprites.GetSpritesAgartha("SuperNewRoles.Resources.Animation.Conjurer_Maker_30fps", 60);
                    /*========1個目==========*/
                    Transform Conjurer_Marker1 = GameObject.Instantiate(GameObject.Find("Marker" + RoleClass.Conjurer.AddedCount).transform);
                    Conjurer_Marker_Animation.Start(30, Conjurer_Marker1);
                }
                /* else//それ以外の時
                 {
                     boxAnimationSprites[index] = null;
                 }*/
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
            gameObject = new GameObject("Marker" + RoleClass.Conjurer.AddedCount) { layer = 11 };
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

