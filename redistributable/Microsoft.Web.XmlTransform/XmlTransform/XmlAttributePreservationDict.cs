using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;

namespace Microsoft.Web.XmlTransform
{
    internal class XmlAttributePreservationDict
    {
        #region Private data members
        private List<string> orderedAttributes = new List<string>();
        private Dictionary<string, string> leadingSpaces = new Dictionary<string, string>();

        private string attributeNewLineString = null;
        private bool computedOneAttributePerLine = false;
        private bool oneAttributePerLine = false;
        #endregion

        private bool OneAttributePerLine {
            get {
                if (!computedOneAttributePerLine) {
                    computedOneAttributePerLine = true;
                    oneAttributePerLine = ComputeOneAttributePerLine();
                }
                return oneAttributePerLine;
            }
        }

        internal void ReadPreservationInfo(string elementStartTag) {
            Debug.Assert(elementStartTag.StartsWith("<", StringComparison.Ordinal) && elementStartTag.EndsWith(">", StringComparison.Ordinal), "Expected string containing exactly a single tag");
            WhitespaceTrackingTextReader whitespaceReader = new WhitespaceTrackingTextReader(new StringReader(elementStartTag));

            int lastCharacter = EnumerateAttributes(elementStartTag, 
                (int line, int linePosition, string attributeName) => 
                {
                    orderedAttributes.Add(attributeName);
                    if (whitespaceReader.ReadToPosition(line, linePosition))
                    {
                        leadingSpaces.Add(attributeName, whitespaceReader.PrecedingWhitespace);
                    }
                    else
                    {
                        Debug.Fail("Couldn't get leading whitespace for attribute");
                    }
                }
                );

            if (whitespaceReader.ReadToPosition(lastCharacter)) {
                leadingSpaces.Add(String.Empty, whitespaceReader.PrecedingWhitespace);
            }
            else {
                Debug.Fail("Couldn't get trailing whitespace for tag");
            }
        }

        private int EnumerateAttributes(string elementStartTag, Action<int, int, string> onAttributeSpotted)
        {
            bool selfClosed = (elementStartTag.EndsWith("/>", StringComparison.Ordinal));
            string xmlDocString = elementStartTag;
            if (!selfClosed)
            {
                xmlDocString = elementStartTag.Substring(0, elementStartTag.Length-1) + "/>" ;
            }

            XmlTextReader xmlReader = new XmlTextReader(new StringReader(xmlDocString));

            xmlReader.Namespaces = false;
            xmlReader.Read();

            bool hasMoreAttributes = xmlReader.MoveToFirstAttribute();
            while (hasMoreAttributes)
            {
                onAttributeSpotted(xmlReader.LineNumber, xmlReader.LinePosition, xmlReader.Name);
                hasMoreAttributes = xmlReader.MoveToNextAttribute();
            }

            int lastCharacter = elementStartTag.Length;
            if (selfClosed)
            {
                lastCharacter--;
            }
            return lastCharacter;
        }

        internal void WritePreservedAttributes(XmlAttributePreservingWriter writer, XmlAttributeCollection attributes) {
            string oldNewLineString = null;
            if (attributeNewLineString != null) {
                oldNewLineString = writer.SetAttributeNewLineString(attributeNewLineString);
            }

            try {
                foreach (string attributeName in orderedAttributes) {
                    XmlAttribute attr = attributes[attributeName];
                    if (attr != null) {
                        if (leadingSpaces.ContainsKey(attributeName)) {
                            writer.WriteAttributeWhitespace(leadingSpaces[attributeName]);
                        }

                        attr.WriteTo(writer);
                    }
                }

                if (leadingSpaces.ContainsKey(String.Empty)) {
                    writer.WriteAttributeTrailingWhitespace(leadingSpaces[String.Empty]);
                }
            }
            finally {
                if (oldNewLineString != null) {
                    writer.SetAttributeNewLineString(oldNewLineString);
                }
            }
        }

        internal void UpdatePreservationInfo(XmlAttributeCollection updatedAttributes, XmlFormatter formatter) {
            if (updatedAttributes.Count == 0) {
                if (orderedAttributes.Count > 0) {
                    // All attributes were removed, clear preservation info
                    leadingSpaces.Clear();
                    orderedAttributes.Clear();
                }
            }
            else {
                Dictionary<string, bool> attributeExists = new Dictionary<string, bool>();

                // Prepopulate the list with attributes that existed before
                foreach (string attributeName in orderedAttributes) {
                    attributeExists[attributeName] = false;
                }

                // Update the list with attributes that exist now
                foreach (XmlAttribute attribute in updatedAttributes) {
                    if (!attributeExists.ContainsKey(attribute.Name)) {
                        orderedAttributes.Add(attribute.Name);
                    }
                    attributeExists[attribute.Name] = true;
                }

                bool firstAttribute = true;
                string keepLeadingWhitespace = null;
                foreach (string key in orderedAttributes) {
                    bool exists = attributeExists[key];

                    // Handle removal
                    if (!exists) {
                        // We need to figure out whether to keep the leading
                        // or trailing whitespace. The logic is:
                        //   1. The whitespace before the first attribute is
                        //      always kept, so remove trailing space.
                        //   2. We want to keep newlines, so if the leading
                        //      space contains a newline then remove trailing
                        //      space. If multiple newlines are being removed,
                        //      keep the last one.
                        //   3. Otherwise, remove leading space.
                        //
                        // In order to remove trailing space, we have to 
                        // remove the leading space of the next attribute, so
                        // we store this leading space to replace the next.
                        if (leadingSpaces.ContainsKey(key)) {
                            string leadingSpace = leadingSpaces[key];
                            if (firstAttribute) {
                                if (keepLeadingWhitespace == null) {
                                    keepLeadingWhitespace = leadingSpace;
                                }
                            }
                            else if (ContainsNewLine(leadingSpace)) {
                                keepLeadingWhitespace = leadingSpace;
                            }

                            leadingSpaces.Remove(key);
                        }
                    }

                    else if (keepLeadingWhitespace != null) {
                        // Exception to rule #2 above: Don't replace an existing
                        // newline with one that was removed
                        if (firstAttribute || !leadingSpaces.ContainsKey(key) || !ContainsNewLine(leadingSpaces[key])) {
                            leadingSpaces[key] = keepLeadingWhitespace;
                        }
                        keepLeadingWhitespace = null;
                    }

                    // Handle addition
                    else if (!leadingSpaces.ContainsKey(key)) {
                        if (firstAttribute) {
                            // This will prevent the textwriter from writing a
                            // newline before the first attribute
                            leadingSpaces[key] = " ";
                        }
                        else if (OneAttributePerLine) {
                            // Add the indent space between each attribute
                            leadingSpaces[key] = GetAttributeNewLineString(formatter);
                        }
                        else {
                            // Don't add any hard-coded spaces. All new attributes
                            // should be at the end, so they'll be formatted while
                            // writing. Make sure we have the right indent string,
                            // though.
                            EnsureAttributeNewLineString(formatter);
                        }
                    }

                    // firstAttribute remains true until we find the first
                    // attribute that actually exists
                    firstAttribute = firstAttribute && !exists;
                }
            }
        }

        private bool ComputeOneAttributePerLine() {
            if (leadingSpaces.Count > 1) {
                // If there is a newline between each pair of attributes, then
                // we'll continue putting newlines between all attributes. If
                // there's no newline between one pair, then we won't.
                bool firstAttribute = true;
                foreach (string attributeName in orderedAttributes) {
                    // The space in front of the first attribute doesn't count,
                    // because that space isn't between attributes.
                    if (firstAttribute) {
                        firstAttribute = false;
                    }
                    else if (leadingSpaces.ContainsKey(attributeName) &&
                        !ContainsNewLine(leadingSpaces[attributeName])) {
                        // This means there are two attributes on one line
                        return false;
                    }
                }

                return true;
            }
            else {
                // If there aren't at least two original attributes on this
                // tag, then it's not possible to tell if more than one would
                // be on a line. Default to more than one per line.
                // TODO(jodavis): Should we look at sibling tags to decide?
                return false;
            }
        }

        private bool ContainsNewLine(string space) {
            return space.IndexOf("\n", StringComparison.Ordinal) >= 0;
        }

        public string GetAttributeNewLineString(XmlFormatter formatter) {
            if (attributeNewLineString == null) {
                attributeNewLineString = ComputeAttributeNewLineString(formatter);
            }
            return attributeNewLineString;
        }

        private string ComputeAttributeNewLineString(XmlFormatter formatter) {
            string lookAheadString = LookAheadForNewLineString();
            if (lookAheadString != null) {
                return lookAheadString;
            }
            else if (formatter != null) {
                return formatter.CurrentAttributeIndent;
            }
            else {
                return null;
            }
        }

        private string LookAheadForNewLineString() {
            foreach (string space in leadingSpaces.Values) {
                if (ContainsNewLine(space)) {
                    return space;
                }
            }
            return null;
        }

        private void EnsureAttributeNewLineString(XmlFormatter formatter) {
            GetAttributeNewLineString(formatter);
        }
    }
}
