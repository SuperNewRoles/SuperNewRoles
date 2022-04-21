using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Map.Agartha.Patch
{
    public static class SetTasksClass
    {
        public static List<Console> AddConsoles;
        public static void SetTasks(this Transform MiraShip)
        {
            AddConsoles = new List<Console>();
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
            FixWiring3.localPosition = new Vector3(-4.5f, 0f, 0.1f);
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
            MedScanner.position = new Vector3(-2.2f, 13.1f, 0.1f);

            Transform MedBayConsole = GameObject.Instantiate(MapLoader.SkeldObject.transform.FindChild("Medical").FindChild("Ground").FindChild("MedBayConsole"));
            MedBayConsole.gameObject.SetActive(true);
            MedBayConsole.position = new Vector3(2.2f, 14.4f, 0.1f);
            AddConsoles.Add(MedBayConsole.GetComponent<Console>());

            Transform Upload1 = GameObject.Instantiate(MapLoader.Skeld.transform.FindChild("Admin").FindChild("Ground").FindChild("admin_walls").FindChild("UploadDataConsole"));
            Upload1.GetComponent<Console>().ConsoleId = 0;
            Upload1.position = new Vector3(15.1f, 20.4f, 4f);
            AddConsoles.Add(Upload1.GetComponent<Console>());

            Transform Download_Aisle1 = GameObject.Instantiate(MapLoader.Skeld.transform.FindChild("Cockpit").FindChild("Ground").FindChild("UploadDataConsole"));
            Download_Aisle1.GetComponent<Console>().ConsoleId = 1;
            Download_Aisle1.position = new Vector3(14.1f, 0.9f, 4f);
            AddConsoles.Add(Download_Aisle1.GetComponent<Console>());

            Transform Download_Aisle2 = GameObject.Instantiate(Download_Aisle1);
            Download_Aisle2.GetComponent<Console>().ConsoleId = 2;
            Download_Aisle2.position = new Vector3(21.6f, 9.4f, 4f);
            AddConsoles.Add(Download_Aisle2.GetComponent<Console>());

            Transform Download_Aisle3 = GameObject.Instantiate(Download_Aisle1);
            Download_Aisle3.GetComponent<Console>().ConsoleId = 3;
            Download_Aisle3.position = new Vector3(17.7f, 24.3f, 4f);
            AddConsoles.Add(Download_Aisle3.GetComponent<Console>());

            Transform Download_Aisle4 = GameObject.Instantiate(Download_Aisle1);
            Download_Aisle4.GetComponent<Console>().ConsoleId = 4;
            Download_Aisle4.position = new Vector3(-5, 5.9f, 4f);
            AddConsoles.Add(Download_Aisle4.GetComponent<Console>());

            Transform Download_Aisle5 = GameObject.Instantiate(Download_Aisle1);
            Download_Aisle5.GetComponent<Console>().ConsoleId = 5;
            Download_Aisle5.position = new Vector3(2f, 1.5f, 4f);
            AddConsoles.Add(Download_Aisle5.GetComponent<Console>());

            Transform Download_Aisle6 = GameObject.Instantiate(Download_Aisle1);
            Download_Aisle6.GetComponent<Console>().ConsoleId = 6;
            Download_Aisle6.position = new Vector3(-2.2f, 23.5f, 4f);
            AddConsoles.Add(Download_Aisle6.GetComponent<Console>());

            Transform reactordesc = MiraShip.FindChild("Reactor").FindChild("reactor-desk-elec");
            reactordesc.gameObject.SetActive(true);
            Transform DivertPowerConsoleMain = reactordesc.FindChild("DivertPowerConsoleMain");
            DivertPowerConsoleMain.GetComponent<Console>().ConsoleId = 0;
            DivertPowerConsoleMain.position = new Vector3(21.1f, 11.3f, 4f);

            Transform DivertPowerConsole1 = MiraShip.FindChild("Comms").FindChild("DivertPowerConsole (9)");
            DivertPowerConsole1.gameObject.SetActive(true);
            DivertPowerConsole1.GetComponent<Console>().ConsoleId = 1;
            DivertPowerConsole1.position = new Vector3(1.5f, 23.5f, 4f);

            Transform DivertPowerConsole2 = MiraShip.FindChild("Cafe").FindChild("DivertPowerConsole (3)");
            DivertPowerConsole2.gameObject.SetActive(true);
            DivertPowerConsole2.GetComponent<Console>().ConsoleId = 2;
            DivertPowerConsole2.Rotate(new Vector3(75, 0, 0));
            DivertPowerConsole2.localPosition = new Vector3(1f, -4.5f, 4f);

            Transform DivertPowerConsole3 = MiraShip.FindChild("Laboratory").FindChild("DivertPowerConsole (6)");
            DivertPowerConsole3.gameObject.SetActive(true);
            DivertPowerConsole3.GetComponent<Console>().ConsoleId = 3;
            DivertPowerConsole3.position = new Vector3(19.7f, 24.4f, 4f);

            Transform DivertPowerConsole4 = MiraShip.FindChild("MedBay").FindChild("DivertPowerConsole (8)");
            DivertPowerConsole4.gameObject.SetActive(true);
            DivertPowerConsole4.GetComponent<Console>().ConsoleId = 4;
            DivertPowerConsole4.position = new Vector3(-2.7f, 15.3f, 4f);

            Transform DivertPowerConsole5 = MiraShip.FindChild("Admin").FindChild("DivertPowerConsoleAdmin");
            DivertPowerConsole5.gameObject.SetActive(true);
            DivertPowerConsole5.GetComponent<Console>().ConsoleId = 5;
            DivertPowerConsole5.position = new Vector3(8.9f, 0.9f, 4f);

            Transform OfficeStant = MiraShip.FindChild("Office").FindChild("divertElevStand");
            OfficeStant.gameObject.SetActive(true);
            Transform DivertPowerConsole6 = OfficeStant.FindChild("DivertPowerConsoleOffice");
            DivertPowerConsole6.gameObject.SetActive(true);
            DivertPowerConsole6.GetComponent<Console>().ConsoleId = 6;
            DivertPowerConsole6.position = new Vector3(-6.9f, -0.8f, 4f);

        }
        public static void ShipSetTask()
        {
            List<NormalPlayerTask> CommonTasks = new List<NormalPlayerTask>();
            List<NormalPlayerTask> NormalTasks = new List<NormalPlayerTask>();
            List<NormalPlayerTask> LongTasks = new List<NormalPlayerTask>();
            foreach (NormalPlayerTask task in ShipStatus.Instance.CommonTasks)
            {
                switch (task.name)
                {
                    case "FixWiring 1":
                    case "EnterCodeTask":
                        CommonTasks.Add(task);
                        break;
                }
                SuperNewRolesPlugin.Logger.LogInfo("(C)" + task.name);
            }
            foreach (NormalPlayerTask task in ShipStatus.Instance.NormalTasks)
            {
                switch (task.name)
                {
                    case "VentCleaning":
                        NormalTasks.Add(task);
                        break;
                }
                SuperNewRolesPlugin.Logger.LogInfo("(N)" + task.name);
            }
            foreach (NormalPlayerTask task in ShipStatus.Instance.LongTasks)
            {
                switch (task.name)
                {
                    case "DivertHqCommsPower":
                    case "DivertCafePower":
                    case "DivertLaunchPadPower":
                    case "DivertMedbayPower":
                    case "DivertAdminPower":
                    case "DivertOfficePower":
                    case "MedScanTask":
                        LongTasks.Add(task);
                        break;

                }
                SuperNewRolesPlugin.Logger.LogInfo("(L)" + task.name);
            }
            foreach (NormalPlayerTask task in MapLoader.Skeld.LongTasks)
            {
                switch (task.name)
                {
                    case "InspectSample":
                        LongTasks.Add(task);
                        break;

                }
                SuperNewRolesPlugin.Logger.LogInfo("(Skeld)(L)" + task.name);
            }
            foreach (NormalPlayerTask task in MapLoader.Skeld.CommonTasks)
            {
                switch (task.name)
                {
                    case "InspectSample":
                        break;
                }
                SuperNewRolesPlugin.Logger.LogInfo("(Skeld)(C)" + task.name);
            }
            foreach (NormalPlayerTask task in MapLoader.Skeld.NormalTasks)
            {
                switch (task.name)
                {
                    case "UploadNav":
                        NormalTasks.Add(task);
                        break;
                }
                SuperNewRolesPlugin.Logger.LogInfo("(Skeld)(N)" + task.name);
            }
            foreach (NormalPlayerTask task in MapLoader.Airship.LongTasks)
            {
                switch (task.name)
                {
                    case "InspectSample":
                        break;

                }
                SuperNewRolesPlugin.Logger.LogInfo("(Airship)(L)" + task.name);
            }
            foreach (NormalPlayerTask task in MapLoader.Airship.CommonTasks)
            {
                switch (task.name)
                {
                    case "InspectSample":
                        break;
                }
                SuperNewRolesPlugin.Logger.LogInfo("(Airship)(C)" + task.name);
            }
            foreach (NormalPlayerTask task in MapLoader.Airship.NormalTasks)
            {
                switch (task.name)
                {
                    case "UploadNav":
                        break;
                }
                SuperNewRolesPlugin.Logger.LogInfo("(Airship)(N)" + task.name);
            }

            ShipStatus.Instance.CommonTasks = CommonTasks.ToArray();
            ShipStatus.Instance.NormalTasks = NormalTasks.ToArray();
            ShipStatus.Instance.LongTasks = LongTasks.ToArray();

            List<Console> newconsole = ShipStatus.Instance.AllConsoles.ToList();
            foreach (Console console in newconsole) {
                SuperNewRolesPlugin.Logger.LogInfo("コンソール名:"+console.name);
            }
            foreach (Console console in MapLoader.Airship.AllConsoles.ToList())
            {
                SuperNewRolesPlugin.Logger.LogInfo("(Airship)コンソール名:" + console.name);
            }
            foreach (Console console in AddConsoles)
            {
                newconsole.Add(console);
            }
            ShipStatus.Instance.AllConsoles = newconsole.ToArray();
        }
        public static void SetSabotage(Transform Miraship)
        {
            Transform O2_Locker = Miraship.FindChild("Locker").FindChild("NoOxyConsole");
            O2_Locker.gameObject.SetActive(true);
            O2_Locker.position = new Vector3(-2.5221f, 9.813f, 0.1f);

            Transform GardenGreenHousePanel = Miraship.FindChild("Garden").FindChild("greenhousePanel");
            GardenGreenHousePanel.gameObject.SetActive(true);
            Transform O2_Garden = GardenGreenHousePanel.FindChild("NoOxyConsole");
            O2_Garden.gameObject.SetActive(true);
            O2_Garden.position = new Vector3(1.8404f, 9.813f, 0.1f);

            Transform Comms_Comm = Miraship.FindChild("Comms").FindChild("comms-top").FindChild("FixCommsConsole");
            Comms_Comm.gameObject.SetActive(true);
            Comms_Comm.position = new Vector3(0.6f, 24f, 0.1f);
            Comms_Comm.localScale *= 2f;

            Transform OfficeMid = Miraship.FindChild("Office").FindChild("office-mid");
            OfficeMid.gameObject.SetActive(true);
            Transform Comms_Ofice = OfficeMid.FindChild("FixCommsConsole");
            Comms_Ofice.gameObject.SetActive(true);
            Comms_Ofice.position = new Vector3(-1.1f, 23.5f, 0.1f);

            Transform Powerdown = Miraship.FindChild("Office").FindChild("SwitchConsole");
            Powerdown.gameObject.SetActive(true);
            Powerdown.position = new Vector3(19.8f, 14.7f, 0.1f);
        }
    }
}
