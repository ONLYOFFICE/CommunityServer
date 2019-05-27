using System;
using log4net;

namespace Novell.Directory.Ldap
{
    public static class Logger
    {
        private static ILog _log;

        static Logger()
        {
            Init();
        }

        public static ILog Log
        {
            get { return _log; }
        }

        public static void LogWarning(this ILog logger, string message, Exception ex)
        {
            Log.WarnFormat(message + " - {0}", ex);
        }

        public static void LogError(this ILog logger, string message, Exception ex)
        {
            Log.ErrorFormat(message + " - {0}", ex);
        }

        private static void Init()
        {
            _log = LogManager.GetLogger("Ldap"); //_loggerFactory.CreateLogger( "Ldap");
        }
    }
}