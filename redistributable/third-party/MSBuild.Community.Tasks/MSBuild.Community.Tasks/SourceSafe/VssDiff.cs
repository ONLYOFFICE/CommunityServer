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
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.VisualStudio.SourceSafe.Interop;

namespace MSBuild.Community.Tasks.SourceSafe
{
    /// <summary>
    /// Task that records differences between the latest version
    /// of all the items in a Visual SourceSafe project and another version or label
    /// to a file.
    /// </summary>
    /// <example>
    /// <para>Generates a file containing all of the differences between the 
    /// current version and the label &quot;Test Label&quot;</para>
    /// <code><![CDATA[<VssDiff UserName="uid"
    ///  Password="pwd"
    ///  DatabasePath="\\VSSServer\VSS2\srcsafe.ini"
    ///  Path="$/Test"
    ///  OutputFile="Diff.xml"
    ///  Label="Test Label"
    /// />]]></code>
    /// </example>
    public class VssDiff : VssBase
    {
        private string _label;
        private string _outputFile;
        private XmlDocument _outputDoc;

        /// <summary>
        /// The value of the label to compare to.
        /// </summary>
        [Required]
        public string Label
        {
            get { return _label; }
            set { _label = value; }
        }

        /// <summary>
        /// The name of the file to send the output to.
        /// </summary>
        [Required]
        public string OutputFile
        {
            get { return _outputFile; }
            set { _outputFile = value; }
        }

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns><see langword="true"/> if the task ran successfully; 
        /// otherwise <see langword="false"/>.</returns>
        public override bool Execute()
        {
            try
            {
                ConnectToDatabase();

                Log.LogMessage(MessageImportance.Normal, "Examining: " + this.Path);

                //Setup the XmlOutput File
                _outputDoc = new XmlDocument();
                XmlElement root = _outputDoc.CreateElement("vssdiff");
                XmlAttribute attrib = _outputDoc.CreateAttribute("label");
                attrib.Value = _label;
                root.Attributes.Append(attrib);
                attrib = _outputDoc.CreateAttribute("generated");
                attrib.Value = System.DateTime.Now.ToString();
                root.Attributes.Append(attrib);
                attrib = _outputDoc.CreateAttribute("project");
                attrib.Value = this.Path;
                root.Attributes.Append(attrib);

                _outputDoc.AppendChild(root);

                //Start the recursive search
                ProjectDiff(this.Path);
                _outputDoc.Save(_outputFile);

                Log.LogMessage(MessageImportance.Normal, "Diff file {0} generated", OutputFile);
                return true;
            }
            catch (Exception e)
            {
                LogErrorFromException(e);
                return false;
            }
        }

        private  void ItemDiff(IVSSItem ssItem)
        {
            Log.LogMessage(MessageImportance.Low, "Processing item " + ssItem.Name);

            bool addVersion = true;
            int labeledVersion = 0;
            foreach (IVSSVersion version in ssItem.get_Versions(0))
            {
                // VSS returns the versions in descending order, meaning the
                // most recent versions appear first.
                string action = version.Action;

                // We found our version so stop adding versions to our list
                if (action.StartsWith("Labeled '" + _label + "'"))
                {
                    labeledVersion = version.VersionNumber;
                    addVersion = false;
                    //This is a bit annoying, it would be more efficient to break
                    //out of the loop here but VSS throws an exception !%?!
                    //http://tinyurl.com/nmct
                    //break;
                }
                if (addVersion == true)
                {
                    // Only add versions that have been added,created or checked in.  Ignore label actions.
                    if ((action.StartsWith("Add")) || (action.StartsWith("Create")) || (action.StartsWith("Check")))
                    {
                        Log.LogMessage(MessageImportance.Low, "Adding: " + version.VSSItem.Name);

                        // Build our XML Element with hopefully useful information.
                        XmlElement node = _outputDoc.CreateElement("item");
                        XmlAttribute attrib = _outputDoc.CreateAttribute("name");
                        attrib.Value = version.VSSItem.Name;
                        node.Attributes.Append(attrib);

                        attrib = _outputDoc.CreateAttribute("path");
                        attrib.Value = version.VSSItem.Spec;
                        node.Attributes.Append(attrib);

                        attrib = _outputDoc.CreateAttribute("action");
                        attrib.Value = action;
                        node.Attributes.Append(attrib);

                        attrib = _outputDoc.CreateAttribute("date");
                        attrib.Value = version.Date.ToString();
                        node.Attributes.Append(attrib);

                        attrib = _outputDoc.CreateAttribute("version");
                        attrib.Value = version.VersionNumber.ToString();
                        node.Attributes.Append(attrib);

                        attrib = _outputDoc.CreateAttribute("user");
                        attrib.Value = version.Username;
                        node.Attributes.Append(attrib);

                        attrib = _outputDoc.CreateAttribute("comment");
                        attrib.Value = version.Comment;
                        node.Attributes.Append(attrib);

                        _outputDoc.ChildNodes.Item(0).AppendChild(node);
                    }
                }
            }
        }

        private void ProjectDiff(string project)
        {
            // Recursively loop through our vss projects
            Log.LogMessage(MessageImportance.Low, "Processing project " + project);
            IVSSItem ssProj = this.Database.get_VSSItem(project, false);
            IVSSItems ssSubItems = ssProj.get_Items(false);
            
            foreach (IVSSItem subItem in ssSubItems)
            {
                if (subItem.Type == 0)
                {
                    //Type=0 is a Project
                    ProjectDiff(project + "/" + subItem.Name);
                }
                else
                {
                    ItemDiff(subItem);
                }
            }
        }
    }
}
