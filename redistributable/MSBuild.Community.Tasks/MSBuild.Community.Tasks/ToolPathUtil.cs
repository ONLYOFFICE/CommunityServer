using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Win32;

namespace MSBuild.Community.Tasks
{
    internal static class ToolPathUtil
    {
        public static bool SafeFileExists(string path, string toolName)
        {
            return SafeFileExists(Path.Combine(path, toolName));
        }
        
        public static bool SafeFileExists(string file)
        {
            try { return File.Exists(file); }
            catch { } // eat exception

            return false;
        }

        public static string MakeToolName(string name)
        {
            return (Environment.OSVersion.Platform == PlatformID.Unix) ?
                name : name + ".exe";
        }

        public static string FindInRegistry(string toolName)
        {
            try
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + toolName, false);
                if (key != null)
                {
                    string possiblePath = key.GetValue(null) as string;
                    if (SafeFileExists(possiblePath))
                        return Path.GetDirectoryName(possiblePath);
                }
            }
            catch (System.Security.SecurityException) { }

            return null;
        }

        public static string FindInPath(string toolName)
        {
            string pathEnvironmentVariable = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
            string[] paths = pathEnvironmentVariable.Split(new[] { Path.PathSeparator }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string path in paths)
            {
                if (SafeFileExists(path, toolName))
                {
                    return path;
                }
            }

            return null;
        }

        public static string FindInProgramFiles(string toolName, params string[] commonLocations)
        {
            foreach (string location in commonLocations)
            {
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), location);
                if (SafeFileExists(path, toolName))
                {
                    return path;
                }
            }

            return null;
        }

        public static string FindInLocalPath(string toolName, string localPath)
        {
            if (localPath == null)
                return null;

            string path = new DirectoryInfo(localPath).FullName;
            if (SafeFileExists(localPath, toolName))
            {
                return path;
            }

            return null;
        }
    }
}
