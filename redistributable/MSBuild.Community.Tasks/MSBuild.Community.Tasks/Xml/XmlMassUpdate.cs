
using System;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.Xml;

namespace MSBuild.Community.Tasks.Xml
{
    /// <summary>
    /// Performs multiple updates on an XML file
    /// </summary>
    /// <include file='..\AdditionalDocumentation.xml' path='docs/task[@name="XmlMassUpdate"]/*'/>
    public class XmlMassUpdate : Task
    {
        private ITaskItem contentFile;

        /// <summary>
        /// The original file whose content is to be updated
        /// </summary>
        /// <remarks>This task is currently under construction and not necessarily feature complete.</remarks>
        [Required]
        public ITaskItem ContentFile
        {
            get { return contentFile; }
            set { contentFile = value; }
        }

        private ITaskItem substitutionsFile;

        /// <summary>
        /// The file containing the list of updates to perform
        /// </summary>
        public ITaskItem SubstitutionsFile
        {
            get { return substitutionsFile; }
            set { substitutionsFile = value; }
        }

        private ITaskItem mergedFile;

        /// <summary>
        /// The file created after performing the updates
        /// </summary>
        public ITaskItem MergedFile
        {
            get { return mergedFile; }
            set { mergedFile = value; }
        }


        private string substitutionsRoot;

        /// <summary>
        /// The XPath expression used to locate the list of substitutions to perform
        /// </summary>
        /// <remarks>When not specified, the default is the document root: <c>/</c>
        /// <para>When there is a set of elements with the same name, and you want to update
        /// a single element which can be identified by one of its attributes, you need to include an attribute
        /// named 'key' in the namespace <c>urn:msbuildcommunitytasks-xmlmassupdate</c>. The value of the
        /// attribute is the name of the attribute that should be used as the unique identifier.</para></remarks>
        public string SubstitutionsRoot
        {
            get { return substitutionsRoot; }
            set { substitutionsRoot = value; }
        }

        private string contentRoot;

        /// <summary>
        /// The XPath expression identifying root node that substitions are relative to
        /// </summary>
        /// <remarks>When not specified, the default is the document root: <c>/</c></remarks>
        public string ContentRoot
        {
            get { return contentRoot; }
            set { contentRoot = value; }
        }

        /// <summary>
        /// A collection of prefix=namespace definitions used to query the XML documents
        /// </summary>
        /// <include file='..\AdditionalDocumentation.xml' path='docs/task[@name="XmlQuery.NamespaceDefinitions"]/*'/>
        public ITaskItem[] NamespaceDefinitions
        {
            get { return namespaceDefinitions; }
            set { namespaceDefinitions = value; }
        }
        private ITaskItem[] namespaceDefinitions;
        XmlNamespaceManager namespaceManager;

        private bool ignoreMissingSubstitutionsRoot = true;

        /// <summary>
        /// If set to true, the task won't fail if the substitution root is missing
        /// </summary>
        public bool IgnoreMissingSubstitutionsRoot
        {
            get { return ignoreMissingSubstitutionsRoot; }
            set { ignoreMissingSubstitutionsRoot = value; }
        }

        /// <summary>
        /// The full path of the file containing content updated by the task
        /// </summary>
        [Output]
        public string ContentPathUsedByTask
        {
            get { return contentPathUsedByTask; }
        }
        private string contentPathUsedByTask;

        /// <summary>
        /// The full path of the file containing substitutions used by the task
        /// </summary>
        [Output]
        public string SubstitutionsPathUsedByTask
        {
            get { return substitutionsPathUsedByTask; }
        }
        string substitutionsPathUsedByTask;

        /// <summary>
        /// The full path of the file containing the results of the task
        /// </summary>
        [Output]
        public string MergedPathUsedByTask
        {
            get { return mergedPathUsedByTask; }
        }
        string mergedPathUsedByTask;

        /// <summary>
        /// When overridden in a derived class, executes the task.
        /// </summary>
        /// <returns>
        /// True if the task successfully executed; otherwise, false.
        /// </returns>
        public override bool Execute()
        {
            if (substitutionsFile == null) substitutionsFile = contentFile;
            if (mergedFile == null) mergedFile = contentFile;

            setContentPath();
            setSubstitutionsPath();
            setMergedPath();

            if (String.IsNullOrEmpty(substitutionsRoot)) substitutionsRoot = "/";
            if (String.IsNullOrEmpty(contentRoot)) contentRoot = "/";

            if (contentPathUsedByTask.Equals(substitutionsPathUsedByTask, StringComparison.InvariantCultureIgnoreCase) && (contentRoot == substitutionsRoot))
            {
                Log.LogError("The SubstitutionsRoot must be different from the ContentRoot when the ContentFile and SubstitutionsFile are the same.");
                return false;
            }

            XmlDocument contentDocument = LoadContentDocument();
            if (contentDocument == null) return false;

            XmlDocument substitutionsDocument = LoadSubstitutionsDocument();
            if (substitutionsDocument == null) return false;

            namespaceManager = new XmlNamespaceManager(contentDocument.NameTable);
            XmlTaskHelper.LoadNamespaceDefinitionItems(namespaceManager, namespaceDefinitions);

            XmlNode substitutionsRootNode = substitutionsDocument.SelectSingleNode(substitutionsRoot, namespaceManager);
            XmlNode contentRootNode = contentDocument.SelectSingleNode(contentRoot, namespaceManager);

            if (substitutionsRootNode == null)
            {
                if (ignoreMissingSubstitutionsRoot)
                {
                    Log.LogMessage("Skip task: unable to locate '{0}' in {1}.", substitutionsRoot, substitutionsPathUsedByTask);
                    return true;
                }
                else
                {
                    var msg = String.Format("Unable to locate '{0}' in {1}.", substitutionsRoot, substitutionsPathUsedByTask);
                    Log.LogError(msg);
                    return false;
                }
            }
            if (contentRootNode == null)
            {
                Log.LogError("Unable to locate '{0}' in {1}.", contentRoot, contentPathUsedByTask);
                return false;
            }

            try
            {
                addAllChildNodes(contentDocument, contentRootNode, substitutionsRootNode);
            }
            catch (MultipleRootNodesException)
            {
                Log.LogError("Cannot create a new document root node because one already exists. Make sure to set the SubstitutionsRoot property.");
                return false;
            }

            return SaveMergedDocument(contentDocument);
        }

        private void setContentPath()
        {
            contentPathUsedByTask = contentFile.GetMetadata("FullPath");
            if (String.IsNullOrEmpty(contentPathUsedByTask)) contentPathUsedByTask = contentFile.ItemSpec;
        }

        private void setSubstitutionsPath()
        {
            substitutionsPathUsedByTask = substitutionsFile.GetMetadata("FullPath");
            if (String.IsNullOrEmpty(substitutionsPathUsedByTask)) substitutionsPathUsedByTask = substitutionsFile.ItemSpec;
        }

        private void setMergedPath()
        {
            mergedPathUsedByTask = mergedFile.GetMetadata("FullPath");
            if (String.IsNullOrEmpty(mergedPathUsedByTask)) mergedPathUsedByTask = mergedFile.ItemSpec;
        }

        /// <summary>
        /// Returns <see cref="SubstitutionsFile"/> as an <see cref="XmlDocument"/>.
        /// </summary>
        /// <remarks>This method is not intended for use by consumers. It is exposed for testing purposes.</remarks>
        /// <returns></returns>
        /// <exclude />
        protected virtual XmlDocument LoadSubstitutionsDocument()
        {
            XmlDocument substitutionsDocument;
            if (contentPathUsedByTask.Equals(substitutionsPathUsedByTask, StringComparison.InvariantCultureIgnoreCase))
            {
                substitutionsDocument = LoadContentDocument();
            }
            else
            {
                if (!System.IO.File.Exists(substitutionsPathUsedByTask))
                {
                    Log.LogError("Unable to load substitutions file {0}", substitutionsPathUsedByTask);
                    return null;
                }
                substitutionsDocument = new XmlDocument();
                substitutionsDocument.Load(substitutionsPathUsedByTask);
            }
            return substitutionsDocument;
        }

        /// <summary>
        /// Returns <see cref="ContentFile"/> as an <see cref="XmlDocument"/>.
        /// </summary>
        /// <remarks>This method is not intended for use by consumers. It is exposed for testing purposes.</remarks>
        /// <returns></returns>
        /// <exclude />
        protected virtual XmlDocument LoadContentDocument()
        {
            if (!System.IO.File.Exists(contentPathUsedByTask))
            {
                Log.LogError("Unable to load content file {0}", contentPathUsedByTask);
                return null;
            }
            XmlDocument contentDocument = new XmlDocument();
            contentDocument.Load(contentPathUsedByTask);
            return contentDocument;
        }

        /// <summary>
        /// Creates <see cref="MergedFile"/> from the specified <see cref="XmlDocument"/>
        /// </summary>
        /// <param name="mergedDocument">The XML to save to a file</param>
        /// <remarks>This method is not intended for use by consumers. It is exposed for testing purposes.</remarks>
        /// <returns></returns>
        /// <exclude />
        protected virtual bool SaveMergedDocument(XmlDocument mergedDocument)
        {
            try
            {
                mergedDocument.Save(mergedPathUsedByTask);
            }
            catch (System.IO.IOException exception)
            {
                Log.LogError("Unable to create MergedFile - {0}", exception.Message);
                return false;
            }
            return true;
        }

        private void addAllChildNodes(XmlDocument mergedDocument, XmlNode contentParentNode, XmlNode substitutionsParentNode)
        {
            XmlNode substitutionNode = substitutionsParentNode.FirstChild;
            while (substitutionNode != null)
            {
                switch(substitutionNode.NodeType)
                {
                    case XmlNodeType.Element:
                        if (shouldDeleteElement(substitutionNode))
                        {
                            removeChildNode(mergedDocument, contentParentNode, substitutionNode);
                        }
                        else
                        {
                            XmlNode mergedNode = addChildNode(mergedDocument, contentParentNode, substitutionNode);
                            addAllChildNodes(mergedDocument, mergedNode, substitutionNode);
                        }
                        break;
                    case XmlNodeType.Text:
                        contentParentNode.InnerText = substitutionNode.Value;
                        break;
                    case XmlNodeType.CDATA:
                        contentParentNode.RemoveAll();
                        contentParentNode.AppendChild(mergedDocument.CreateCDataSection(substitutionNode.Value));
                        break;
                    default:
                        break;
                }
                substitutionNode = substitutionNode.NextSibling;
            }
        }

        private void removeChildNode(XmlDocument mergedDocument, XmlNode contentParentNode, XmlNode substitutionNode)
        {
            modifyNode(mergedDocument, contentParentNode, substitutionNode, true);
        }

        private XmlNode addChildNode(XmlDocument mergedDocument, XmlNode destinationParentNode, XmlNode nodeToAdd)
        {
            XmlNode newNode = modifyNode(mergedDocument, destinationParentNode, nodeToAdd);

            foreach (XmlAttribute sourceAttribute in nodeToAdd.Attributes)
            {
                if (!isUpdateControlAttribute(sourceAttribute))
                {
                    setAttributeValue(mergedDocument, newNode, sourceAttribute.Name, sourceAttribute.Value);
                }
            }
            return newNode;
        }

        private static bool isUpdateControlAttribute(XmlAttribute attribute)
        {
            // check for the control namespace declaration
            if ((attribute.Prefix == "xmlns") && (attribute.Value == updateControlNamespace)) return true;
            // check for attributes within the control namespace
            if (attribute.NamespaceURI == updateControlNamespace) return true;
            return false;
        }

        private bool shouldDeleteElement(XmlNode sourceNode)
        {
            string action = getActionAttribute(sourceNode);
            return action.Equals("remove", StringComparison.InvariantCultureIgnoreCase);
        }

        private void setAttributeValue(XmlDocument mergedDocument, XmlNode modifiedNode, string attributeName, string attributeValue)
        {
            XmlAttribute targetAttribute = modifiedNode.Attributes[attributeName];
            if (targetAttribute == null)
            {
                Log.LogMessage(MessageImportance.Low, "Creating attribute '{0}' on '{1}'", attributeName, getFullPathOfNode(modifiedNode));
                targetAttribute = modifiedNode.Attributes.Append(mergedDocument.CreateAttribute(attributeName));
            }
            targetAttribute.Value = attributeValue;
            Log.LogMessage("Setting attribute '{0}' to '{1}' on '{2}'", targetAttribute.Name, targetAttribute.Value, getFullPathOfNode(modifiedNode));
        }

        private string getFullPathOfNode(XmlNode node)
        {
            string fullPath = String.Empty;
            XmlNode currentNode = node;
            while (currentNode != null && currentNode.NodeType != XmlNodeType.Document)
            {
                fullPath = "/" + currentNode.Name + fullPath;
                currentNode = currentNode.ParentNode;
            }
            return fullPath;
        }

        /// <summary>
        /// The namespace used for XmlMassUpdate pre-defined attributes
        /// </summary>
        /// <remarks>Evaluates to: <c>urn:msbuildcommunitytasks-xmlmassupdate</c>
        /// <para>Attributes decorated with this namespace are used to control how a substitutions element
        /// or attribute is handled during the update. For example, the key attribute is used to identify the
        /// attribute on an element that uniquely identifies the element in a set.</para></remarks>
        public string UpdateControlNamespace { get { return updateControlNamespace; } }
        private const string updateControlNamespace = "urn:msbuildcommunitytasks-xmlmassupdate";

        private XmlNode modifyNode(XmlDocument mergedDocument, XmlNode destinationParentNode, XmlNode nodeToModify)
        {
            return modifyNode(mergedDocument, destinationParentNode, nodeToModify, false);
        }
        private XmlNode modifyNode(XmlDocument mergedDocument, XmlNode destinationParentNode, XmlNode nodeToModify, bool remove)
        {
            XmlAttribute keyAttribute = getKeyAttribute(nodeToModify);
            XmlNode targetNode = locateTargetNode(destinationParentNode, nodeToModify, keyAttribute);
            if (targetNode == null)
            {
                if (remove) return null;
                if (destinationParentNode.NodeType == XmlNodeType.Document)
                {
                    throw new MultipleRootNodesException();
                }
                targetNode = destinationParentNode.AppendChild(mergedDocument.CreateNode(XmlNodeType.Element, nodeToModify.Name, nodeToModify.NamespaceURI));
                Log.LogMessage(MessageImportance.Low, "Created node '{0}'", getFullPathOfNode(targetNode));
                if (keyAttribute != null)
                {
                    XmlAttribute keyAttributeOnNewNode = targetNode.Attributes.Append(mergedDocument.CreateAttribute(keyAttribute.LocalName));
                    keyAttributeOnNewNode.Value = keyAttribute.Value;
                }
            }
            else
            {
                if (remove)
                {
                    Log.LogMessage(MessageImportance.Normal, "Removing node '{0}'", getFullPathOfNode(targetNode));
                    destinationParentNode.RemoveChild(targetNode);
                }
            }
            return targetNode;
        }

        private XmlNode locateTargetNode(XmlNode parentNode, XmlNode nodeToFind, XmlAttribute keyAttribute)
        {
            var prefix = namespaceManager.LookupPrefix(nodeToFind.NamespaceURI);
            var qname = String.IsNullOrEmpty(prefix) ? nodeToFind.LocalName : prefix + ":" + nodeToFind.LocalName;

            string xpath;
            if (keyAttribute == null)
            {
                xpath = qname;
            }
            else
            {
                Log.LogMessage(MessageImportance.Low, "Using keyed attribute '{0}={1}' to locate node '{2}'", keyAttribute.LocalName, keyAttribute.Value, getFullPathOfNode(parentNode) + "/" + qname);
                xpath = String.Format("{0}[@{1}='{2}']", qname, keyAttribute.LocalName, keyAttribute.Value);
            }
            XmlNode foundNode = parentNode.SelectSingleNode(xpath, namespaceManager);
            return foundNode;
        }

        private XmlAttribute getKeyAttribute(XmlNode sourceNode)
        {
            XmlAttribute keyAttribute = null;
            for (int i = 0; i < sourceNode.Attributes.Count; i++)
            {
                if ((sourceNode.Attributes[i].NamespaceURI == updateControlNamespace) &&
                    (sourceNode.Attributes[i].LocalName == "key"))
                {
                    string keyAttributeName = sourceNode.Attributes[i].Value;
                    keyAttribute = sourceNode.Attributes[keyAttributeName];
                    break;
                }
            }
            return keyAttribute;
        }

        private string getActionAttribute(XmlNode sourceNode)
        {
            XmlNamespaceManager specialNamespaces = new XmlNamespaceManager(sourceNode.OwnerDocument.NameTable);
            specialNamespaces.AddNamespace("xmu", updateControlNamespace);
            XmlNode actionNode = sourceNode.SelectSingleNode("@xmu:action", specialNamespaces);
            if (actionNode == null) return String.Empty;
            return actionNode.Value;
        }

        private class MultipleRootNodesException : Exception { }
    }
}
