using System.Runtime.Serialization;

namespace ACMESharp.Jose
{
    /// <summary>
    /// Defines the format of a JSON Web Key (JWK) as described
    /// in <see cref="https://tools.ietf.org/html/rfc7517#section-4">RFC7517</see>.
    /// </summary>
    /// <remarks>
    /// Additional resources:
    /// <list type="bullet">
    /// <item>http://jose.readthedocs.io/en/latest/</item>
    /// </list>
    /// </remarks>
    [DataContract]
    public class Jwk : ExtensibleEntity
    { 
        /// <summary>
        /// https://tools.ietf.org/html/rfc7517#section-4.1
        /// </summary>
        [DataMember(Name = "kty", EmitDefaultValue = false)]
        public string KeyType
        { get; set; }

        /// <summary>
        /// https://tools.ietf.org/html/rfc7517#section-4.2
        /// </summary>
        [DataMember(Name = "use", EmitDefaultValue = false)]
        public string PublicKeyUse
        { get; set; }
        
        /// <summary>
        /// https://tools.ietf.org/html/rfc7517#section-4.3
        /// </summary>
        [DataMember(Name = "key_ops", EmitDefaultValue = false)]
        public string KeyOperations
        { get; set; }
        
        /// <summary>
        /// https://tools.ietf.org/html/rfc7517#section-4.4
        /// </summary>
        [DataMember(Name = "alg", EmitDefaultValue = false)]
        public string Algorithm
        { get; set; }
        
        /// <summary>
        /// https://tools.ietf.org/html/rfc7517#section-4.5
        /// </summary>
        [DataMember(Name = "kid", EmitDefaultValue = false)]
        public string KeyId
        { get; set; }
        
        /// <summary>
        /// https://tools.ietf.org/html/rfc7517#section-4.6
        /// </summary>
        [DataMember(Name = "x5u", EmitDefaultValue = false)]
        public string X509Url
        { get; set; }
        
        /// <summary>
        /// https://tools.ietf.org/html/rfc7517#section-4.7
        /// </summary>
        [DataMember(Name = "x5c", EmitDefaultValue = false)]
        public string X509CertificateChain
        { get; set; }
        
        /// <summary>
        /// https://tools.ietf.org/html/rfc7517#section-4.8
        /// </summary>
        [DataMember(Name = "x5t", EmitDefaultValue = false)]
        public string X509CertificateSha1Thumbprint
        { get; set; }
        
        /// <summary>
        /// https://tools.ietf.org/html/rfc7517#section-4.9
        /// </summary>
        [DataMember(Name = "x5t#S256", EmitDefaultValue = false)]
        public string X509CertificateSha256Thumbprint
        { get; set; }
    }
}