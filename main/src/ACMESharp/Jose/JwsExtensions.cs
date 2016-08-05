using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using ACMESharp.Json;

namespace ACMESharp.Jose
{
    public static class JwsExtensions
    {
        /// <summary>
        /// Computes a thumbprint of the JWK using the argument Hash Algorithm
        /// as per <see cref="https://tools.ietf.org/html/rfc7638">RFC 7638</see>,
        /// JSON Web Key (JWK) Thumbprint.
        /// </summary>
        /// <param name="algor"></param>
        /// <returns></returns>
        public static byte[] ComputeThumbprint(this Jwk jwk, HashAlgorithm algor)
        {
            // As per RFC 7638 Section 3, we export the JWK in a canonical form
            // and then produce a JSON object with no whitespace or line breaks

            // Serialize to JSON as usual which will capture both first-order
            // properties and extensible properties, thereby merging the two
            var jwkJson = JsonSerializer.Serialize(jwk);

            // Then deserialize to SortedDictionary which will automatically
            // sort the entries by their key as needed for the canonical form
            var jwkDict = JsonSerializer.Deserialize<SortedDictionary<string, object>>(jwkJson);

            // Finally we re-serialize the SortedDictionary
            // which will preserve the sorted-by-key order
            jwkJson = JsonSerializer.Serialize(jwkDict);

            // Convert to bytes using UTF-8 encoding and hash
            var jwkBytes = Encoding.UTF8.GetBytes(jwkJson);
            var jwkHash = algor.ComputeHash(jwkBytes);

            return jwkHash;
        }
    }
}