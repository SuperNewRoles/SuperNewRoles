using System.Collections.Generic;
using Hazel;
using SuperNewRoles.Helpers;
using UnityEngine;

namespace SuperNewRoles.Roles.Impostor;

public static class Matryoshka
{
    public static void FixedUpdate()
    {
        foreach (KeyValuePair<PlayerControl, DeadBody> Data in (Dictionary<PlayerControl, DeadBody>)RoleClass.Matryoshka.Data)
        {
            if (Data.Value == null) continue;
            Data.Value.Reported = !CustomOptionHolder.MatryoshkaWearReport.GetBool();
            foreach (SpriteRenderer deadbody in Data.Value.bodyRenderers) deadbody.enabled = false;
            Data.Value.transform.position = Data.Key.transform.position;
            if (!Data.Key.IsRole(RoleId.Matryoshka))
            {
                Set(Data.Key, null, false);
            }
        }
    }
    public static void WrapUp()
    {
        RoleClass.Matryoshka.Data = new();
    }
    public static void Set(PlayerControl source, PlayerControl target, bool Is)
    {
        if (!Is)
        {
            source.setOutfit(source.Data.DefaultOutfit);
            if (!RoleClass.Matryoshka.Data.TryGetValue(source.PlayerId, out DeadBody deadBody))
                return;
            RoleClass.Matryoshka.Data.Remove(source.PlayerId);
            if (deadBody == null)
                return; ;
            deadBody.Reported = false;
            foreach (SpriteRenderer render in deadBody.bodyRenderers)
                render.enabled = true;
            DeadBodyManager.EndedUseDeadbody(deadBody, DeadBodyUser.Matryoshka);
        }
        else
        {
            DeadBody targetDeadBody = null;
            DeadBody[] array = UnityEngine.Object.FindObjectsOfType<DeadBody>();
            foreach (DeadBody deadBody in array)
            {
                if (deadBody.ParentId != target.PlayerId)
                    continue;
                targetDeadBody = deadBody;
                break;
            }
            if (targetDeadBody == null || DeadBodyManager.IsDeadbodyUsed(targetDeadBody))
                return;
            RoleClass.Matryoshka.Data[source.PlayerId] = targetDeadBody;
            source.setOutfit(target.Data.DefaultOutfit);
            DeadBodyManager.UseDeadbody(targetDeadBody, DeadBodyUser.Matryoshka);
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