using System.Collections.Generic;
using System.Runtime.Serialization;
using ACMESharp.Jose;

namespace ACMESharp.Proto.Resources
{
    [DataContract]
    public class Registration : Resource
    {
        [DataMember(Name = "key")]
        public Jwk Key
        { get; set; }
        
        [DataMember(Name = "status")]
        public string Status
        { get; set; }
        
        [DataMember(Name = "contact")]
        public IEnumerable<string> Contact
        { get; set; }
        
        [DataMember(Name = "agreement")]
        public string Agreement
        { get; set; }
        
        [DataMember(Name = "applications")]
        public string Applications
        { get; set; }
        
        [DataMember(Name = "certificates")]
        public string Certificates
        { get; set; }
        
    }
}