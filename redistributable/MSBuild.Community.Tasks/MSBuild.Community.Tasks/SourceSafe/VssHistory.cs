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
using System.Globalization;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.Xml;
using Microsoft.VisualStudio.SourceSafe.Interop;



namespace MSBuild.Community.Tasks.SourceSafe
{
    #region Documentation
    /// <summary>
    /// Generates an XML file containing details of all changes made
    /// to a Visual SourceSafe project or file between specified labels or dates.
    /// </summary>
    /// <example>
    /// <para>Generates a file containing details of all the changes made to the <c>$/Test</c>
    /// project by a user called joe.bloggs</para>
    /// <code><![CDATA[<VssHistory UserName="uid"
    ///  Password="pwd"
    ///  DatabasePath="\\VSSServer\VSS\srcsafe.ini"
    ///  Path="$/Test"
    ///  User="joe.bloggs" 
    ///  OutputFile="History.xml"
    ///  Recursive="True"
    /// />
    /// ]]></code>
    /// </example>
    /// <example>
    /// <para>Generates a file containing details of all the changes made between the
    /// labels Build1 and Build2 in the <c>$/Test</c> project.</para>
    /// <code><![CDATA[<VssHistory UserName="uid"
    ///  Password="pwd"
    ///  DatabasePath="\\VSSServer\VSS\srcsafe.ini"
    ///  Path="$/Test"
    ///  FromLabel="Build1"
    ///  ToLabel="Build2"
    ///  OutputFile="History.xml"
    ///  Recursive="True"
    /// />
    /// ]]></code>
    /// </example>
    /// <example>
    /// <para>Generates a file containing details of all the changes made between the
    /// 1st December 2005 and 10th December 2005in the <c>$/Test</c> project.</para>
    /// <code><![CDATA[<VssHistory UserName="uid"
    ///  Password="pwd"
    ///  DatabasePath="\\VSSServer\VSS\srcsafe.ini"
    ///  Path="$/Test"
    ///  FromDate="2005-12-01 00:00"
    ///  ToDate="2005-12-10 00:00"
    ///  OutputFile="History.xml"
    ///  Recursive="True"
    /// />
    /// ]]></code>
    /// </example>
    #endregion
    public class VssHistory : VssRecursiveBase
    {
        private string _toLabel;
        private string _fromLabel;
        private DateTime _toDate;
        private DateTime _fromDate;
        private string _outputFile;
        private string _user;
        private XmlDocument _outputDoc;

        /// <summary>
        /// The label to start comparing to.
        /// </summary>
        public string ToLabel
        {
            get { return _toLabel; }
            set { _toLabel = value; }
        }

        /// <summary>
        /// The label to compare up to.
        /// </summary>
        public string FromLabel
        {
            get { return _fromLabel; }
            set { _fromLabel = value; }
        }

        /// <summary>
        /// The Start Date for the history.
        /// </summary>
        public DateTime ToDate
        {
            get { return _toDate; }
            set { _toDate = value; }
        }

        /// <summary>
        /// The End Date for the history.
        /// </summary>
        public DateTime FromDate
        {
            get { return _fromDate; }
            set { _fromDate = value; }
        }

        /// <summary>
        /// The name and path of the XML output file.
        /// </summary>
        [Required]
        public string OutputFile
        {
            get { return _outputFile; }
            set { _outputFile = value; }
        }

        /// <summary>
        /// The name of the user whose changes should be listed in 
        /// the history.
        /// </summary>
        public string User
        {
            get { return _user; }
            set { _user = value; }
        }

        private int VersionFlags
        {
            get
            {
                return Recursive ? (int)VSSFlags.VSSFLAG_RECURSYES : (int)VSSFlags.VSSFLAG_RECURSNO;
            }
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
                Log.LogMessage(MessageImportance.Normal, "Examining {0}...", this.Path);

                //Setup the XmlOutput File
                _outputDoc = new XmlDocument();
                XmlElement root = _outputDoc.CreateElement("VssHistory");
                XmlAttribute attrib = _outputDoc.CreateAttribute("FromLabel");
                attrib.Value = _fromLabel;
                root.Attributes.Append(attrib);

                attrib = _outputDoc.CreateAttribute("ToLabel");
                attrib.Value = _toLabel;
                root.Attributes.Append(attrib);

                attrib = _outputDoc.CreateAttribute("FromDate");
                if (FromDate != DateTime.MinValue)
                {
                    attrib.Value = XmlConvert.ToString(FromDate, XmlDateTimeSerializationMode.Utc);
                }
                root.Attributes.Append(attrib);

                attrib = _outputDoc.CreateAttribute("ToDate");
                if (ToDate != DateTime.MinValue)
                {
                    attrib.Value = XmlConvert.ToString(ToDate, XmlDateTimeSerializationMode.Utc);
                }
                root.Attributes.Append(attrib);

                attrib = _outputDoc.CreateAttribute("Path");
                attrib.Value = this.Path;
                root.Attributes.Append(attrib);

                attrib = _outputDoc.CreateAttribute("Recursive");
                attrib.Value = XmlConvert.ToString(this.Recursive);
                root.Attributes.Append(attrib);

                attrib = _outputDoc.CreateAttribute("User");
                attrib.Value = this.User;
                root.Attributes.Append(attrib);

                attrib = _outputDoc.CreateAttribute("Generated");
                attrib.Value = XmlConvert.ToString(DateTime.Now, XmlDateTimeSerializationMode.Utc);
                root.Attributes.Append(attrib);

                _outputDoc.AppendChild(root);

                ItemDiff(Item);

                _outputDoc.Save(OutputFile);

                Log.LogMessage(MessageImportance.Normal, "Generated diff file: {0}", OutputFile);

                return true;
            }
            catch (Exception e)
            {
                LogErrorFromException(e);
                return false;
            }
        }

        private void ItemDiff(IVSSItem ssItem)
        {
            Log.LogMessage(MessageImportance.Normal, "History of {0}...", ssItem.Name);

            if (FromLabel != null || ToLabel != null)
            {
                DiffByLabel(ssItem);
            }
            else
            {
                DiffByDate(ssItem);
            }
        }

        private void DiffByDate(IVSSItem ssItem)
        {
            bool startLogging = false;
            bool stopLogging = false;

            string user = User != null ? User.ToLower(CultureInfo.InvariantCulture) : null;

            foreach (IVSSVersion version in ssItem.get_Versions(VersionFlags))
            {
                // VSS returns the versions in descending order, meaning the
                // most recent versions appear first.
                if (ToDate == DateTime.MinValue || version.Date <= ToDate)
                {
                    startLogging = true;
                }
                if (FromDate != DateTime.MinValue && FromDate > version.Date)
                {
                    stopLogging = true;
                }
                if (startLogging && !stopLogging)
                {
                    // if user was specified, then skip changes that were not 
                    // performed by that user
                    if (user != null && version.Username.ToLower(CultureInfo.InvariantCulture) != user)
                    {
                        continue;
                    }

                    LogChange(version);
                }
            }
        }

        private void DiffByLabel(IVSSItem ssItem)
        {
            bool startLogging = false;
            bool stopLogging = false;

            string user = User != null ? User.ToLower(CultureInfo.InvariantCulture) : null;

            foreach (IVSSVersion version in ssItem.get_Versions(VersionFlags))
            {
                // VSS returns the versions in descending order, meaning the
                // most recent versions appear first.
                if (ToLabel == null || version.Action.StartsWith(string.Format("Labeled '{0}'", ToLabel)))
                {
                    startLogging = true;
                }
                if (FromLabel != null && version.Action.StartsWith(string.Format("Labeled '{0}'", FromLabel)))
                {
                    stopLogging = true;
                }
                if (startLogging && !stopLogging)
                {
                    // if user was specified, then skip changes that were not 
                    // performed by that user
                    if (user != null && version.Username.ToLower(CultureInfo.InvariantCulture) != user)
                    {
                        continue;
                    }

                    LogChange(version);
                }
            }
        }

        private void LogChange(IVSSVersion version)
        {
            const int FILE_OR_PROJECT_DOES_NOT_EXIST = -2147166577;

            XmlElement node;
            XmlAttribute attrib;

            try
            {
                node = _outputDoc.CreateElement("Entry");
                attrib = _outputDoc.CreateAttribute("Name");
                attrib.Value = version.VSSItem.Name;
                node.Attributes.Append(attrib);

                attrib = _outputDoc.CreateAttribute("Path");
                attrib.Value = version.VSSItem.Spec;
                node.Attributes.Append(attrib);
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                if (ex.ErrorCode != FILE_OR_PROJECT_DOES_NOT_EXIST)
                {
                    throw;
                }

                return;
            }

            attrib = _outputDoc.CreateAttribute("Action");
            attrib.Value = version.Action;
            node.Attributes.Append(attrib);

            attrib = _outputDoc.CreateAttribute("Date");
            attrib.Value = XmlConvert.ToString(version.Date, XmlDateTimeSerializationMode.Utc);
            node.Attributes.Append(attrib);

            attrib = _outputDoc.CreateAttribute("Version");
            attrib.Value = XmlConvert.ToString(version.VersionNumber);
            node.Attributes.Append(attrib);

            attrib = _outputDoc.CreateAttribute("User");
            attrib.Value = version.Username;
            node.Attributes.Append(attrib);

            attrib = _outputDoc.CreateAttribute("Comment");
            attrib.Value = version.Comment;
            node.Attributes.Append(attrib);

            attrib = _outputDoc.CreateAttribute("Label");
            attrib.Value = version.Label;
            node.Attributes.Append(attrib);

            attrib = _outputDoc.CreateAttribute("LabelComment");
            attrib.Value = version.LabelComment;
            node.Attributes.Append(attrib);

            _outputDoc.ChildNodes.Item(0).AppendChild(node);
        }
    }
}
