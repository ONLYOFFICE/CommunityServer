using System;
using System.Diagnostics;
using System.Text;
using Microsoft.Web.XmlTransform;

namespace Microsoft.Web.XmlTransform.Test
{
    internal class TestTransformationLogger : IXmlTransformationLogger
    {
        private int indentLevel = 0;
        private readonly string indentStringPiece = "  ";
        private string indentString = null;
        private StringBuilder _log = new StringBuilder();

        private string IndentString
        {
            get
            {
                if (indentString == null)
                {
                    indentString = String.Empty;
                    for (int i = 0; i < indentLevel; i++)
                    {
                        indentString += indentStringPiece;
                    }
                }
                return indentString;
            }
        }

        private int IndentLevel
        {
            get
            {
                return indentLevel;
            }
            set
            {
                if (indentLevel != value)
                {
                    indentLevel = value;
                    indentString = null;
                }
            }
        }

        public string LogText
        {
            get
            {
                return  _log.ToString();
            }
        }

        public void LogMessage(string message, params object[] messageArgs)
        {
            LogMessage(MessageType.Normal, message, messageArgs);
        }

        public void LogMessage(MessageType type, string message, params object[] messageArgs)
        {
            _log.AppendLine(String.Concat(IndentString, string.Format(message, messageArgs)));
        }

        public void LogWarning(string message, params object[] messageArgs)
        {
            LogWarning(message, messageArgs);
        }

        public void LogWarning(string file, string message, params object[] messageArgs)
        {
            LogWarning(file, 0, 0, message, messageArgs);
        }

        public void LogWarning(string file, int lineNumber, int linePosition, string message, params object[] messageArgs)
        {
            // we will format like: transform.xml (30, 10) warning: Argument 'snap' did not match any attributes
            string format = "{0} ({1}, {2}) warning: {3}";
            _log.AppendLine(string.Format(format, System.IO.Path.GetFileName(file), lineNumber, linePosition, string.Format(message,messageArgs)));
        }

        public void LogError(string message, params object[] messageArgs)
        {
            LogError(message, messageArgs);
        }

        public void LogError(string file, string message, params object[] messageArgs)
        {
            LogError(file, 0, 0, message, messageArgs);
        }

        public void LogError(string file, int lineNumber, int linePosition, string message, params object[] messageArgs)
        {
            //transform.xml(33, 10) error: Could not resolve 'ThrowException' as a type of Transform
            string format = "{0} ({1}, {2}) error: {3}";
            _log.AppendLine(string.Format(format, System.IO.Path.GetFileName(file), lineNumber, linePosition, string.Format(message,messageArgs)));
        }

        public void LogErrorFromException(Exception ex) {}

        public void LogErrorFromException(Exception ex, string file) {}

        public void LogErrorFromException(Exception ex, string file, int lineNumber, int linePosition) 
        {
            string message = ex.Message;
            LogError(file, lineNumber, linePosition, message);
        }

        public void StartSection(string message, params object[] messageArgs)
        {
            StartSection(MessageType.Normal, message, messageArgs);
        }

        public void StartSection(MessageType type, string message, params object[] messageArgs)
        {
            LogMessage(type, message, messageArgs);
            IndentLevel++;
        }

        public void EndSection(string message, params object[] messageArgs)
        {
            EndSection(MessageType.Normal, message, messageArgs);
        }

        public void EndSection(MessageType type, string message, params object[] messageArgs)
        {
            Debug.Assert(IndentLevel > 0);
            if (IndentLevel > 0)
            {
                IndentLevel--;
            }

            LogMessage(type, message, messageArgs);
        }
    }
}
