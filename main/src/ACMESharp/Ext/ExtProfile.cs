using System.Collections.Generic;

namespace ACMESharp.Ext
{
    public class ExtProfile<E, EP>
        where E : IExtension
        where EP : IExtProvider<E>
    {
        public string ExtensionType
        { get { return typeof(E).FullName; } }

        public string ProviderName
        { get; set; }

        public IReadOnlyDictionary<string, object> ExtParamValues
        { get; set; }
    }
}