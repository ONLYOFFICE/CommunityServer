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
using System.IO;
using System.Xml;
using System.Xml.XPath;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;



namespace MSBuild.Community.Tasks
{
    /// <summary>
    /// Reads a value from a XML document using a XPath.
    /// </summary>
    /// <example>Read all targest from a build project.
    /// <code><![CDATA[
    /// <XmlRead Prefix="n"
    ///     Namespace="http://schemas.microsoft.com/developer/msbuild/2003" 
    ///     XPath="/n:Project/n:Target/@Name"
    ///     XmlFileName="Subversion.proj">
    ///     <Output TaskParameter="Value" PropertyName="BuildTargets" />
    /// </XmlRead>
    /// <Message Text="Build Targets: $(BuildTargets)"/>
    /// ]]></code>
    /// </example>
    /// <remarks>
    /// If the XPath returns multiple nodes, the Value will 
    /// be a semicolon delimited list of the nodes text.
    /// </remarks>
    public class XmlRead : Task
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:XmlRead"/> class.
        /// </summary>
        public XmlRead()
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
        /// Gets the value read from file.
        /// </summary>
        /// <value>The value.</value>
        /// <remarks>
        /// If the XPath returns multiple nodes, the values will be semicolon delimited.
        /// </remarks>
        [Output]
        public string Value
        {
            get { return _value; }
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
                Log.LogMessage(Properties.Resources.XmlReadDocument, _xmlFileName);
                XPathDocument document = new XPathDocument(_xmlFileName);
                XPathNavigator navigator = document.CreateNavigator();
                XmlNamespaceManager manager = new XmlNamespaceManager(navigator.NameTable);
                    
                if(!string.IsNullOrEmpty(_prefix) && !string.IsNullOrEmpty(_namespace))
                {
                    manager.AddNamespace(_prefix, _namespace);
                }

                XPathExpression expression = XPathExpression.Compile(_xpath, manager);
                switch (expression.ReturnType)
                {
                    case XPathResultType.Number:
                    case XPathResultType.Boolean:
                    case XPathResultType.String:
                        _value = navigator.Evaluate(expression).ToString();
                        break;
                    case XPathResultType.NodeSet:
                        XPathNodeIterator nodes = navigator.Select(expression);
                        
                        Log.LogMessage(Properties.Resources.XmlReadNodes, nodes.Count);

                        StringBuilder builder = new StringBuilder();
                        while (nodes.MoveNext())
                            builder.AppendFormat("{0};", nodes.Current.Value);

                        if (builder.Length > 0)
                            builder.Remove(builder.Length - 1, 1);

                        _value = builder.ToString();

                        break;
                }
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex);
                return false;                
            }

            Log.LogMessage(Properties.Resources.XmlReadResult, _value);

            return true;
        }

        
    }
}
