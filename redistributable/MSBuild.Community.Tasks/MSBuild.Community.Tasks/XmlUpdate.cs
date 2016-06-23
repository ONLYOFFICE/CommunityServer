#region Copyright © 2005 Paul Welter. All rights reserved.
/*
Copyright © 2005 Paul Welter. All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions
are met:

1. Redistributions of source code must retain the above copyright
   notice, this list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright
   notice, this list of conditions and the following disclaimer in the
   documentation and/or other materials provided with the distribution.
3. The name of the author may not be used to endorse or promote products
   derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE AUTHOR "AS IS" AND ANY EXPRESS OR
IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE. 
*/
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.Xml.XPath;



namespace MSBuild.Community.Tasks
{
    /// <summary>
    /// Updates a XML document using a XPath.
    /// </summary>
    /// <example>Update a XML element.
    /// <code><![CDATA[
    /// <XmlUpdate Prefix="n"
    ///     Namespace="http://schemas.microsoft.com/developer/msbuild/2003" 
    ///     XPath="/n:Project/n:PropertyGroup/n:TestUpdate"
    ///     XmlFileName="Subversion.proj"
    ///     Value="Test from $(MSBuildProjectFile)"/>
    /// ]]></code>
    /// </example>
    /// <remarks>
    /// The XML node being updated must exist before using the XmlUpdate task.
    /// </remarks>
    public class XmlUpdate : Task
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:XmlUpdate"/> class.
        /// </summary>
        public XmlUpdate()
        {

        }

        #region Properties
        private string _xmlFileName;

        /// <summary>
        /// Gets or sets the name of the XML file.
        /// </summary>
        /// <value>The name of the XML file.</value>
        [Required]
        public string XmlFileName
        {
            get { return _xmlFileName; }
            set { _xmlFileName = value; }
        }

        private string _xpath;

        /// <summary>
        /// Gets or sets the XPath.
        /// </summary>
        /// <value>The XPath.</value>
        [Required]
        public string XPath
        {
            get { return _xpath; }
            set { _xpath = value; }
        }

        private string _value;

        /// <summary>
        /// Gets or sets the value to write.
        /// </summary>
        /// <value>The value.</value>
        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }

        private bool _delete = false;

        /// <summary>
        /// Gets or sets a value indicating whether the matched node is deleted.
        /// </summary>
        /// <value><c>true</c> to delete matched node; otherwise, <c>false</c>.</value>
        public bool Delete
        {
            get { return _delete; }
            set { _delete = value; }
        }

        private string _namespace;

        /// <summary>
        /// Gets or sets the default namespace.
        /// </summary>
        /// <value>The namespace.</value>
        public string Namespace
        {
            get { return _namespace; }
            set { _namespace = value; }
        }

        private string _prefix;

        /// <summary>
        /// Gets or sets the prefix to associate with the namespace being added.
        /// </summary>
        /// <value>The namespace prefix.</value>
        public string Prefix
        {
            get { return _prefix; }
            set { _prefix = value; }
        }

        #endregion

        /// <summary>
        /// When overridden in a derived class, executes the task.
        /// </summary>
        /// <returns>
        /// true if the task successfully executed; otherwise, false.
        /// </returns>
        public override bool Execute()
        {
            try
            {
                Log.LogMessage(Properties.Resources.XmlUpdateDocument, _xmlFileName);

                XmlDocument document = new XmlDocument();
                document.Load(_xmlFileName);

                XPathNavigator navigator = document.CreateNavigator();
                XmlNamespaceManager manager = new XmlNamespaceManager(navigator.NameTable);

                if (!string.IsNullOrEmpty(_prefix) && !string.IsNullOrEmpty(_namespace))
                {
                    manager.AddNamespace(_prefix, _namespace);
                }

                XmlNodeList nodes = document.SelectNodes(_xpath, manager);
                Log.LogMessage(Properties.Resources.XmlUpdateNodes, nodes.Count);

                var xml = new List<string>();
                for (int i = 0; i < nodes.Count; i++)
                {
                    xml.Add(nodes[i].OuterXml);
                }

                for (int i = 0; i < nodes.Count; i++)
                {
                    if (_delete)
                        nodes[i].ParentNode.RemoveChild(nodes[i]);
                    else
                        nodes[i].InnerText = _value ?? "";
                }

                using (XmlTextWriter writer = new XmlTextWriter(_xmlFileName, Encoding.UTF8))
                {
                    writer.Formatting = Formatting.Indented;
                    document.Save(writer);
                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex);
                return false;
            }

            Log.LogMessage(Properties.Resources.XmlUpdateResult, _value);
            return true;
        }
    }
}
