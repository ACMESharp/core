using System;
using System.Collections.Generic;
using ACMESharp.Ext;

namespace ACMESharp.Vault.Impl
{
    public class LocalVaultProvider : IVaultProvider
    {
        public const string PROVIDER_NAME = "local";

        private static readonly ExtInfo EXT_INFO = new ExtInfo(
                PROVIDER_NAME,
                "Local Disk Vault",
                "Vault provider based on system-local folder and files.");

        private static readonly IEnumerable<ExtParamInfo> EXT_PARAMS = new[]
        {
            new ExtParamInfo(nameof(LocalVault.RootPath), ExtParamType.STRING,
                    description: "Root directory path of vault storage.",
                    isRequired: true),

            new ExtParamInfo(nameof(LocalVault.Password), ExtParamType.SECRET,
                    description:  "Optional password used to derive crypto to"
                            + " secure sensitive Vault Assets"),
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
            var lv = vault as LocalVault;
            if (lv == null)
                throw new ArgumentException(nameof(vault), "missing or invalid type");

            return ExtUtil.NO_TRAITS;
        }

        public IVault Get(IDictionary<string, object> extParams = null)
        {
            return ExtUtil.ResolveParams(EXT_PARAMS, extParams,
                    new LocalVault());
        }
    }
}