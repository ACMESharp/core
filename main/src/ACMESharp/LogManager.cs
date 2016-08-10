using Microsoft.Extensions.Logging;

namespace ACMESharp
{
    public static class LogManager
    {
        // TODO:  build this out with some more structure
        //    Use this as guidance:
        //    https://msdn.microsoft.com/en-us/magazine/mt694089.aspx?f=255&MSPPError=-2147217396

        public static ILoggerFactory LoggerFactory
        { get; set; }

        public static ILogger<T> GetLogger<T>()
        {
            return LoggerFactory.CreateLogger<T>();
        }
    }
}