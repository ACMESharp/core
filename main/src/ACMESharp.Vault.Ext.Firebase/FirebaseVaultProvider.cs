using System;
using System.Collections.Generic;
using System.Security;
using ACMESharp.Ext;

namespace ACMESharp.Vault.Ext.Firebase
{
    public class FirebaseVaultProvider : IVaultProvider
    {
        public const string PROVIDER_NAME = "firebase";

        private static ExtInfo EXT_INFO = new ExtInfo(PROVIDER_NAME, "Firebase DB");

        private static IEnumerable<ExtParamInfo> EXT_PARAMS = new[]
        {
            new ExtParamInfo(nameof(FirebaseVault.DatabaseUrl),
                    ExtParamType.STRING,
                    isRequired: true),
            new ExtParamInfo(nameof(FirebaseVault.AuthCredential),
                    ExtParamType.SECRET,
                    isRequired: true),
        };

        public ExtInfo Describe()
        {
            return EXT_INFO;
        }

        public IEnumerable<ExtParamInfo> DescribeParams()
        {
            return EXT_PARAMS;
        }

        public IEnumerable<KeyValuePair<string, object>> DescribeTraits(IVault vault)
        {
            var fbv = vault as FirebaseVault;
            if (fbv == null)
                throw new ArgumentException(nameof(vault), "missing or invalid type");

            return ExtUtil.NO_TRAITS;
        }

        public IVault Get(IDictionary<string, object> extParams = null)
        {
            return ExtUtil.ResolveParams(EXT_PARAMS, extParams,
                    new FirebaseVault());
        }
    }
}