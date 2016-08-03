using System;

namespace ACMESharp.Ext
{
    public interface IExtension : IDisposable
    { 
        bool IsDisposed
        { get; }
    }
}