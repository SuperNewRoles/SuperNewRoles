using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Map.Agartha.Patch
{
    public static class SetTasksClass
    {
        public static void SetTasks(this Transform MiraShip)
        {
            Transform FixWiring3 = MiraShip.FindChild("Cafe").FindChild("FixWiringConsole (3)");
            FixWiring3.gameObject.SetActive(true);
            FixWiring3.position = new Vector3(8.9f, 16f, 0.1f);
            FixWiring3.localScale *= 0.8f;
            FixWiring3.GetComponent<SpriteRenderer>().sprite = ImageManager.Task_FixWiring1;

            Transform FixWiring4 = MiraShip.FindChild("Locker").FindChild("FixWiringConsole (4)");
            FixWiring4.gameObject.SetActive(true);
            FixWiring4.position = new Vector3(22.6f, 24f, 0.1f);
            FixWiring4.localScale *= 0.8f;
            FixWiring4.GetComponent<SpriteRenderer>().sprite = ImageManager.Task_FixWiring1;

            Transform FixWiring = MiraShip.FindChild("LabHall").FindChild("FixWiringConsole");
            FixWiring.gameObject.SetActive(true);
            FixWiring.position = new Vector3(-2f, 1.55f, 0.1f);
            FixWiring.localScale *= 0.8f;
            FixWiring.GetComponent<SpriteRenderer>().sprite = ImageManager.Task_FixWiring1;
        }
    }
}
