#region Copyright © 2006 Andy Johns. All rights reserved.
/*
Copyright © 2006 Andy Johns. All rights reserved.

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

//$Id$

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;

namespace MSBuild.Community.Tasks
{
    /// <summary>
    /// Retrieves the list of Projects contained within a Visual Studio Solution (.sln) file 
    /// </summary>
    /// <example>
    /// Returns project name, GUID, and path information from test solution
    /// <code><![CDATA[
    ///   <Target Name="Test">
    ///       <GetSolutionProjects Solution="TestSolution.sln">
    ///           <Output ItemName="ProjectFiles" TaskParameter="Output"/>
    ///       </GetSolutionProjects>
    /// 
    ///     <Message Text="Project names:" />
    ///     <Message Text="%(ProjectFiles.ProjectName)" />
    ///     <Message Text="Relative project paths:" />
    ///     <Message Text="%(ProjectFiles.ProjectPath)" />
    ///     <Message Text="Project GUIDs:" />
    ///     <Message Text="%(ProjectFiles.ProjectGUID)" />
    ///     <Message Text="Full paths to project files:" />
    ///     <Message Text="%(ProjectFiles.FullPath)" />
    ///     <Message Text="Project Type GUIDs:" />
    ///     <Message Text="%(ProjectFiles.ProjectTypeGUID)" />
    ///   </Target>
    /// ]]></code>
    /// </example>
    public class GetSolutionProjects : Task
    {
        private const string SolutionFolderProjectType = "{2150E333-8FDC-42A3-9474-1A3956D46DE8}";
        private const string ExtractProjectsFromSolutionRegex = @"Project\(""(?<ProjectTypeGUID>.+?)""\)\s*=\s*""(?<ProjectName>.+?)""\s*,\s*""(?<ProjectFile>.+?)""\s*,\s*""(?<ProjectGUID>.+?)""";
        private string solutionFile = "";
        private ITaskItem[] output = null;

        /// <summary>
        /// A list of the project files found in <see cref="Solution" />
        /// </summary>
        /// <remarks>
        /// The name of the project can be retrieved by reading metadata tag <c>ProjectName</c>.
        /// <para>
        /// The path to the project as it is is stored in the solution file retrieved by reading metadata tag <c>ProjectPath</c>.
        /// </para>
        /// <para>
        /// The project's GUID can be retrieved by reading metadata tag <c>ProjectGUID</c>.
        /// </para>
        /// </remarks>
        [Output]
        public ITaskItem[] Output
        {
            get { return output; }
            set { output = value; }
        }

        /// <summary>
        /// Name of Solution to get Projects from
        /// </summary>
        [Required]
        public string Solution
        {
            get { return solutionFile; }
            set { solutionFile = value; }
        }

        /// <summary>
        /// Perform task
        /// </summary>
        /// <returns>true on success</returns>
        public override bool Execute()
        {
            if (!File.Exists(solutionFile))
            {
                Log.LogError(Properties.Resources.SolutionNotFound, solutionFile);
                return false;
            }
            string solutionFolder = Path.GetDirectoryName(solutionFile);

            string solutionText = File.ReadAllText(solutionFile);
            MatchCollection matches = Regex.Matches(solutionText, ExtractProjectsFromSolutionRegex);
            List<ITaskItem> taskItems = new List<ITaskItem>();

            for(int i=0; i<matches.Count; i++)
            {
                string projectPathRelativeToSolution = matches[i].Groups["ProjectFile"].Value;
                string projectPathOnDisk = Path.Combine(solutionFolder, projectPathRelativeToSolution);
                string projectFile = projectPathRelativeToSolution;
                string projectName = matches[i].Groups["ProjectName"].Value;
                string projectGUID = matches[i].Groups["ProjectGUID"].Value;
                string projectTypeGUID = matches[i].Groups["ProjectTypeGUID"].Value;

				// do not include Solution Folders in output
				if (projectTypeGUID.Equals(SolutionFolderProjectType, StringComparison.OrdinalIgnoreCase))
				{
				    continue;
				}

                ITaskItem project = new TaskItem(projectPathOnDisk);
                project.SetMetadata("ProjectPath", projectFile);
                project.SetMetadata("ProjectName", projectName);
                project.SetMetadata("ProjectGUID", projectGUID);
                project.SetMetadata("ProjectTypeGUID", projectGUID);
				taskItems.Add(project);
            }

            output = taskItems.ToArray();
            return true;
        }


    }
}
