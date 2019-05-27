#region Copyright � 2005 Paul Welter. All rights reserved.
/*
Copyright � 2005 Paul Welter. All rights reserved.

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

namespace MSBuild.Community.Tasks
{
    /// <summary>
    /// Run NUnit 3.x on a group of assemblies.
    /// </summary>
    /// <example>Run NUnit tests.
    /// <code><![CDATA[
    /// <ItemGroup>
    ///     <TestAssembly Include="C:\Program Files\NUnit.org\*.tests.dll" />
    /// </ItemGroup>
    /// <Target Name="NUnit3">
    ///     <NUnit3 Assemblies="@(TestAssembly)" 
	///             Process="Multiple" 
    ///             Agents="1" 
	///		        TestTimeout="1000" 
	///             Framework="v4.0" 
	///		        Force32Bit="true" 
	///		        Workers="10" 
	///		        EnableShadowCopy="true" 
	///		        OutputXmlFile="MyTestOutput.xml"
	///		        WorkingDirectory="./"
	///		        ShowLabels="All"
	///		        InternalTrace="Verbose"
	///		        NoHeader="true"
	///		        NoColor="true"
	///		        Verbose="true"/>
    /// </Target>
    /// ]]></code>
    /// </example>
    public class NUnit3 : ToolTask
    {
        #region Constants

        /// <summary>
        /// The default relative path of the NUnit installation.
        /// The value is <c>@"NUnit.org\nunit-console"</c>.
        /// </summary>
        public const string DEFAULT_NUNIT_DIRECTORY = @"NUnit.org\nunit-console";
        private const string InstallDirKey = @"HKEY_CURRENT_USER\Software\NUnit.org";

        #endregion Constants

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="T:NUnit"/> class.
        /// </summary>
        public NUnit3()
        {
        }

        #endregion Constructor

        #region Properties
        private ITaskItem[] _assemblies;

        /// <summary>
        /// Gets or sets the assemblies.
        /// </summary>
        /// <value>The assemblies.</value>
        [Required]
        public ITaskItem[] Assemblies
        {
            get { return _assemblies; }
            set { _assemblies = value; }
        }

        private string _outputXmlFile;

        /// <summary>
        /// Gets or sets the output XML file.
        /// </summary>
        /// <value>The output XML file.</value>
        public string OutputXmlFile
        {
            get { return _outputXmlFile; }
            set { _outputXmlFile = value; }
        }

        private string _errorOutputFile;

        /// <summary>
        /// The file to receive test error details.
        /// </summary>
        public string ErrorOutputFile
        {
            get { return _errorOutputFile; }
            set { _errorOutputFile = value; }
        }


        private string _textOutputFile;

        /// <summary>
        /// The file to redirect standard output to.
        /// </summary>
        public string TextOutputFile
        {
            get { return _textOutputFile; }
            set { _textOutputFile = value; }
        }


        private string _workingDirectory;

        /// <summary>
        /// Gets or sets the working directory.
        /// </summary>
        /// <value>The working directory.</value>
        /// <returns>
        /// The directory in which to run the executable file, or a null reference (Nothing in Visual Basic) if the executable file should be run in the current directory.
        /// </returns>
        public string WorkingDirectory
        {
            get { return _workingDirectory; }
            set { _workingDirectory = value; }
        }

        private bool _enableShadowCopy;

        /// <summary>
        /// Determines whether assemblies are copied to a shadow folder during testing.
        /// </summary>
        /// <remarks>Shadow copying is disabled by default. If you want to test the assemblies "in the shadow folder",
        /// you must set this property to <c>True</c>.</remarks>
        public bool EnableShadowCopy
        {
            get { return _enableShadowCopy; }
            set { _enableShadowCopy = value; }
        }

        private string _projectConfiguration;

        /// <summary>
        /// The project configuration to run.
        /// </summary>
        /// <remarks>Only applies when a project file is used. The default is the first configuration, usually Debug.</remarks>
        public string ProjectConfiguration
        {
            get { return _projectConfiguration; }
            set { _projectConfiguration = value; }
        }

        // make this nullable so we have a thrid state, not set
        private bool? _testInNewThread = null;

        /// <summary>
        /// Allows tests to be run in a new thread, allowing you to take advantage of ApartmentState and ThreadPriority settings in the config file.
        /// </summary>
        public bool TestInNewThread
        {
            get { return _testInNewThread.HasValue ? _testInNewThread.Value : true; }
            set { _testInNewThread = value; }
        }

        private bool _force32Bit;

        /// <summary>
        /// Determines whether the tests are run in a 32bit process on a 64bit OS.
        /// </summary>
        public bool Force32Bit
        {
            get { return _force32Bit; }
            set { _force32Bit = value; }
        }

        private string _framework;

        /// <summary>
        /// Determines the framework to run aganist.
        /// </summary>
        public string Framework
        {
          get { return _framework; }
          set { _framework = value; }
        }

        private string _showLabels;

        /// <summary>
        /// Whether or not to show test labels in output.
        /// On - Labels are shown in the output.
        /// Off - Labels are not shown in the output.
        /// All - 
        /// </summary>
        public string ShowLabels
        {
            get { return _showLabels; }
            set { _showLabels = value; }
        }

        private string _process;

        /// <summary>
        /// The --process option controls how NUnit loads tests in processes. The following values are recognized.
        /// Single - All the tests are run in the nunit-console process. This is the default.
        /// Separate - A separate process is created to run the tests.
        /// Multiple - A separate process is created for each test assembly, whether specified on the command line or listed in an NUnit project file.
        /// Note: This option is not available using the .NET 1.1 build of nunit-console.
        /// </summary>
        public string Process
        {
            get { return _process; }
            set { _process = value; }
        }

        private string _agents;

        /// <summary>
        /// The --agents  option controls the number of agents that may be allowed to run simultaneously assuming you are not running inprocess. 
        /// If not specified, all agent processes run tests at the same time, whatever the number of assemblies. 
        /// This setting is used to control running your assemblies in parallel.
        /// </summary>
        public string Agents
        {
            get { return _agents; }
            set { _agents = value; }
        }

        private string _domain;

        /// <summary>
        /// The --domain option controls of the creation of AppDomains for running tests. The following values are recognized:
        /// None - No domain is created - the tests are run in the primary domain. This normally requires copying the NUnit assemblies into the same directory as your tests.
        /// Single - A test domain is created - this is how NUnit worked prior to version 2.4
        /// Multiple - A separate test domain is created for each assembly
        /// The default is to use multiple domains if multiple assemblies are listed on the command line. Otherwise a single domain is used.
        /// </summary>
        public string Domain
        {
            get { return _domain; }
            set { _domain = value; }
        }

        private string _apartment;

        /// <summary>
        /// The --apartment option may be used to specify the ApartmentState (STA or MTA) of the test runner thread. Since the default is MTA, the option is only needed to force execution in the Single Threaded Apartment.
        /// Note: If a given test must always run in a particular apartment, as is the case with many Gui tests, you should use an attribute on the test rather than specifying this option at the command line.
        /// </summary>
        public string Apartment
        {
            get { return _apartment; }
            set { _apartment = value; }
        }

        private string _where;

        /// <summary>
        /// The --where option may be used to specify an expression indicating which tests to run. 
        /// It may specify test names, classes, methods, catgories or properties comparing them to actual values with the operators ==, !=, =~ and !~. 
        /// See NUnit 3.0 Test Selection Language for a full description of the syntax.
        /// </summary>
        /// <remarks></remarks>
        public string Where
        {
            get { return _where; }
            set { _where = value; }
        }

        private string _timeout;

        /// <summary>
        /// The --timeout option may be used to specify the timeout for each test case in MILLISECONDS.
        /// </summary>
        /// <remarks></remarks>
        public string TestTimeout
        {
            get { return _timeout; }
            set { _timeout = value; }
        }

        private string _workers;

        /// <summary>
        /// The --workers option may be used to specify the number of worker threads to be used in running tests.
        /// </summary>
        /// <remarks></remarks>
        public string Workers
        {
            get { return _workers; }
            set { _workers = value; }
        }

        private string _trace;

        /// <summary>
        /// The --trace option may be used to specify the internal trace level. 
        /// Off
        /// Error
        /// Warning
        /// Info
        /// Verbose (Debug)
        /// </summary>
        /// <remarks></remarks>
        public string InternalTrace
        {
            get { return _trace; }
            set { _trace = value; }
        }

        private bool _noHeader;

        /// <summary>
        /// The --noheader suppress display of program information at start of run.
        /// </summary>
        /// <remarks></remarks>
        public bool NoHeader
        {
            get { return _noHeader; }
            set { _noHeader = value; }
        }

        private bool _noColor;

        /// <summary>
        /// The --nocolor displays console output without color.
        /// </summary>
        /// <remarks></remarks>
        public bool NoColor
        {
            get { return _noColor; }
            set { _noColor = value; }
        }

        private bool _verbose;

        /// <summary>
        /// The --verbose displays additional information as the test runs.
        /// </summary>
        /// <remarks></remarks>
        public bool Verbose
        {
            get { return _verbose; }
            set { _verbose = value; }
        }
        #endregion

        #region Task Overrides
        /// <summary>
        /// Returns a string value containing the command line arguments to pass directly to the executable file.
        /// </summary>
        /// <returns>
        /// A string value containing the command line arguments to pass directly to the executable file.
        /// </returns>
        protected override string GenerateCommandLineCommands()
        {
            CommandLineBuilder builder = new CommandLineBuilder();

            string c = Environment.OSVersion.Platform == PlatformID.Unix ? "-" : "--";

            if (EnableShadowCopy)
            {
                builder.AppendSwitch(c+"shadowcopy");
            }
            if (_testInNewThread.HasValue && !_testInNewThread.Value)
            {
                builder.AppendSwitch(c+"nothread");
            }
            if(Force32Bit)
            {
                builder.AppendSwitch(c+"x86");
            }
            if (NoHeader)
            {
                builder.AppendSwitch(c+"noheader");
            }
            if (NoColor)
            {
                builder.AppendSwitch(c+"nocolor");
            }
            if (Verbose)
            {
                builder.AppendSwitch(c+"verbose");
            }
            builder.AppendFileNamesIfNotNull(_assemblies, " ");

            builder.AppendSwitchIfNotNull(c+"config=", _projectConfiguration);

            builder.AppendSwitchIfNotNull(c+"err=", _errorOutputFile);

            builder.AppendSwitchIfNotNull(c+"out=", _textOutputFile);

            builder.AppendSwitchIfNotNull(c+"framework=",_framework);

            builder.AppendSwitchIfNotNull(c+"process=",_process);

            builder.AppendSwitchIfNotNull(c+"agents=", _agents);

            builder.AppendSwitchIfNotNull(c+"domain=",_domain);

            builder.AppendSwitchIfNotNull(c+"apartment=",_apartment);
            
            builder.AppendSwitchIfNotNull(c+"where=", _where);

            builder.AppendSwitchIfNotNull(c+"timeout=", _timeout);

            builder.AppendSwitchIfNotNull(c+"workers=", _workers);

            builder.AppendSwitchIfNotNull(c+"result=", _outputXmlFile);

            builder.AppendSwitchIfNotNull(c+"work=", _workingDirectory);

            builder.AppendSwitchIfNotNull(c+"labels=", _showLabels);

            builder.AppendSwitchIfNotNull(c+"trace=", _trace);
           
            return builder.ToString();
        }

        private void CheckToolPath()
        {
            string nunitPath = ToolPath == null ? String.Empty : ToolPath.Trim();
            if (!String.IsNullOrEmpty(nunitPath))
            {
                ToolPath = nunitPath;
                return;
            }

            nunitPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            nunitPath = Path.Combine(nunitPath, DEFAULT_NUNIT_DIRECTORY);

            if (Directory.Exists(nunitPath) == false)
            {
                nunitPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                nunitPath = Path.Combine(nunitPath, DEFAULT_NUNIT_DIRECTORY);    
            }

            try
            {
                string value = Registry.GetValue(InstallDirKey, "InstallDir", nunitPath) as string;
                if (!string.IsNullOrEmpty(value))
                    nunitPath = Path.Combine(value, "nunit-console");
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex);
            }
            ToolPath = nunitPath;
        }

        /// <summary>
        /// Returns the fully qualified path to the executable file.
        /// </summary>
        /// <returns>
        /// The fully qualified path to the executable file.
        /// </returns>
        protected override string GenerateFullPathToTool()
        {
            CheckToolPath();
            return Path.Combine(ToolPath, ToolName);
        }

        /// <summary>
        /// Logs the starting point of the run to all registered loggers.
        /// </summary>
        /// <param name="message">A descriptive message to provide loggers, usually the command line and switches.</param>
        protected override void LogToolCommand(string message)
        {
            Log.LogCommandLine(MessageImportance.Low, message);
        }

        /// <summary>
        /// Gets the name of the executable file to run.
        /// </summary>
        /// <value></value>
        /// <returns>The name of the executable file to run.</returns>
        protected override string ToolName
        {
            get
            {
                string toolName = @"nunit3-console";
                return ToolPathUtil.MakeToolName(toolName);
            }
        }

        /// <summary>
        /// Gets the <see cref="T:Microsoft.Build.Framework.MessageImportance"></see> with which to log errors.
        /// </summary>
        /// <value></value>
        /// <returns>The <see cref="T:Microsoft.Build.Framework.MessageImportance"></see> with which to log errors.</returns>
        protected override MessageImportance StandardOutputLoggingImportance
        {
            get
            {
                return MessageImportance.Normal;
            }
        }

        /// <summary>
        /// Returns the directory in which to run the executable file.
        /// </summary>
        /// <returns>
        /// The directory in which to run the executable file, or a null reference (Nothing in Visual Basic) if the executable file should be run in the current directory.
        /// </returns>
        protected override string GetWorkingDirectory()
        {
            return string.IsNullOrEmpty(_workingDirectory) ? base.GetWorkingDirectory() : _workingDirectory;
        }

        #endregion Task Overrides

    }
}
