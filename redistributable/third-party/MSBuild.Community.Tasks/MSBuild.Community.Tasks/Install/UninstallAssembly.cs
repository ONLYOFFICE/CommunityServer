

using System;

namespace MSBuild.Community.Tasks.Install
{
    /// <summary>Uninstalls assemblies.</summary>    
    /// <remarks>
    /// Uses InstallUtil.exe to execute the 
    /// <see href="http://msdn2.microsoft.com/system.configuration.install.installer.uninstall.aspx">Uninstall</see>
    /// method of
    /// <see href="http://msdn2.microsoft.com/system.configuration.install.installer.aspx">Installer</see>
    /// classes contained within specified assemblies.
    /// </remarks>
    /// <example>Uninstall multiple assemblies by specifying the file names:
    /// <code><![CDATA[
    /// <UninstallAssembly AssemblyFiles="Engine.dll;Presenter.dll" />
    /// ]]></code>
    /// </example>
    /// <example>Unnstall an assembly using the assembly name. Also disable the log file by setting it to a single space:
    /// <code><![CDATA[
    /// <UninstallAssembly AssemblyNames="Engine,Version=1.5.0.0,Culture=neutral" LogFile=" "/>
    /// ]]></code>
    /// </example>
    public class UninstallAssembly : InstallAssembly
    {
        /// <summary>
        /// Determines whether assemblies are installed or uninstalled.
        /// </summary>
        protected override bool IsUninstall
        {
            get
            {
                return true;
            }
        }
    }
}
