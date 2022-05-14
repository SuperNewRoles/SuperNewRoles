using System;
using System.Collections.Generic;
using System.Text;
using BepInEx.IL2CPP.Utils;
using PowerTools;
using System.Collections;
using System.Linq;
using UnityEngine;
using Hazel;
using SuperNewRoles.Helpers;
using HarmonyLib;
using BepInEx.IL2CPP.Utils;
using PowerTools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Sabotage.Blizzard.Patch
{
    [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Awake))]
    class Setposition
    {
        public static void Postfix(ShipStatus __instance)
        {
            if (Options.BlizzardSetting.getBool())
            {
                if (PlayerControl.GameOptions.MapId == 0)//Skeld
                {
                    Transform Ship = GameObject.Find("SkeldShip(Clone)").transform;
                    SetObject0(Ship);
                }
                if (PlayerControl.GameOptions.MapId == 1)//Mira
                {
                    Transform Ship = GameObject.Find("MiraShip(Clone)").transform;
                    SetObject1(Ship);
                }
                if (PlayerControl.GameOptions.MapId == 2)//Polus
                {
                    Transform Ship = GameObject.Find("PolusShip(Clone)").transform;
                    SetObject2(Ship);
                }
                /*if (PlayerControl.GameOptions.MapId == 3)//dlekS ehT
                {
                    //SetObject3();
                }*/
                if (PlayerControl.GameOptions.MapId == 4)//AirShip
                {
                    Transform Ship = GameObject.Find("AirShip(Clone)").transform;
                    SetObject4(Ship);
                }
                if (PlayerControl.GameOptions.MapId == 5)//Agartha
                {
                    //SetObject5();
                }
            }
        }

        public static void SetObject0(Transform Skeld)
        {
            Transform Template = GameObject.Instantiate(Skeld.FindChild("Cockpit").FindChild("DivertPowerConsole"));//クローン
            Template.gameObject.SetActive(true);

            Transform Object_ONDO0 = GameObject.Instantiate(Template, Skeld);//Templateのクローン (Skeldの記述はペア)
            Object_ONDO0.localPosition = new Vector3(1.1f, 0.5f, -0.1f);//座標セット
            Object_ONDO0.GetComponent<SpriteRenderer>().sprite = ImageManager.ONDOgetSprite("ONDO");//見た目
            Object_ONDO0.gameObject.AddComponent<PolygonCollider2D>();//判定
            Object_ONDO0.name = "ONDO";

            GameObject.Destroy(Template.gameObject);
            Template = null;
        }
        public static void SetObject1(Transform Mira)
        {
            Transform Template = GameObject.Instantiate(Mira.FindChild("Office").FindChild("computerTableB"));
            //GameObject.Destroy(Template.GetComponent<PolygonCollider2D>());
            Template.gameObject.SetActive(true);
            Template.localScale *= 0.5f;
            
            Transform Object_ONDO0 = GameObject.Instantiate(Template, Mira);
            Object_ONDO0.localPosition = new Vector3(19.8f, 3.225f, -0.1f);
            Object_ONDO0.GetComponent<SpriteRenderer>().sprite = ImageManager.ONDOgetSprite("ONDO");
            Object_ONDO0.gameObject.AddComponent<PolygonCollider2D>();
            Object_ONDO0.name = "ONDO";

            GameObject.Destroy(Template.gameObject);
            Template = null;
        }
        public static void SetObject2(Transform Polus)
        {
            Transform Template = GameObject.Instantiate(Polus.FindChild("Science").FindChild("panel_tempcold"));
            //GameObject.Destroy(Template.GetComponent<PolygonCollider2D>());
            Template.gameObject.SetActive(true);
            Template.localScale *= 1f;
            
            Transform Object_ONDO0 = GameObject.Instantiate(Template, Polus);
            Object_ONDO0.localPosition = new Vector3(19.8f, 3.225f, -0.1f);
            Object_ONDO0.GetComponent<SpriteRenderer>().sprite = ImageManager.ONDOgetSprite("ONDO");
            Object_ONDO0.gameObject.AddComponent<PolygonCollider2D>();
            Object_ONDO0.name = "ONDO";

            GameObject.Destroy(Template.gameObject);
            Template = null;
        }
        //3はドレクスのIdなので省略
        public static void SetObject4(Transform Airship)
        {
            Transform Template = GameObject.Instantiate(Airship.FindChild("Office").FindChild("computerTableB"));
            //GameObject.Destroy(Template.GetComponent<PolygonCollider2D>());
            Template.gameObject.SetActive(true);
            Template.localScale *= 0.5f;
            
            Transform Object_ONDO0 = GameObject.Instantiate(Template, Airship);        
            Object_ONDO0.localPosition = new Vector3(19.8f, 3.225f, -0.1f);
            Object_ONDO0.GetComponent<SpriteRenderer>().sprite = ImageManager.ONDOgetSprite("ONDO");
            Object_ONDO0.gameObject.AddComponent<PolygonCollider2D>();
            Object_ONDO0.name = "ONDO";

            GameObject.Destroy(Template.gameObject);
            Template = null;
        }
        public static void SetObject5(Transform Agartha)
        {
            Transform Template = GameObject.Instantiate(Agartha.FindChild("Office").FindChild("computerTableB"));
            //GameObject.Destroy(Template.GetComponent<PolygonCollider2D>());
            Template.gameObject.SetActive(true);
            Template.localScale *= 0.5f;

            Transform Object_ONDO0 = GameObject.Instantiate(Template, Agartha);
            Object_ONDO0.localPosition = new Vector3(19.8f, 3.225f, -0.1f);
            Object_ONDO0.GetComponent<SpriteRenderer>().sprite = ImageManager.ONDOgetSprite("ONDO");
            Object_ONDO0.gameObject.AddComponent<PolygonCollider2D>();
            Object_ONDO0.name = "ONDO";

            GameObject.Destroy(Template.gameObject);
            Template = null;
        }        
    }
}
