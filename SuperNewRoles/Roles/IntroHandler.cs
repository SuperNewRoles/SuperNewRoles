using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static SuperNewRoles.Roles.EvilGambler;

namespace SuperNewRoles.Roles
{
    class IntroHandler
    {
        public static void Handler() {
            var e = UnityEngine.Object.FindObjectsOfType<SystemConsole>().FirstOrDefault(x => x.gameObject.name.Contains("panel_vitals"));
            if (e == null || Camera.main == null) return;
            var vitals = UnityEngine.Object.Instantiate(e.MinigamePrefab, Camera.main.transform, false);
            vitals.transform.SetParent(Camera.main.transform, false);
            vitals.transform.localPosition = new Vector3(0.0f, 0.0f, -50f);
            vitals.Begin(null);
        }
    }
}
