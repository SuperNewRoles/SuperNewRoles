using System.Collections.Generic;
using Agartha;
using HarmonyLib;
using SuperNewRoles.MapCustoms;

namespace SuperNewRoles.Modules;

public static class VentDataPatch
{
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.ShowRole))]
    class IntroCutsceneOnDestroy
    {
        public static void Postfix()
        {
            VentData.VentMap = new();
            foreach (Vent vent in ShipStatus.Instance.AllVents)
            {
                VentData.VentMap.Add(vent.gameObject.name, new(vent));
            }
        }
    }
}
public static class VentDataModules
{
    public static void ConnectAllVent(bool connect)
    {
        Dictionary<string, VentData> ventMap = VentData.VentMap;
        switch (GameManager.Instance.LogicOptions.currentGameOptions.MapId)
        {
            case 0:
                //Skeld
                ventMap["NavVentNorth"].Vent.Right = connect ? ventMap["NavVentSouth"] : new Vent();
                ventMap["NavVentSouth"].Vent.Right = connect ? ventMap["NavVentNorth"] : new Vent();

                ventMap["ShieldsVent"].Vent.Left = connect ? ventMap["WeaponsVent"] : new Vent();
                ventMap["WeaponsVent"].Vent.Left = connect ? ventMap["ShieldsVent"] : new Vent();

                ventMap["ReactorVent"].Vent.Left = connect ? ventMap["UpperReactorVent"] : new Vent();
                ventMap["UpperReactorVent"].Vent.Left = connect ? ventMap["ReactorVent"] : new Vent();

                ventMap["SecurityVent"].Vent.Center = connect ? ventMap["ReactorVent"] : new Vent();
                ventMap["ReactorVent"].Vent.Center = connect ? ventMap["SecurityVent"] : new Vent();

                ventMap["REngineVent"].Vent.Center = connect ? ventMap["LEngineVent"] : new Vent();
                ventMap["LEngineVent"].Vent.Center = connect ? ventMap["REngineVent"] : new Vent();

                ventMap["AdminVent"].Vent.Center = connect ? ventMap["MedVent"] : new Vent();
                ventMap["MedVent"].Vent.Center = connect ? ventMap["AdminVent"] : new Vent();

                ventMap["CafeVent"].Vent.Center = connect ? ventMap["WeaponsVent"] : new Vent();
                ventMap["WeaponsVent"].Vent.Center = connect ? ventMap["CafeVent"] : new Vent();
                break;
            case 1:
                //MIRA HQ
                if (MapCustom.MiraAdditionalVents.GetBool())
                {
                    ventMap["AdditionalVent_12"].Vent.Center = connect ? ventMap["LaunchVent"] : new Vent();
                    ventMap["LaunchVent"].Vent.Center = connect ? ventMap["AdditionalVent_12"] : new Vent();

                    ventMap["AdditionalVent_13"].Vent.Right = connect ? ventMap["MedVent"] : new Vent();
                    ventMap["MedVent"].Vent.Center = connect ? ventMap["AdditionalVent_13"] : new Vent();

                    ventMap["AdditionalVent_14"].Vent.Center = connect ? ventMap["YHallRightVent"] : new Vent();
                    ventMap["YHallRightVent"].Vent.Center = connect ? ventMap["AdditionalVent_14"] : new Vent();
                }
                break;
            case 2:
                //Polus
                ventMap["CommsVent"].Vent.Center = connect ? ventMap["ElecFenceVent"] : new Vent();
                ventMap["ElecFenceVent"].Vent.Center = connect ? ventMap["CommsVent"] : new Vent();

                ventMap["ElectricalVent"].Vent.Center = connect ? ventMap["ElectricBuildingVent"] : new Vent();
                ventMap["ElectricBuildingVent"].Vent.Center = connect ? ventMap["ElectricalVent"] : new Vent();

                ventMap["ScienceBuildingVent"].Vent.Right = connect ? ventMap["BathroomVent"] : new Vent();
                ventMap["BathroomVent"].Vent.Center = connect ? ventMap["ScienceBuildingVent"] : new Vent();

                ventMap["AdminVent"].Vent.Center = connect ? ventMap["OfficeVent"] : new Vent();
                ventMap["OfficeVent"].Vent.Center = connect ? ventMap["AdminVent"] : new Vent();

                if (MapCustom.PolusAdditionalVents.GetBool())
                {
                    ventMap["AdditionalVent_12"].Vent.Center = connect ? ventMap["BathroomVent"] : new Vent();
                    ventMap["BathroomVent"].Vent.Left = connect ? ventMap["AdditionalVent_12"] : new Vent();

                    ventMap["AdditionalVent_13"].Vent.Right = connect ? ventMap["SouthVent"] : new Vent();
                    ventMap["SouthVent"].Vent.Left = connect ? ventMap["AdditionalVent_13"] : new Vent();

                    ventMap["AdditionalVent_14"].Vent.Center = connect ? ventMap["ScienceBuildingVent"] : new Vent();
                    ventMap["ScienceBuildingVent"].Vent.Center = connect ? ventMap["AdditionalVent_14"] : new Vent();
                }
                break;
            case 3 when MapData.IsMap(CustomMapNames.Agartha):
                break;
            case 4:
                //Airship
                ventMap["VaultVent"].Vent.Right = connect ? ventMap["GaproomVent1"] : new Vent();
                ventMap["GaproomVent1"].Vent.Center = connect ? ventMap["VaultVent"] : new Vent();

                ventMap["EjectionVent"].Vent.Right = connect ? ventMap["KitchenVent"] : new Vent();
                ventMap["KitchenVent"].Vent.Center = connect ? ventMap["EjectionVent"] : new Vent();

                ventMap["HallwayVent1"].Vent.Center = connect ? ventMap["HallwayVent2"] : new Vent();
                ventMap["HallwayVent2"].Vent.Center = connect ? ventMap["HallwayVent1"] : new Vent();

                ventMap["GaproomVent2"].Vent.Center = connect ? ventMap["RecordsVent"] : new Vent();
                ventMap["RecordsVent"].Vent.Center = connect ? ventMap["GaproomVent2"] : new Vent();

                if (MapCustom.AirShipAdditionalVents.GetBool())
                {
                    ventMap["AdditionalVent_12"].Vent.Left = connect ? ventMap["AdditionalVent_16"] : new Vent();
                    ventMap["AdditionalVent_16"].Vent.Center = connect ? ventMap["AdditionalVent_12"] : new Vent();

                    ventMap["AdditionalVent_12"].Vent.Center = connect ? ventMap["AdditionalVent_17"] : new Vent();
                    ventMap["AdditionalVent_17"].Vent.Center = connect ? ventMap["AdditionalVent_12"] : new Vent();

                    ventMap["AdditionalVent_13"].Vent.Center = connect ? ventMap["StorageVent"] : new Vent();
                    ventMap["StorageVent"].Vent.Center = connect ? ventMap["AdditionalVent_13"] : new Vent();

                    ventMap["AdditionalVent_14"].Vent.Center = connect ? ventMap["AdditionalVent_15"] : new Vent();
                    ventMap["AdditionalVent_15"].Vent.Center = connect ? ventMap["AdditionalVent_14"] : new Vent();

                    ventMap["AdditionalVent_14"].Vent.Left = connect ? ventMap["EjectionVent"] : new Vent();
                    ventMap["EjectionVent"].Vent.Center = connect ? ventMap["AdditionalVent_14"] : new Vent();

                    ventMap["AdditionalVent_15"].Vent.Left = connect ? ventMap["EngineVent"] : new Vent();
                    ventMap["EngineVent"].Vent.Center = connect ? ventMap["AdditionalVent_15"] : new Vent();

                    ventMap["AdditionalVent_17"].Vent.Right = connect ? ventMap["ShowersVent"] : new Vent();
                    ventMap["ShowersVent"].Vent.Center = connect ? ventMap["AdditionalVent_17"] : new Vent();
                }
                break;
            case 5:
                // Fungle
                ventMap["RecRoomVent"].Vent.Center = connect ? ventMap["KitchenVent"].Vent : new Vent();
                ventMap["KitchenVent"].Vent.Center = connect ? ventMap["RecRoomVent"].Vent : new Vent();

                ventMap["SouthWestJungleVent"].Vent.Center = connect ? ventMap["NorthWestJungleVent"].Vent : new Vent();
                ventMap["NorthWestJungleVent"].Vent.Center = connect ? ventMap["SouthWestJungleVent"].Vent : new Vent();

                ventMap["LookoutVent"].Vent.Center = connect ? ventMap["StorageVent"].Vent : new Vent();
                ventMap["StorageVent"].Vent.Center = connect ? ventMap["LookoutVent"].Vent : new Vent();

                ventMap["NorthEastJungleVent"].Vent.Center = connect ? ventMap["SouthEastJungleVent"].Vent : new Vent();
                ventMap["SouthEastJungleVent"].Vent.Center = connect ? ventMap["NorthEastJungleVent"].Vent : new Vent();

                ventMap["CommunicationsVent"].Vent.Center = connect ? ventMap["MeetingRoomVent"].Vent : new Vent();
                ventMap["MeetingRoomVent"].Vent.Center = connect ? ventMap["CommunicationsVent"].Vent : new Vent();

                ventMap["KitchenVent"].Vent.Right = connect ? ventMap["SouthEastJungleVent"].Vent : new Vent();
                ventMap["SouthEastJungleVent"].Vent.Right = connect ? ventMap["KitchenVent"].Vent : new Vent();
                break;
        }
    }
}
public class VentData
{
    public static Dictionary<string, VentData> VentMap;

    public Vent Vent { get; }

    public int Id { get; }

    public bool PreSealed, Sealed;

    public VentData(Vent vent)
    {
        Vent = vent;
        Id = vent.Id;
        PreSealed = false;
        Sealed = false;
    }

    public static implicit operator Vent(VentData vent)
    {
        return vent.Vent;
    }
}