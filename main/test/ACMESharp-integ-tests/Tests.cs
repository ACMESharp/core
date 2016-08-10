using System;
using Xunit;

namespace Tests
{
    public
    class AcmeServerIntegrationTests
    {
        [Fact]
        public void Test1() 
        {
            Assert.True(true);
        }

        public void AcmeRegister()
        {
            Assert.True(true);
        }

        public void AcmeRegisterDuplicateKey()
        {
            // TODO:

            // From https://ietf-wg-acme.github.io/acme/#rfc.section.6.2:
            // The server creates a registration object with the included contact information. The “key” element of the registration is set to the public key used to verify the JWS (i.e., the “jwk” element of the JWS header). The server returns this registration object in a 201 (Created) response, with the registration URI in a Location header field.
            // If the server already has a registration object with the provided account key, then it MUST return a 409 (Conflict) response and provide the URI of that registration in a Location header field. This allows a client that has an account key but not the corresponding registration URI to recover the registration URI.            
        }
    }
}
