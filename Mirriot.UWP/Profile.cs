using System.Runtime.Serialization;

namespace Miriot
{
    [DataContract]
    public class Profile
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string ImagePath { get; set; }
    }
}
