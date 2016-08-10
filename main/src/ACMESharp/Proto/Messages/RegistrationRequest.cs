using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ACMESharp.Proto.Messages
{
    [DataContract]
    public class RegistrationRequest
    {
        [DataMember(Name = "contact")]
        public IEnumerable<string> contact
        { get; set; }
    }
}