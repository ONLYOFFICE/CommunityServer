using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Web.XmlTransform
{
    public enum MessageType {
        Normal,
        Verbose,
    }

    public interface IXmlTransformationLogger
    {
        void LogMessage(string message, params object[] messageArgs);

        void LogMessage(MessageType type, string message, params object[] messageArgs);

        void LogWarning(string message, params object[] messageArgs);

        void LogWarning(string file, string message, params object[] messageArgs);

        void LogWarning(string file, int lineNumber, int linePosition, string message, params object[] messageArgs);

        void LogError(string message, params object[] messageArgs);

        void LogError(string file, string message, params object[] messageArgs);

        void LogError(string file, int lineNumber, int linePosition, string message, params object[] messageArgs);

        void LogErrorFromException(Exception ex);

        void LogErrorFromException(Exception ex, string file);

        void LogErrorFromException(Exception ex, string file, int lineNumber, int linePosition);

        void StartSection(string message, params object[] messageArgs);

        void StartSection(MessageType type, string message, params object[] messageArgs);

        void EndSection(string message, params object[] messageArgs);

        void EndSection(MessageType type, string message, params object[] messageArgs);
    }
}
