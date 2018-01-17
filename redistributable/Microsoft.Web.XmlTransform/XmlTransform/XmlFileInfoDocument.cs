using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Diagnostics;

namespace Microsoft.Web.XmlTransform
{
    public class XmlFileInfoDocument : XmlDocument, IDisposable
    {
        private Encoding _textEncoding = null;
        private XmlTextReader _reader = null;
        private XmlAttributePreservationProvider _preservationProvider = null;
        private bool _firstLoad = true;
        private string _fileName = null;

        private int _lineNumberOffset = 0;
        private int _linePositionOffset = 0;

        public override void Load(string filename) {
            LoadFromFileName(filename);

            _firstLoad = false;
        }

        public override void Load(XmlReader reader) {
            _reader = reader as XmlTextReader;
            if (_reader != null) {
                _fileName = _reader.BaseURI;
            }

            base.Load(reader);

            if (_reader != null) {
                _textEncoding = _reader.Encoding;
            }

            _firstLoad = false;
        }

        private void LoadFromFileName(string filename) {
            _fileName = filename;

            StreamReader reader = null;
            try {
                if (PreserveWhitespace) {
                    _preservationProvider = new XmlAttributePreservationProvider(filename);
                }

                reader = new StreamReader(filename, true);
                LoadFromTextReader(reader);
            }
            finally {
                if (_preservationProvider != null)
                {
                    _preservationProvider.Close();
                    _preservationProvider = null;
                }
                if (reader != null) {
                    reader.Close();
                }
            }
        }

        private void LoadFromTextReader(TextReader textReader) {
            StreamReader streamReader = textReader as StreamReader;
            if (streamReader != null) {
                FileStream fileStream = streamReader.BaseStream as FileStream;
                if (fileStream != null) {
                    _fileName = fileStream.Name;
                }

                _textEncoding = GetEncodingFromStream(streamReader.BaseStream);
            }

            _reader = new XmlTextReader(_fileName, textReader);

            base.Load(_reader);

            if (_textEncoding == null) {
                _textEncoding = _reader.Encoding;
            }
        }

        private Encoding GetEncodingFromStream(Stream stream) {
            Encoding encoding = null;
            if (stream.CanSeek) {
                byte[] buffer = new byte[3];
                stream.Read(buffer, 0, buffer.Length);

                if (buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF)
                    encoding = Encoding.UTF8;
                else if (buffer[0] == 0xFE && buffer[1] == 0xFF)
                    encoding = Encoding.BigEndianUnicode;
                else if (buffer[0] == 0xFF && buffer[1] == 0xFE)
                    encoding = Encoding.Unicode;
                else if (buffer[0] == 0x2B && buffer[1] == 0x2F && buffer[2] == 0x76)
                    encoding = Encoding.UTF7;

                // Reset the stream
                stream.Seek(0, SeekOrigin.Begin);
            }

            return encoding;
        }

        internal XmlNode CloneNodeFromOtherDocument(XmlNode element) {
            XmlTextReader oldReader = _reader;
            string oldFileName = _fileName;

            XmlNode clone = null;
            try {
                IXmlLineInfo lineInfo = element as IXmlLineInfo;
                if (lineInfo != null) {
                    _reader = new XmlTextReader(new StringReader(element.OuterXml));

                    _lineNumberOffset = lineInfo.LineNumber - 1;
                    _linePositionOffset = lineInfo.LinePosition - 2;
                    _fileName = element.OwnerDocument.BaseURI;

                    clone = ReadNode(_reader);
                }
                else {
                    _fileName = null;
                    _reader = null;

                    clone = ReadNode(new XmlTextReader(new StringReader(element.OuterXml)));
                }
            }
            finally {
                _lineNumberOffset = 0;
                _linePositionOffset = 0;
                _fileName = oldFileName;

                _reader = oldReader;
            }

            return clone;
        }

        internal bool HasErrorInfo {
            get {
                return _reader != null;
            }
        }

        internal string FileName {
            get {
                return _fileName;
            }
        }

        private int CurrentLineNumber {
            get {
                return _reader != null ? _reader.LineNumber + _lineNumberOffset : 0;
            }
        }

        private int CurrentLinePosition {
            get {
                return _reader != null ? _reader.LinePosition + _linePositionOffset : 0;
            }
        }

        private bool FirstLoad {
            get {
                return _firstLoad;
            }
        }

        private XmlAttributePreservationProvider PreservationProvider {
            get {
                return _preservationProvider;
            }
        }

        private Encoding TextEncoding {
            get {
                if (_textEncoding != null) {
                    return _textEncoding;
                }
                else {
                    // Copied from base implementation of XmlDocument
                    if (HasChildNodes) {
                        XmlDeclaration declaration = FirstChild as XmlDeclaration;
                        if (declaration != null) {
                            string value = declaration.Encoding;
                            if (value.Length > 0) {
                                return System.Text.Encoding.GetEncoding(value);
                            }
                        }
                    }
                }
                return null;
            }
        }

        public override void Save(string filename)
        {
            XmlWriter xmlWriter = null;
            try{
                if (PreserveWhitespace){
                    XmlFormatter.Format(this);
                    xmlWriter = new XmlAttributePreservingWriter(filename, TextEncoding);
                }
                else {
                    XmlTextWriter textWriter = new XmlTextWriter(filename, TextEncoding);
                    textWriter.Formatting = Formatting.Indented;
                    xmlWriter = textWriter;
                }
                WriteTo(xmlWriter);
            }
            finally {
                if (xmlWriter != null) {
                    xmlWriter.Flush();
                    xmlWriter.Close();
                }
            }
        }

        public override void Save(Stream w)
        {
            XmlWriter xmlWriter = null;
            try {
                if (PreserveWhitespace) {
                    XmlFormatter.Format(this);
                    xmlWriter = new XmlAttributePreservingWriter(w, TextEncoding);
                }
                else {
                    XmlTextWriter textWriter = new XmlTextWriter(w, TextEncoding);
                    textWriter.Formatting = Formatting.Indented;
                    xmlWriter = textWriter;
                }
                WriteTo(xmlWriter);
            }
            finally {
                if (xmlWriter != null) {
                    xmlWriter.Flush();
                }
            }
        }

        public override XmlElement CreateElement(string prefix, string localName, string namespaceURI) {
            if (HasErrorInfo) {
                return new XmlFileInfoElement(prefix, localName, namespaceURI, this);
            }
            else {
                return base.CreateElement(prefix, localName, namespaceURI);
            }
        }

        public override XmlAttribute CreateAttribute(string prefix, string localName, string namespaceURI) {
            if (HasErrorInfo) {
                return new XmlFileInfoAttribute(prefix, localName, namespaceURI, this);
            }
            else {
                return base.CreateAttribute(prefix, localName, namespaceURI);
            }
        }

        internal bool IsNewNode(XmlNode node) {
            // The transformation engine will only add elements. Anything
            // else that gets added must be contained by a new element.
            // So to determine what's new, we search up the tree for a new
            // element that contains this node.
            XmlFileInfoElement element = FindContainingElement(node) as XmlFileInfoElement;
            return element != null && !element.IsOriginal;
        }

        private XmlElement FindContainingElement(XmlNode node) {
            while (node != null && !(node is XmlElement)) {
                node = node.ParentNode;
            }
            return node as XmlElement;
        }

        #region XmlElement override
        private class XmlFileInfoElement : XmlElement, IXmlLineInfo, IXmlFormattableAttributes
        {
            private int lineNumber;
            private int linePosition;
            private bool isOriginal;

            private XmlAttributePreservationDict preservationDict = null;

            internal XmlFileInfoElement(string prefix, string localName, string namespaceUri, XmlFileInfoDocument document)
                : base(prefix, localName, namespaceUri, document) {
                lineNumber = document.CurrentLineNumber;
                linePosition = document.CurrentLinePosition;
                isOriginal = document.FirstLoad;

                if (document.PreservationProvider != null) {
                    preservationDict = document.PreservationProvider.GetDictAtPosition(lineNumber, linePosition - 1);
                }
                if (preservationDict == null) {
                    preservationDict = new XmlAttributePreservationDict();
                }
            }

            public override void WriteTo(XmlWriter w) {
                string prefix = Prefix;
                if (!String.IsNullOrEmpty(NamespaceURI)) {
                    prefix = w.LookupPrefix(NamespaceURI);
                    if (prefix == null) {
                        prefix = Prefix;
                    }
                }

                w.WriteStartElement(prefix, LocalName, NamespaceURI);

                if (HasAttributes) {
                    XmlAttributePreservingWriter preservingWriter = w as XmlAttributePreservingWriter;
                    if (preservingWriter == null || preservationDict == null) {
                        WriteAttributesTo(w);
                    }
                    else {
                        WritePreservedAttributesTo(preservingWriter);
                    }
                }

                if (IsEmpty) {
                    w.WriteEndElement();
                }
                else {
                    WriteContentTo(w);
                    w.WriteFullEndElement();
                }
            }

            private void WriteAttributesTo(XmlWriter w) {
                XmlAttributeCollection attrs = Attributes;
                for (int i = 0; i < attrs.Count; i += 1) {
                    XmlAttribute attr = attrs[i];
                    attr.WriteTo(w);
                }
            }

            private void WritePreservedAttributesTo(XmlAttributePreservingWriter preservingWriter) {
                preservationDict.WritePreservedAttributes(preservingWriter, Attributes);
            }

            #region IXmlLineInfo Members
            public bool HasLineInfo() {
                return true;
            }

            public int LineNumber {
                get {
                    return lineNumber;
                }
            }

            public int LinePosition {
                get {
                    return linePosition;
                }
            }

            public bool IsOriginal {
                get {
                    return isOriginal;
                }
            }
            #endregion

            #region IXmlFormattableNode Members
            void IXmlFormattableAttributes.FormatAttributes(XmlFormatter formatter) {
                preservationDict.UpdatePreservationInfo(Attributes, formatter);
            }

            string IXmlFormattableAttributes.AttributeIndent {
                get {
                    return preservationDict.GetAttributeNewLineString(null);
                }
            }
            #endregion
        }
        #endregion

        #region XmlAttribute override
        private class XmlFileInfoAttribute : XmlAttribute, IXmlLineInfo
        {
            private int lineNumber;
            private int linePosition;

            internal XmlFileInfoAttribute(string prefix, string localName, string namespaceUri, XmlFileInfoDocument document)
                : base(prefix, localName, namespaceUri, document) {
                lineNumber = document.CurrentLineNumber;
                linePosition = document.CurrentLinePosition;
            }

            #region IXmlLineInfo Members
            public bool HasLineInfo() {
                return true;
            }

            public int LineNumber {
                get {
                    return lineNumber;
                }
            }

            public int LinePosition {
                get {
                    return linePosition;
                }
            }
            #endregion
        }
        #endregion

        #region Dispose Pattern
        protected virtual void Dispose(bool disposing)
        {
            if (_reader != null)
            {
                _reader.Close();
                _reader = null;
            }

            if (_preservationProvider != null)
            {
                _preservationProvider.Close();
                _preservationProvider = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);        
        }

        ~XmlFileInfoDocument()
        {
            Debug.Fail("call dispose please");
            Dispose(false);
        }
        #endregion
    }
}
