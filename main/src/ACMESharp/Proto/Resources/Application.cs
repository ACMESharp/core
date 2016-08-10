using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using ACMESharp.Json;

namespace ACMESharp.Proto.Resources
{
    /// <summary>
    /// An ACME registration resource represents a client’s request for a certificate,
    /// and is used to track the progress of that application through to issuance. Thus,
    /// the object contains information about the requested certificate, the server’s
    /// requirements, and any certificates that have resulted from this application.
    /// </summary>
    [DataContract]
    public class Application : Resource
    {
        [DataMember(Name = "status")]
        public string Status
        { get; set; }
        
        [DataMember(Name = "expires")]
        public DateTime? Expires
        { get; set; }
        
        [DataMember(Name = "csr")]
        public string Csr
        { get; set; }
        
        [DataMember(Name = "notBefore")]
        public DateTime? NotBefore
        { get; set; }
        
        [DataMember(Name = "notAfter")]
        public DateTime? NotAfter
        { get; set; }
        
        [DataMember(Name = "requirements")]
        public IEnumerable<ApplicationRequirements> Requirements
        { get; set; }
        
        [DataMember(Name = "certificate")]
        public string Certificate
        { get; set; }

        [DataContract]
        public class ApplicationRequirements : ExtensibleEntity
        {
            /// <summary>
            /// Required for all requirements objects.
            /// </summary>
            [DataMember(Name = "type")]
            public string Type
            { get; set; }

            /// <summary>
            /// Required for all requirements objects.
            /// </summary>
            [DataMember(Name = "status")]
            public string Status
            { get; set; }

            /// <summary>
            /// Optional, but potentially common to many different requirements objects
            /// (e.g. Authorization and Out-of-Band). 
            /// </summary>
            [DataMember(Name = "url")]
            public string Url
            { get; set; }
        }
    }
}