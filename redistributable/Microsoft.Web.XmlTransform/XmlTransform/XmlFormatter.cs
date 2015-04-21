using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;
using System.Globalization;

namespace Microsoft.Web.XmlTransform
{
    internal interface IXmlFormattableAttributes
    {
        void FormatAttributes(XmlFormatter formatter);

        string AttributeIndent { get; }
    }

    internal class XmlFormatter
    {
        private XmlFileInfoDocument document;
        private string originalFileName;

        private LinkedList<string> indents = new LinkedList<string>();
        private LinkedList<string> attributeIndents = new LinkedList<string>();
        private string currentIndent = String.Empty;
        private string currentAttributeIndent = null;
        private string oneTab = null;
        private string defaultTab = "\t";
        private XmlNode currentNode = null;
        private XmlNode previousNode = null;


        public static void Format(XmlDocument document) {
            XmlFileInfoDocument errorInfoDocument = document as XmlFileInfoDocument;
            if (errorInfoDocument != null) {
                XmlFormatter formatter = new XmlFormatter(errorInfoDocument);
                formatter.FormatLoop(errorInfoDocument);
            }
        }

        private XmlFormatter(XmlFileInfoDocument document) {
            this.document = document;
            this.originalFileName = document.FileName;
        }

        private XmlNode CurrentNode {
            get {
                return currentNode;
            }
            set {
                previousNode = currentNode;
                currentNode = value;
            }
        }

        private XmlNode PreviousNode {
            get {
                return previousNode;
            }
        }

        private string PreviousIndent {
            get {
                Debug.Assert(indents.Count > 0, "Expected at least one previous indent");
                return indents.Last.Value;
            }
        }

        private string CurrentIndent {
            get {
                if (currentIndent == null) {
                    currentIndent = ComputeCurrentIndent();
                }
                return currentIndent;
            }
        }

        public string CurrentAttributeIndent {
            get {
                if (currentAttributeIndent == null) {
                    currentAttributeIndent = ComputeCurrentAttributeIndent();
                }
                return currentAttributeIndent;
            }
        }

        private string OneTab {
            get {
                if (oneTab == null) {
                    oneTab = ComputeOneTab();
                }
                return oneTab;
            }
        }

        public string DefaultTab {
            get {
                return defaultTab;
            }
            set {
                defaultTab = value;
            }
        }

        private void FormatLoop(XmlNode parentNode) {
            for (int i = 0; i < parentNode.ChildNodes.Count; i++) {
                XmlNode node = parentNode.ChildNodes[i];
                CurrentNode = node;

                switch (node.NodeType) {
                    case XmlNodeType.Element:
                        i += HandleElement(node);
                        break;
                    case XmlNodeType.Whitespace:
                        i += HandleWhiteSpace(node);
                        break;
                    case XmlNodeType.Comment:
                    case XmlNodeType.Entity:
                        i += EnsureNodeIndent(node, false);
                        break;
                    case XmlNodeType.ProcessingInstruction:
                    case XmlNodeType.CDATA:
                    case XmlNodeType.Text:
                    case XmlNodeType.EntityReference:
                    case XmlNodeType.DocumentType:
                    case XmlNodeType.XmlDeclaration:
                        // Do nothing
                        break;
                    default:
                        Debug.Fail(String.Format(CultureInfo.InvariantCulture, "Unexpected element type '{0}' while formatting document", node.NodeType.ToString()));
                        break;
                }
            }
        }

        private void FormatAttributes(XmlNode node) {
            IXmlFormattableAttributes formattable = node as IXmlFormattableAttributes;
            if (formattable != null) {
                formattable.FormatAttributes(this);
            }
        }

        private int HandleElement(XmlNode node) {
            int indexChange = HandleStartElement(node);

            ReorderNewItemsAtEnd(node);

            // Loop over children
            FormatLoop(node);

            CurrentNode = node;

            indexChange += HandleEndElement(node);

            return indexChange;
        }

        // This is a special case for preserving the whitespace that existed
        // before the end tag of this node. If elements were inserted after
        // that whitespace, the whitespace needs to be moved back to the
        // end of the child list, and new whitespaces should be inserted
        // *before* the new nodes.
        private void ReorderNewItemsAtEnd(XmlNode node) {
            // If this is a new node, then there couldn't be original
            // whitespace before the end tag
            if (!IsNewNode(node)) {

                // If the last child isn't whitespace, new elements might
                // have been added
                XmlNode iter = node.LastChild;
                if (iter != null && iter.NodeType != XmlNodeType.Whitespace) {

                    // The loop continues until we find something that isn't
                    // a new Element. If it's whitespace, then that will be
                    // the whitespace we need to move.
                    XmlNode whitespace = null;
                    while (iter != null) {
                        switch (iter.NodeType) {
                            case XmlNodeType.Whitespace:
                                // Found the whitespace, loop can stop
                                whitespace = iter;
                                break;
                            case XmlNodeType.Element:
                                // Loop continues over new Elements
                                if (IsNewNode(iter)) {
                                    iter = iter.PreviousSibling;
                                    continue;
                                }
                                break;
                            default:
                                // Anything else stops the loop
                                break;
                        }
                        break;
                    }

                    if (whitespace != null) {
                        // We found whitespace to move. Remove it from where
                        // it is and add it back to the end
                        node.RemoveChild(whitespace);
                        node.AppendChild(whitespace);
                    }
                }
            }
        }

        private int HandleStartElement(XmlNode node) {
            int indexChange = EnsureNodeIndent(node, false);

            FormatAttributes(node);

            PushIndent();

            return indexChange;
        }

        private int HandleEndElement(XmlNode node) {
            int indexChange = 0;

            PopIndent();

            if (!((XmlElement)node).IsEmpty) {
                indexChange = EnsureNodeIndent(node, true);
            }

            return indexChange;
        }

        private int HandleWhiteSpace(XmlNode node) {
            int indexChange = 0;

            // If we find two WhiteSpace nodes in a row, it means some node
            // was removed in between. Remove the previous whitespace.
            // Dev10 bug 830497 Need to improve Error messages from the publish pipeline
            // basically the PreviousNode can be null, it need to be check before use the note.
            if (IsWhiteSpace(PreviousNode))
            {
                // Prefer to keep 'node', but if 'PreviousNode' has a newline
                // and 'node' doesn't, keep the whitespace with the newline
                XmlNode removeNode = PreviousNode;
                if (FindLastNewLine(node.OuterXml) < 0 &&
                    FindLastNewLine(PreviousNode.OuterXml) >= 0) {
                    removeNode = node;
                }

                removeNode.ParentNode.RemoveChild(removeNode);
                indexChange = -1;
            }

            string indent = GetIndentFromWhiteSpace(node);
            if (indent != null) {
                SetIndent(indent);
            }

            return indexChange;
        }

        private int EnsureNodeIndent(XmlNode node, bool indentBeforeEnd) {
            int indexChange = 0;

            if (NeedsIndent(node, PreviousNode)) {
                if (indentBeforeEnd) {
                    InsertIndentBeforeEnd(node);
                }
                else {
                    InsertIndentBefore(node);
                    indexChange = 1;
                }
            }

            return indexChange;
        }

        private string GetIndentFromWhiteSpace(XmlNode node) {
            string whitespace = node.OuterXml;
            int index = FindLastNewLine(whitespace);
            if (index >= 0) {
                return whitespace.Substring(index);
            }
            else {
                // If there's no newline, then this is whitespace in the
                // middle of a line, not an indent
                return null;
            }
        }

        private int FindLastNewLine(string whitespace) {
            for (int i = whitespace.Length - 1; i >= 0; i--) {
                switch (whitespace[i]) {
                    case '\r':
                        return i;
                    case '\n':
                        if (i > 0 && whitespace[i - 1] == '\r') {
                            return i - 1;
                        }
                        else {
                            return i;
                        }
                    case ' ':
                    case '\t':
                        break;
                    default:
                        // Non-whitespace character, not legal in indent text
                        return -1;
                }
            }

            // No newline found
            return -1;
        }

        private void SetIndent(string indent) {
            if (currentIndent == null || !currentIndent.Equals(indent)) {
                currentIndent = indent;

                // These strings will be determined on demand
                oneTab = null;
                currentAttributeIndent = null;
            }
        }

        private void PushIndent() {
            indents.AddLast(new LinkedListNode<string>(CurrentIndent));

            // The next indent will be determined on demand, assuming
            // we don't find one before it's needed
            currentIndent = null;

            // Don't use the property accessor to push the attribute
            // indent. These aren't always needed, so we don't compute
            // them until necessary. Also, we don't walk through this
            // stack like we do the indents stack.
            attributeIndents.AddLast(new LinkedListNode<string>(currentAttributeIndent));
            currentAttributeIndent = null;
        }

        private void PopIndent() {
            if (indents.Count > 0) {
                currentIndent = indents.Last.Value;
                indents.RemoveLast();

                currentAttributeIndent = attributeIndents.Last.Value;
                attributeIndents.RemoveLast();
            }
            else {
                Debug.Fail("Popped too many indents");
                throw new InvalidOperationException();
            }
        }

        private bool NeedsIndent(XmlNode node, XmlNode previousNode) {
            return !IsWhiteSpace(previousNode)
                && !IsText(previousNode)
                && (IsNewNode(node) || IsNewNode(previousNode));
        }

        private bool IsWhiteSpace(XmlNode node) {
            return node != null
                && node.NodeType == XmlNodeType.Whitespace;
        }

        public bool IsText(XmlNode node) {
            return node != null
                && node.NodeType == XmlNodeType.Text;
        }

        private bool IsNewNode(XmlNode node) {
            return node != null
                && document.IsNewNode(node);
        }

        private void InsertIndentBefore(XmlNode node) {
            node.ParentNode.InsertBefore(document.CreateWhitespace(CurrentIndent), node);
        }

        private void InsertIndentBeforeEnd(XmlNode node) {
            node.AppendChild(document.CreateWhitespace(CurrentIndent));
        }

        private string ComputeCurrentIndent() {
            string lookAheadIndent = LookAheadForIndent();
            if (lookAheadIndent != null) {
                return lookAheadIndent;
            }
            else {
                return PreviousIndent + OneTab;
            }
        }

        private string LookAheadForIndent() {
            if (currentNode.ParentNode == null) {
                return null;
            }

            foreach (XmlNode siblingNode in currentNode.ParentNode.ChildNodes) {
                if (IsWhiteSpace(siblingNode) && siblingNode.NextSibling != null)
                {
                    string whitespace = siblingNode.OuterXml;
                    int index = FindLastNewLine(whitespace);
                    if (index >= 0) {
                        return whitespace.Substring(index);
                    }
                }
            }

            return null;
        }

        private string ComputeOneTab() {
            Debug.Assert(indents.Count > 0, "Expected at least one previous indent");
            if (indents.Count < 0) {
                return DefaultTab;
            }

            LinkedListNode<string> currentIndentNode = indents.Last;
            LinkedListNode<string> previousIndentNode = currentIndentNode.Previous;

            while (previousIndentNode != null) {
                // If we can determine the difference between the current indent
                // and the previous one, then that's the value of one tab
                if (currentIndentNode.Value.StartsWith(previousIndentNode.Value, StringComparison.Ordinal)) {
                    return currentIndentNode.Value.Substring(previousIndentNode.Value.Length);
                }

                currentIndentNode = previousIndentNode;
                previousIndentNode = currentIndentNode.Previous;
            }

            return ConvertIndentToTab(currentIndentNode.Value);
        }

        private string ConvertIndentToTab(string indent) {
            for (int index = 0; index < indent.Length - 1; index++) {
                switch (indent[index]) {
                    case '\r':
                    case '\n':
                        break;
                    default:
                        return indent.Substring(index + 1);
                }
            }

            // There were no characters after the newlines, or the string was
            // empty. Fall back on the default value
            return DefaultTab;
        }

        private string ComputeCurrentAttributeIndent() {
            string siblingIndent = LookForSiblingIndent(CurrentNode);
            if (siblingIndent != null) {
                return siblingIndent;
            }
            else {
                return CurrentIndent + OneTab;
            }
        }

        private string LookForSiblingIndent(XmlNode currentNode) {
            bool beforeCurrentNode = true;
            string foundIndent = null;

            foreach (XmlNode node in currentNode.ParentNode.ChildNodes) {
                if (node == currentNode) {
                    beforeCurrentNode = false;
                }
                else {
                    IXmlFormattableAttributes formattable = node as IXmlFormattableAttributes;
                    if (formattable != null) {
                        foundIndent = formattable.AttributeIndent;
                    }
                }

                if (!beforeCurrentNode && foundIndent != null) {
                    return foundIndent;
                }
            }

            return null;
        }
    }
}
