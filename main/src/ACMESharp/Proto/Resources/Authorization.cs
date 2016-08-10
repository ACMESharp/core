using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ACMESharp.Proto.Resources
{
    /// <summary>
    /// A requirement with type “authorization” requests that the ACME client complete
    /// an authorization transaction. The server specifies the authorization by
    /// pre-provisioning a pending authorization resource and providing the URI for
    /// this resource in the requirement.
    /// </summary>
    [DataContract]
    public class Authorization : Resource
    {
        [DataMember(Name = "identifier")]
        public AuthorizationIdentifier Identifier
        { get; set; }
        
        [DataMember(Name = "status")]
        public string Status
        { get; set; }
        
        [DataMember(Name = "expires")]
        public DateTime? Expires
        { get; set; }
        
        [DataMember(Name = "scope")]
        public string Scope
        { get; set; }
        
        [DataMember(Name = "challenges")]
        public IEnumerable<AuthorizationChallenge> Challenges
        { get; set; }
        
        [DataMember(Name = "combinations")]
        public IEnumerable<IEnumerable<int>> Combinations
        { get; set; }

        [DataContract]
        public class AuthorizationIdentifier : Resource
        {
            [DataMember(Name = "type")]
            public string Type
            { get; set; }

            [DataMember(Name = "value")]
            public string Value
            { get; set; }

        }

        [DataContract]
        public class AuthorizationChallenge : Resource
        {
            [DataMember(Name = "type")]
            public string Type
            { get; set; }

            [DataMember(Name = "status")]
            public string Status
            { get; set; }
        }
    }
}