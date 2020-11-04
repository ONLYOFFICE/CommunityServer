
using System;
using Microsoft.Build.Framework;
using System.DirectoryServices;
using System.Runtime.InteropServices;

namespace MSBuild.Community.Tasks.IIS
{
    /// <summary>
    /// Reads and modifies a web directory configuration setting.
    /// </summary>
    /// <example>Display the file system path of the MyWeb web directory:
    /// <code><![CDATA[
    /// <WebDirectorySetting VirtualDirectoryName="MyWeb" SettingName="Path">
    ///     <Output TaskParameter="SettingValue" PropertyName="LocalPath" />
    /// </WebDirectorySetting>
    /// <Message Text="MyWeb is located at $(LocalPath)" />
    /// ]]></code>
    /// </example>
    /// <example>Set the default document for the MyWeb directory to Default.aspx:
    /// <code><![CDATA[
    /// <WebDirectorySetting VirtualDirectoryName="MyWeb" SettingName="DefaultDoc" SettingValue="Default.aspx" />
    /// <WebDirectorySetting VirtualDirectoryName="MyWeb" SettingName="EnableDefaultDoc" SettingValue="True" />
    /// ]]></code>
    /// </example>

    public class WebDirectorySetting : WebBase
    {
        private string mVirtualDirectoryName;
        /// <summary>
        /// Gets or sets the name of the virtual directory.
        /// </summary>
        /// <value>The name of the virtual directory.</value>
        [Required]
        public string VirtualDirectoryName
        {
            get
            {
                return mVirtualDirectoryName;
            }
            set
            {
                mVirtualDirectoryName = value;
            }
        }

        private string settingName;

        /// <summary>
        /// Gets or sets the configuration setting to read or modify.
        /// </summary>
        [Required]
        public string SettingName
        {
            get { return settingName; }
            set { settingName = value; }
        }

        private string settingValue;

        /// <summary>
        /// Gets or sets the value of <see cref="SettingName" /> on the web directory
        /// </summary>
        [Output]
        public string SettingValue
        {
            get { return settingValue; }
            set { settingValue = value; }
        }

        /// <summary>
        /// When overridden in a derived class, executes the task.
        /// </summary>
        /// <returns>
        /// True if the task successfully executed; otherwise, false.
        /// </returns>
        public override bool Execute()
        {
            DirectoryEntry targetDirectory = null;
            try
            {
                bool modifySetting = !String.IsNullOrEmpty(settingValue);
                string actionMessage = modifySetting 
                    ? Properties.Resources.WebDirectorySettingStatusSet
                    : Properties.Resources.WebDirectorySettingStatusRead;
                Log.LogMessage(MessageImportance.Normal, actionMessage, SettingName, VirtualDirectoryName, ServerName);
                
                VerifyIISRoot();

                string targetDirectoryPath = (VirtualDirectoryName == "/")
                    ? targetDirectoryPath = IISServerPath
                    : targetDirectoryPath = IISServerPath + "/" + VirtualDirectoryName;

                targetDirectory = new DirectoryEntry(targetDirectoryPath);
                
                try
                {
                        string directoryExistsTest = targetDirectory.SchemaClassName;
                }
                catch (COMException)
                {
                    Log.LogError(Properties.Resources.WebDirectoryInvalidDirectory, VirtualDirectoryName, ServerName);
                    return false;
                }

                PropertyValueCollection propertyValues = targetDirectory.Properties[settingName];
                if (propertyValues == null)
                {
                    Log.LogError(Properties.Resources.WebDirectorySettingInvalidSetting, VirtualDirectoryName, ServerName, SettingName);
                    return false;
                }

                if (modifySetting)
                {
                    try
                    {
                        if (propertyValues.Count > 0)
                        {
                            propertyValues[0] = settingValue;
                        }
                        else
                        {
                            propertyValues.Value = settingValue;
                        }
                    }
                    catch (COMException)
                    {
                        Log.LogError(Properties.Resources.WebDirectorySettingInvalidSetting, VirtualDirectoryName, ServerName, SettingName);
                        return false;
                    }
                    targetDirectory.CommitChanges();
                }

                object settingValueHolder;
                if (propertyValues.Count > 0)
                {
                    settingValueHolder = propertyValues[0];
                }
                else
                {
                    settingValueHolder = propertyValues.Value;
                }

                if (settingValueHolder == null)
                {
                    Log.LogError(Properties.Resources.WebDirectorySettingInvalidSetting, VirtualDirectoryName, ServerName, SettingName);
                    return false;
                }
                else
                {
                    settingValue = settingValueHolder.ToString();
                }
            }
            finally
            {
                if (targetDirectory != null) targetDirectory.Dispose();
            }
            return true;
        }

    }
}
