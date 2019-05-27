using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.IO;
using System.Globalization;

namespace Microsoft.Web.XmlTransform
{
    internal class XmlElementContext : XmlNodeContext
    {
        #region private data members
        private XmlElementContext parentContext;
        private string xpath = null;
        private string parentXPath = null;
        private XmlDocument xmlTargetDoc;

        private IServiceProvider serviceProvider;

        private XmlNode transformNodes = null;
        private XmlNodeList targetNodes = null;
        private XmlNodeList targetParents = null;

        private XmlAttribute transformAttribute = null;
        private XmlAttribute locatorAttribute = null;

        private XmlNamespaceManager namespaceManager = null;
        #endregion

        public XmlElementContext(XmlElementContext parent, XmlElement element, XmlDocument xmlTargetDoc, IServiceProvider serviceProvider)
            : base(element) {
            this.parentContext = parent;
            this.xmlTargetDoc = xmlTargetDoc;
            this.serviceProvider = serviceProvider;
        }

        public T GetService<T>() where T : class {
            if (serviceProvider != null) {
                T service = serviceProvider.GetService(typeof(T)) as T;
                // now it is legal to return service that's null -- due to SetTokenizeAttributeStorage
                //Debug.Assert(service != null, String.Format(CultureInfo.InvariantCulture, "Service provider didn't provide {0}", typeof(ServiceType).Name));
                return service;
            }
            else {
                Debug.Fail("No ServiceProvider");
                return null;
            }
        }

        #region data accessors
        public XmlElement Element {
            get {
                return Node as XmlElement;
            }
        }

        public string XPath {
            get {
                if (xpath == null) {
                    xpath = ConstructXPath();
                }
                return xpath;
            }
        }

        public string ParentXPath {
            get {
                if (parentXPath == null) {
                    parentXPath = ConstructParentXPath();
                }
                return parentXPath;
            }
        }

        public Transform ConstructTransform(out string argumentString) {
            try {
                return CreateObjectFromAttribute<Transform>(out argumentString, out transformAttribute);
            }
            catch (Exception ex) {
                throw WrapException(ex);
            }
        }

        public int TransformLineNumber {
            get {
                IXmlLineInfo lineInfo = transformAttribute as IXmlLineInfo;
                if (lineInfo != null) {
                    return lineInfo.LineNumber;
                }
                else {
                    return LineNumber;
                }
            }
        }

        public int TransformLinePosition {
            get {
                IXmlLineInfo lineInfo = transformAttribute as IXmlLineInfo;
                if (lineInfo != null) {
                    return lineInfo.LinePosition;
                }
                else {
                    return LinePosition;
                }
            }
        }

        public XmlAttribute TransformAttribute {
            get {
                return transformAttribute;
            }
        }

        public XmlAttribute LocatorAttribute {
            get {
                return locatorAttribute;
            }
        }
        #endregion

        #region XPath construction
        private string ConstructXPath() {
            try {
                string argumentString;
                string parentPath = parentContext == null ? String.Empty : parentContext.XPath;

                Locator locator = CreateLocator(out argumentString);

                return locator.ConstructPath(parentPath, this, argumentString);
            }
            catch (Exception ex) {
                throw WrapException(ex);
            }
        }

        private string ConstructParentXPath() {
            try {
                string argumentString;
                string parentPath = parentContext == null ? String.Empty : parentContext.XPath;

                Locator locator = CreateLocator(out argumentString);

                return locator.ConstructParentPath(parentPath, this, argumentString);
            }
            catch (Exception ex) {
                throw WrapException(ex);
            }
        }

        private Locator CreateLocator(out string argumentString) {
            Locator locator = CreateObjectFromAttribute<Locator>(out argumentString, out locatorAttribute);
            if (locator == null) {
                argumentString = null;
                //avoid using singleton of "DefaultLocator.Instance", so unit tests can run parallel
                locator = new DefaultLocator();
            }
            return locator;
        }
        #endregion

        #region Context information
        internal XmlNode TransformNode {
            get {
                if (transformNodes == null) {
                    transformNodes = CreateCloneInTargetDocument(Element);
                }
                return transformNodes;
            }
        }

        internal XmlNodeList TargetNodes {
            get {
                if (targetNodes == null) {
                    targetNodes = GetTargetNodes(XPath);
                }
                return targetNodes;
            }
        }

        internal XmlNodeList TargetParents {
            get {
                if (targetParents == null && parentContext != null) {
                    targetParents = GetTargetNodes(ParentXPath);
                }
                return targetParents;
            }
        }
        #endregion

        #region Node helpers
        private XmlDocument TargetDocument {
            get {
                return xmlTargetDoc;
            }
        }

        private XmlNode CreateCloneInTargetDocument(XmlNode sourceNode) {
            XmlFileInfoDocument infoDocument = TargetDocument as XmlFileInfoDocument;
            XmlNode clonedNode;
            
            if (infoDocument != null) {
                clonedNode = infoDocument.CloneNodeFromOtherDocument(sourceNode);
            }
            else {
                XmlReader reader = new XmlTextReader(new StringReader(sourceNode.OuterXml));
                clonedNode = TargetDocument.ReadNode(reader);
            }

            ScrubTransformAttributesAndNamespaces(clonedNode);

            return clonedNode;
        }

        private void ScrubTransformAttributesAndNamespaces(XmlNode node) {
            if (node.Attributes != null) {
                List<XmlAttribute> attributesToRemove = new List<XmlAttribute>();
                foreach (XmlAttribute attribute in node.Attributes) {
                    if (attribute.NamespaceURI == XmlTransformation.TransformNamespace) {
                        attributesToRemove.Add(attribute);
                    }
                    else if (attribute.Prefix.Equals("xmlns") || attribute.Name.Equals("xmlns")) {
                        attributesToRemove.Add(attribute);
                    }
                    else {
                        attribute.Prefix = string.Empty;
                    }
                }
                foreach (XmlAttribute attributeToRemove in attributesToRemove) {
                    node.Attributes.Remove(attributeToRemove);
                }
            }

            // Do the same recursively for child nodes
            foreach (XmlNode childNode in node.ChildNodes) {
                ScrubTransformAttributesAndNamespaces(childNode);
            }
        }

        private XmlNodeList GetTargetNodes(string xpath) {
            XmlNamespaceManager mgr = GetNamespaceManager();
            return TargetDocument.SelectNodes(xpath, GetNamespaceManager());
        }

        private Exception WrapException(Exception ex) {
            return XmlNodeException.Wrap(ex, Element);
        }

        private Exception WrapException(Exception ex, XmlNode node) {
            return XmlNodeException.Wrap(ex, node);
        }

        private XmlNamespaceManager GetNamespaceManager() {
            if (namespaceManager == null) {
                XmlNodeList localNamespaces = Element.SelectNodes("namespace::*");

                if (localNamespaces.Count > 0) {
                    namespaceManager = new XmlNamespaceManager(Element.OwnerDocument.NameTable);

                    foreach (XmlAttribute nsAttribute in localNamespaces) {
                        string prefix = String.Empty;
                        int index = nsAttribute.Name.IndexOf(':');
                        if (index >= 0) {
                            prefix = nsAttribute.Name.Substring(index + 1);
                        }
                        else {
                            prefix = "_defaultNamespace";
                        }

                        namespaceManager.AddNamespace(prefix, nsAttribute.Value);
                    }
                }
                else {
                    namespaceManager = new XmlNamespaceManager(GetParentNameTable());
                }
            }
            return namespaceManager;
        }

        private XmlNameTable GetParentNameTable() {
            if (parentContext == null) {
                return Element.OwnerDocument.NameTable;
            }
            else {
                return parentContext.GetNamespaceManager().NameTable;
            }
        }
        #endregion

        #region Named object creation
        private static Regex nameAndArgumentsRegex = null;
        private Regex NameAndArgumentsRegex {
            get {
                if (nameAndArgumentsRegex == null) {
                    nameAndArgumentsRegex = new Regex(@"\A\s*(?<name>\w+)(\s*\((?<arguments>.*)\))?\s*\Z", RegexOptions.Compiled|RegexOptions.Singleline);
                }
                return nameAndArgumentsRegex;
            }
        }

        private string ParseNameAndArguments(string name, out string arguments) {
            arguments = null;

            System.Text.RegularExpressions.Match match = NameAndArgumentsRegex.Match(name);
            if (match.Success) {
                if (match.Groups["arguments"].Success) {
                    CaptureCollection argumentCaptures = match.Groups["arguments"].Captures;
                    if (argumentCaptures.Count == 1 && !String.IsNullOrEmpty(argumentCaptures[0].Value)) {
                        arguments = argumentCaptures[0].Value;
                    }
                }

                return match.Groups["name"].Captures[0].Value;
            }
            else {
                throw new XmlTransformationException(SR.XMLTRANSFORMATION_BadAttributeValue);
            }
        }

        private ObjectType CreateObjectFromAttribute<ObjectType>(out string argumentString, out XmlAttribute objectAttribute) where ObjectType : class {
            objectAttribute = Element.Attributes.GetNamedItem(typeof(ObjectType).Name, XmlTransformation.TransformNamespace) as XmlAttribute;
            try {
                if (objectAttribute != null) {
                    string typeName = ParseNameAndArguments(objectAttribute.Value, out argumentString);
                    if (!String.IsNullOrEmpty(typeName)) {
                        NamedTypeFactory factory = GetService<NamedTypeFactory>();
                        return factory.Construct<ObjectType>(typeName);
                    }
                }
            }
            catch (Exception ex) {
                throw WrapException(ex, objectAttribute);
            }

            argumentString = null;
            return null;
        }
        #endregion

        #region Error reporting helpers
        internal bool HasTargetNode(out XmlElementContext failedContext, out bool existedInOriginal) {
            failedContext = null;
            existedInOriginal = false;

            if (TargetNodes.Count == 0) {
                failedContext = this;
                while (failedContext.parentContext != null &&
                    failedContext.parentContext.TargetNodes.Count == 0) {

                    failedContext = failedContext.parentContext;
                }

                existedInOriginal = ExistedInOriginal(failedContext.XPath);
                return false;
            }

            return true;
        }

        internal bool HasTargetParent(out XmlElementContext failedContext, out bool existedInOriginal) {
            failedContext = null;
            existedInOriginal = false;

            if (TargetParents.Count == 0) {
                failedContext = this;
                while (failedContext.parentContext != null &&
                    !String.IsNullOrEmpty(failedContext.parentContext.ParentXPath) &&
                    failedContext.parentContext.TargetParents.Count == 0) {

                    failedContext = failedContext.parentContext;
                }

                existedInOriginal = ExistedInOriginal(failedContext.XPath);
                return false;
            }

            return true;
        }

        private bool ExistedInOriginal(string xpath) {
            IXmlOriginalDocumentService service = GetService<IXmlOriginalDocumentService>();
            if (service != null) {
                XmlNodeList nodeList = service.SelectNodes(xpath, GetNamespaceManager());
                if (nodeList != null && nodeList.Count > 0) {
                    return true;
                }
            }

            return false;
        }
        #endregion
    }
}
