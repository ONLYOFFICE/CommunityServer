using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Diagnostics;
using System.Globalization;

namespace Microsoft.Web.XmlTransform
{
    internal class XmlAttributePreservingWriter : XmlWriter
    {
        private XmlTextWriter xmlWriter;
        private AttributeTextWriter textWriter;

        public XmlAttributePreservingWriter(string fileName, Encoding encoding)
            : this(encoding == null ? new StreamWriter(fileName) : new StreamWriter(fileName, false, encoding)) {
        }

        public XmlAttributePreservingWriter(Stream w, Encoding encoding)
            : this(encoding == null ? new StreamWriter(w) : new StreamWriter(w, encoding))
        {
        }

        public XmlAttributePreservingWriter(TextWriter textWriter) {
            this.textWriter = new AttributeTextWriter(textWriter);
            this.xmlWriter = new XmlTextWriter(this.textWriter);
        }

        public void WriteAttributeWhitespace(string whitespace) {
            Debug.Assert(IsOnlyWhitespace(whitespace));

            // Make sure we're in the right place to write
            // whitespace between attributes
            if (WriteState == WriteState.Attribute) {
                WriteEndAttribute();
            }
            else if (WriteState != WriteState.Element) {
                throw new InvalidOperationException();
            }

            // We don't write right away. We're going to wait until an
            // attribute is being written
            textWriter.AttributeLeadingWhitespace = whitespace;
        }

        public void WriteAttributeTrailingWhitespace(string whitespace) {
            Debug.Assert(IsOnlyWhitespace(whitespace));

            if (WriteState == WriteState.Attribute) {
                WriteEndAttribute();
            }
            else if (WriteState != WriteState.Element) {
                throw new InvalidOperationException();
            }

            textWriter.Write(whitespace);
        }

        public string SetAttributeNewLineString(string newLineString) {
            string old = textWriter.AttributeNewLineString;

            if (newLineString == null && xmlWriter.Settings != null) {
                newLineString = xmlWriter.Settings.NewLineChars;
            }
            if (newLineString == null) {
                newLineString = "\r\n";
            }
            textWriter.AttributeNewLineString = newLineString;

            return old;
        }

        private bool IsOnlyWhitespace(string whitespace) {
            foreach (char whitespaceCharacter in whitespace) {
                if (!Char.IsWhiteSpace(whitespaceCharacter)) {
                    return false;
                }
            }
            return true;
        }

        #region SkippingTextWriter class
        private class AttributeTextWriter : TextWriter
        {
            enum State
            {
                Writing,
                WaitingForAttributeLeadingSpace,
                ReadingAttribute,
                Buffering,
                FlushingBuffer,
            }

            #region private data members
            State state = State.Writing;
            StringBuilder writeBuffer = null;

            private TextWriter baseWriter;
            string leadingWhitespace = null;

            int lineNumber = 1;
            int linePosition = 1;
            int maxLineLength = 160;
            string newLineString = "\r\n";
            #endregion

            public AttributeTextWriter(TextWriter baseWriter)
                : base(CultureInfo.InvariantCulture) {
                this.baseWriter = baseWriter;
            }

            public string AttributeLeadingWhitespace {
                set {
                    leadingWhitespace = value;
                }
            }

            public string AttributeNewLineString {
                get {
                    return newLineString;
                }
                set {
                    newLineString = value;
                }
            }

            public void StartAttribute() {
                Debug.Assert(state == State.Writing);

                ChangeState(State.WaitingForAttributeLeadingSpace);
            }

            public void EndAttribute() {
                Debug.Assert(state == State.ReadingAttribute);

                WriteQueuedAttribute();
            }

            public int MaxLineLength {
                get {
                    return maxLineLength;
                }
                set {
                    maxLineLength = value;
                }
            }

            public override void Write(char value) {
                UpdateState(value);

                switch (state) {
                    case State.WaitingForAttributeLeadingSpace:
                        if (value == ' ') {
                            ChangeState(State.ReadingAttribute);
                            break;
                        }
                        goto case State.Writing;
                    case State.Writing:
                    case State.FlushingBuffer:
                        ReallyWriteCharacter(value);
                        break;
                    case State.ReadingAttribute:
                    case State.Buffering:
                        writeBuffer.Append(value);
                        break;
                }
            }

            private void UpdateState(char value) {
                // This logic prevents writing the leading space that
                // XmlTextWriter wants to put before "/>". 
                switch (value) {
                    case ' ':
                        if (state == State.Writing) {
                            ChangeState(State.Buffering);
                        }
                        break;
                    case '/':
                        break;
                    case '>':
                        if (state == State.Buffering) {
                            string currentBuffer = writeBuffer.ToString();
                            if (currentBuffer.EndsWith(" /", StringComparison.Ordinal)) {
                                // We've identified the string " />" at the
                                // end of the buffer, so remove the space
                                writeBuffer.Remove(currentBuffer.LastIndexOf(' '), 1);
                            }
                            ChangeState(State.Writing);
                        }
                        break;
                    default:
                        if (state == State.Buffering) {
                            ChangeState(State.Writing);
                        }
                        break;
                }
            }

            private void ChangeState(State newState) {
                if (state != newState) {
                    State oldState = state;
                    state = newState;

                    // Handle buffer management for different states
                    if (StateRequiresBuffer(newState)) {
                        CreateBuffer();
                    }
                    else if (StateRequiresBuffer(oldState)) {
                        FlushBuffer();
                    }
                }
            }

            private bool StateRequiresBuffer(State state) {
                return state == State.Buffering || state == State.ReadingAttribute;
            }

            private void CreateBuffer() {
                Debug.Assert(writeBuffer == null);
                if (writeBuffer == null) {
                    writeBuffer = new StringBuilder();
                }
            }

            private void FlushBuffer() {
                Debug.Assert(writeBuffer != null);
                if (writeBuffer != null) {
                    State oldState = state;
                    try {
                        state = State.FlushingBuffer;

                        Write(writeBuffer.ToString());
                        writeBuffer = null;
                    }
                    finally {
                        state = oldState;
                    }
                }
            }

            private void ReallyWriteCharacter(char value) {
                baseWriter.Write(value);

                if (value == '\n') {
                    lineNumber++;
                    linePosition = 1;
                }
                else {
                    linePosition++;
                }
            }

            private void WriteQueuedAttribute() {
                // Write leading whitespace
                if (leadingWhitespace != null) {
                    writeBuffer.Insert(0, leadingWhitespace);
                    leadingWhitespace = null;
                }
                else {
                    int lineLength = linePosition + writeBuffer.Length + 1;
                    if (lineLength > MaxLineLength) {
                        writeBuffer.Insert(0, AttributeNewLineString);
                    }
                    else {
                        writeBuffer.Insert(0, ' ');
                    }
                }

                // Flush the buffer and start writing characters again
                ChangeState(State.Writing);
            }

            public override Encoding Encoding {
                get {
                    return baseWriter.Encoding;
                }
            }

            public override void Flush() {
                baseWriter.Flush();
            }

            public override void Close() {
                baseWriter.Close();
            }
        }
        #endregion

        #region XmlWriter implementation
        public override void Close() {
            xmlWriter.Close();
        }

        public override void Flush() {
            xmlWriter.Flush();
        }

        public override string LookupPrefix(string ns) {
            return xmlWriter.LookupPrefix(ns);
        }

        public override void WriteBase64(byte[] buffer, int index, int count) {
            xmlWriter.WriteBase64(buffer, index, count);
        }

        public override void WriteCData(string text) {
            xmlWriter.WriteCData(text);
        }

        public override void WriteCharEntity(char ch) {
            xmlWriter.WriteCharEntity(ch);
        }

        public override void WriteChars(char[] buffer, int index, int count) {
            xmlWriter.WriteChars(buffer, index, count);
        }

        public override void WriteComment(string text) {
            xmlWriter.WriteComment(text);
        }

        public override void WriteDocType(string name, string pubid, string sysid, string subset) {
            xmlWriter.WriteDocType(name, pubid, sysid, subset);
        }

        public override void WriteEndAttribute() {
            xmlWriter.WriteEndAttribute();

            textWriter.EndAttribute();
        }

        public override void WriteEndDocument() {
            xmlWriter.WriteEndDocument();
        }

        public override void WriteEndElement() {
            xmlWriter.WriteEndElement();
        }

        public override void WriteEntityRef(string name) {
            xmlWriter.WriteEntityRef(name);
        }

        public override void WriteFullEndElement() {
            xmlWriter.WriteFullEndElement();
        }

        public override void WriteProcessingInstruction(string name, string text) {
            xmlWriter.WriteProcessingInstruction(name, text);
        }

        public override void WriteRaw(string data) {
            xmlWriter.WriteRaw(data);
        }

        public override void WriteRaw(char[] buffer, int index, int count) {
            xmlWriter.WriteRaw(buffer, index, count);
        }

        public override void WriteStartAttribute(string prefix, string localName, string ns) {
            textWriter.StartAttribute();

            xmlWriter.WriteStartAttribute(prefix, localName, ns);
        }

        public override void WriteStartDocument(bool standalone) {
            xmlWriter.WriteStartDocument(standalone);
        }

        public override void WriteStartDocument() {
            xmlWriter.WriteStartDocument();
        }

        public override void WriteStartElement(string prefix, string localName, string ns) {
            xmlWriter.WriteStartElement(prefix, localName, ns);
        }

        public override WriteState WriteState {
            get {
                return xmlWriter.WriteState;
            }
        }

        public override void WriteString(string text) {
            xmlWriter.WriteString(text);
        }

        public override void WriteSurrogateCharEntity(char lowChar, char highChar) {
            xmlWriter.WriteSurrogateCharEntity(lowChar, highChar);
        }

        public override void WriteWhitespace(string ws) {
            xmlWriter.WriteWhitespace(ws);
        }
        #endregion
    }
}
