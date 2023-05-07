using System;
using System.Collections.Generic;
using System.Text;

namespace SuperNewRoles.Replay
{
    public class ReplayPlayer
    {
        public byte PlayerId;
        public string PlayerName;
        public int ColorId;
        public string HatId;
        public string PetId;
        public string VisorId;
        public string NamePlateId;
        public string SkinId;
        public List<(uint,byte)> Tasks;
        public RoleId RoleId;
        public bool IsImpostor;
    }
}
