
using System;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.Xml.XPath;
using System.Xml;
using System.Collections.Generic;

namespace MSBuild.Community.Tasks.Xml
{
    /// <summary>
    /// Reads a value or values from lines of XML
    /// </summary>
    /// <include file='..\AdditionalDocumentation.xml' path='docs/task[@name="XmlQuery"]/*'/>
    public class XmlQuery : Task
    {
        private ITaskItem[] lines;

        /// <summary>
        /// The lines of a valid XML document
        /// </summary>
        public ITaskItem[] Lines
        {
            get { return lines; }
            set { lines = value; }
        }

        private string xmlFileName;

        /// <summary>
        /// Gets or sets the name of an XML file to query
        /// </summary>
        /// <value>The full path of the XML file.</value>
        public string XmlFileName
        {
            get { return xmlFileName; }
            set { xmlFileName = value; }
        }

        private ITaskItem[] namespaceDefinitions;

        /// <summary>
        /// A collection of prefix=namespace definitions used to query the XML document
        /// </summary>
        /// <include file='..\AdditionalDocumentation.xml' path='docs/task[@name="XmlQuery.NamespaceDefinitions"]/*'/>
        public ITaskItem[] NamespaceDefinitions
        {
            get { return namespaceDefinitions; }
            set { namespaceDefinitions = value; }
        }

        private string xpath;

        /// <summary>
        /// The query used to identify the values in the XML document
        /// </summary>
        [Required]
        public string XPath
        {
            get { return xpath; }
            set { xpath = value; }
        }

        private List<ITaskItem> values = new List<ITaskItem>();

        /// <summary>
        /// The values selected by <see cref="XPath"/>
        /// </summary>
        [Output]
        public ITaskItem[] Values
        {
            get { return values.ToArray(); }
        }

        /// <summary>
        /// The number of values returned in <see cref="Values"/>
        /// </summary>
        [Output]
        public int ValuesCount { get { return values.Count; } }


        private string reservedMetaDataPrefix = "_";

        /// <summary>
        /// The string that is prepended to all reserved metadata properties.
        /// </summary>
        /// <remarks>The default value is a single underscore: '_'
        /// <para>All attributes of an element node are added as metadata to the returned ITaskItem,
        /// so this property can be used to avoid clashes with the reserved properties.
        /// For example, if you selected the following node:
        /// <code><![CDATA[ <SomeNode _name="x" _value="y" /> ]]></code>
        /// the <c>_value</c> attribute would clash with the <c>value</c> reserved property, when using
        /// the default prefix. If you set the ReservedMetaDataPrefix to another value (two underscores '__')
        /// there would be no clash. You would be able to select the attribute using <c>%(item._value)</c>
        /// and the value of the node using <c>%(item.__value)</c>.</para></remarks>
        public string ReservedMetaDataPrefix
        {
            get { return reservedMetaDataPrefix; }
            set { reservedMetaDataPrefix = value; }
        }


        /// <summary>
        /// When overridden in a derived class, executes the task.
        /// </summary>
        /// <returns>
        /// True if the task successfully executed; otherwise, false.
        /// </returns>
        public override bool Execute()
        {
            if (!validParameters()) return false;

            XPathDocument document = loadXmlContent();
            XPathNavigator navigator = document.CreateNavigator();

            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(navigator.NameTable);
            XmlTaskHelper.LoadNamespaceDefinitionItems(namespaceManager, namespaceDefinitions);

            XPathExpression expression = XPathExpression.Compile(xpath, namespaceManager);

            //Expressions that return a node set can be used in the Select and Evaluate methods. Expressions that return a Boolean, number, or string can be used in the Evaluate method.
            switch (expression.ReturnType)
            {
                case XPathResultType.Boolean:
                case XPathResultType.Number:
                case XPathResultType.String:
                    values.Add(new TaskItem(navigator.Evaluate(expression).ToString()));
                    break;
                case XPathResultType.NodeSet:
                    XPathNodeIterator nodes = navigator.Select(expression);
                    while (nodes.MoveNext())
                    {
                        values.Add(new XmlNodeTaskItem(nodes.Current, reservedMetaDataPrefix));
                    }
                    break;
                default:
                    throw new ArgumentException("Unable to evaluate XPath expression.", "XPath");
            }


            return true;
        }

        private XPathDocument loadXmlContent()
        {
            if (xmlFileName != null)
            {
                return new XPathDocument(xmlFileName);
            }
            
            System.IO.StringReader xmlContent = new System.IO.StringReader(XmlTaskHelper.JoinItems(lines));
            return new XPathDocument(xmlContent);
        }

        private bool validParameters()
        {
            if (xpath == null)
            {
                Log.LogError("You must provide a value for the XPath property.");
                return false;
            }

            if ((lines == null && xmlFileName == null) || (lines != null && xmlFileName != null))
            {
                Log.LogError("You must provide a value for either the Lines or XmlFileName property.");
                return false;
            }
            if (xmlFileName != null && !System.IO.File.Exists(xmlFileName))
            {
                Log.LogError("Could not find the file provided in the XmlFileName property.");
                return false;
            }
            return true;
        }
    }
}
