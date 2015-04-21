
using System;
using Microsoft.Win32;

namespace MSBuild.Community.Tasks.Services
{
    /// <summary>
    /// The contract for a service that will provide access to the registry.
    /// </summary>
    /// <exclude />
    public interface IRegistry
    {
        /// <summary>
        /// Returns the names of the subkeys under the provided key.
        /// </summary>
        /// <param name="hive">The hive where <paramref name="key"/> is located.</param>
        /// <param name="key">The key to search.</param>
        /// <returns>A list of subkeys.</returns>
        string[] GetSubKeys(RegistryHive hive, string key);
        /// <summary>
        /// Returns the value of an entry in the registry.
        /// </summary>
        /// <param name="key">The key of the registry entry that contains <paramref name="valueName"/></param>
        /// <param name="valueName">The name of the value to return.</param>
        /// <returns>The value of the registry entry.</returns>
        object GetValue(string key, string valueName);
    }

    /// <summary>
    /// Provides access to the Windows registry.
    /// </summary>
    /// <exclude />
    public class Win32Registry : IRegistry
    {
        /// <summary>
        /// Returns the names of the subkeys under the provided key.
        /// </summary>
        /// <param name="hive">The hive where <paramref name="key"/> is located.</param>
        /// <param name="key">The key to search.</param>
        /// <returns>A list of subkeys.</returns>
        public string[] GetSubKeys(RegistryHive hive, string key)
        {
            RegistryKey hiveKey = getHiveKey(hive);
            using (RegistryKey openKey = hiveKey.OpenSubKey(key))
            {
                return openKey.GetSubKeyNames();
            }
        }

        private static RegistryKey getHiveKey(RegistryHive hive)
        {
            switch(hive)
            {
                case RegistryHive.ClassesRoot:
                    return Registry.ClassesRoot;
                case RegistryHive.CurrentConfig:
                    return Registry.CurrentConfig;
                case RegistryHive.CurrentUser:
                    return Registry.CurrentUser;
                case RegistryHive.DynData:
                    return Registry.DynData;
                case RegistryHive.LocalMachine:
                    return Registry.LocalMachine;
                case RegistryHive.PerformanceData:
                    return Registry.PerformanceData;
                case RegistryHive.Users:
                    return Registry.Users;
                default:
                    throw new ArgumentOutOfRangeException("hive", hive, "Unrecognized RegistryHive.");
            }
        }

        /// <summary>
        /// Returns the value of an entry in the registry.
        /// </summary>
        /// <param name="key">The key of the registry entry that contains <paramref name="valueName"/></param>
        /// <param name="valueName">The name of the value to return.</param>
        /// <returns>The value of the registry entry.</returns>
        public object GetValue(string key, string valueName)
        {
            return Registry.GetValue(key, valueName, null);
        }
    }
}
