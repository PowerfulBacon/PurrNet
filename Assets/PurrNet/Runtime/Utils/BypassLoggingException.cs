using System;

namespace PurrNet
{
    public class BypassLoggingException : Exception
    {
        public static BypassLoggingException instance = new BypassLoggingException();
    }
}
