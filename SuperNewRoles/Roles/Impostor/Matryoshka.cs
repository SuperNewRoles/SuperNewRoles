using Hazel;
using SuperNewRoles.Helpers;
using UnityEngine;

namespace SuperNewRoles.Roles.Impostor;

public static class Matryoshka
{
    public static void FixedUpdate()
    {
        foreach (var Data in RoleClass.Matryoshka.Data)
        {
            if (Data.Value == null) continue;
            Data.Value.Reported = !CustomOptionHolder.MatryoshkaWearReport.GetBool();
            foreach (SpriteRenderer deadbody in Data.Value.bodyRenderers) deadbody.enabled = false;
            PlayerControl player = ModHelpers.PlayerById(Data.Key);
            Data.Value.transform.position = player.transform.position;
            if (!player.IsRole(RoleId.Matryoshka))
            {
                Set(player, null, false);
            }
        }
    }
    public static void WrapUp()
    {
        RoleClass.Matryoshka.Data = new();
    }
    public static void Set(PlayerControl source, PlayerControl target, bool Is)
    {
        if (Is)
        {
            source.setOutfit(target.Data.DefaultOutfit);
        }
        else
        {
            source.setOutfit(source.Data.DefaultOutfit);
        }
        if (!Is)
        {
            if (RoleClass.Matryoshka.Data.ContainsKey(source.PlayerId))
            {
                if (RoleClass.Matryoshka.Data[source.PlayerId] != null)
                {
                    RoleClass.Matryoshka.Data[source.PlayerId].Reported = false;
                    foreach(SpriteRenderer render in RoleClass.Matryoshka.Data[source.PlayerId].bodyRenderers) render.enabled = true;
                }
                RoleClass.Matryoshka.Data.Remove(source.PlayerId);
            }
        }
        else
        {
            DeadBody[] array = UnityEngine.Object.FindObjectsOfType<DeadBody>();
            for (int i = 0; i < array.Length; i++)
            {
                if (GameData.Instance.GetPlayerById(array[i].ParentId).PlayerId == target.PlayerId)
                {
                    RoleClass.Matryoshka.Data[source.PlayerId] = array[i];
                }
            }
        }
    }
    public static void RpcSet(PlayerControl target, bool Is)
    {
        MessageWriter writer = RPCHelper.StartRPC(CustomRPC.SetMatryoshkaDeadbody);
        writer.Write(CachedPlayer.LocalPlayer.PlayerId);
        writer.Write(target == null ? (byte)255 : target.PlayerId);
        writer.Write(Is);
        writer.EndRPC();
        RPCProcedure.SetMatryoshkaDeadBody(CachedPlayer.LocalPlayer.PlayerId, target == null ? (byte)255 : target.PlayerId, Is);
    }
}