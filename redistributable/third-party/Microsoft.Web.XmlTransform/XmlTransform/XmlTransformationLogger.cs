using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;
using System.Globalization;

namespace Microsoft.Web.XmlTransform
{
    public class XmlTransformationLogger
    {
        #region private data members
        private bool hasLoggedErrors = false;

        private IXmlTransformationLogger externalLogger;
        private XmlNode currentReferenceNode = null;

        private bool fSupressWarnings = false;
        #endregion

        internal XmlTransformationLogger(IXmlTransformationLogger logger) {
            this.externalLogger = logger;
        }

        internal void LogErrorFromException(Exception ex) {
            hasLoggedErrors = true;

            if (externalLogger != null) {
                XmlNodeException nodeException = ex as XmlNodeException;
                if (nodeException != null && nodeException.HasErrorInfo) {
                    externalLogger.LogErrorFromException(
                        nodeException,
                        ConvertUriToFileName(nodeException.FileName),
                        nodeException.LineNumber,
                        nodeException.LinePosition);
                }
                else {
                    externalLogger.LogErrorFromException(ex);
                }
            }
            else {
                throw ex;
            }
        }

        internal bool HasLoggedErrors {
            get {
                return hasLoggedErrors;
            }
            set {
                hasLoggedErrors = false;
            }
        }

        internal XmlNode CurrentReferenceNode {
            get {
                return currentReferenceNode;
            }
            set {
                // I don't feel like implementing a stack for this for no
                // reason. Only one thing should try to set this property
                // at a time, and that thing should clear it when done.
                Debug.Assert(currentReferenceNode == null || value == null, "CurrentReferenceNode is being overwritten");

                currentReferenceNode = value;
            }
        }

        #region public interface

        public bool SupressWarnings
        {
            get { return fSupressWarnings; }
            set { fSupressWarnings = value; }
        }

        public void LogMessage(string message, params object[] messageArgs) {
            if (externalLogger != null) {
                externalLogger.LogMessage(message, messageArgs);
            }
        }

        public void LogMessage(MessageType type, string message, params object[] messageArgs) {
            if (externalLogger != null) {
                externalLogger.LogMessage(type, message, messageArgs);
            }
        }

        public void LogWarning(string message, params object[] messageArgs) {
            if (SupressWarnings)
            {
                // SupressWarnings downgrade the Warning to LogMessage
                LogMessage(message, messageArgs);
            }
            else
            {
                if (CurrentReferenceNode != null)
                {
                    LogWarning(CurrentReferenceNode, message, messageArgs);
                }
                else if (externalLogger != null)
                {
                    externalLogger.LogWarning(message, messageArgs);
                }
            }
        }

        public void LogWarning(XmlNode referenceNode, string message, params object[] messageArgs) {
            if (SupressWarnings)
            {
                // SupressWarnings downgrade the Warning to LogMessage
                LogMessage(message, messageArgs);
            }
            else
            {
                if (externalLogger != null)
                {
                    string fileName = ConvertUriToFileName(referenceNode.OwnerDocument);
                    IXmlLineInfo lineInfo = referenceNode as IXmlLineInfo;

                    if (lineInfo != null)
                    {
                        externalLogger.LogWarning(
                            fileName,
                            lineInfo.LineNumber,
                            lineInfo.LinePosition,
                            message,
                            messageArgs);
                    }
                    else
                    {
                        externalLogger.LogWarning(
                            fileName,
                            message,
                            messageArgs);
                    }
                }
            }
        }

        public void LogError(string message, params object[] messageArgs) {
            hasLoggedErrors = true;

            if (CurrentReferenceNode != null) {
                LogError(CurrentReferenceNode, message, messageArgs);
            }
            else if (externalLogger != null) {
                externalLogger.LogError(message, messageArgs);
            }
            else {
                throw new XmlTransformationException(String.Format(CultureInfo.CurrentCulture, message, messageArgs));
            }
        }

        public void LogError(XmlNode referenceNode, string message, params object[] messageArgs) {
            hasLoggedErrors = true;

            if (externalLogger != null) {
                string fileName = ConvertUriToFileName(referenceNode.OwnerDocument);
                IXmlLineInfo lineInfo = referenceNode as IXmlLineInfo;

                if (lineInfo != null) {
                    externalLogger.LogError(
                        fileName,
                        lineInfo.LineNumber,
                        lineInfo.LinePosition,
                        message,
                        messageArgs);
                }
                else {
                    externalLogger.LogError(
                        fileName,
                        message,
                        messageArgs);
                }
            }
            else {
                throw new XmlNodeException(String.Format(CultureInfo.CurrentCulture, message, messageArgs), referenceNode);
            }
        }

        public void StartSection(string message, params object[] messageArgs) {
            if (externalLogger != null) {
                externalLogger.StartSection(message, messageArgs);
            }
        }

        public void StartSection(MessageType type, string message, params object[] messageArgs) {
            if (externalLogger != null) {
                externalLogger.StartSection(type, message, messageArgs);
            }
        }

        public void EndSection(string message, params object[] messageArgs) {
            if (externalLogger != null) {
                externalLogger.EndSection(message, messageArgs);
            }
        }

        public void EndSection(MessageType type, string message, params object[] messageArgs) {
            if (externalLogger != null) {
                externalLogger.EndSection(type, message, messageArgs);
            }
        }

        #endregion

        private string ConvertUriToFileName(XmlDocument xmlDocument) {
            string uri;
            XmlFileInfoDocument errorInfoDocument = xmlDocument as XmlFileInfoDocument;
            if (errorInfoDocument != null) {
                uri = errorInfoDocument.FileName;
            }
            else {
                uri = errorInfoDocument.BaseURI;
            }

            return ConvertUriToFileName(uri);
        }

        private string ConvertUriToFileName(string fileName) {
            try {
                Uri uri = new Uri(fileName);
                if (uri.IsFile && String.IsNullOrEmpty(uri.Host)) {
                    fileName = uri.LocalPath;
                }
            }
            catch (UriFormatException) {
                // Bad URI format, so just return the original filename
            }

            return fileName;
        }
    }
}
