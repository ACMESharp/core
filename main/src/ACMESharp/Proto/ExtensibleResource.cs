using System.Collections.Generic;

namespace ACMESharp.Proto
{
    public class ExtensibleResource : Resource
    {
            [Newtonsoft.Json.JsonExtensionData]
            internal IDictionary<string, object> _extData;

            public object this[string key]
            {
                get { return _extData?[key]; }
            }
    }

    public static class ExtensibleResourceExtensions
    {
            public static int GetExtDataCount(this ExtensibleResource res)
            {
                return (int)res?._extData.Count;
            }

            public static IEnumerable<string> GetExtDataKeys(this ExtensibleResource res)
            {
                return res._extData.Keys;
            }
    }
}