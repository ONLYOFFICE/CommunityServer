#region Copyright © 2005 Paul Welter. All rights reserved.
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

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.IO;



namespace MSBuild.Community.Tasks
{
    /// <summary>
    /// Generates version information based on various algorithms
    /// </summary>
    /// <example>Get version information from file and increment revision.
    /// <code><![CDATA[
    /// <Version VersionFile="number.txt" BuildType="Automatic" RevisionType="Increment">
    ///     <Output TaskParameter="Major" PropertyName="Major" />
    ///     <Output TaskParameter="Minor" PropertyName="Minor" />
    ///     <Output TaskParameter="Build" PropertyName="Build" />
    ///     <Output TaskParameter="Revision" PropertyName="Revision" />
    /// </Version>
    /// <Message Text="Version: $(Major).$(Minor).$(Build).$(Revision)"/>
    /// ]]></code>
    /// </example>
    /// <example>Specify Major and Minor version information and generate Build and Revision.
    /// <code><![CDATA[
    /// <Version BuildType="Automatic" RevisionType="Automatic" Major="1" Minor="3" >
    ///     <Output TaskParameter="Major" PropertyName="Major" />
    ///     <Output TaskParameter="Minor" PropertyName="Minor" />
    ///     <Output TaskParameter="Build" PropertyName="Build" />
    ///     <Output TaskParameter="Revision" PropertyName="Revision" />
    /// </Version>
    /// <Message Text="Version: $(Major).$(Minor).$(Build).$(Revision)"/>
    /// ]]></code>
    /// </example>
    public class Version : Task
    {

        #region Enumerators
        private enum MajorTypeEnum
        {
            None,
            Increment,
        }
        private enum MinorTypeEnum
        {
            None,
            Increment,
            Reset
        }
        private enum BuildTypeEnum
        {
            None,
            Automatic,
            Increment,
            Reset
        }
        private enum RevisionTypeEnum
        {
            None,
            Automatic,
            Increment,
            BuildIncrement,
            Reset
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Version"/> class.
        /// </summary>
        public Version()
        {
        }

        #endregion Constructor

        private System.Version _originalValues;

        #region Output Parameters
        private int _major = 1;

        /// <summary>
        /// Gets or sets the major version number.
        /// </summary>
        /// <value>The major version number.</value>
        [Output]
        public int Major
        {
            get { return _major; }
            set { _major = value; }
        }

        private int _minor;

        /// <summary>
        /// Gets or sets the minor version number.
        /// </summary>
        /// <value>The minor version number.</value>
        [Output]
        public int Minor
        {
            get { return _minor; }
            set { _minor = value; }
        }

        private int _build;

        /// <summary>
        /// Gets or sets the build version number.
        /// </summary>
        /// <seealso cref="BuildType"/>
        /// <value>The build version number.</value>
        [Output]
        public int Build
        {
            get { return _build; }
            set { _build = value; }
        }

        private int _revision;

        /// <summary>
        /// Gets or sets the revision version number.
        /// </summary>
        /// <seealso cref="RevisionType"/>
        /// <value>The revision version number.</value>
        [Output]
        public int Revision
        {
            get { return _revision; }
            set { _revision = value; }
        }

        #endregion Output Parameters

        #region Input Parameters
        private string _versionFile;

        /// <summary>
        /// Gets or sets the file used to initialize and persist the version.
        /// </summary>
        /// <value>The version file.</value>
        /// <remarks>
        /// When specified, the task will attempt to load the previous version information from the file.
        /// After updating the version, the new value will be saved to the file.
        /// <para>
        /// If you do not specify a value for this property, the version will be calculated
        /// based on the values passed to the <see cref="Major"/>, <see cref="Minor"/>,
        /// <see cref="Build"/>, and <see cref="Revision"/> properties. The new version will not be persisted.</para></remarks>
        public string VersionFile
        {
            get { return _versionFile; }
            set { _versionFile = value; }
        }

        private MajorTypeEnum _majorTypeEnum = MajorTypeEnum.None;

        /// <summary>
        /// Gets or sets the method used to generate a <see cref="Major"/> number
        /// </summary>
        /// <remarks>
        /// If value is not provided, None is assumed.
        /// The <see cref="Major"/> number is set according to the following table:
        /// <list type="table">
        /// <listheader><term>MajorType</term><description>Description</description></listheader>
        /// <item><term>None</term><description>The number is not modified.</description></item>
        /// <item><term>Increment</term><description>Increases the previous <see cref="Major"/> value by 1.</description></item>
        /// </list>
        /// </remarks>
        public string MajorType
        {
            get { return _majorTypeEnum.ToString(); }
            set
            {
                object parsedMajorType;
                if (EnumTryParse("MajorType", typeof(MajorTypeEnum), value, out parsedMajorType))
                {
                    _majorTypeEnum = (MajorTypeEnum)parsedMajorType;
                }
                else
                {
                    validParameters = false;
                }
            }
        }

        private MinorTypeEnum _minorTypeEnum = MinorTypeEnum.None;

        /// <summary>
        /// Gets or sets the method used to generate a <see cref="Minor"/> number
        /// </summary>
        /// <remarks>
        /// If value is not provided, None is assumed.
        /// The <see cref="Minor"/> number is set according to the following table:
        /// <list type="table">
        /// <listheader><term>MinorType</term><description>Description</description></listheader>
        /// <item><term>None</term><description>The number is not modified.</description></item>
        /// <item><term>Increment</term><description>Increases the previous <see cref="Minor"/> value by 1.</description></item>
        /// <item><term>Reset</term><description>Resets the previous <see cref="Minor"/> value to 0.</description></item>
        /// </list>
        /// </remarks>
        public string MinorType
        {
            get { return _minorTypeEnum.ToString(); }
            set
            {
                object parsedMinorType;
                if (EnumTryParse("MinorType", typeof(MinorTypeEnum), value, out parsedMinorType))
                {
                    _minorTypeEnum = (MinorTypeEnum)parsedMinorType;
                }
                else
                {
                    validParameters = false;
                }
            }
        }

        private BuildTypeEnum _buildTypeEnum = BuildTypeEnum.None;

        /// <summary>
        /// Gets or sets the method used to generate a <see cref="Build"/> number
        /// </summary>
        /// <remarks>
        /// If value is not provided, None is assumed.
        /// The <see cref="Build"/> number is set according to the following table:
        /// <list type="table">
        /// <listheader><term>BuildType</term><description>Description</description></listheader>
        /// <item><term>None</term><description>The number is not modified.</description></item>
        /// <item><term>Automatic</term><description>The number of days since <see cref="StartDate"/>.</description></item>
        /// <item><term>Increment</term><description>Increases the previous <see cref="Build"/> value by 1.</description></item>
        /// <item><term>Reset</term><description>Resets the previous <see cref="Build"/> value to 0.</description></item>
        /// </list>
        /// </remarks>
        public string BuildType
        {
            get { return _buildTypeEnum.ToString(); }
            set
            {
                if (attemptedUseOfObsoleteBuildType(value)) return;
                object parsedBuildType;
                if (EnumTryParse("BuildType", typeof(BuildTypeEnum), value, out parsedBuildType))
                {
                    _buildTypeEnum = (BuildTypeEnum)parsedBuildType;
                }
                else
                {
                    validParameters = false;
                }
            }
        }

        private bool attemptedUseOfObsoleteBuildType(string buildType)
        {
            if (buildType.Equals("DateIncrement", StringComparison.CurrentCultureIgnoreCase))
            {
                Log.LogError("DateIncrement is no longer a valid BuildType. Use Automatic instead.");
                validParameters = false;
                return true;
            }
            if (buildType.Equals("Date", StringComparison.CurrentCultureIgnoreCase))
            {
                Log.LogError("Date is no longer a valid BuildType. Use the Time task instead. See http://msbuildtasks.tigris.org/ReleaseNotes.html#v12_Version");
                validParameters = false;
                return true;
            }
            return false;
        }

        private bool validParameters = true;

        //TODO: figure out how to factor this out to be used by all tasks
        private bool EnumTryParse(string propertyName, Type enumType, string valueToParse, out object parsedValue)
        {
            bool success = false;
            parsedValue = null;
            try
            {
                parsedValue = Enum.Parse(enumType, valueToParse);
                success = true;
            }
            catch (ArgumentException)
            {
                Log.LogError("The {0} value '{1}' is invalid. Valid values are: {2}", propertyName, valueToParse,
                    String.Join(", ", Enum.GetNames(enumType)));
            }
            return success;
        }

        private RevisionTypeEnum _revisionTypeEnum = RevisionTypeEnum.None;

        /// <summary>
        /// Gets or sets the method used to generate a <see cref="Revision"/> number
        /// </summary>
        /// <remarks>
        /// If value is not provided, None is assumed.
        /// The <see cref="Revision"/> number is set according to the following table:
        /// <list type="table">
        /// <listheader><term>RevisionType</term><description>Description</description></listheader>
        /// <item><term>None</term><description>The number is not modified.</description></item>
        /// <item><term>Automatic</term><description>A number that starts at 0 at midnight, and constantly increases throughout the day (changing roughly every 1.3 seconds). Guaranteed to be safe for components of the AssemblyVersion attribute.</description></item>
        /// <item><term>Increment</term><description>Increases the previous <see cref="Revision"/> value by 1.</description></item>
        /// <item><term>BuildIncrement</term><description>Increases the previous <see cref="Revision"/> value by 1 when the value of <see cref="Build"/> is unchanged. If the value of <see cref="Build"/> has changed, <see cref="Revision"/> is reset to 0.</description></item>
        /// <item><term>Reset</term><description>Resets the previous <see cref="Revision"/> value to 0.</description></item>
        /// </list>
        /// </remarks>
        public string RevisionType
        {
            get { return _revisionTypeEnum.ToString(); }
            set
            {
                object parsedRevisionType;
                if (EnumTryParse("RevisionType", typeof(RevisionTypeEnum), value, out parsedRevisionType))
                {
                    _revisionTypeEnum = (RevisionTypeEnum)parsedRevisionType;
                }
                else
                {
                    validParameters = false;
                }
            }
        }

        private DateTime _startDate = new DateTime(2000, 1, 1);
        /// <summary>
        /// Gets or sets the starting date used to calculate the <see cref="Build"/> number when <see cref="BuildType"/> is Automatic.
        /// </summary>
        /// <value>The starting date for calculation of the build number.</value>
        /// <remarks>
        /// This value is only used when the <see cref="BuildType"/> is Automatic.
        /// <para>This default value is January 1, 2000.</para>
        /// </remarks>
        public string StartDate
        {
            get { return _startDate.ToString(); }
            set { _startDate = DateTime.Parse(value); }
        }

        #endregion Input Parameters

        #region Task Overrides
        /// <summary>
        /// When overridden in a derived class, executes the task.
        /// </summary>
        /// <returns>
        /// true if the task successfully executed; otherwise, false.
        /// </returns>
        public override bool Execute()
        {
            if (!validParameters)
            {
                return false;
            }
            _originalValues = new System.Version(_major, _minor, _build, _revision);
            if (!ReadVersionFromFile()) return false;

            Log.LogMessage(MessageImportance.Low, Properties.Resources.VersionOriginalValue, _originalValues.ToString());
            CalculateMajorNumber();
            CalculateMinorNumber();
            CalculateBuildNumber();
            CalculateRevisionNumber();

            return FileWouldChange() ? WriteVersionToFile() : true;
        }

        #endregion Task Overrides

        #region Private Methods
        private bool ReadVersionFromFile()
        {
            string textVersion = null;
            System.Version version = null;
            if (String.IsNullOrEmpty(_versionFile)) return true;

            if (!System.IO.File.Exists(_versionFile))
            {
                Log.LogWarning(Properties.Resources.VersionFileNotFound, _versionFile);
                return true;
            }

            Log.LogMessage(MessageImportance.Low, Properties.Resources.VersionRead, _versionFile);
            try
            {
                textVersion = File.ReadAllText(_versionFile);
            }
            catch (Exception ex)
            {
                Log.LogError(Properties.Resources.VersionReadException,
                    _versionFile, ex.Message);
                return false;
            }

            try
            {
                version = new System.Version(textVersion);
            }
            catch (Exception ex)
            {
                Log.LogError(Properties.Resources.VersionReadException,
                    _versionFile, ex.Message);
                return false;
            }

            if (version != null)
            {
                _major = version.Major;
                _minor = version.Minor;
                _build = version.Build;
                _revision = version.Revision;
                _originalValues = version;
            }
            return true;
        }

        private bool WriteVersionToFile()
        {
            System.Version version = new System.Version(_major, _minor, _build, _revision);
            Log.LogMessage(MessageImportance.Low, Properties.Resources.VersionModifiedValue, version.ToString());

            if (String.IsNullOrEmpty(_versionFile)) return true;

            try
            {
                File.WriteAllText(_versionFile, version.ToString());
            }
            catch (Exception ex)
            {
                Log.LogError(Properties.Resources.VersionWriteException,
                    _versionFile, ex.Message);
                return false;
            }

            Log.LogMessage(MessageImportance.Low, Properties.Resources.VersionWrote, _versionFile);

            return true;
        }

        private void CalculateMajorNumber()
        {
            switch (_majorTypeEnum)
            {
                case MajorTypeEnum.Increment:
                    _major++;
                    break;
                case MajorTypeEnum.None:
                default:
                    break;
            }
        }

        private void CalculateMinorNumber()
        {
            switch (_minorTypeEnum)
            {
                case MinorTypeEnum.Increment:
                    _minor++;
                    break;
                case MinorTypeEnum.Reset:
                    _minor = 0;
                    break;
                case MinorTypeEnum.None:
                default:
                    break;
            }
        }

        private void CalculateBuildNumber()
        {
            switch (_buildTypeEnum)
            {
                case BuildTypeEnum.Automatic:
                    _build = CalculateDaysSinceStartDate();
                    break;
                case BuildTypeEnum.Increment:
                    _build++;
                    break;
                case BuildTypeEnum.Reset:
                    _build = 0;
                    break;
                case BuildTypeEnum.None:
                default:
                    break;
            }
        }

        private int CalculateDaysSinceStartDate()
        {
            return DateTime.Today.Subtract(_startDate).Days;
        }

        private void CalculateRevisionNumber()
        {
            switch (_revisionTypeEnum)
            {
                case RevisionTypeEnum.Automatic:
                    _revision = CalculateFractionalPartOfDay();
                    break;
                case RevisionTypeEnum.Increment:
                    _revision++;
                    break;
                case RevisionTypeEnum.BuildIncrement:
                    _revision = CalculateBuildIncrementRevision();
                    break;
                case RevisionTypeEnum.Reset:
                    _revision = 0;
                    break;
                case RevisionTypeEnum.None:
                default:
                    break;
            }
        }

        private int CalculateFractionalPartOfDay()
        {
            //break down a day into fractional seconds
            float factor = (float)(UInt16.MaxValue - 1) / (24 * 60 * 60);

            return (int)(DateTime.Now.TimeOfDay.TotalSeconds * factor);
        }

        private int CalculateBuildIncrementRevision()
        {
            if (_build == _originalValues.Build)
            {
                return _revision + 1;
            }
            return 0;
        }

        private bool FileWouldChange()
        {
            return (VersionChanged() || String.IsNullOrEmpty(_versionFile) || !System.IO.File.Exists(_versionFile));
        }

        private bool VersionChanged()
        {
            System.Version _currentValues = new System.Version(_major, _minor, _build, _revision);
            return _originalValues != _currentValues;
        }
        #endregion Private Methods

    }
}
