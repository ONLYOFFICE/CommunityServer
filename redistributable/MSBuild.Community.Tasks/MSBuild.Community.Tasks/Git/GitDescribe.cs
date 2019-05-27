#region Copyright © 2013 Malachi Burke. All rights reserved.
/*
Copyright © 2013 Malachi Burke. All rights reserved.

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
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace MSBuild.Community.Tasks.Git
{
    /// <summary>
    /// A task for git to get the most current tag, commit count since tag, and commit hash.
    /// </summary>
    public class GitDescribe : GitClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GitDescribe"/> class.
        /// </summary>
        public GitDescribe()
        {
            Command = "describe";
        }

        /// <summary>
        /// Gets the number of commits in this branch since the last tag
        /// </summary>
        [Output]
        public int CommitCount{ get; set; }

        /// <summary>
        /// Gets or sets the commit hash.
        /// </summary>
        [Output]
        public string CommitHash { get; set; }

        /// <summary>
        /// Gets the last tagname for this branch
        /// </summary>
        [Output]
        public string Tag { get; set; }

        /// <summary>
        /// Not active yet
        /// </summary>
        [Output]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// When true, any processing errors will push error status out into <see cref="Tag"/>
        /// </summary>
        public bool SoftErrorMode { get; set; }

        /// <summary>
        /// When true, will use unannotated tags
        /// </summary>
        public bool LightWeight { get; set; }

        /// <summary>
        /// Matches the specified pattern
        /// </summary>
        public string Match { get; set; }

        /// <summary>
        /// When true, Git describe will always return at least hash
        /// </summary>
        public bool Always { get; set; }

        /// <summary>
        /// Make sure we specify abbrev=40 to get full CommitHash
        /// </summary>
        /// <param name="builder"></param>
        protected override void GenerateArguments(CommandLineBuilder builder)
        {
            builder.AppendSwitch("--long --abbrev=40");

            if (Always)
                builder.AppendSwitch("--always");

            if (LightWeight)
                builder.AppendSwitch("--tags");
            if (!String.IsNullOrEmpty(Match))
                builder.AppendSwitch("--match \"" + Match + "\"");
            base.GenerateArguments(builder);
        }

        /// <summary>
        /// Parses a single line of text to identify any errors or warnings in canonical format.
        /// </summary>
        /// <param name="singleLine">A single line of text for the method to parse.</param>
        /// <param name="messageImportance">A value of <see cref="T:Microsoft.Build.Framework.MessageImportance"/> that indicates the importance level with which to log the message.</param>
        protected override void LogEventsFromTextOutput(string singleLine, MessageImportance messageImportance)
        {
            bool isError = messageImportance == StandardErrorLoggingImportance;

            if (isError)
                base.LogEventsFromTextOutput(singleLine, messageImportance);
            else
            {
                var line = singleLine.Trim();
                // hashPosition includes the git-describe 'g' delimiter
                var hashPosition = singleLine.Length - 40 - 1;

                // TODO: get rid of these "soft" errors once unit tests are in place
                try
                {
                    if (hashPosition == -1) { 

                        // In case there are no tags, git describe output will contain only hash
                        // (without 'g' delimiter)
                        CommitHash = line.Substring(0);
                    }
                    else
                    {
                        // We do +1 to skip the git-describe 'g' delimiter
                        CommitHash = line.Substring(hashPosition + 1);
                    }
                }
                catch
                {
                    if (!SoftErrorMode) throw;

                    CommitCount = -1;
                    Tag = "Failure Parsing Git Describe: " + line;
                    return;
                }

                if (hashPosition == -1)
                {
                    // hashPosition is -1, meaning there is no dash in git describe output
                    // Set Tag to blank and CommitCount to 0 and then return
                    Tag = "";
                    CommitCount = 0;

                    return;
                }

                int i;
                // move backwards through git describe, now we've got our hash we move backwards 
                // skipping one dash
                // until we encounter another dash
                for (i = hashPosition - 2; i > 0; i--)
                {
                    var c = line[i];

                    if (c == '-')
                        break;
                }

                // sanity check, just incase parsing goes badly wrong (maybe git will change.. ?)
                if (i == 0)
                {
                    Tag = "Failure Parsing Git Describe: " + line;

                    if (!SoftErrorMode) throw new FormatException(Tag);

                    CommitCount = -1;
                    return;
                }
                else
                {
                    var commitCount = line.Substring(i + 1, (hashPosition - 2) - i);
                    try
                    {
                        CommitCount = int.Parse(commitCount);
                        Tag = line.Substring(0, i);
                    }
                    catch 
                    {
                        if (!SoftErrorMode) throw;

                        Tag = "Failure Parsing Git Describe: commitCount = '" + commitCount + "' / line = " + line;
                        CommitCount = -1;
                    }
                }
            }
        }
    }
}
