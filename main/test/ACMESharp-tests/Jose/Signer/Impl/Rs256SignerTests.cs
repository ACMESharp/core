using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ACMESharp.Json;
using ACMESharp.Util;
using Xunit;
using Xunit.Abstractions;

namespace ACMESharp.Jose.Signer.Impl
{
    public
    class Rs256SignerTests
    {
        private readonly ITestOutputHelper _out;

        public Rs256SignerTests(ITestOutputHelper output)
        {
            _out = output;
        }

        [Fact]
        public void GetSignerByExtManager()
        {
            var em = new JwsSignerExtManager();

            var sp = em.GetProvider(Rs256SignerProvider.PROVIDER_NAME);
            Assert.NotNull(sp);
            Assert.IsType<Rs256SignerProvider>(sp);

            var s = sp.Get();
            Assert.NotNull(sp);
            Assert.IsType<Rs256Signer>(s);
        }

        [Fact]
        public void GetSignerWithRsaCngAndKeyLength()
        {
            var em = new JwsSignerExtManager();

            var RsaTypeName = typeof(System.Security.Cryptography.RSACng).AssemblyQualifiedName;
            var KeyLength = 1024;

            var signerParams = new Dictionary<string, object>
            {
                [nameof(RsaTypeName)] = RsaTypeName,
                [nameof(KeyLength)] = KeyLength,
            };
            _out.WriteLine($"Signer Params: {JsonSerializer.Serialize(signerParams)}");

            var sp = em.GetProvider(Rs256SignerProvider.PROVIDER_NAME);

            using (var s1 = sp.Get(signerParams))
            {
                var traits1 = sp.DescribeTraits(s1);
                Assert.Equal(RsaTypeName, traits1.First(x => x.Key == "RsaTypeName").Value);
                Assert.Equal(KeyLength, traits1.First(x => x.Key == "KeyLength").Value);
            }

            signerParams[nameof(KeyLength)] = KeyLength = 4096;
            using (var s2 = sp.Get(signerParams))
            {
                var traits2 = sp.DescribeTraits(s2);
                Assert.Equal(RsaTypeName, traits2.First(x => x.Key == "RsaTypeName").Value);
                Assert.Equal(KeyLength, traits2.First(x => x.Key == "KeyLength").Value);
            }
        }

        [Fact]
        public void SaveAndLoadNoParams()
        {
            var em = new JwsSignerExtManager();

            var sp = em.GetProvider(Rs256SignerProvider.PROVIDER_NAME);
            var s1 = (Rs256Signer)sp.Get();

            var stream1 = new MemoryStream();

            s1.Save(stream1);
            var data = stream1.ToArray();
            Assert.NotNull(data);
            Assert.NotEmpty(data);
            _out.WriteLine($"Data Stream:  {Encoding.UTF8.GetString(data)}");

            var stream2 = new MemoryStream(data);
            var s2 = (Rs256Signer)sp.Load(stream2);

            Assert.Equal(s1.Algorithm, s2.Algorithm);
            Assert.Equal(s1.Params.RsaTypeName, s2.Params.RsaTypeName);
            Assert.Equal(s1.Params.KeyLength, s2.Params.KeyLength);
            Assert.Equal(s1.Params.Sha256TypeName, s2.Params.Sha256TypeName);

            var traits1 = sp.DescribeTraits(s1)?.ToArray();
            var traits2 = sp.DescribeTraits(s2)?.ToArray();

            _out.WriteLine("Traits:");
            foreach (var t in traits1)
                _out.WriteLine($"* {t.Key} = {t.Value}");

            Assert.Equal(traits1, traits2, new KeyValuePairComparer<string, object>());
        }


        [Fact]
        public void SaveAndLoadWithParams()
        {
            var em = new JwsSignerExtManager();

            // We need to dynamically compute the default implementation classes
            // for each of these because they may differ on each OS platform
            var defaultRsa = RSA.Create();
            var defaultRsaTypeName = defaultRsa.GetType().AssemblyQualifiedName;
            var defaultShaTypeName = SHA256.Create().GetType().AssemblyQualifiedName;

            var signerParams = new Dictionary<string, object>
            {
                ["RsaTypeName"] = defaultRsaTypeName,
                ["Sha256TypeName"] = defaultShaTypeName,
                ["KeyLength"] = defaultRsa.LegalKeySizes[0].MinSize,
            };
            _out.WriteLine($"Signer Params: {JsonSerializer.Serialize(signerParams)}");

            var sp = em.GetProvider(Rs256SignerProvider.PROVIDER_NAME);
            var s1 = (Rs256Signer)sp.Get(signerParams);

            var stream1 = new MemoryStream();

            s1.Save(stream1);
            var data = stream1.ToArray();
            Assert.NotNull(data);
            Assert.NotEmpty(data);
            _out.WriteLine($"Data Stream:  {Encoding.UTF8.GetString(data)}");

            var stream2 = new MemoryStream(data);
            var s2 = (Rs256Signer)sp.Load(stream2);

            Assert.Equal(s1.Algorithm, s2.Algorithm);
            Assert.Equal(s1.Params.RsaTypeName, s2.Params.RsaTypeName);
            Assert.Equal(s1.Params.KeyLength, s2.Params.KeyLength);
            Assert.Equal(s1.Params.Sha256TypeName, s2.Params.Sha256TypeName);

            var traits1 = sp.DescribeTraits(s1)?.ToArray();
            var traits2 = sp.DescribeTraits(s2)?.ToArray();

            _out.WriteLine("Traits:");
            foreach (var t in traits1)
                _out.WriteLine($"* {t.Key} = {t.Value}");

            Assert.Equal(traits1, traits2, new KeyValuePairComparer<string, object>());
        }
    }
}