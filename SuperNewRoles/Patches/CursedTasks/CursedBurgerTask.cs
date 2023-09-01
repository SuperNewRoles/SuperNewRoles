using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace SuperNewRoles.Patches.CursedTasks;

public class CursedBurgerTask
{
    [HarmonyPatch(typeof(BurgerMinigame))]
    public static class BurgerMinigamePatch
    {
        [HarmonyPatch(nameof(BurgerMinigame.Begin)), HarmonyPostfix]
        public static void BeginPostfix(BurgerMinigame __instance)
        {
            if (!Main.IsCursed) return;
            switch (Random.RandomRange(0f, 1f))
            {
                case <= 0.50f: // 50%
                    __instance.ExpectedToppings = new(6);
                    __instance.ExpectedToppings[0] = BurgerToppingTypes.Plate;
                    for (int i = 1; i < __instance.ExpectedToppings.Count; i++)
                    {
                        BurgerToppingTypes topping = (BurgerToppingTypes)IntRange.Next(0, 6);
                        bool set = __instance.ExpectedToppings.Count(t => t == topping) < topping switch
                        {
                            BurgerToppingTypes.TopBun => 1,
                            BurgerToppingTypes.BottomBun => 1,
                            BurgerToppingTypes.Lettuce => 3,
                            _ => 2
                        };
                        if (set) __instance.ExpectedToppings[i] = topping;
                        else i--;
                    }
                    break;
                case <= 0.70f: // 20%
                    __instance.ExpectedToppings = new(6);
                    __instance.ExpectedToppings[0] = BurgerToppingTypes.Plate;
                    BurgerToppingTypes bun = (new BurgerToppingTypes[] { BurgerToppingTypes.Meat, BurgerToppingTypes.Onion, BurgerToppingTypes.Tomato }).GetRandom();
                    __instance.ExpectedToppings[1] = bun;
                    __instance.ExpectedToppings[5] = bun;
                    for (int i = 2; i < __instance.ExpectedToppings.Count - 1; i++)
                    {
                        BurgerToppingTypes topping = (BurgerToppingTypes)IntRange.Next(2, 6);
                        bool set = __instance.ExpectedToppings.Count(t => t == topping) < topping switch
                        {
                            BurgerToppingTypes.TopBun => 1,
                            BurgerToppingTypes.BottomBun => 1,
                            BurgerToppingTypes.Lettuce => 3,
                            _ => 2
                        };
                        if (set) __instance.ExpectedToppings[i] = topping;
                        else i--;
                    }
                    break;
                case <= 0.90f: // 20%
                    __instance.ExpectedToppings = new(6);
                    __instance.ExpectedToppings[0] = BurgerToppingTypes.Plate;
                    __instance.ExpectedToppings[1] = BurgerToppingTypes.Lettuce;
                    __instance.ExpectedToppings[5] = BurgerToppingTypes.Lettuce;
                    for (int i = 2; i < __instance.ExpectedToppings.Count - 1; i++)
                    {
                        BurgerToppingTypes topping = (new BurgerToppingTypes[] { BurgerToppingTypes.Lettuce, BurgerToppingTypes.Onion, BurgerToppingTypes.Tomato }).GetRandom();
                        bool set = __instance.ExpectedToppings.Count(t => t == topping) < topping switch
                        {
                            BurgerToppingTypes.TopBun => 1,
                            BurgerToppingTypes.BottomBun => 1,
                            BurgerToppingTypes.Lettuce => 3,
                            _ => 2
                        };
                        if (set) __instance.ExpectedToppings[i] = topping;
                        else i--;
                    }
                    break;
                case <= 0.95f: // 5%
                    __instance.ExpectedToppings = new(3);
                    __instance.ExpectedToppings[0] = BurgerToppingTypes.Plate;
                    __instance.ExpectedToppings[1] = BurgerToppingTypes.BottomBun;
                    __instance.ExpectedToppings[2] = BurgerToppingTypes.TopBun;
                    break;
                case <= 1.00f: // 5%
                    __instance.ExpectedToppings = new(6);
                    __instance.ExpectedToppings[0] = BurgerToppingTypes.Plate;
                    if (BoolRange.Next(0.1f))
                    {
                        __instance.ExpectedToppings[1] = BurgerToppingTypes.Lettuce;
                        __instance.ExpectedToppings[5] = BurgerToppingTypes.Lettuce;
                    }
                    else
                    {
                        __instance.ExpectedToppings[1] = BurgerToppingTypes.BottomBun;
                        __instance.ExpectedToppings[5] = BurgerToppingTypes.TopBun;
                    }
                    for (int i = 2; i < __instance.ExpectedToppings.Count - 1; i++)
                    {
                        BurgerToppingTypes topping = (BurgerToppingTypes)IntRange.Next(2, 6);
                        if (__instance.ExpectedToppings.Count(t => t == topping) >= 2) i--;
                        else __instance.ExpectedToppings[i] = topping;
                    }
                    if (BoolRange.Next(0.01f))
                    {
                        BurgerToppingTypes burgerToppingTypes = __instance.ExpectedToppings[5];
                        __instance.ExpectedToppings[5] = __instance.ExpectedToppings[4];
                        __instance.ExpectedToppings[4] = burgerToppingTypes;
                    }
                    break;
            }

            for (int i = 0; i < __instance.PaperSlots.Length; i++)
            {
                if (i < __instance.ExpectedToppings.Count - 1) __instance.PaperSlots[i].sprite = __instance.PaperToppings[(int)__instance.ExpectedToppings[i + 1]];
                else __instance.PaperSlots[i].enabled = false;
            }
        }
    }
}
