#region Copyright © 2007 Paul Welter. All rights reserved.
/*
Copyright © 2007 Paul Welter. All rights reserved.

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
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.IO;



namespace MSBuild.Community.Tasks
{
    /// <summary>
    /// Task wrapping the Window Resource Kit Robocopy.exe command.
    /// </summary>
    /// <example>Deploy website to web server.
    /// <code><![CDATA[
    /// <RoboCopy 
    ///     SourceFolder="$(MSBuildProjectDirectory)" 
    ///     DestinationFolder="\\server\webroot\" 
    ///     Mirror="true"
    ///     ExcludeFolders=".svn;obj;Test"
    ///     ExcludeFiles="*.cs;*.resx;*.csproj;*.webinfo;*.log"
    ///     NoJobHeader="true"
    /// />  
    /// ]]></code>
    /// </example>
    public class RoboCopy : ToolTask
    {
        #region Properties
        private string _sourceFolder;
        /// <summary>
        /// Source directory 
        /// </summary>
        /// <remarks>
        /// You can use drive:\path or \\server\share\path
        /// </remarks>
        [Required]
        public string SourceFolder
        {
            get { return _sourceFolder; }
            set { _sourceFolder = value; }
        }
        
        private string[] _sourceFiles;
        /// <summary>
        /// Names of files to act upon.
        /// </summary>
        /// <remarks>
        /// You can use wildcard characters (? and *). If no 
        /// files are listed, Robocopy defaults to all files (*.*).
        /// </remarks>
        public string[] SourceFiles
        {
            get { return _sourceFiles; }
            set { _sourceFiles = value; }
        }
        
        private string _destinationFolder;
        /// <summary>
        /// Destination directory.
        /// </summary>
        /// <remarks>
        /// You can use drive:\path or \\server\share\path
        /// </remarks>
        [Required]
        public string DestinationFolder
        {
            get { return _destinationFolder; }
            set { _destinationFolder = value; }
        }
        
        private bool _subdirectories;
        /// <summary>
        /// /S	Copies subdirectories (excluding empty ones).
        /// </summary>
        public bool Subdirectories
        {
            get { return _subdirectories; }
            set { _subdirectories = value; }
        }
        private bool _allSubdirectories;

        /// <summary>
        /// /E	Copies all subdirectories (including empty ones).
        /// </summary>
        public bool AllSubdirectories
        {
            get { return _allSubdirectories; }
            set { _allSubdirectories = value; }
        }

        private bool _restartableMode;

        /// <summary>
        /// /Z	Copies files in restartable mode (that is, restarts the copy process from the point of failure).
        /// </summary>
        public bool RestartableMode
        {
            get { return _restartableMode; }
            set { _restartableMode = value; }
        }
        private bool _backupMode;

        /// <summary>
        /// /B	Copies files in Backup mode (Backup copies are not restartable, but can copy some files that restartable mode cannot).
        /// </summary>
        public bool BackupMode
        {
            get { return _backupMode; }
            set { _backupMode = value; }
        }
        private string _copyFlags;

        /// <summary>
        /// /COPY:copyflags Copies the file information specified by copyflags.
        /// </summary>
        /// <remarks>
        /// D – file Data, S – file Security (NTFS ACLs), A – file Attributes,
        /// O – file Ownership information, T – file Timestamps, U – file Auditing infomation.
        /// </remarks>
        public string CopyFlags
        {
            get { return _copyFlags; }
            set { _copyFlags = value; }
        }
        private bool _copyAll;

        /// <summary>
        /// /COPYALL	Copies Everything. Equivalent to /COPY:DATSOU.
        /// </summary>
        public bool CopyAll
        {
            get { return _copyAll; }
            set { _copyAll = value; }
        }
        private bool _noCopy;

        /// <summary>
        /// /NOCOPY	Copies Nothing. Can be useful with /PURGE. 
        /// </summary>
        public bool NoCopy
        {
            get { return _noCopy; }
            set { _noCopy = value; }
        }
        private bool _security;

        /// <summary>
        /// /SEC	Copies NTFS security information. (Source and destination volumes must both be NTFS). Equivalent to /COPY:DATS.
        /// </summary>
        public bool Security
        {
            get { return _security; }
            set { _security = value; }
        }
        private bool _moveFiles;

        /// <summary>
        /// /MOV	Moves files (that is, deletes source files after copying).
        /// </summary>
        public bool MoveFiles
        {
            get { return _moveFiles; }
            set { _moveFiles = value; }
        }
        private bool _move;

        /// <summary>
        /// /MOVE	Moves files and directories (that is, deletes source files and directories after copying).
        /// </summary>
        public bool Move
        {
            get { return _move; }
            set { _move = value; }
        }
        private bool _purge;

        /// <summary>
        /// /PURGE	Deletes destination files and directories that no longer exist in the source.
        /// </summary>
        public bool Purge
        {
            get { return _purge; }
            set { _purge = value; }
        }
        private bool _mirror;

        /// <summary>
        /// /MIR	Mirrors a directory tree (equivalent to running both /E and /PURGE).
        /// </summary>
        public bool Mirror
        {
            get { return _mirror; }
            set { _mirror = value; }
        }

        private bool _create;

        /// <summary>
        /// /CREATE	Creates a directory tree structure containing zero-length files only (that is, no file data is copied).
        /// </summary>
        public bool Create
        {
            get { return _create; }
            set { _create = value; }
        }
        
        private bool _fatFileNames;

        /// <summary>
        /// /FAT 	Creates destination files using only 8.3 FAT file names.
        /// </summary>
        public bool FatFileNames
        {
            get { return _fatFileNames; }
            set { _fatFileNames = value; }
        }
        private bool _fatFileTimes;

        /// <summary>
        /// /FFT	Assume FAT File Times (2-second granularity).
        /// </summary>
        public bool FatFileTimes
        {
            get { return _fatFileTimes; }
            set { _fatFileTimes = value; }
        }
        private string _includeAttributes;

        /// <summary>
        /// /IA:{R|A|S|H|C|N|E|T|O} Includes files with the specified attributes.
        /// </summary>
        /// <remarks>
        /// The following file attributes can be acted upon:
        /// R – Read only, A – Archive, S – System, H – Hidden, 
        /// C – Compressed, N – Not content indexed, E – Encrypted, 
        /// T – Temporary, O - Offline
        /// </remarks>
        public string IncludeAttributes
        {
            get { return _includeAttributes; }
            set { _includeAttributes = value; }
        }
        private string _excluedAttributes;

        /// <summary>
        /// /XA:{R|A|S|H|C|N|E|T|O} Excludes files with the specified attributes.
        /// </summary>
        /// <remarks>
        /// The following file attributes can be acted upon:
        /// R – Read only, A – Archive, S – System, H – Hidden, 
        /// C – Compressed, N – Not content indexed, E – Encrypted, 
        /// T – Temporary, O - Offline
        /// </remarks>
        public string ExcluedAttributes
        {
            get { return _excluedAttributes; }
            set { _excluedAttributes = value; }
        }
        private bool _includeArchive;

        /// <summary>
        /// /A	Copies only files with the archive attribute set.
        /// </summary>
        public bool IncludeArchive
        {
            get { return _includeArchive; }
            set { _includeArchive = value; }
        }
        private bool _includeArchiveClear;

        /// <summary>
        /// /M	Copies only files with the archive attribute set and then resets (turns off) the archive attribute in the source files.
        /// </summary>
        public bool IncludeArchiveClear
        {
            get { return _includeArchiveClear; }
            set { _includeArchiveClear = value; }
        }
        private bool _excludeJunctions;

        /// <summary>
        /// /XJ	Excludes Junction points.
        /// </summary>
        public bool ExcludeJunctions
        {
            get { return _excludeJunctions; }
            set { _excludeJunctions = value; }
        }
        private string[] _excludeFiles;

        /// <summary>
        /// /XF file [file]	Excludes files with the specified names, paths, or wildcard characters.
        /// </summary>
        public string[] ExcludeFiles
        {
            get { return _excludeFiles; }
            set { _excludeFiles = value; }
        }
        private string[] _excludeFolders;

        /// <summary>
        /// /XD dir [dir]	Excludes directories with the specified names, paths, or wildcard characters.
        /// </summary>
        public string[] ExcludeFolders
        {
            get { return _excludeFolders; }
            set { _excludeFolders = value; }
        }

        private bool _verbose;

        /// <summary>
        /// /V 	Produces verbose output (including skipped files).
        /// </summary>
        public bool Verbose
        {
            get { return _verbose; }
            set { _verbose = value; }
        }

        private bool _noFileLogging;

        /// <summary>
        /// /NFL 	Turns off logging of file names. File names are still shown, however, if file copy errors occur.
        /// </summary>
        public bool NoFileLogging
        {
            get { return _noFileLogging; }
            set { _noFileLogging = value; }
        }
        private bool _noFolderLogging;

        /// <summary>
        /// /NDL 	Turns off logging of directory names. Full file pathnames (as opposed to simple file names) will be shown if /NDL is used.
        /// </summary>
        public bool NoFolderLogging
        {
            get { return _noFolderLogging; }
            set { _noFolderLogging = value; }
        }

        private bool _noJobHeader;

        /// <summary>
        /// /NJH	Turns of logging of the job header.
        /// </summary>
        public bool NoJobHeader
        {
            get { return _noJobHeader; }
            set { _noJobHeader = value; }
        }
        private bool _noJobSummary;

        /// <summary>
        /// /NJS	Turns off logging of the job summary.
        /// </summary>
        public bool NoJobSummary
        {
            get { return _noJobSummary; }
            set { _noJobSummary = value; }
        }
        
        private bool _noProgress = true;

        /// <summary>
        /// /NP 	Turns off copy progress indicator (% copied).
        /// </summary>
        public bool NoProgress
        {
            get { return _noProgress; }
            set { _noProgress = value; }
        }

        private string _logFile;

        /// <summary>
        /// /LOG:file	Redirects output to the specified file, overwriting the file if it already exists.
        /// </summary>
        public string LogFile
        {
            get { return _logFile; }
            set { _logFile = value; }
        }
        private string _appendLogFile;

        /// <summary>
        /// /LOG+:file	Redirects output to the specified file, appending it to the file if it already exists.
        /// </summary>
        public string AppendLogFile
        {
            get { return _appendLogFile; }
            set { _appendLogFile = value; }
        }

        private string[] _options;

        /// <summary>
        /// Manually entered options.
        /// </summary>
        public string[] Options
        {
            get { return _options; }
            set { _options = value; }
        } 
        #endregion

        /// <summary>
        /// Handles execution errors raised by the executable file.
        /// </summary>
        /// <returns>
        /// true if the method runs successfully; otherwise, false.
        /// </returns>
        protected override bool HandleTaskExecutionErrors()
        {
            //The return code from Robocopy is a bit map, defined as follows:
            //16    Serious error. Robocopy did not copy any files. 
            //8 	Some files or directories could not be copied.
            //4	    Some Mismatched files or directories were detected.
            //2	    Some Extra files or directories were detected.
            //1	    One or more files were copied successfully (that is, new files have arrived).
            //0	    No errors occurred, and no copying was done.
            return !(((ExitCode & 16) == 16) || ((ExitCode & 8) == 8));
        }

        /// <summary>
        /// Returns a string value containing the command line arguments to pass directly to the executable file.
        /// </summary>
        /// <returns>
        /// A string value containing the command line arguments to pass directly to the executable file.
        /// </returns>
        protected override string GenerateCommandLineCommands()
        {
            CommandLineBuilder builder = new CommandLineBuilder();
            builder.AppendFileNameIfNotNull(_sourceFolder);
            builder.AppendFileNameIfNotNull(_destinationFolder);
            builder.AppendFileNamesIfNotNull(_sourceFiles, " ");
            
            if (_subdirectories)
                builder.AppendSwitch("/S");
            if (_allSubdirectories)
                builder.AppendSwitch("/E");
            if (_restartableMode)
                builder.AppendSwitch("/Z");
            if (_backupMode)
                builder.AppendSwitch("/B");
            builder.AppendSwitchIfNotNull("/COPY:", _copyFlags);
            if (_copyAll)
                builder.AppendSwitch("/COPYALL");
            if (_noCopy)
                builder.AppendSwitch("/NOCOPY");
            if (_security)
                builder.AppendSwitch("/SEC");
            if (_moveFiles)
                builder.AppendSwitch("/MOV");
            if (_move)
                builder.AppendSwitch("/move");
            if (_purge)
                builder.AppendSwitch("/PURGE");
            if (_mirror)
                builder.AppendSwitch("/MIR");
            if (_noCopy)
                builder.AppendSwitch("/NOCOPY");
            if (_create)
                builder.AppendSwitch("/CREATE");
            if (_fatFileNames)
                builder.AppendSwitch("/FAT");
            if (_fatFileTimes)
                builder.AppendSwitch("/FFT");
            builder.AppendSwitchIfNotNull("/IA:", _includeAttributes);
            builder.AppendSwitchIfNotNull("/XA:", _excluedAttributes);
            if (_includeArchive)
                builder.AppendSwitch("/A");
            if (_includeArchiveClear)
                builder.AppendSwitch("/M");
            if (_excludeJunctions)
                builder.AppendSwitch("/XJ");
            
            builder.AppendSwitchIfNotNull("/XF ", _excludeFiles, " ");
            builder.AppendSwitchIfNotNull("/XD ", _excludeFolders, " ");

            if (_verbose)
                builder.AppendSwitch("/V");
            if (_noFileLogging)
                builder.AppendSwitch("/NFL");
            if (_noFolderLogging)
                builder.AppendSwitch("/NDL");
            if (_noJobHeader)
                builder.AppendSwitch("/NJH");
            if (_noJobSummary)
                builder.AppendSwitch("/NJS");
            if (_noProgress)
                builder.AppendSwitch("/NP");

            builder.AppendSwitchIfNotNull("/LOG:", _logFile);
            builder.AppendSwitchIfNotNull("/LOG+:", _appendLogFile);
            builder.AppendSwitchIfNotNull("", _options, " ");
            
            return builder.ToString();
        }

        /// <summary>
        /// Returns the fully qualified path to the executable file.
        /// </summary>
        /// <returns>
        /// The fully qualified path to the executable file.
        /// </returns>
        protected override string GenerateFullPathToTool()
        {
            return string.IsNullOrEmpty(ToolPath) ? ToolName : Path.Combine(ToolPath, ToolName);
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
            get { return "robocopy.exe"; }
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
    }
}
