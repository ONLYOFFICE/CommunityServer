using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Xml;

namespace Microsoft.Web.XmlTransform
{
    internal class XmlAttributePreservationProvider : IDisposable
    {
        private StreamReader streamReader;
        private PositionTrackingTextReader reader;

        public XmlAttributePreservationProvider(string fileName) {
            streamReader = new StreamReader(File.OpenRead(fileName));
            reader = new PositionTrackingTextReader(streamReader);
        }

        public XmlAttributePreservationDict GetDictAtPosition(int lineNumber, int linePosition) {
            if (reader.ReadToPosition(lineNumber, linePosition)) {
                Debug.Assert((char)reader.Peek() == '<');

                StringBuilder sb = new StringBuilder();
                int character;
                bool inAttribute = false;
                do {
                    character = reader.Read();
                    if (character == '\"'){
                        inAttribute = !inAttribute;
                    }
                    sb.Append((char)character);
                }
                while (character > 0 && ((char)character != '>' || inAttribute));

                if (character > 0) {
                    XmlAttributePreservationDict dict = new XmlAttributePreservationDict();
                    dict.ReadPreservationInfo(sb.ToString());
                    return dict;
                }
            }

            Debug.Fail("Failed to get preservation info");
            return null;
        }

        public void Close()
        {
            Dispose();
            GC.SuppressFinalize(this);
        }

        public void Dispose()
        {
            if (streamReader != null)
            {
                streamReader.Close();
                streamReader = null;
            }
            if (reader != null)
            {
                reader.Dispose();
                reader = null;
            }
        }

        ~XmlAttributePreservationProvider()
        {
            Debug.Fail("cal dispose please");
            Dispose();
        }
    }
}
