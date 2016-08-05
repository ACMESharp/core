using System.Security.Cryptography;
using Xunit;
using Xunit.Abstractions;

namespace ACMESharp.Jose
{
    public
    class JwsExtensionsTests
    {
        private readonly ITestOutputHelper _out;

        public JwsExtensionsTests(ITestOutputHelper output)
        {
            _out = output;
        }

        [Fact]
        public void ComputeThumbprint()
        {
            // Based on the sample in:
            //    https://tools.ietf.org/html/rfc7638#section-3.1

            var kty = "RSA";
            var n = "0vx7agoebGcQSuuPiLJXZptN9nndrQmbXEps2aiAFbWhM78LhWx4cbbfAAt" +
                    "VT86zwu1RK7aPFFxuhDR1L6tSoc_BJECPebWKRXjBZCiFV4n3oknjhMstn6" +
                    "4tZ_2W-5JsGY4Hc5n9yBXArwl93lqt7_RN5w6Cf0h4QyQ5v-65YGjQR0_FD" +
                    "W2QvzqY368QQMicAtaSqzs8KJZgnYb9c7d0zgdAZHzu6qMQvRL5hajrn1n9" +
                    "1CbOpbISD08qNLyrdkt-bFTWhAI4vMQFh6WeZu0fM4lFd2NcRwr3XPksINH" +
                    "aQ-G_xBniIqbw0Ls1jF44-csFCur-kEgU8awapJzKnqDKgw";
            var e = "AQAB";

            var expectedHash = new byte[] {
                55, 54, 203, 177, 120, 124, 184, 48, 156, 119, 238, 140, 55, 5, 197,
                225, 111, 251, 158, 133, 151, 21, 144, 31, 30, 76, 89, 177, 17, 130,
                245, 123
            };

            var jwk1 = new Jwk
            {
                KeyType = kty,
                ["n"] = n,
                ["e"] = e,
            };
            var jwkHash1 = jwk1.ComputeThumbprint(SHA256.Create());
            
            Assert.Equal(expectedHash, jwkHash1);
        }
    }
}