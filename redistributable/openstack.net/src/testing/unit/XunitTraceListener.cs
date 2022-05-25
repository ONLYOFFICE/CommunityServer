using System;
using System.Diagnostics;
using Xunit.Abstractions;

namespace OpenStack
{
    public class XunitTraceListener : TraceListener
    {
        private readonly ITestOutputHelper _testLog;

        public XunitTraceListener(ITestOutputHelper testLog)
        {
            _testLog = testLog;
        }

        public override void Write(string message)
        {
            if (message.StartsWith(OpenStackNet.Tracing.Http.Name))
                return;

            TryLog(message);
        }

        public override void WriteLine(string message)
        {
            TryLog(message);
        }

        private void TryLog(string message)
        {
            try
            {
                _testLog.WriteLine(message);
            }
            catch (InvalidOperationException)
            {
                // Unable to log to xunit because it thinks no test is active...
            }
        }
    }
}
