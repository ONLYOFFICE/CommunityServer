using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.XPath;
using Microsoft.Build.Framework;
using System.Xml;

// $Id: SvnCheckout.cs 102 2006-01-09 18:01:13Z iko $

namespace MSBuild.Community.Tasks.Subversion
{
    /// <summary>
    /// The kind of Subversion node. The names match the text output
    /// by "svn info".
    /// </summary>
    public enum NodeKind
    {
        /// <summary>
        /// Node is a file
        /// </summary>
        file,
        /// <summary>
        /// Node is a directory
        /// </summary>
        dir,
        /// <summary>
        /// Unknown node type
        /// </summary>
        unknown
    }

    /// <summary>
    /// The Subversion schedule type.
    /// </summary>
    public enum Schedule
    {
        /// <summary>
        /// Normal schedule
        /// </summary>
        normal,
        /// <summary>
        /// Unknown schedule.
        /// </summary>
        unknown
    }

    /// <summary>
    /// Run the "svn info" command and parse the output
    /// </summary>
    /// <example>
    /// This example will determine the Subversion repository root for
    /// a working directory and print it out.
    /// <code><![CDATA[
    /// <Target Name="printinfo">
    ///   <SvnInfo LocalPath="c:\code\myapp">
    ///     <Output TaskParameter="RepositoryRoot" PropertyName="root" />
    ///   </SvnInfo>
    ///   <Message Text="root: $(root)" />
    /// </Target>
    /// ]]></code>
    /// </example>
    /// <remarks>You can retrieve Subversion information for a <see cref="SvnClient.LocalPath"/> or <see cref="SvnClient.RepositoryPath"/>.
    /// If you do not provide a value for <see cref="SvnClient.LocalPath"/> or <see cref="SvnClient.RepositoryPath"/>, the current directory is assumed.</remarks>
    public class SvnInfo : SvnClient
    {
        #region Properties
        /// <summary>
        /// Return the repository root or null if not set by Subversion.
        /// </summary>
        [Output]
        public string RepositoryRoot { get; private set; }

        /// <summary>
        /// Return the repository UUID value from Subversion.
        /// </summary>
        [Output]
        public string RepositoryUuid { get; private set; }

        /// <summary>
        /// The Subversion node kind.
        /// </summary>
        /// <enum cref="MSBuild.Community.Tasks.Subversion.NodeKind"/>
        [Output]
        public string NodeKind { get; private set; }

        /// <summary>
        /// The author who last changed this node.
        /// </summary>
        [Output]
        public string LastChangedAuthor { get; private set; }

        /// <summary>
        /// The last changed revision number.
        /// </summary>
        [Output]
        public int LastChangedRevision { get; private set; }

        /// <summary>
        /// The date this node was last changed.
        /// </summary>
        [Output]
        public DateTime LastChangedDate { get; private set; }

        /// <summary>
        /// The Subversion schedule type.
        /// </summary>
        /// <enum cref="MSBuild.Community.Tasks.Subversion.Schedule"/>
        [Output]
        public string Schedule { get; private set; }
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="T:SvnInfo"/> class.
        /// </summary>
        public SvnInfo()
        {
            base.Command = "info";
            base.NonInteractive = true;
            base.NoAuthCache = true;
            base.Xml = true;
            ResetMemberVariables();
        }

        /// <summary>
        /// Reset all instance variables to their default (unset) state.
        /// </summary>
        private void ResetMemberVariables()
        {
            RepositoryPath = string.Empty;
            RepositoryRoot = string.Empty;
            RepositoryUuid = string.Empty;
            NodeKind = string.Empty;
            LastChangedAuthor = string.Empty;
            LastChangedRevision = 0;
            LastChangedDate = DateTime.Now;
            Schedule = string.Empty;
        }

        /// <summary>
        /// Execute the task.
        /// </summary>
        /// <returns>true if execution is successful, false if not.</returns>
        public override bool Execute()
        {
            ResetMemberVariables();
            var result = base.Execute();

            if (result)
                Parse();

            return result;            
        }

        private void Parse()
        {            
            using (var sr = new StringReader(StandardOutput))
            using (var reader = XmlReader.Create(sr))
            {
                // since no names are dulicated we can read as flat xml file
                while (reader.Read())
                {
                    if (reader.NodeType != XmlNodeType.Element)
                        continue;

                    string name = reader.Name;

                    switch (name)
                    {
                        case "entry":
                            int rev;
                            if (int.TryParse(reader.GetAttribute("revision"), out rev))
                                Revision = rev;

                            NodeKind = reader.GetAttribute("kind");
                            break;
                        case "url":
                            RepositoryPath = reader.ReadString();
                            break;
                        case "root":
                            RepositoryRoot = reader.ReadString();
                            break;
                        case "uuid":
                            RepositoryUuid = reader.ReadString();
                            break;
                        case "schedule":
                            Schedule = reader.ReadString();
                            break;
                        case "commit":
                            int lastRev;
                            if (int.TryParse(reader.GetAttribute("revision"), out lastRev))
                                LastChangedRevision = lastRev;

                            break;
                        case "author":
                            LastChangedAuthor = reader.ReadString();
                            break;
                        case "date":
                            DateTime lastDate;
                            if (DateTime.TryParse(reader.ReadString(), out lastDate))
                                LastChangedDate = lastDate;

                            break;
                    } // switch element name
                } // while read
            } // using reader
        }

    }
}
