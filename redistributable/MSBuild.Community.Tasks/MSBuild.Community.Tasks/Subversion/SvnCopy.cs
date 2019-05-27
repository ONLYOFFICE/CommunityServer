#region Copyright © 2007 Geoff Lane. All rights reserved.
/*
Copyright © 2007 Geoff Lane <geoff@zorched.net>. All rights reserved.

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



namespace MSBuild.Community.Tasks.Subversion
{
    /// <summary>
    /// Copy a file or folder in Subversion
    /// </summary>
    /// <remarks>
    /// This is most useful for automatically tagging your source code during a build. 
    /// You can create a tag by copying a path from one server location to another.
    /// </remarks>
    /// <example>Create a tag of the trunk with the current Cruise Control build number:
    /// <code><![CDATA[
    /// <Target Name="TagTheBuild">
    ///   <SvnCopy SourcePath="file:///d:/svn/repo/Test/trunk"
    ///            DestinationPath="file:///d:/svn/repo/Test/tags/BUILD-$(CCNetLabel)" 
    ///            Message="Automatic build of $(CCNetProject)" />      
    /// </Target>
    /// ]]></code>
    /// </example>
    public class SvnCopy : SvnClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:MSBuild.Community.Tasks.Subversion.SvnCopy"/> class.
        /// </summary>
        public SvnCopy()
        {
            Command = "copy";
            base.NonInteractive = true;
            base.NoAuthCache = true;
        }

        private string sourcePath;
        /// <summary>
        /// The path of the source file or folder that should be copied
        /// </summary>
        public string SourcePath
        {
            get { return sourcePath; }
            set { sourcePath = value; }
        }

        private string destinationPath;
        /// <summary>
        /// The path to which the SourcePath should be copied
        /// </summary>
        public string DestinationPath
        {
            get { return destinationPath; }
            set { destinationPath = value; }
        }

        private bool buildTree;
        /// <summary>
        /// Specifies whether to create any missing directories and subdirectories 
        /// in the specified <see cref="DestinationPath"/>
        /// </summary>
        public bool BuildTree
        {
            get { return buildTree; }
            set { buildTree = value; }
        }


        /// <summary>
        /// Generates the SVN command.
        /// </summary>
        /// <returns></returns>
        protected override string GenerateSvnCommand()
        {
            return String.Format("{0} \"{2}\" \"{3}\"{1}", base.GenerateSvnCommand(), BuildTree ? " --parents" : "", sourcePath, destinationPath);
        }

        /// <summary>
        /// Indicates whether all task paratmeters are valid.
        /// </summary>
        /// <returns>
        /// true if all task parameters are valid; otherwise, false.
        /// </returns>
        protected override bool ValidateParameters()
        {
            if (string.IsNullOrEmpty(sourcePath))
            {
                Log.LogError(Properties.Resources.ParameterRequired, "SvnCopy", "SourcePath");
                return false;
            }

            if (String.IsNullOrEmpty(destinationPath))
            {
                Log.LogError(Properties.Resources.ParameterRequired, "SvnCopy", "DestinationPath");
                return false;
            }

            if (!String.IsNullOrEmpty(RepositoryPath))
            {
                Log.LogError(Properties.Resources.ParameterNotUsed, "SvnCopy", "RepositoryPath");
                return false;
            }

            if (!String.IsNullOrEmpty(LocalPath))
            {
                Log.LogError(Properties.Resources.ParameterNotUsed, "SvnCopy", "LocalPath");
                return false;
            }

            return base.ValidateParameters();
        }
    }
}
