using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cpp2IL.Core.InstructionSets;

namespace SuperNewRoles.Modules;
public class VentInfo
{
    public static Dictionary<int, VentInfo> VentInfos = null;
    public Vent Vent { get; }
    public int VentId { get; }
    public int UsedCount { get; private set; }
    public int UsedCountCurrentTurn { get; private set; }
    public List<byte> UsedPlayers { get; private set; }
    public List<byte> UsedPlayersCurrentTurn;

    public VentInfo(Vent vent)
    {
        if (vent == null)
            throw new ArgumentNullException(nameof(vent)+" is null");
        if (VentInfos.ContainsKey(vent.Id))
            throw new ArgumentException($"VentInfo already exists: {vent.Id}");
        Vent = vent;
        VentId = vent.Id;
        UsedCount = 0;
        UsedCountCurrentTurn = 0;
        UsedPlayers = new();
        UsedPlayersCurrentTurn = new();
        VentInfos.Add(VentId, this);
    }
    public static VentInfo GetVentInfoById(int ventId)
    {
        return VentInfos.TryGetValue(ventId, out VentInfo ventInfo) ? ventInfo : null;
    }
    public static void ShipStatusAwake()
    {
        VentInfos = new();
        foreach (Vent vent in ShipStatus.Instance.AllVents)
        {
            _ = new VentInfo(vent);
        }
    }
    public static void OnEnterVent(PlayerControl player, int id)
    {
        if (VentInfos.TryGetValue(id, out VentInfo ventInfo))
        {
            ventInfo.UsedCount++;
            ventInfo.UsedCountCurrentTurn++;
            ventInfo.UsedPlayers.Add(player.PlayerId);
            ventInfo.UsedPlayersCurrentTurn.Add(player.PlayerId);
        }
    }
    public static void OnExitVent(PlayerControl player, int id)
        => OnEnterVent(player, id);
    public static void OnWrapUp()
    {
        foreach (VentInfo ventInfo in VentInfos.Values)
        {
            ventInfo.UsedCountCurrentTurn = 0;
            ventInfo.UsedPlayersCurrentTurn.Clear();
        }
    }
}