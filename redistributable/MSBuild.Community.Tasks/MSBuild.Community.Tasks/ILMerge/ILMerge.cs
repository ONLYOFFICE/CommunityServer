//-----------------------------------------------------------------------
// <copyright file="ILMerge.cs" company="MSBuild Community Tasks Project">
//     Copyright © 2006 Ignaz Kohlbecker
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace MSBuild.Community.Tasks
{
    /// <summary>
    /// A wrapper for the ILMerge tool.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The ILMerge tool itself must be installed separately.
    /// It is available <a href="http://research.microsoft.com/~mbarnett/ILMerge.aspx">here</a>.
    /// </para>
    /// <para>
    /// The command line options "/wildcards" and "/lib" of ILMerge are not supported,
    /// because MSBuild is in charge of expanding wildcards for item groups.
    /// </para>
    /// </remarks>
    /// <example>
    /// This example merges two assemblies A.dll and B.dll into one:
    /// <code><![CDATA[
    /// <PropertyGroup>
    ///     <outputFile>$(testDir)\ilmergetest.dll</outputFile>
    ///     <keyFile>$(testDir)\keypair.snk</keyFile>
    ///     <excludeFile>$(testDir)\ExcludeTypes.txt</excludeFile>
    ///     <logFile>$(testDir)\ilmergetest.log</logFile>
    /// </PropertyGroup>
    /// <ItemGroup>
    ///     <inputAssemblies Include="$(testDir)\A.dll" />
    ///     <inputAssemblies Include="$(testDir)\B.dll" />
    ///     <allowDuplicates Include="ClassAB" />
    /// </ItemGroup>
    /// <Target Name="merge" >
    ///    <ILMerge InputAssemblies="@(inputAssemblies)" 
    ///        AllowDuplicateTypes="@(allowDuplicates)"
    ///        ExcludeFile="$(excludeFile)"
    ///        OutputFile="$(outputFile)" LogFile="$(logFile)"
    ///        DebugInfo="true" XmlDocumentation="true" 
    ///        KeyFile="$(keyFile)" DelaySign="true" />
    /// </Target>]]></code>
    /// </example>
    public class ILMerge : ToolTask
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ILMerge"/> class.
        /// </summary>
        public ILMerge()
        {
            DebugInfo = true;
        }

        #region Input Parameters

        /// <summary>
        /// Gets or sets the names of public types
        /// to be renamed when they are duplicates.
        /// </summary>
        /// <remarks>
        /// <para>Set to an empty item group to allow all public types to be renamed.</para>
        /// <para>Don't provide this parameter if no duplicates of public types are allowed.</para>
        /// <para>Corresponds to command line option "/allowDup".</para>
        /// <para>The default value is <c>null</c>.</para>
        /// </remarks>
        public ITaskItem[] AllowDuplicateTypes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to treat an assembly 
        /// with a zero PeKind flag 
        /// (this is the value of the field listed as .corflags in the Manifest)
        /// as if it was ILonly.
        /// </summary>
        /// <remarks>
        /// <para>Corresponds to command line option "/zeroPeKind".</para>
        /// <para>The default value is <c>false</c>.</para>
        /// </remarks>
        public bool AllowZeroPeKind { get; set; }

        /// <summary>
        /// Gets or sets the attribute assembly
        /// from whre to get all of the assembly-level attributes
        /// such as Culture, Version, etc.
        /// It will also be used to get the Win32 Resources from.
        /// </summary>
        /// <remarks>
        /// <para>This property is mutually exclusive with <see cref="CopyAttributes"/>.</para>
        /// <para>
        /// When not specified, then the Win32 Resources from the primary assembly 
        /// are copied over into the target assembly.
        /// </para>
        /// <para>Corresponds to command line option "/attr".</para>
        /// <para>The default value is <c>null</c>.</para>
        /// </remarks>
        public ITaskItem AttributeFile { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether 
        /// to augment the list of input assemblies
        /// to its "transitive closure".
        /// </summary>
        /// <remarks>
        /// <para>
        /// An assembly is considered part of the transitive closure if it is referenced,
        /// either directly or indirectly, 
        /// from one of the originally specified input assemblies 
        /// and it has an external reference to one of the input assemblies, 
        /// or one of the assemblies that has such a reference.
        /// </para>
        /// <para>Corresponds to command line option "/closed".</para>
        /// <para>The default value is <c>false</c>.</para>
        /// </remarks>
        public bool Closed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether 
        /// to copy the assembly level attributes
        /// of each input assembly over into the target assembly.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Any duplicate attribute overwrites a previously copied attribute.
        /// The input assemblies are processed in the order they are specified.
        /// </para>
        /// <para>This parameter is mutually exclusive with <see cref="AttributeFile"/>.</para>
        /// <para>Corresponds to command line option "/copyattrs".</para>
        /// <para>The default value is <c>false</c>.</para>
        /// </remarks>
        public bool CopyAttributes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to copy assembly attributes from all merged assemblies
        /// into the unified assembly even if duplicate assembly attributes would result.
        /// </summary>
        /// <remarks>
        /// <para>Applicable only when <see cref="CopyAttributes"/> is <c>true</c></para>
        /// <para>Corresponds to command line option "/allowMultiple".</para>
        /// </remarks>
        public bool AllowDuplicateAttributes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether 
        /// to preserve any .pdb files
        /// that are found for the input assemblies
        /// into a .pdb file for the target assembly.
        /// </summary>
        /// <remarks>
        /// <para>Corresponds to command line option "/ndebug".</para>
        /// <para>The default value is <c>true</c>.</para>
        /// </remarks>
        public bool DebugInfo { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether 
        /// the target assembly will be delay signed.
        /// </summary>
        /// <remarks>
        /// <para>This property can be set only in conjunction with <see cref="KeyFile"/>.</para>
        /// <para>Corresponds to command line option "/delaysign".</para>
        /// <para>The default value is <c>false</c>.</para>
        /// </remarks>
        public bool DelaySign { get; set; }

        /// <summary>
        /// Gets or sets the file
        /// that will be used to identify types
        /// that are not to have their visibility modified.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If an empty item group is provided, 
        /// then all types in any assembly other than the primary assembly are made non-public.
        /// </para>
        /// <para>Omit this parameter to prevent ILMerge from modifying the visibility of any types.</para>
        /// <para>
        /// The contents of the file should be one <see cref="System.Text.RegularExpressions.Regex"/> per line. 
        /// The regular expressions are matched against each type's full name, 
        /// e.g., <c>System.Collections.IList</c>. 
        /// If the match fails, it is tried again with the assembly name (surrounded by square brackets) 
        /// prepended to the type name. 
        /// Thus, the pattern <c>\[A\].*</c> excludes all types in assembly <c>A</c> from being made non-public. 
        /// The pattern <c>N.T</c> will match all types named <c>T</c> in the namespace named <c>N</c>
        /// no matter what assembly they are defined in.
        /// </para>
        /// <para>Corresponds to command line option "/internalize".</para>
        /// <para>The default value is <c>null</c>.</para>
        /// </remarks>
        public ITaskItem ExcludeFile { get; set; }

        /// <summary>
        /// Gets or sets the input assemblies to merge.
        /// </summary>
        [Required]
        public ITaskItem[] InputAssemblies { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether whether types in assemblies other than the primary assembly have their visibility modified.
        /// </summary>
        /// <value><c>true</c> if internalize; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// <para>This controls whether types in assemblies other than the primary assembly have 
        /// their visibility modified. When it is true, then all non-exempt types that are visible 
        /// outside of their assembly have their visibility modified so that they are not visible 
        /// from outside of the merged assembly. A type is exempt if its full name matches a line 
        /// from the ExcludeFile (Section 2.9) using the .NET regular expression engine.</para>
        /// <para>Corresponds to command line option "/internalize".</para>
        /// <para>The default value is <c>false</c>.</para>
        /// </remarks>
        public bool Internalize { get; set; }

        /// <summary>
        /// Gets or sets the .snk file
        /// to sign the target assembly.
        /// </summary>
        /// <remarks>
        /// <para>Can be used with <see cref="DelaySign"/>.</para>
        /// <para>Corresponds to command line option "/keyfile".</para>
        /// <para>The default value is <c>null</c>.</para>
        /// </remarks>
        public ITaskItem KeyFile { get; set; }

        /// <summary>
        /// Gets or sets a log file
        /// to write log messages to.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If an empty item group is provided,
        /// then log messages are writte to <see cref="Console.Out"/>.
        /// </para>
        /// <para>Corresponds to command line option "/log".</para>
        /// <para>The default value is <c>null</c>.</para>
        /// </remarks>
        public ITaskItem LogFile { get; set; }

        /// <summary>
        /// Gets or sets the target assembly.
        /// </summary>
        /// <remarks>
        /// <para>Corresponds to command line option "/out".</para>
        /// </remarks>
        [Required]
        public ITaskItem OutputFile { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether 
        /// external assembly references in the manifest
        /// of the target assembly will use public keys (<c>false</c>)
        /// or public key tokens (<c>true</c>).
        /// </summary>
        /// <remarks>
        /// <para>Corresponds to command line option "/publickeytokens".</para>
        /// <para>The default value is <c>false</c>.</para>
        /// </remarks>
        public bool PublicKeyTokens { get; set; }

        /// <summary>
        /// Gets or sets the directories to be used to search for input assemblies.
        /// </summary>
        /// <value>The search directories.</value>
        public ITaskItem[] SearchDirectories { get; set; }

        /// <summary>
        /// Gets or sets the .NET framework version for the target assembly.
        /// </summary>
        /// <remarks>
        /// <para>Valid values are "v1", "v1.1", "v2".</para>
        /// <para>Corresponds to the first part of command line option "/targetplatform".</para>
        /// <para>The default value is <c>null</c>.</para>
        /// </remarks>
        public string TargetPlatformVersion { get; set; }

        /// <summary>
        /// Gets or sets the directory in which <c>mscorlib.dll</c> is to be found.
        /// </summary>
        /// <remarks>
        /// <para>Can only be used in conjunction with <see cref="TargetPlatformVersion"/>.</para>
        /// <para>Corresponds to the second part of command line option "/targetplatform".</para>
        /// <para>The default value is <c>null</c>.</para>
        /// </remarks>
        public ITaskItem TargetPlatformDirectory { get; set; }

        /// <summary>
        /// Gets or sets the indicator
        /// whether the target assembly is created as a library (<c>Dll</c>),
        /// a console application (<c>Exe</c>) or as a Windows application (<c>WinExe</c>).
        /// </summary>
        /// <remarks>
        /// <para>Corresponds to command line option "/target".</para>
        /// <para>The default value is the same kind as that of the primary assembly.</para>
        /// </remarks>
        public string TargetKind { get; set; }

        /// <summary>
        /// Gets or sets the version number of the target assembly.
        /// </summary>
        /// <remarks>
        /// <para>The parameter should look like <c>6.2.1.3</c>.</para>
        /// <para>Corresponds to command line option "/ver".</para>
        /// <para>The default value is null.</para>
        /// </remarks>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether 
        /// to merge XML documentation files
        /// into one for the target assembly.
        /// </summary>
        /// <remarks>
        /// <para>Corresponds to command line option "/xmldocs".</para>
        /// <para>The default value is <c>false</c>.</para>
        /// </remarks>
        public bool XmlDocumentation { get; set; }

        #endregion Input Parameters

        /// <summary>
        /// Gets the name of the executable file to run.
        /// </summary>
        protected override string ToolName
        {
            get { return ToolPathUtil.MakeToolName("ILMerge"); }
        }

        /// <summary>
        /// Gets the standard installation path of ILMerge.exe.
        /// </summary>
        /// <remarks>
        /// If ILMerge is not installed at its standard installation path,
        /// provide its location to <see cref="ToolTask.ToolPath"/>.
        /// </remarks>
        /// <returns>Returns [ProgramFiles]\Microsoft\ILMerge.exe.</returns>
        protected override string GenerateFullPathToTool()
        {
            if (ToolPath != null) {
                return Path.Combine(ToolPath, ToolName);
            }
            string toolPath = ToolPathUtil.FindInProgramFiles(this.ToolName, @"Microsoft\ILMerge") ??
                              ToolPathUtil.FindInPath(this.ToolName);

            return Path.Combine(toolPath, ToolName);
        }

        /// <summary>
        /// Generates a string value containing the command line arguments
        /// to pass directly to the executable file.
        /// </summary>
        /// <returns>
        /// Returns a string value containing the command line arguments
        /// to pass directly to the executable file.
        /// </returns>
        protected override string GenerateCommandLineCommands()
        {
            var builder = new CommandLineBuilder();


            builder.AppendSwitchIfNotNull("/allowDup:", AllowDuplicateTypes, " /allowDup:");

            if (AllowZeroPeKind)
                builder.AppendSwitch("/zeroPeKind");

            builder.AppendSwitchIfNotNull("/attr:", AttributeFile);

            if (Closed)
                builder.AppendSwitch("/closed");

            if (CopyAttributes)
            {
                builder.AppendSwitch("/copyattrs");
                if (AllowDuplicateAttributes)
                {
                    builder.AppendSwitch("/allowMultiple");
                }
            }

            if (!DebugInfo)
                builder.AppendSwitch("/ndebug");

            if (DelaySign)
                builder.AppendSwitch("/delaysign");

            if (ExcludeFile != null)
                if (ExcludeFile.ItemSpec.Length == 0)
                    builder.AppendSwitch("/internalize");
                else
                    builder.AppendSwitchIfNotNull("/internalize:", ExcludeFile);
            else if (Internalize)
                builder.AppendSwitch("/internalize");

            builder.AppendSwitchIfNotNull("/keyfile:", KeyFile);

            if (LogFile != null)
                if (LogFile.ItemSpec.Length == 0)
                    builder.AppendSwitchIfNotNull("/log:", LogFile);
                else
                    builder.AppendSwitch("/log");

            builder.AppendSwitchIfNotNull("/out:", OutputFile);

            if (PublicKeyTokens)
                builder.AppendSwitch("/publickeytokens");

            if (TargetPlatformVersion != null)
                if (TargetPlatformDirectory == null)
                    builder.AppendSwitch("/targetplatform:" + TargetPlatformVersion);
                else
                    builder.AppendSwitchIfNotNull("/targetplatform:" + TargetPlatformVersion + ",", TargetPlatformDirectory);                                    

            builder.AppendSwitchIfNotNull("/target:", TargetKind);

            builder.AppendSwitchIfNotNull("/ver:", Version);

            if (XmlDocumentation)
                builder.AppendSwitch("/xmldocs");

            builder.AppendSwitchIfNotNull("/lib:", SearchDirectories, " /lib:");

            builder.AppendFileNamesIfNotNull(InputAssemblies, " ");

            return builder.ToString();
        }

        /// <summary>
        /// Gets the <see cref="T:Microsoft.Build.Framework.MessageImportance"></see> with which to log errors.
        /// </summary>
        /// <value></value>
        /// <returns>The <see cref="T:Microsoft.Build.Framework.MessageImportance"></see> with which to log errors.</returns>
        protected override MessageImportance StandardOutputLoggingImportance
        {
            get { return MessageImportance.Normal; }
        }

        /// <summary>
        /// Gets the <see cref="T:Microsoft.Build.Framework.MessageImportance"/> with which to log errors.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The <see cref="T:Microsoft.Build.Framework.MessageImportance"/> with which to log errors.
        /// </returns>
        protected override MessageImportance StandardErrorLoggingImportance
        {
            get { return MessageImportance.High; }
        }
    }
}