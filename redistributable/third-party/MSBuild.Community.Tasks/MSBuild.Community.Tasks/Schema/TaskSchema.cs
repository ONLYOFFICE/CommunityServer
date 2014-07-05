//Copyright © 2006, Jonathan de Halleux
//http://blog.dotnetwiki.org/default,month,2005-07.aspx

using System;
using System.IO;
using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;


namespace MSBuild.Community.Tasks.Schema
{
    /// <summary>
    /// Different ways to specify the assembly in a UsingTask element.
    /// </summary>
    public enum TaskListAssemblyFormatType
    {
        /// <summary>
        /// Assembly file name (Default): &lt;UsingTask AssemblyFile=&quot;foo.dll&quot; /&gt;
        /// </summary>
        AssemblyFileName,
        /// <summary>
        /// Assembly location: &lt;UsingTask AssemblyName=&quot;foo&quot; /&gt;
        /// </summary>
        AssemblyFileFullPath,
        /// <summary>
        /// Assembly Name: &lt;UsingTask AssemblyFile=&quot;bin\debug\foo.dll&quot; /&gt;
        /// </summary>
        AssemblyName,
        /// <summary>
        /// Assembly fully qualified name: &lt;UsingTask AssemblyName=&quot;foo.dll,version ....&quot; /&gt;
        /// </summary>
        AssemblyFullName
    }

    /// <summary>
    /// A Task that generates a XSD schema of the tasks in an assembly.
    /// </summary>
    /// <example>
    /// <para>Creates schema for MSBuild Community Task project</para>
    /// <code><![CDATA[
    /// <TaskSchema Assemblies="Build\MSBuild.Community.Tasks.dll" 
    ///     OutputPath="Build" 
    ///     CreateTaskList="true" 
    ///     IgnoreMsBuildSchema="true"
    ///     Includes="Microsoft.Build.Commontypes.xsd"/>
    /// ]]></code>
    /// </example>
    public class TaskSchema : AppDomainIsolatedTask
    {
        private ITaskItem[] assemblies;
        private ITaskItem[] schemas;
        private ITaskItem[] taskLists;
        private string outputPath;
        private bool createTaskList = true;
        private bool ignoreDocumentation = false;
        private string taskListAssemblyFormat = TaskListAssemblyFormatType.AssemblyFileName.ToString();
        private bool ignoreMsBuildSchema = false;
        private ITaskItem[] includes;

        /// <summary>
        /// Gets or sets the list of <see cref="Assembly"/> path to analyse.
        /// </summary>
        [Required]
        public ITaskItem[] Assemblies
        {
            get { return this.assemblies; }
            set { this.assemblies = value; }
        }

        /// <summary>
        /// Gets or sets the output path for the generated files.
        /// </summary>
        public string OutputPath
        {
            get { return this.outputPath; }
            set { this.outputPath = value; }
        }

        /// <summary>
        /// Gets the list of path to the generated XSD schema.
        /// </summary>
        [Output]
        public ITaskItem[] Schemas
        {
            get { return this.schemas; }
        }

        /// <summary>
        /// Gets or sets a value indicating if the task list (using UsingTask)
        /// has to be genereted.
        /// </summary>
        public bool CreateTaskList
        {
            get { return this.createTaskList; }
            set { this.createTaskList = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating how the assembly is specified in the
        /// UsingTask element.
        /// </summary>
        /// <enum cref="TaskListAssemblyFormatType" />
        public string TaskListAssemblyFormat
        {
            get { return this.taskListAssemblyFormat; }
            set { this.taskListAssemblyFormat = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating wheter documentation should be ignored
        /// even if available (Default is false).
        /// </summary>
        public bool IgnoreDocumentation
        {
            get { return this.ignoreDocumentation; }
            set { this.ignoreDocumentation = value; }
        }

        /// <summary>
        /// Gets the path to the task list if it was generated.
        /// </summary>
        [Output]
        public ITaskItem[] TaskLists
        {
            get { return this.taskLists; }
        }

        /// <summary>
        /// Gets or sets a value indicating if the 
        /// MsBuild schema inclusing should be ignored
        /// </summary>
        public bool IgnoreMsBuildSchema
        {
            get { return this.ignoreMsBuildSchema; }
            set { this.ignoreMsBuildSchema = value; }
        }

        /// <summary>
        /// Gets or sets a list of included schemas
        /// </summary>
        public ITaskItem[] Includes
        {
            get { return this.includes; }
            set { this.includes = value; }
        }

        private string GetTaskListName(Assembly taskAssembly)
        {
            string schemaFileName =
                String.Format(taskAssembly.GetName().Name + ".Targets");
            if (string.IsNullOrEmpty(this.OutputPath))
            {
                schemaFileName =
                    Path.Combine(
                        Path.GetDirectoryName(taskAssembly.Location),
                        schemaFileName);
            }
            else
            {
                schemaFileName = Path.Combine(this.OutputPath, schemaFileName);
            }
            return schemaFileName;
        }

        private string GetSchemaFileName(Assembly taskAssembly)
        {
            string schemaFileName =
                String.Format(taskAssembly.GetName().Name + ".xsd");
            if (string.IsNullOrEmpty(this.OutputPath))
            {
                schemaFileName =
                    Path.Combine(
                        Path.GetDirectoryName(taskAssembly.Location),
                        schemaFileName);
            }
            else
                schemaFileName = Path.Combine(this.OutputPath, schemaFileName);
            return schemaFileName;
        }

        /// <summary>
        /// When overridden in a derived class, executes the task.
        /// </summary>
        /// <returns>
        /// true if the task successfully executed; otherwise, false.
        /// </returns>
        public override bool Execute()
        {
            if (!String.IsNullOrEmpty(this.OutputPath) && !Directory.Exists(this.OutputPath))
            {
                this.Log.LogMessage("Create output path {0}", this.OutputPath);
                Directory.CreateDirectory(this.OutputPath);
            }

            this.schemas = new ITaskItem[this.Assemblies.Length];
            if (this.CreateTaskList)
                this.taskLists = new ITaskItem[this.Assemblies.Length];

            for (int i = 0; i < this.Assemblies.Length; ++i)
            {
                ITaskItem taskAssemblyFile = this.Assemblies[i];
                if (!AnalyseAssembly(taskAssemblyFile, i))
                    return false;
            }

            return true;
        }

        private bool AnalyseAssembly(ITaskItem taskAssemblyFile, int i)
        {
            string assemblyFullName = taskAssemblyFile.GetMetadata("FullPath");
            this.Log.LogMessage("Analysing {0}", assemblyFullName);
            Assembly taskAssembly = ReflectionHelper.LoadAssembly(this.Log, taskAssemblyFile);
            if (taskAssembly == null)
                return false;

            TaskSchemaAnalyser analyzer = new TaskSchemaAnalyser(this, taskAssembly);
            analyzer.CreateSchema();
            this.schemas[i] = analyzer.WriteSchema(GetSchemaFileName(taskAssembly));

            if (this.CreateTaskList)
            {
                analyzer.CreateUsingDocument();
                this.taskLists[i] = analyzer.WriteUsingDocument(GetTaskListName(taskAssembly));
            }

            return true;
        }
    }
}