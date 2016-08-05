using System;
using System.IO;
using System.Security.Cryptography;
using ACMESharp.Json;

namespace ACMESharp.Jose.Signer.Impl
{
    public class Rs256Signer : IJwsSigner
    {
        /// https://tools.ietf.org/html/rfc7518#section-6.3
        public const string JWA_KTY = "RSA";
        /// https://tools.ietf.org/html/rfc7518#section-6.3.1.1
        public const string JWA_MODULUS = "n";
        /// https://tools.ietf.org/html/rfc7518#section-6.3.1.2
        public const string JWA_EXPONENT = "e";

        private RSA _rsa;

        public Rs256Signer()
        {
            Params = new SignerParams();
        }

        public Rs256Signer(SignerParams @params)
        {
            Params = @params;
        }

        public SignerParams Params
        { get; }

        public string ActualRsaTypeName
        {
            get { return _rsa?.GetType().AssemblyQualifiedName; }
        }

        private static RSA CreateRsa(string typeName, int keyLength)
        {
            RSA rsa;
            if (typeName == null && keyLength <= 0)
            {
                // Default implementaiton with no provider params
                rsa = RSA.Create();
            }
            else
            {
                if (typeName == null)
                    // Default RSA implementation type name
                    typeName = "System.Security.Cryptography.RSACryptoServiceProvider";
                
                var type = Type.GetType(typeName, true);

                var constructorParams = (object[])null;
                if (keyLength > 0)
                    constructorParams = new object[] { keyLength };
                
                rsa = (RSA)Activator.CreateInstance(type, constructorParams);
            }

            return rsa;
        }

        public static Rs256Signer Load(Stream stream)
        {
            SignerState state;
            using (var sr = new StreamReader(stream))
            {
                state = JsonSerializer.Deserialize<SignerState>(sr.ReadToEnd());
            }

            var signer = new Rs256Signer(state.Params);
            signer.Init();
            signer._rsa.ImportParameters(state.RsaParameters);

            return signer;
        }
        public void Init()
        {
            _rsa = CreateRsa(Params.RsaTypeName, Params.KeyLength);
        }

        public string Algorithm
        {
            get { return "RS256"; }
        }

        public void Save(Stream stream)
        {
            AssertReady();

            var saveObject = new SignerState
            {
                Params = Params,
                RsaParameters = _rsa.ExportParameters(true),
            };

            using (var sw = new StreamWriter(stream))
            {
                sw.Write(JsonSerializer.Serialize(saveObject));
            }
        }

        public Jwk ExportJwk()
        {
            AssertReady();

            var exp = _rsa.ExportParameters(false);
            var jwk = new Jwk
            {
                // As per:
                //    https://tools.ietf.org/html/rfc7638#section-3
                //    https://tools.ietf.org/html/rfc7518#section-6.3
                // these are the *required* elements of the JWK
                KeyType = JWA_KTY,
                [JWA_EXPONENT] = EncodingHelper.Base64UrlEncode(exp.Exponent),
                [JWA_MODULUS] = EncodingHelper.Base64UrlEncode(exp.Modulus), 
            };
            return jwk;
        }

        public byte[] Sign(byte[] raw)
        {
            AssertReady();

            return _rsa.SignData(
                raw, 0, raw.Length,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);
        }

        protected void AssertReady()
        {
            if (_IsDisposed
                    || _rsa == null)
                throw new InvalidOperationException();    

        }

#region -- IDisposable Support --

        private bool _IsDisposed = false; // To detect redundant calls

        public bool IsDisposed
        {
            get { return _IsDisposed; }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_IsDisposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    _rsa?.Dispose();
                    _rsa = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _IsDisposed = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Rs256Signer() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

#endregion -- IDisposable Support --

        public class SignerParams
        {
            public string RsaTypeName
            { get; set; }

            public int KeyLength
            { get; set; }

            public string Sha256TypeName
            { get; set; }
       }

        /// <summary>
        /// Captures enough information to be able to serialize and deserialize
        /// an instance of the Rs256Signer class.
        /// </summary>
        public class SignerState
        {
            public const int VER = 0x0100;

            /// <summary>
            /// Serves as a magic header/number and a verstion string.
            /// </summary>
            public int Rs256Signer
            { get; set; } = VER;

            public SignerParams Params
            { get; set; }

            public RSAParameters RsaParameters
            { get; set; }
        }
    }
}