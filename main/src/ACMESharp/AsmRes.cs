using System;

namespace ACMESharp
{
    public class AsmRes
    {
        public static readonly AsmRes EXCEPTIONS = new AsmRes();

        public string this[string key]
        {
            get
            {
                return key;
            }
        }

        public string With(string key, params object[] args)
        {
            return string.Format(key, args);
        }
    }
}
