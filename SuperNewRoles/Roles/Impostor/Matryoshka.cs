using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hazel;
using SuperNewRoles.Helpers;
using UnityEngine;

namespace SuperNewRoles.Roles.Impostor
{
    public static class Matryoshka
    {
        public static void FixedUpdate()
        {
            foreach (var Data in RoleClass.Matryoshka.Datas)
            {
                if (Data.Value == null) continue;
                Data.Value.Reported = !CustomOptions.MatryoshkaWearReport.GetBool();
                Data.Value.bodyRenderer.enabled = false;
                Data.Value.transform.position = ModHelpers.PlayerById(Data.Key).transform.position;
            }
        }
        public static void WrapUp()
        {
            RoleClass.Matryoshka.Datas = new();
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
                if (RoleClass.Matryoshka.Datas.ContainsKey(source.PlayerId))
                {
                    if (RoleClass.Matryoshka.Datas[source.PlayerId] != null)
                    {
                        RoleClass.Matryoshka.Datas[source.PlayerId].Reported = false;
                        if (RoleClass.Matryoshka.Datas[source.PlayerId].bodyRenderer != null)
                            RoleClass.Matryoshka.Datas[source.PlayerId].bodyRenderer.enabled = true;
                    }
                    RoleClass.Matryoshka.Datas.Remove(source.PlayerId);
                }
            }
            else
            {
                DeadBody[] array = UnityEngine.Object.FindObjectsOfType<DeadBody>();
                for (int i = 0; i < array.Length; i++)
                {
                    if (GameData.Instance.GetPlayerById(array[i].ParentId).PlayerId == target.PlayerId)
                    {
                        RoleClass.Matryoshka.Datas[source.PlayerId] = array[i];
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
}