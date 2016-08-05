using System.IO;
using ACMESharp.Ext;

namespace ACMESharp.Jose.Signer
{
    public interface IJwsSignerProvider : IExtProvider<IJwsSigner>
    {
        /// <summary>
        /// Deserializes the state of a signer instance from a stream.
        /// </summary>
        /// <remarks>
        /// This routine is the complement of the <see cref="IJwsSigner#Save"/>
        /// routine which serializes the state of the associated signer to a
        /// form which can be understood and deserialized by this method. 
        /// </remarks>
        IJwsSigner Load(Stream stream);
    }
}