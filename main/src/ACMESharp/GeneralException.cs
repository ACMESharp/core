using System;

namespace ACMESharp
{
    public class GeneralException : Exception
    {
        public GeneralException()
        {
        }

        public GeneralException(string message) : base(message)
        {
        }

        public GeneralException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
