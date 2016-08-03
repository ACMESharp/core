using System.Collections.Generic;
using System.IO;
using ACMESharp.Ext;

namespace ACMESharp.Vault
{
    public interface IVault : IExtension
    {
        /// <summary>
        /// Tests to see if the backing-store for this Vault instance already
        /// exists and is initialized for use.
        /// </summary>
        bool Exists();

        /// <summary>
        /// Creates and initializes the backing-store for this Vault instance.
        /// </summary>
        /// <param name="forceOverwrite">
        /// If the backing-store already exists and initialized for use,
        /// a value of <c>true</c> would force it to be re-initialized;
        /// otherwise, an exception will be thrown 
        /// </param>
        void Create(bool forceOverwrite = false);

        /// <summary>
        /// Opens the Vault instance for use.
        /// </summary>
        /// <param name="forceCreate">
        /// If the backing-store for this Vault instance does not yet exist or
        /// is not initialized for use, a value of <c>true</c> would force the
        /// <see cref="Create" /> operation; otherwise an exception will be
        /// thrown
        /// </param>
        void Open(bool forceCreate = false);

        void Save(VaultContent content);

        VaultContent Load();

        IEnumerable<VaultAsset> ListAssets(string nameRegex = null,
                params VaultAssetType[] types);

        VaultAsset CreateAsset(VaultAssetType type, string name,
                bool isSensitive = false,
                bool forceCreate = false,
                bool forceOverwrite = false);

        VaultAsset GetAsset(VaultAsset asset);

        Stream SaveAsset(VaultAsset asset);

        Stream LoadAsset(VaultAsset asset);

        void RemoveAsset(VaultAsset asset);
    }
}
