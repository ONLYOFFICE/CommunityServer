#region Copyright © 2008 Paul Welter. All rights reserved.
/*
Copyright © 2008 Paul Welter. All rights reserved.

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
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Win32;
using System.Xml;
using System.Reflection;
using MSBuild.Community.Tasks.HtmlHelp;



namespace MSBuild.Community.Tasks.Sandcastle
{
    /// <summary>
    /// The Sandcastle task.
    /// </summary>
    /// <example>Create the Html Help for MSBuild Community Task project.
    /// <code><![CDATA[
    /// <Sandcastle TopicStyle="vs2005"
    ///     WorkingDirectory="$(MSBuildProjectDirectory)\Help"
    ///     Assemblies="@(Assemblies)"
    ///     Comments="@(Comments)"
    ///     References="@(References)"
    ///     ChmName="MSBuildTasks"
    ///     HxName="MSBuildTasks" />
    /// ]]></code>
    /// </example>
    public class Sandcastle : Task
    {

        private static readonly string[] outputDirectories = new string[] 
        {
            "html", "icons", "media", "scripts", "styles"
        };

        // must be sorted
        private static readonly string[] topicStyles = new string[] 
        {
            "hana", "prototype", "vs2005"
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="Sandcastle"/> class.
        /// </summary>
        public Sandcastle()
        {
            SandcastleEnviroment = new SandcastleEnviroment();
            WorkingDirectory = Path.GetFullPath(@".\Help");
            TopicStyle = "vs2005";
            LanguageId = "1033";
            NoInfoMessages = true;
        }

        #region Internal Working Properties
        internal string AssemblyDirectory
        {
            get { return Path.Combine(WorkingDirectory, "assembly"); }
        }
        internal string DependencyDirectory
        {
            get { return Path.Combine(AssemblyDirectory, "dependency"); }
        }
        internal string CommentsDirectory
        {
            get { return Path.Combine(WorkingDirectory, "comments"); }
        }
        internal string TopicStyleDirectory
        {
            get { return Path.Combine(WorkingDirectory, TopicStyle); }
        }
        internal string OutputDirectory
        {
            get { return Path.Combine(TopicStyleDirectory, "output"); }
        }
        internal string ChmDirectory
        {
            get { return Path.Combine(TopicStyleDirectory, "chm"); }
        }

        internal string ReflectionFile
        {
            get { return Path.Combine(TopicStyleDirectory, "reflection.xml"); }
        }
        internal string ManifestFile
        {
            get { return Path.Combine(TopicStyleDirectory, "manifest.xml"); }
        }
        internal string TocFile
        {
            get { return Path.Combine(TopicStyleDirectory, "toc.xml"); }
        }

        internal ITaskItem[] WorkingReferences { get; set; }

        internal ITaskItem[] WorkingAssemblies { get; set; }

        internal ITaskItem[] WorkingComments { get; set; }

        internal SandcastleEnviroment SandcastleEnviroment { get; set; }
        #endregion

        #region Task Properties
        /// <summary>
        /// Gets or sets the sandcastle install root directory.
        /// </summary>
        /// <value>The sandcastle root directory.</value>
        public string SandcastleRoot
        {
            get
            {
                return SandcastleEnviroment.SandcastleRoot;
            }
            set
            {
                if (SandcastleEnviroment == null
                    || !string.Equals(SandcastleEnviroment.SandcastleRoot,
                        value, StringComparison.OrdinalIgnoreCase))
                {
                    SandcastleEnviroment = new SandcastleEnviroment(value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the working directory.
        /// </summary>
        /// <value>The working directory.</value>
        public string WorkingDirectory { get; set; }

        private string _topicStyle;

        /// <summary>
        /// Gets or sets the html help topic style.
        /// </summary>
        /// <value>The html help topic style.</value>
        /// <remarks>
        /// The styles supported are hana, prototype and vs2005. 
        /// The default style is vs2005.
        /// </remarks>
        public string TopicStyle
        {
            get { return _topicStyle; }
            set
            {
                _topicStyle = value.ToLowerInvariant();
                if (Array.BinarySearch<string>(topicStyles, _topicStyle) < 0)
                    throw new ArgumentException("The topic style is not supported. Valid styles are hana, prototype and vs2005.");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether working directory is cleaned.
        /// </summary>
        /// <value><c>true</c> if clean; otherwise, <c>false</c>.</value>
        public bool Clean { get; set; }

        /// <summary>
        /// Gets or sets the references.
        /// </summary>
        /// <value>The references.</value>
        public ITaskItem[] References { get; set; }

        /// <summary>
        /// Gets or sets the assemblies.
        /// </summary>
        /// <value>The assemblies.</value>
        [Required]
        public ITaskItem[] Assemblies { get; set; }

        /// <summary>
        /// Gets or sets the comments.
        /// </summary>
        /// <value>The comments.</value>
        [Required]
        public ITaskItem[] Comments { get; set; }

        /// <summary>
        /// Gets or sets the build assembler config file.
        /// </summary>
        /// <value>The build assembler config.</value>
        public ITaskItem SandcastleConfig { get; set; }

        /// <summary>
        /// Gets or sets the name of the CHM.
        /// </summary>
        /// <value>The name of the CHM.</value>
        public string ChmName { get; set; }

        /// <summary>
        /// Gets or sets the language id.
        /// </summary>
        /// <value>The language id.</value>
        public string LanguageId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating a Html Help 2x project will be created.
        /// </summary>
        /// <value>The name of the Html Help 2x project.</value>
        public string HxName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether no info messages will be output.
        /// </summary>
        /// <value><c>true</c> if no info messages; otherwise, <c>false</c>.</value>
        public bool NoInfoMessages { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether no warning messages will be output.
        /// </summary>
        /// <value><c>true</c> if no warning messages; otherwise, <c>false</c>.</value>
        public bool NoWarnMessages { get; set; }

        #endregion        /// <summary>

        /// <summary>
        /// When overridden in a derived class, executes the task.
        /// </summary>
        /// <returns>
        /// true if the task successfully executed; otherwise, false.
        /// </returns>
        public override bool Execute()
        {
            CreateWorkDirectories();

            CopySourceFiles();

            if (!Reflection())
                return false;

            CopyOutputFiles();

            if (!Manifest())
                return false;

            if (!OutputHtml())
                return false;

            if (!Toc())
                return false;

            if (!string.IsNullOrEmpty(ChmName))
                CreateChm();

            if (!string.IsNullOrEmpty(HxName))
                CreateHx();

            return true;
        }

        //step 1
        private void CreateWorkDirectories()
        {
            Log.LogMessage("Creating work directories at '{0}'.", WorkingDirectory);
            if (Clean)
            {
                if (Directory.Exists(AssemblyDirectory))
                    Directory.Delete(AssemblyDirectory, true);
                if (Directory.Exists(CommentsDirectory))
                    Directory.Delete(CommentsDirectory, true);
                if (Directory.Exists(TopicStyleDirectory))
                    Directory.Delete(TopicStyleDirectory, true);
            }

            if (!Directory.Exists(WorkingDirectory))
                Directory.CreateDirectory(WorkingDirectory);
            if (!Directory.Exists(AssemblyDirectory))
                Directory.CreateDirectory(AssemblyDirectory);
            if (!Directory.Exists(CommentsDirectory))
                Directory.CreateDirectory(CommentsDirectory);
            if (!Directory.Exists(TopicStyleDirectory))
                Directory.CreateDirectory(TopicStyleDirectory);
            if (!Directory.Exists(DependencyDirectory))
                Directory.CreateDirectory(DependencyDirectory);
            if (!Directory.Exists(OutputDirectory))
                Directory.CreateDirectory(OutputDirectory);

            foreach (string d in outputDirectories)
            {
                string path = Path.Combine(OutputDirectory, d);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }
        }

        //step 2
        private void CopySourceFiles()
        {
            WorkingAssemblies = CopyItems(Assemblies, AssemblyDirectory);
            WorkingReferences = CopyItems(References, DependencyDirectory);
            WorkingComments = CopyItems(Comments, CommentsDirectory);
        }

        //step 3
        private bool Reflection()
        {
            Log.LogMessage("Creating reflection data.");

            ITaskItem reflectionItem = new TaskItem(
                Path.Combine(TopicStyleDirectory, "reflection.org"));

            MRefBuilder mref = new MRefBuilder();
            CopyBuildEngine(mref);

            mref.Assemblies = WorkingAssemblies;
            mref.References = WorkingReferences;
            mref.OutputFile = reflectionItem;

            if (!mref.Execute())
                return false;

            XslTransform xslt = new XslTransform();
            CopyBuildEngine(xslt);

            xslt.XmlFile = reflectionItem;
            xslt.OutputFile = new TaskItem(ReflectionFile);

            if (TopicStyle == "prototype")
            {
                xslt.XsltFiles = new ITaskItem[] {
                    new TaskItem(Path.Combine(
                        SandcastleEnviroment.TransformsDirectory, 
                        "ApplyPrototypeDocModel.xsl")),
                    new TaskItem(Path.Combine(
                        SandcastleEnviroment.TransformsDirectory, 
                        "AddGuidFilenames.xsl"))
                };

            }
            else
            {
                xslt.XsltFiles = new ITaskItem[] {
                    new TaskItem(Path.Combine(
                        SandcastleEnviroment.TransformsDirectory, 
                        "ApplyVSDocModel.xsl")),
                    new TaskItem(Path.Combine(
                        SandcastleEnviroment.TransformsDirectory, 
                        "AddFriendlyFilenames.xsl"))
                };
                if (TopicStyle == "hana")
                    xslt.Arguments = new string[] {
                        "IncludeAllMembersTopic=false",
                        "IncludeInheritedOverloadTopics=true" };
                else
                    xslt.Arguments = new string[] {
                        "IncludeAllMembersTopic=true",
                        "IncludeInheritedOverloadTopics=true" };
            }
            return xslt.Execute();
        }

        //step 4
        private void CopyOutputFiles()
        {
            Log.LogMessage("Coping additional files for topic style '{0}'.", TopicStyle);

            string root = Path.Combine(
                SandcastleEnviroment.PresentationDirectory, TopicStyle);

            foreach (string o in outputDirectories)
            {
                string s = Path.Combine(root, o);
                if (!Directory.Exists(s))
                    continue;

                string d = Path.Combine(OutputDirectory, o);

                CopyDirectoryItems(s, d);
            }
        }

        //step 5
        private bool Manifest()
        {
            Log.LogMessage("Creating manifest from reflection data.");

            XslTransform xslt = new XslTransform();
            CopyBuildEngine(xslt);

            xslt.XmlFile = new TaskItem(ReflectionFile);

            xslt.OutputFile = new TaskItem(ManifestFile);

            xslt.XsltFiles = new ITaskItem[] {
                new TaskItem(Path.Combine(
                    SandcastleEnviroment.TransformsDirectory, 
                    "ReflectionToManifest.xsl"))
            };

            return xslt.Execute();
        }

        //step 6
        private bool OutputHtml()
        {
            Log.LogMessage("Generating html files for topic style '{0}'.", TopicStyle);

            string config = null;

            if (SandcastleConfig != null && File.Exists(SandcastleConfig.ItemSpec))
                config = File.ReadAllText(SandcastleConfig.ItemSpec);
            else
                config = GetBuildAssemblerConfig();

            //config = config.Replace("%DXROOT%", SandcastleEnviroment.SandcastleRoot);
            //config = config.Replace("%CommentsDir%", CommentsDirectory);
            //config = config.Replace("%OutputDir%", Path.Combine(OutputDirectory, "html"));
            //config = config.Replace("%reflectionfile%", ReflectionFile);

            string configFile = Path.Combine(TopicStyleDirectory, "Sandcastle.config");
            File.WriteAllText(configFile, config);

            BuildAssembler build = new BuildAssembler();
            CopyBuildEngine(build);

            build.EnviromentVariables["CommentsDir"] = CommentsDirectory;
            build.EnviromentVariables["OutputDir"] = Path.Combine(OutputDirectory, "html");
            build.EnviromentVariables["reflectionfile"] = ReflectionFile;

            build.ConfigFile = new TaskItem(configFile);
            build.ManifestFile = new TaskItem(ManifestFile);
            return build.Execute();
        }

        //step 7
        private bool Toc()
        {
            Log.LogMessage("Creating help table of contents.");

            XslTransform xslt = new XslTransform();
            CopyBuildEngine(xslt);

            xslt.XmlFile = new TaskItem(ReflectionFile);
            xslt.OutputFile = new TaskItem(TocFile);

            if (TopicStyle == "prototype")
            {
                xslt.XsltFiles = new ITaskItem[] {
                    new TaskItem(Path.Combine(
                        SandcastleEnviroment.TransformsDirectory, 
                        "createPrototypetoc.xsl"))
                };
            }
            else
            {
                xslt.XsltFiles = new ITaskItem[] {
                    new TaskItem(Path.Combine(
                        SandcastleEnviroment.TransformsDirectory, 
                        "createvstoc.xsl"))
                };
            }


            return xslt.Execute();
        }

        //step 8
        private bool CreateChm()
        {
            Log.LogMessage("Creating html help chm file.");

            //make directories
            if (!Directory.Exists(ChmDirectory))
                Directory.CreateDirectory(OutputDirectory);

            foreach (string d in outputDirectories)
            {
                string path = Path.Combine(ChmDirectory, d);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }

            // copy outputs
            string root = OutputDirectory;

            foreach (string o in outputDirectories)
            {
                if (o == "html")
                    continue; //skip html

                string s = Path.Combine(root, o);
                if (!Directory.Exists(s))
                    continue;

                string d = Path.Combine(ChmDirectory, o);
            
                CopyDirectoryItems(s, d);
            }

            Log.LogMessage("Compiling html help project '{0}'.", ChmName);
            // run chm builder
            ChmBuilder chm = new ChmBuilder();
            CopyBuildEngine(chm);

            chm.HtmlDirectory = Path.Combine(OutputDirectory, "html");
            chm.TocFile = new TaskItem(TocFile);
            chm.OutputDirectory = ChmDirectory;
            chm.ProjectName = ChmName;
            chm.LanguageId = LanguageId ?? "1033";

            if (!chm.Execute())
                return false;

            DBCSFix fix = new DBCSFix();
            CopyBuildEngine(fix);

            fix.LanguageId = LanguageId ?? "1033";
            fix.ChmDirectory = ChmDirectory;
            if (!fix.Execute())
                return false;

            //compile
            ChmCompiler compiler = new ChmCompiler();
            CopyBuildEngine(compiler);

            compiler.ProjectFile = new TaskItem(
                Path.Combine(ChmDirectory, ChmName + ".hhp"));

            return compiler.Execute();
        }

        //step 9
        private bool CreateHx()
        {
            Log.LogMessage("Creating html help 2.0 files.");

            string hxsTemplate = Path.Combine(
                SandcastleEnviroment.PresentationDirectory,
                @"shared\HxsTemplate");

            DirectoryInfo source = new DirectoryInfo(hxsTemplate);
            FileInfo[] files = source.GetFiles();
            foreach (FileInfo file in files)
            {
                file.CopyTo(Path.Combine(
                    OutputDirectory,
                    file.Name.Replace("template", HxName)));
            }

            string projectFile = Path.Combine(OutputDirectory, HxName + ".HxC");

            XslTransform xslt = new XslTransform();
            CopyBuildEngine(xslt);

            xslt.XmlFile = new TaskItem(TocFile);
            xslt.OutputFile = new TaskItem(projectFile);
            xslt.XsltFiles = new ITaskItem[] {
                new TaskItem(Path.Combine(
                    SandcastleEnviroment.TransformsDirectory, 
                    "CreateHxc.xsl"))
            };
            xslt.Arguments = new string[] { "fileNamePrefix=" + HxName };

            if (!xslt.Execute())
                return false;

            xslt = new XslTransform();
            CopyBuildEngine(xslt);

            xslt.XmlFile = new TaskItem(TocFile);
            xslt.OutputFile = new TaskItem(
                Path.Combine(OutputDirectory, HxName + ".HxT"));
            xslt.XsltFiles = new ITaskItem[] {
                new TaskItem(Path.Combine(
                    SandcastleEnviroment.TransformsDirectory, 
                    "TocToHxSContents.xsl"))
            };

            if (!xslt.Execute())
                return false;

            Log.LogMessage("Compiling html help 2.0 project '{0}'.",
                Path.GetFileName(projectFile));

            HxCompiler hx = new HxCompiler();
            CopyBuildEngine(hx);

            hx.ProjectFile = new TaskItem(projectFile);
            return hx.Execute();
        }

        private void CopyDirectoryItems(string sourceDirectory, string targetDirectory)
        {
            DirectoryInfo source = new DirectoryInfo(sourceDirectory);
            FileInfo[] files = source.GetFiles();

            foreach (FileInfo file in files)
                file.CopyTo(Path.Combine(
                    targetDirectory, file.Name), true);
        }

        private ITaskItem[] CopyItems(ITaskItem[] items, string targetDirectory)
        {
            if (items == null || items.Length == 0)
                return new ITaskItem[0];

            ITaskItem[] copies = new ITaskItem[items.Length];

            for (int i = 0; i < items.Length; i++)
            {
                string s = items[i].ItemSpec;

                string d = Path.Combine(
                    targetDirectory,
                    Path.GetFileName(s));

                File.Copy(s, d, true);

                copies[i] = new TaskItem(d);
            }

            return copies;
        }

        private void CopyBuildEngine(SandcastleToolBase task)
        {
            task.BuildEngine = this.BuildEngine;
            task.HostObject = this.HostObject;
            task.SandcastleEnviroment = this.SandcastleEnviroment;
            task.NoInfoMessages = this.NoInfoMessages;
            task.NoWarnMessages = this.NoWarnMessages;
        }

        private void CopyBuildEngine(ITask task)
        {
            task.BuildEngine = this.BuildEngine;
            task.HostObject = this.HostObject;
        }

        private string GetBuildAssemblerConfig()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(Sandcastle));

            string configName = TopicStyle + ".config";

            using (StreamReader reader = new StreamReader(
                assembly.GetManifestResourceStream(typeof(Sandcastle), configName)))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
