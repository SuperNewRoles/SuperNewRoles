using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SuperNewRoles.Patches.CursedTasks;

public class CursedWireTask
{
    public static readonly int[] WiresOrder = { 4, 8, 16, 32, 64 };
    public static float ScalarY;

    [HarmonyPatch(typeof(WireMinigame))]
    public static class WireMinigamePatch
    {
        [HarmonyPatch(nameof(WireMinigame.Begin)), HarmonyPrefix]
        public static void BeginPrefix(WireMinigame __instance)
        {
            if (!Main.IsCursed)
            {
                WireMinigame.colors = new Color[4]
                {
                    new Color(1f, 0f, 0f),
                    new Color(0.15f, 0.15f, 1f),
                    new Color(1f, 0.9216f, 0.0157f),
                    new Color(1f, 0f, 1f),
                };
                return;
            }
            int NumWires = WiresOrder[Random.RandomRange(0, WiresOrder.Length)];

            ScalarY = NumWires < 12 ? 1f : (8f / NumWires) + 0.3f;

            Transform ParentAll = __instance.transform;
            __instance.ExpectedWires = new sbyte[NumWires];
            WireMinigame.colors = new Color[NumWires];
            __instance.Symbols = new Sprite[NumWires];
            __instance.ActualWires = new sbyte[NumWires];
            __instance.LeftLights = new SpriteRenderer[NumWires];
            __instance.RightLights = new SpriteRenderer[NumWires];
            __instance.LeftNodes = new Wire[NumWires];
            __instance.RightNodes = new WireNode[NumWires];

            Transform ParentLeftNode = ParentAll.FindChild("LeftWires").transform;
            for (int i = 0; i < ParentLeftNode.childCount; i++)
            {
                GameObject ChildNode = ParentLeftNode.GetChild(i).gameObject;
                if (!ChildNode.name.Contains("WireNode")) continue;
                Object.Destroy(ChildNode);
            }

            float positionY = 2.25f;
            for (int i = 0; i < NumWires; i++)
            {
                positionY -= 4.6f / (NumWires + 1);
                GameObject NewLeftNode = Object.Instantiate(ParentLeftNode.FindChild("LeftWireNode").gameObject, ParentLeftNode.FindChild("LeftWireNode").gameObject.transform.parent);
                NewLeftNode.transform.localPosition = new Vector3(NewLeftNode.transform.localPosition.x, positionY, NewLeftNode.transform.localPosition.z);
                NewLeftNode.transform.FindChild("BaseSymbol").gameObject.active = false;
                for (int j = 0; j < NewLeftNode.transform.childCount; j++) NewLeftNode.transform.GetChild(j).localScale = new Vector3(1f, ScalarY, 1f);
                Transform headTransform = NewLeftNode.transform.FindChild("Head");
                headTransform.localPosition = new Vector3(0.235f, headTransform.localPosition.y, headTransform.localPosition.z);
                headTransform.GetComponent<CircleCollider2D>().enabled = true;
                headTransform.GetComponent<CircleCollider2D>().radius = (1.5f / NumWires) + 0.1f;
                Wire wireComponent = NewLeftNode.GetComponent<Wire>();
                wireComponent.enabled = true;
                __instance.LeftNodes[i] = wireComponent;
            }

            Transform ParentRightNode = ParentAll.FindChild("RightWires").transform;
            for (int i = 0; i < ParentRightNode.childCount; i++)
            {
                GameObject ChildNode = ParentRightNode.GetChild(i).gameObject;
                if (!ChildNode.name.Contains("WireNode")) continue;
                Object.Destroy(ChildNode);
            }

            positionY = 2.25f;
            for (int i = 0; i < NumWires; i++)
            {
                positionY -= 4.6f / (NumWires + 1);
                GameObject NewRightNode = Object.Instantiate(ParentRightNode.FindChild("RightWireNode").gameObject, ParentRightNode.FindChild("RightWireNode").gameObject.transform.parent);
                NewRightNode.transform.localPosition = new Vector3(NewRightNode.transform.localPosition.x, positionY, NewRightNode.transform.localPosition.z);
                NewRightNode.transform.FindChild("BaseSymbol").gameObject.active = false;
                Transform headTransform = NewRightNode.transform.FindChild("electricity_wiresConnectBase");
                NewRightNode.transform.localScale = new Vector3(1f, ScalarY, 1f);
                headTransform.localPosition = new Vector3(0.4f, 0f, headTransform.localPosition.z);
                NewRightNode.transform.GetComponent<CircleCollider2D>().enabled = true;
                NewRightNode.GetComponent<CircleCollider2D>().radius = 0.45f;
                WireNode wireComponent = NewRightNode.GetComponent<WireNode>();
                wireComponent.enabled = true;
                __instance.RightNodes[i] = wireComponent;
            }

            ParentAll.FindChild("LeftLights").gameObject.active = false;
            ParentAll.FindChild("RightLights").gameObject.active = false;

            for (int i = 0; i < NumWires; i++)
            {
                __instance.ExpectedWires[i] = (sbyte)i;
                WireMinigame.colors[i] = Color.HSVToRGB((float)i / NumWires, 1f, 1f);
                __instance.ActualWires[i] = -1;
                __instance.Symbols[i] = new Sprite();
            }
        }

        [HarmonyPatch(nameof(WireMinigame.UpdateLights)), HarmonyPrefix]
        public static bool UpdateLightsPrefix()
        {
            if (!Main.IsCursed) return true;
            return false;
        }

        [HarmonyPatch(nameof(WireMinigame.CheckRightSide)), HarmonyPrefix]
        public static bool CheckRightSidePrefix(WireMinigame __instance, ref WireNode __result, Vector2 pos)
        {
            if (!Main.IsCursed) return true;
            Collider2D leftNode = __instance.myController.amTouching;
            int leftId = leftNode.transform.parent.GetComponent<Wire>().WireId;
            for (int i = 0; i < __instance.RightNodes.Length; i++)
            {
                WireNode wireNode = __instance.RightNodes[i];
                if (wireNode.hitbox.OverlapPoint(pos) && (__instance.ExpectedWires[leftId] == wireNode.WireId || __instance.RightNodes.Length < 16))
                    __result = wireNode;
            }

            if (!__result) __result = null;
            return false;
        }
    }
    [HarmonyPatch(typeof(Wire))]
    public static class WirePatch
    {
        [HarmonyPatch(nameof(Wire.ResetLine)), HarmonyPostfix]
        public static void ResetLinePostfix(Wire __instance, [HarmonyArgument(1)] bool reset)
        {
            if (!Main.IsCursed) return;
            if (reset)
            {
                __instance.ColorBase.transform.localScale = new Vector3(5f, ScalarY, 1f);
                __instance.ColorBase.transform.localPosition = new Vector3(-0.3f, 0f, 1f);
                return;
            }

            __instance.ColorBase.transform.localScale = new Vector3(7.8f, ScalarY, 1f);
            __instance.ColorBase.transform.localPosition = new Vector3(-0.22f, 0f, 1f);
            __instance.Liner.transform.localScale = new Vector3(__instance.Liner.transform.localScale.x, ScalarY, 1f);
        }
    }
}