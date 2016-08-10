using System.Collections.Generic;

namespace ACMESharp.Json
{
    public class ExtensibleEntity
    {
            [Newtonsoft.Json.JsonExtensionData]
            internal IDictionary<string, object> _extData;

            public object this[string key]
            {
                get
                {
                     return _extData?[key];
                }
                
                set
                {
                    if (_extData == null)
                        _extData = new Dictionary<string, object>();
                    _extData[key] = value;
                }
            }
    }

    public static class ExtensibleResourceExtensions
    {
            public static int GetExtDataCount(this ExtensibleEntity res)
            {
                return (int)res?._extData.Count;
            }

            public static IEnumerable<string> GetExtDataKeys(this ExtensibleEntity res)
            {
                return res._extData.Keys;
            }
    }
}