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
using System.Linq;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.Xml.XPath;
using System.Xml.Linq;



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

                XDocument xdoc = XDocument.Load(_xmlFileName);
                XmlNamespaceManager manager = new XmlNamespaceManager(new NameTable());

                if (!string.IsNullOrEmpty(_namespace))
                {
                    //by default, if _prefix is not specified, set it to "", this way,
                    //manager.AddNamespace will add the _namespace as the default namespace
                    if (_prefix == null)
                        _prefix = String.Empty;

                    manager.AddNamespace(_prefix, _namespace);
                }


                var items = xdoc.XPathEvaluate(_xpath, manager) as IEnumerable<object>;

                Log.LogMessage(Properties.Resources.XmlUpdateNodes, items.Count());

                foreach (var item in items.ToArray())
                {
                    var attr = item as XAttribute;
                    if (attr != null)
                    {
                        if (_delete)
                        {
                            attr.Remove();
                        }
                        else
                        {
                            attr.SetValue(_value);
                        }
                    }

                    var ele = item as XElement;
                    if (ele != null)
                    {
                        if (_delete)
                        {
                            ele.Remove();
                        }
                        else
                        {
                            ele.SetValue(_value);
                        }
                    }
                }

                xdoc.Save(_xmlFileName);
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
