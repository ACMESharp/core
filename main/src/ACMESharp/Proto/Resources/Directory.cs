
using System.Collections.Generic;
using System.Runtime.Serialization;
using ACMESharp.Json;

namespace ACMESharp.Proto.Resources
{

    /// <summary>
    /// As defined by ACME protocol specification:
    ///    https://ietf-wg-acme.github.io/acme/#rfc.section.6.1.1 
    /// </summary>
    [DataContract]
    public class Directory : Resource
    {
        [DataMember(Name = "new-reg")]
        public string NewReg
        { get; set; }

        [DataMember(Name = "new-app")]
        public string NewApp
        { get; set; }

        [DataMember(Name = "revoke-cert")]
        public string RevokeCert
        { get; set; }

        [DataMember(Name = "key-change")]
        public string KeyChange
        { get; set; }

        [DataMember(Name = "meta", EmitDefaultValue = false)]
        public DirectoryMeta Meta
        { get; set; }

        [DataContract]
        public class DirectoryMeta : ExtensibleEntity
        {
            [DataMember(Name = "terms-of-service")]
            public string TermsOfService
            { get; set; }

            [DataMember(Name = "website")]
            public string Website
            { get; set; }

            [DataMember(Name = "caa-identities")]
            public IEnumerable<string> CaaIdentities
            { get; set; }
        }
    }
}