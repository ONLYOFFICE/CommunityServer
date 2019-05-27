using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MSBuild.Community.Tasks.DependencyGraph
{
    /// <summary>
    /// Base class for all references 
    /// </summary>
    public class BaseReference
    {
        /// <summary>
        /// A name for display in a graph
        /// </summary>
        public string DisplayName { get; protected set; }

        /// <summary>
        /// Creates new instance
        /// </summary>
        /// <param name="displayName">A name for display in a graph</param>
        protected BaseReference(string displayName)
        {
            DisplayName = displayName;
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }

    /// <summary>
    /// Represents an assembly reference inside a project file
    /// </summary>
    public class AssemblyReference : BaseReference
    {
        /// <summary>
        /// Name of the assembly of an assembly reference
        /// </summary>
        public string Include { get; private set; }

        /// <summary>
        /// HintPath, or relative path to file in an assembly reference
        /// </summary>
        public string HintPath { get; private set; }

        /// <summary>
        /// Creates new instance
        /// </summary>
        /// <param name="include">The name of the assembly reference</param>
        /// <param name="hintPath">The hint path, if aplicable</param>
        public AssemblyReference(string include, string hintPath)
            : base(MakeDisplayName(include))
        {
            Include = include;
            HintPath = hintPath;
        }

        private static string MakeDisplayName(string include)
        {
            var result = ProjectFileParser.GetAssemblyNameFromFullName(include);
            return string.IsNullOrEmpty(result) ? include : result;
        }
    }

    /// <summary>
    /// Represents a project reference inside a project file
    /// </summary>
    public class ProjectReference : BaseReference
    {
        private string _assemblyName;

        /// <summary> 
        /// Path to a project file of reference 
        /// </summary>
        public string Include { get; private set; }

        /// <summary>
        /// GUID of referenced project
        /// </summary>
        public string Project { get; private set; }

        /// <summary>
        /// Name of referenced project
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// ProjectGuid property of project file
        /// </summary>
        public string ProjectGuid { get; private set; }

        /// <summary>
        /// AssemblyNAme property of project file
        /// </summary>
        public string AssemblyName
        {
            get { return _assemblyName; }
            private set
            {
                DisplayName = value;
                _assemblyName = value;
            }
        }

        /// <summary>
        /// Creates new instance
        /// </summary>
        /// <param name="include">The name of the assembly reference</param>
        /// <param name="project">GUID of referenced project</param>
        /// <param name="name">Name of referenced project</param>
        public ProjectReference(string include, string project, string name)
            : base (name)
        {
            Include = include;
            Project = project;
            Name = name;
        }

        /// <summary>
        /// Updates Include folder and convertes it to absolute path
        /// </summary>
        /// <param name="baseFolder"></param>
        public void UpdateInclude(string baseFolder)
        {
            if (Path.IsPathRooted(Include))
                return;
            Include = Path.GetFullPath(Path.GetDirectoryName(baseFolder) + "\\" + Include);
        }

        /// <summary>
        /// updates parser dependent properties: <see cref="P:AssemblyName"/> and <see cref="P:ProjectGuid"/>
        /// </summary>
        /// <param name="parser"></param>
        public void Update(ProjectFileParser parser)
        {
            AssemblyName = parser.GetAssemblyName();
            ProjectGuid = parser.GetGuid();
        }
    }
}
