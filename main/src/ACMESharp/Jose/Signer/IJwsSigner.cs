using System.IO;
using ACMESharp.Ext;

namespace ACMESharp.Jose.Signer
{
    public interface IJwsSigner : IExtension
    {
        /// <summary>
        /// Returns the JWS (JWA) algorithm name.
        /// </summary>
        string Algorithm
        { get; }

        /// <summary>
        /// Serializes the state of this signer instance to a stream.
        /// </summary>
        /// <remarks>
        /// This routine produces a serialized form of this signer instance
        /// such that it can be deserialized by the complementary routine
        /// <see cref="IJwsSignerProvider#Load"/> of the associated signer provider.
        /// </remarks>
        void Save(Stream stream);

        /// <summary>
        /// Exports JWK key object in a <b>canonical</b> form.
        /// </summary>
        /// <returns></returns>
        Jwk ExportJwk();
        
        /// <summary>
        /// Returns a signature of the raw input content.
        /// </summary>
        byte[] Sign(byte[] raw);
    }
}