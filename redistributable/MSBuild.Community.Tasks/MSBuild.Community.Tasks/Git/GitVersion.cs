#region Copyright © 2012 Paul Welter. All rights reserved.
/*
Copyright © 2012 Paul Welter. All rights reserved.

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
    /// A task for git to get the current commit hash.
    /// </summary>
    public class GitVersion : GitClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GitVersion"/> class.
        /// </summary>
        public GitVersion()
        {
            Command = "rev-parse";
            Short = true;
            ShortLength = 0;
            Revision = "HEAD";                    
        }

        /// <summary>
        /// Gets or sets the revision to get the version from. Default is HEAD.
        /// </summary>
        public string Revision { get; set; }

        /// <summary>
        /// Gets or sets the commit hash.
        /// </summary>
        [Output]
        public string CommitHash { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to abbreviate to a shorter unique name.
        /// </summary>
        /// <value>
        ///   <c>true</c> if short; otherwise, <c>false</c>.
        /// </value>
        public bool Short { get; set; }

        /// <summary>
        /// Gets or sets a value indicating short hash length.
        /// </summary>
        /// <value>
        ///   Length for short hash.
        /// </value>
        public int ShortLength { get; set; }

        /// <summary>
        /// Generates the arguments.
        /// </summary>
        /// <param name="builder">The builder.</param>
        protected override void GenerateArguments(CommandLineBuilder builder)
        {
            builder.AppendSwitch("--verify");

            if (Short)
            {
                if (ShortLength > 0) builder.AppendSwitch("--short=" + ShortLength);
                else builder.AppendSwitch("--short");
            }

            base.GenerateArguments(builder);

            builder.AppendSwitch(Revision);
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
                CommitHash = singleLine.Trim();
        }
    }
}
