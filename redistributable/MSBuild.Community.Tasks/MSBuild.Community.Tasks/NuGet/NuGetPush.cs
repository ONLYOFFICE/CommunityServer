#region Copyright © 2011 Paul Welter. All rights reserved.
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

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;



namespace MSBuild.Community.Tasks.NuGet
{
    /// <summary>
    /// Pushes a package to the server and optionally publishes it.
    /// </summary>
    public class NuGetPush : NuGetBase
    {
        /// <summary>
        /// The path to the package to push the package to the server.
        /// </summary>
        [Required]
        public ITaskItem File { get; set; }

        /// <summary>
        /// The API key to use for push to the server.
        /// </summary>
        public string APIKey { get; set; }

        /// <summary>
        /// Specifies the server URL.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Specifies if the package should be created and uploaded to the server but not published to the server. False by default.
        /// </summary>
        /// <value>
        ///   <c>true</c> if create only; otherwise, <c>false</c>.
        /// </value>
        public bool CreateOnly { get; set; }

        /// <summary>
        /// Returns a string value containing the command line arguments to pass directly to the executable file.
        /// </summary>
        /// <returns>
        /// A string value containing the command line arguments to pass directly to the executable file.
        /// </returns>
        protected override string GenerateCommandLineCommands()
        {
            var builder = new CommandLineBuilder();
            builder.AppendSwitch("push");
            builder.AppendFileNameIfNotNull(File);
            builder.AppendFileNameIfNotNull(APIKey);
            builder.AppendSwitchIfNotNull("-Source ", Source);
            if (CreateOnly)
                builder.AppendSwitch("-CreateOnly");

            return builder.ToString();
        }
    }
}