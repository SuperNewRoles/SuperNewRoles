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
            Transform FixWiring = MiraShip.FindChild("LabHall").FindChild("FixWiringConsole");
            FixWiring.gameObject.SetActive(true);
            FixWiring.position = new Vector3(-2.2f, 20.7f, 0.1f);
            FixWiring.localScale *= 0.8f;
            FixWiring.Rotate(new Vector3(75, 0, 0));
            FixWiring.GetComponent<SpriteRenderer>().sprite = ImageManager.Task_FixWiring1;

            Transform FixWiring1 = MiraShip.FindChild("Garden").FindChild("FixWiringConsole");
            //FixWiring1.gameObject.SetActive(true);
            FixWiring1.position = new Vector3(-2f, 1.55f, 0.1f);
            FixWiring1.localScale *= 0.8f;
            FixWiring1.GetComponent<SpriteRenderer>().sprite = ImageManager.Task_FixWiring1;

            //研究室
            Transform FixWiring2 = MiraShip.FindChild("SkyBridge").FindChild("FixWiringConsole (2)");
            FixWiring2.gameObject.SetActive(true);
            FixWiring2.position = new Vector3(20.5f, 2.5f, 0.1f);
            FixWiring2.localScale *= 0.8f;
            FixWiring2.Rotate(new Vector3(75, 0, 0));
            FixWiring2.GetComponent<SpriteRenderer>().sprite = ImageManager.Task_FixWiring1;

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

            Transform FixWiring5 = GameObject.Instantiate(MiraShip.FindChild("Locker").FindChild("FixWiringConsole (4)"));
            FixWiring5.name = "FixWiringConsole (5)";
            FixWiring5.GetComponent<Console>().ConsoleId = 5;
            //FixWiring5.gameObject.SetActive(true);
            FixWiring5.position = new Vector3(-12.8f, 4f, 0.1f);
            FixWiring5.localScale *= 0.8f;
            FixWiring5.Rotate(new Vector3(0, 60, 90));
            FixWiring5.GetComponent<SpriteRenderer>().sprite = ImageManager.Task_FixWiring1;

            Transform MedScanner = MiraShip.FindChild("MedBay").FindChild("MedScanner");
            //MedScanner.gameObject.SetActive(true);
            MedScanner.position = new Vector3(-2.2f, 13.1f, 0.1f);
            //MedScanner.localScale *= 0.8f;
            //MedScanner.GetComponent<SpriteRenderer>().sprite = ImageManager.Task_FixWiring1;
        }
    }
}
