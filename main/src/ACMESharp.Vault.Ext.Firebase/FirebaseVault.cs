using System;
using System.Collections.Generic;
using System.IO;
using System.Security;

namespace ACMESharp.Vault.Ext.Firebase
{
    public class FirebaseVault : IVault
    {
        public string DatabaseUrl
        { get; set; }

        public SecureString AuthCredential
        { get; set; }

        public void Create(bool forceOverwrite = false)
        {
            throw new NotImplementedException();
        }

        public VaultAsset CreateAsset(VaultAssetType type, string name,
                bool isSensitive = false,
                bool forceCreate = false,
                bool forceOverwrite = false)
        {
            throw new NotImplementedException();
        }

        public bool Exists()
        {
            throw new NotImplementedException();
        }

        public VaultAsset GetAsset(VaultAsset asset)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<VaultAsset> ListAssets(string nameRegex = null, params VaultAssetType[] types)
        {
            throw new NotImplementedException();
        }

        public VaultContent Load()
        {
            throw new NotImplementedException();
        }

        public Stream LoadAsset(VaultAsset asset)
        {
            throw new NotImplementedException();
        }

        public void Open(bool forceCreate = false)
        {
            throw new NotImplementedException();
        }

        public void RemoveAsset(VaultAsset asset)
        {
            throw new NotImplementedException();
        }

        public void Save(VaultContent content)
        {
            throw new NotImplementedException();
        }

        public Stream SaveAsset(VaultAsset asset)
        {
            throw new NotImplementedException();
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
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _IsDisposed = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~FirebaseVault() {
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
    }
}
