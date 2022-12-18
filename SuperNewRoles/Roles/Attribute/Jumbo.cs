using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SuperNewRoles.Roles.Attribute;

public static class Jumbo
{
    public static void FixedUpdate()
    {
        foreach (PlayerControl p in RoleClass.Jumbo.JumboPlayer)
        {
            if (p == null) continue;
            if (!RoleClass.Jumbo.JumboSize.ContainsKey(p.PlayerId)) RoleClass.Jumbo.JumboSize.Add(p.PlayerId, 0f);
            p.cosmetics.transform.localScale = Vector3.one * ((RoleClass.Jumbo.JumboSize[p.PlayerId] + 1f) * 0.5f);
            p.transform.FindChild("BodyForms").localScale = Vector3.one * (RoleClass.Jumbo.JumboSize[p.PlayerId] + 1f);
            p.transform.FindChild("Animations").localScale = Vector3.one * (RoleClass.Jumbo.JumboSize[p.PlayerId] + 1f);
            if (RoleClass.Jumbo.JumboSize[p.PlayerId] <= 2.4f)
            {
                Logger.Info((Time.deltaTime * 0.01f).ToString());
                RoleClass.Jumbo.JumboSize[p.PlayerId] += Time.deltaTime * 0.01f;
            }
        }
    }
}