using System.Collections.Generic;

namespace ACMESharp.Ext
{
    public interface IExtProvider<E>
        where E : IExtension
    {
        ExtInfo Describe();

        IEnumerable<ExtParamInfo> DescribeParams();

        IEnumerable<KeyValuePair<string, object>> DescribeTraits(E ext);

        E Get(IDictionary<string, object> extParams = null);
    }
}