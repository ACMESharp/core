using System.Runtime.Serialization;

namespace ACMESharp.Jose
{
    /// <summary>
    /// Defines the format of a JSON Web Signature (JWS) as described
    /// in <see cref="https://tools.ietf.org/html/rfc7515#section-3">RFC7515</see>.
    /// </summary>
    /// <remarks>
    /// Additional resources:
    /// <list type="bullet">
    /// <item>http://jose.readthedocs.io/en/latest/</item>
    /// </list>
    /// </remarks>
    [DataContract]
    public class Jws
    {
        public JoseHeader Header
        { get; set; }

        public JwsPayload Payload
        { get; set; }

        public JwsSignature Signature
        { get; set; }

        public class JoseHeader
        {

        }

        public class JwsPayload
        {

        }

        public class JwsSignature
        {

        }
    }
}