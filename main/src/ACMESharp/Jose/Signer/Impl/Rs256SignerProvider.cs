using System;
using System.Collections.Generic;
using System.IO;
using ACMESharp.Ext;

namespace ACMESharp.Jose.Signer.Impl
{
    public class Rs256SignerProvider : IJwsSignerProvider
    {
        public const string PROVIDER_NAME = "rs256";

        private static readonly ExtInfo EXT_INFO = new ExtInfo(
                "rs256",
                "RSA Signature with SHA-256 Signer");
        
        private static readonly IEnumerable<ExtParamInfo> EXT_PARAMS = new[]
        {
            new ExtParamInfo(
                    nameof(Rs256Signer.SignerParams.RsaTypeName),
                    ExtParamType.STRING,
                    "Name of RSA implemenation class to use."),
            new ExtParamInfo(
                    nameof(Rs256Signer.SignerParams.KeyLength),
                    ExtParamType.NUMBER,
                    "Size of RSA Key to create."),
        };

        public ExtInfo Describe()
        {
            return EXT_INFO;
        }

        public IEnumerable<ExtParamInfo> DescribeParams()
        {
            return EXT_PARAMS;
        }

        public IEnumerable<KeyValuePair<string, object>> DescribeTraits(IJwsSigner ext)
        {
            var rs256 = (Rs256Signer)ext;

            return new Dictionary<string, object>
            {
                [nameof(Rs256Signer.Algorithm)] = rs256.Algorithm,
                [nameof(Rs256Signer.SignerParams.RsaTypeName)] = rs256.Params.RsaTypeName,
                [nameof(Rs256Signer.ActualRsaTypeName)] = rs256.ActualRsaTypeName,
                [nameof(Rs256Signer.SignerParams.KeyLength)] = rs256.Params.KeyLength,
            };
        }

        public IJwsSigner Get(IDictionary<string, object> extParams = null)
        {
            var signer = new Rs256Signer();
            ExtUtil.ResolveParams(EXT_PARAMS, extParams, signer.Params);
            signer.Init();
            return signer;
        }

        public IJwsSigner Load(Stream stream)
        {
            return Rs256Signer.Load(stream);
        }
    }
}