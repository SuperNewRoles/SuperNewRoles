using System;
using System.Collections.Generic;
using System.Text;
using SuperNewRoles.Replay.ReplayActions;
using UnityEngine;

namespace SuperNewRoles.Replay
{
    public class ReplayTurn
    {
        public Dictionary<byte, List<Vector2>> Positions;
        public List<ReplayAction> Actions;
    }
}
