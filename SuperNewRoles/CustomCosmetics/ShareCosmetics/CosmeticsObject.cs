using System.Runtime.Serialization;

namespace SuperNewRoles.CustomCosmetics.ShareCosmetics
{
    [DataContract]
    public partial class CosmeticsObject
    {
        [DataMember(Name = "GUID")]
        public string GUID { get; set; }
        [DataMember(Name = "AllNamePlates")]
        public NamePlatesObject[] AllNamePlates { get; set; }
    }
    [DataContract]
    public partial class NamePlatesObject
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }
        [DataMember(Name = "author")]
        public string Author { get; set; }
        [DataMember(Name = "url")]
        public string Url { get; set; }
    }
}