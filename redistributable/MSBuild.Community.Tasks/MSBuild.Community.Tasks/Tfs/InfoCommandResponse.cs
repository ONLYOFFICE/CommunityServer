using System;
using System.Collections.Generic;

namespace MSBuild.Community.Tasks.Tfs
{
    /// <summary>
    /// Represents the response from a tf.exe info command
    /// </summary>
    public class InfoCommandResponse
    {
        /// <summary>
        /// Creates a new instance of <see cref="InfoCommandResponse"/>
        /// </summary>
        /// <param name="output">The raw output from tf.exe info [itemspces]</param>
        public InfoCommandResponse(string output)
        {
            this.LocalInformation = new Dictionary<string, IItemInformation>();
            this.ServerInformation = new Dictionary<string, IItemInformation>();
            this.Response = output;
            this.Parse();
        }

        /// <summary>
        /// Parses the response from a 'tf.exe info' command
        /// </summary>
        /// <example>
        /// Local information:
        ///  Local path : c:\dev\file.cs
        ///  Server path: $/Main/file.cs
        ///  Changeset  : 5
        ///  Change     : none
        ///  Type       : file
        /// Server information:
        ///  Server path  : $/Main/file.cs
        ///  Changeset    : 5
        ///  Deletion ID  : 0
        ///  Lock         : none
        ///  Lock owner   :
        ///  Last modified: 20 January 2014 11:22:27
        ///  Type         : file
        ///  File type    : utf-8
        ///  Size         : 578           
        /// </example>
        private void Parse()
        {
            if (string.IsNullOrEmpty(this.Response))
                return;

            var lines = this.Response.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (line.Contains("Local information:"))
                {
                    var localInformation = new LocalInformation
                    {
                        LocalPath = GetValue(lines[i + 1]),
                        ServerPath = GetValue(lines[i + 2]),
                        Changeset = GetValue(lines[i + 3]),
                        Change = GetValue(lines[i + 4]),
                        Type = GetValue(lines[i + 5]),
                    };

                    this.LocalInformation[localInformation.ServerPath.ToLower()] = localInformation;
                    i = i + 5;
                }
                else if (line.Contains("Server information:"))
                {
                    var serverInformation = new ServerInformation
                    {
                        ServerPath = GetValue(lines[i + 1]),
                        Changeset = GetValue(lines[i + 2]),
                        DeletionID  = GetValue(lines[i + 3]),
                        Lock = GetValue(lines[i + 4]),
                        LockOwner = GetValue(lines[i + 5]),
                        LastModified = GetValue(lines[i + 6]),
                        Type = GetValue(lines[i + 7]),
                        FileType  = GetValue(lines[i + 8]),
                        Size  = GetValue(lines[i + 9]),
                    };

                    this.ServerInformation[serverInformation.ServerPath.ToLower()] = serverInformation;
                    i = i + 9;
                }
            }
        }

        private string GetValue(string line)
        {
            var index = line.IndexOf(':');

            if (index == line.Length)
                return null;

            return line.Substring(index + 1).Trim();
        }

        /// <summary>
        /// Gets or sets the local information.
        /// </summary>
        /// <value>
        /// The local information.
        /// </value>
        public Dictionary<string, IItemInformation> LocalInformation { get; set; }

        /// <summary>
        /// Gets or sets the server information.
        /// </summary>
        /// <value>
        /// The server information.
        /// </value>
        public Dictionary<string, IItemInformation> ServerInformation { get; set; }

        /// <summary>
        /// Gets or sets the response, the raw output of the tf.exe info command.
        /// </summary>
        /// <value>
        /// The response.
        /// </value>
        public string Response { get; set; }
    }

    /// <summary>
    /// Represents the server information section created by a tf.exe info command
    /// </summary>
    public class ServerInformation : IItemInformation
    {
        /// <summary>
        /// Gets or sets the server path.
        /// </summary>
        /// <value>
        /// The server path.
        /// </value>
        public string ServerPath { get; set; }

        /// <summary>
        /// Gets or sets the changeset.
        /// </summary>
        /// <value>
        /// The changeset.
        /// </value>
        public string Changeset { get; set; }

        /// <summary>
        /// Gets or sets the deletion identifier.
        /// </summary>
        /// <value>
        /// The deletion identifier.
        /// </value>
        public string DeletionID { get; set; }

        /// <summary>
        /// Gets or sets the lock.
        /// </summary>
        /// <value>
        /// The lock.
        /// </value>
        public string Lock { get; set; }

        /// <summary>
        /// Gets or sets the lock owner.
        /// </summary>
        /// <value>
        /// The lock owner.
        /// </value>
        public string LockOwner { get; set; }

        /// <summary>
        /// Gets or sets the last modified.
        /// </summary>
        /// <value>
        /// The last modified.
        /// </value>
        public string LastModified { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the type of the file.
        /// </summary>
        /// <value>
        /// The type of the file.
        /// </value>
        public string FileType { get; set; }

        /// <summary>
        /// Gets or sets the size.
        /// </summary>
        /// <value>
        /// The size.
        /// </value>
        public string Size { get; set; }
    }

    public interface IItemInformation
    {
        /// <summary>
        /// Gets or sets the server path.
        /// </summary>
        /// <value>
        /// The server path.
        /// </value>
        string ServerPath { get; set; }

        /// <summary>
        /// Gets or sets the changeset.
        /// </summary>
        /// <value>
        /// The changeset.
        /// </value>
        string Changeset { get; set; }
    }

    /// <summary>
    /// Represents the local information section from a tf.exe info command
    /// </summary>
    public class LocalInformation : IItemInformation
    {
        /// <summary>
        /// Gets or sets the local path.
        /// </summary>
        /// <value>
        /// The local path.
        /// </value>
        public string LocalPath { get; set; }

        /// <summary>
        /// Gets or sets the server path.
        /// </summary>
        /// <value>
        /// The server path.
        /// </value>
        public string ServerPath { get; set; }

        /// <summary>
        /// Gets or sets the changeset.
        /// </summary>
        /// <value>
        /// The changeset.
        /// </value>
        public string Changeset { get; set; }

        /// <summary>
        /// Gets or sets the change.
        /// </summary>
        /// <value>
        /// The change.
        /// </value>
        public string Change { get; set; }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public string Type { get; set; }
    }
}