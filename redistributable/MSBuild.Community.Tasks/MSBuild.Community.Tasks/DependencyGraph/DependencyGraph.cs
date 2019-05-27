using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace MSBuild.Community.Tasks.DependencyGraph
{
    /// <summary>
    /// Reads a set of project files (.csproj, .vbproj) in InputFiles and generate a GraphViz style syntax.
    /// You can paste the result of the graphs in places like http://graphviz-dev.appspot.com/ to see your chart or
    /// run the file using the GraphViz tool http://www.graphviz.org/
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    ///     <ItemGroup>
    ///         <Dependency Include="Project01.csproj" />
    ///     </ItemGroup>
    ///
    ///     <Target Name="Default">
    ///         <DependencyGraph InputFiles="@(Dependency)" IsIncludeProjectDependecies="true" ExcludeReferences="^System" />
    ///     </Target>
    /// 
    ///     Result:
    ///     digraph {
    ///         subgraph ProjectReferences {
    ///             node [shape=box];
    ///             "{4993C164-5F2A-4831-A5B1-E5E579C76B28}" [label="Project01"];
    ///             "{1B5D5300-8070-48DB-8A81-B39764231954}" [label="Project03"];
    ///             "{E7D8035C-3CEA-4D9C-87FD-0F5C0DB5F592}" [label="Project02"];
    ///             "{7DBCDEE7-D048-432E-BEEB-928E362E3063}" [label="Project03"];
    ///         }
    ///         "{4993C164-5F2A-4831-A5B1-E5E579C76B28}" -> "Microsoft.CSharp";
    ///         "{1B5D5300-8070-48DB-8A81-B39764231954}" -> "Microsoft.CSharp";
    ///         "{E7D8035C-3CEA-4D9C-87FD-0F5C0DB5F592}" -> "Microsoft.CSharp";
    ///         "{7DBCDEE7-D048-432E-BEEB-928E362E3063}" -> "Microsoft.CSharp";
    ///         "{4993C164-5F2A-4831-A5B1-E5E579C76B28}" -> "{1B5D5300-8070-48DB-8A81-B39764231954}";
    ///         "{4993C164-5F2A-4831-A5B1-E5E579C76B28}" -> "{E7D8035C-3CEA-4D9C-87FD-0F5C0DB5F592}";
    ///         "{E7D8035C-3CEA-4D9C-87FD-0F5C0DB5F592}" -> "{7DBCDEE7-D048-432E-BEEB-928E362E3063}";
    ///	}    
    /// ]]></code></example>
    /// <para>
    ///     Other attributes:
    /// <list type="table">
    /// <item>
    ///     <term>Exclude</term>
    ///     <description>filter input files</description>
    /// </item>
    ///  <item>
    ///     <term>ExcludeReferences</term>
    ///     <description>filter referenced assemblies</description>
    /// </item>
    /// <item>
    ///     <term>ExcludeProjectReferences</term>
    ///     <description>filter referenced projects</description>
    /// </item>
    /// </list>
    /// </para>
    public class DependencyGraph : Task
    {
        private struct ReferenceBundle
        {
            public readonly ProjectReference Reference;

            public readonly ProjectReference Parent;

            public ReferenceBundle(ProjectReference parent, ProjectReference reference)
            {
                Parent = parent;
                Reference = reference;
            }
        }

        private static readonly Regex[] EmptyRegexBuffer = new Regex[0];

        private readonly Dictionary<string, ProjectReference> _storage = new Dictionary<string, ProjectReference>();
        private readonly Dictionary<ProjectReference, List<BaseReference>> _dependencies = new Dictionary<ProjectReference, List<BaseReference>>();

        private Regex[] _excludeRegex;
        private Regex[] _excludeReferencesRegex;
        private Regex[] _excludeProjectReferencesRegex;

        /// <summary> Project files to parse </summary>
        [Required]
        public ITaskItem[] InputFiles { get; set; }

        /// <summary>FileName to output results</summary>        
        public string OutputFile { get; set; }

        /// <summary>A set of regular expression to filter the input files</summary>
        public ITaskItem[] Exclude { get; set; }

        /// <summary>A set of regular expression to filter the referenced assemblies</summary>
        public ITaskItem[] ExcludeReferences { get; set; }

        /// <summary>A set of regular expression to filter the referenced projects</summary>
        public ITaskItem[] ExcludeProjectReferences { get; set; }

        /// <summary>includes project dependencies to output</summary>
        public bool IsIncludeProjectDependecies { get; set; }

        private Regex[] ExcludeRegex
        {
            get { return MakeFilterRegex(ref _excludeRegex, Exclude); }
        }

        private Regex[] ExcludeReferencesRegex
        {
            get { return MakeFilterRegex(ref _excludeReferencesRegex, ExcludeReferences); }
        }

        private Regex[] ExcludeProjectReferencesRegex
        {
            get { return MakeFilterRegex(ref _excludeProjectReferencesRegex, ExcludeProjectReferences); }
        }

        public override bool Execute()
        {
            if (InputFiles == null)
            {
                Log.LogError("No input files!");
                return false;
            }

            try
            {
                var inputProjects = InputFiles
                    .Select(file => file.GetMetadata("FullPath"))
                    .Select(name => new ProjectReference(name, string.Empty, string.Empty))
                    .Select(reference => new ReferenceBundle(null, reference));
                var processStack = new Stack<ReferenceBundle>(inputProjects);

                while (processStack.Count > 0)
                {
                    var bundle = processStack.Pop();
                    Process(bundle.Parent, bundle.Reference, processStack);
                }

                Stream s = GenerateGraphVizOutput();
                LogToConsole(s);
                LogToFile(s);
            }
            catch (Exception e)
            {
                Log.LogError("Exception: {0} at {1}", e, e.StackTrace);
                return false;
            }
            
            return true;
        }

        private void Process(ProjectReference parent, ProjectReference projectReference, Stack<ReferenceBundle> processStack)
        {
            ProjectReference existProject;
            if (_storage.TryGetValue(projectReference.Include, out existProject))
            {
                AddParentDependency(parent, existProject);
                return;
            }
            
            _storage.Add(projectReference.Include, projectReference);

            var parser = CreateParser(projectReference.Include);
            projectReference.Update(parser);

            AddParentDependency(parent, projectReference);

            var applicableReferences = parser
                .GetAssemblyReferences()
                .Where(reference => IsApplicable(ExcludeReferencesRegex, reference.DisplayName));
            foreach (var reference in applicableReferences)
                AddDependency(projectReference, reference);

            foreach (var reference in parser.GetProjectReferences())
            {
                reference.UpdateInclude(projectReference.Include);
                processStack.Push(new ReferenceBundle(projectReference, reference));
            }
        }

        private void AddParentDependency(ProjectReference parent, ProjectReference projectReference)
        {
            string assemblyName = projectReference.DisplayName;
            var excludeFilters = parent == null ? ExcludeRegex : ExcludeProjectReferencesRegex;
            if (!IsApplicable(excludeFilters, assemblyName))
                return;

            MakeDependencyList(projectReference);
            if (parent != null)
                AddDependency(parent, projectReference);
        }

        private void AddDependency(ProjectReference projectReference, BaseReference baseReference)
        {
            var list = MakeDependencyList(projectReference);
            list.Add(baseReference);
        }

        private List<BaseReference> MakeDependencyList(ProjectReference projectReference)
        {
            List<BaseReference> list;
            if (!_dependencies.TryGetValue(projectReference, out list))
            {
                list = new List<BaseReference>();
                _dependencies.Add(projectReference, list);
            }
            return list;
        }

        private ProjectFileParser CreateParser(string fileName)
        {
            using (FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                return new ProjectFileParser(stream);
        }

        private Stream GenerateGraphVizOutput()
        {
            MemoryStream ms = new MemoryStream();
            StreamWriter sw = new StreamWriter(ms);

            sw.WriteLine("digraph {");

            sw.WriteLine("\tsubgraph ProjectReferences {");
            sw.WriteLine("\t\tnode [shape=box];");
            foreach (var project in _dependencies.Keys)
                sw.WriteLine("\t\t\"{0}\" [label=\"{1}\"];", project.ProjectGuid,  project.DisplayName);
            sw.WriteLine("\t}");

            foreach (var project in _dependencies)
                foreach (var dep in project.Value.OfType<AssemblyReference>())
                    sw.WriteLine("\t\"{0}\" -> \"{1}\";", project.Key.ProjectGuid, dep);

            foreach (var project in _dependencies)
                foreach (var dep in project.Value.OfType<ProjectReference>())
                        sw.WriteLine("\t\"{0}\" -> \"{1}\";", project.Key.ProjectGuid, dep.ProjectGuid);

            sw.WriteLine("}");
            sw.Flush();

            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        private void LogToFile(Stream s)
        {
            if (String.IsNullOrEmpty(OutputFile))
                return;

            s.Seek(0, SeekOrigin.Begin);
            FileStream fs = new FileStream(OutputFile, FileMode.Create, FileAccess.Write);
            using (fs)
                CopyStream(s, fs);
        }

        private void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[32768];
            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, read);
            }
        }

        private void LogToConsole(Stream s)
        {
            s.Seek(0, SeekOrigin.Begin);
            StreamReader rd = new StreamReader(s);
            Log.LogMessage(rd.ReadToEnd());
        }

        private static Regex[] MakeFilterRegex(ref Regex[] buffer, IEnumerable<ITaskItem> items)
        {
            return
                buffer
                ?? (buffer = items == null
                    ? EmptyRegexBuffer
                    : items.Select(filter => new Regex(filter.ItemSpec)).ToArray());
        }

        private static bool IsApplicable(Regex[] filters, string name)
        {
            return filters.Length == 0 || !filters.Any(filter => filter.IsMatch(name));
        }
    }
}
