#region Copyright © 2008 MSBuild Community Task Project. All rights reserved.

/*
Copyright © 2008 MSBuild Community Task Project. All rights reserved.

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
using System.Reflection;
using System.Runtime.InteropServices;



namespace MSBuild.Community.Tasks.Fusion
{
    /// <summary>
    /// A class wrapping fusion api calls
    /// </summary>
    public static class FusionWrapper
    {
        private static readonly object _lock = new object();
        private static readonly string[] proccessors = new[] {"MSIL", "x86", "AMD64"};
        private static string _currentProcessorArchitecture;

        internal static string CurrentProcessorArchitecture
        {
            get
            {
                if (string.IsNullOrEmpty(_currentProcessorArchitecture))
                    lock (_lock)
                    {
                        if (string.IsNullOrEmpty(_currentProcessorArchitecture))
                            _currentProcessorArchitecture = GetProcessorArchitecture();
                    }

                return _currentProcessorArchitecture;
            }
        }

        /// <summary>
        /// Installs the assembly.
        /// </summary>
        /// <param name="assemblyPath">The assembly path.</param>
        /// <param name="force">if set to <c>true</c> force.</param>
        public static void InstallAssembly(string assemblyPath, bool force)
        {
            IAssemblyCache assemblyCache = null;

            int flags = force ? (int) CommitFlags.Force : (int) CommitFlags.Refresh;

            ThrowOnError(NativeMethods.CreateAssemblyCache(out assemblyCache, 0));
            ThrowOnError(assemblyCache.InstallAssembly(flags, assemblyPath, null));
        }

        /// <summary>
        /// Uninstalls the assembly.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <param name="force">if set to <c>true</c> force.</param>
        /// <returns>Returns <c>true</c> if uninstall successful.</returns>
        public static bool UninstallAssembly(string assemblyName, bool force)
        {
            UninstallStatus result = UninstallStatus.Uninstalled;
            return UninstallAssembly(assemblyName, force, out result);
        }

        /// <summary>
        /// Uninstalls the assembly.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <param name="force">if set to <c>true</c> force.</param>
        /// <param name="result">The UninstallStatus result.</param>
        /// <returns>Returns <c>true</c> if uninstall successful.</returns>
        public static bool UninstallAssembly(string assemblyName, bool force, out UninstallStatus result)
        {
            result = UninstallStatus.None;

            string fullName;
            string fullPath = GetAssemblyPath(assemblyName, out fullName);
            if (string.IsNullOrEmpty(fullPath))
            {
                result = UninstallStatus.ReferenceNotFound;
                return true;
            }

            IAssemblyCache cache = null;
            ThrowOnError(NativeMethods.CreateAssemblyCache(out cache, 0));

            int flags = force ? (int) CommitFlags.Force : (int) CommitFlags.Refresh;

            ThrowOnError(cache.UninstallAssembly(flags, fullName, null, out result));

            bool successful = false;

            switch (result)
            {
                case UninstallStatus.Uninstalled:
                case UninstallStatus.AlreadyUninstalled:
                case UninstallStatus.DeletePending:
                    successful = true;
                    break;
            }

            return successful;
        }

        /// <summary>
        /// Gets the assembly path.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <returns>The path to the assembly in the GAC.</returns>
        public static string GetAssemblyPath(string assemblyName)
        {
            string fullName;
            return GetAssemblyPath(assemblyName, out fullName);
        }

        internal static string GetAssemblyPath(string assemblyName, out string fullName)
        {
            IAssemblyCache cache = null;
            ThrowOnError(NativeMethods.CreateAssemblyCache(out cache, 0));

            var info = new AssemblyInfo
                           {
                               cbAssemblyInfo = (uint) Marshal.SizeOf(typeof (AssemblyInfo))
                           };

            //ProcessorArchitecture required for this call
            fullName = assemblyName;

            if (HasProcessorArchitecture(fullName))
                //getting size of string, cchBuf will be the size
                cache.QueryAssemblyInfo(3, fullName, ref info);
            else
                //try using possible proccessors
                foreach (string p in proccessors)
                {
                    fullName = AppendProccessor(assemblyName, p);
                    cache.QueryAssemblyInfo(3, fullName, ref info);

                    //if no size, not found, try another proccessor
                    if (info.cchBuf > 0)
                        break;
                }
            //if no size, not found
            if (info.cchBuf == 0)
                return null;

            //get path
            info.pszCurrentAssemblyPathBuf = new string(new char[info.cchBuf]);
            ThrowOnError(cache.QueryAssemblyInfo(3, fullName, ref info));

            return info.pszCurrentAssemblyPathBuf;
        }

        /// <summary>
        /// Gets the name of the assembly.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <returns>An <see cref="AssemblyName"/> instance.</returns>
        public static AssemblyName GetAssemblyName(string assemblyName)
        {
            string filePath = GetAssemblyPath(assemblyName);
            AssemblyName result = AssemblyName.GetAssemblyName(filePath);
            return result;
        }

        private static void ThrowOnError(int hr)
        {
            if (hr < 0)
                Marshal.ThrowExceptionForHR(hr);
        }

        internal static string AppendProccessor(string fullName)
        {
            return AppendProccessor(fullName, CurrentProcessorArchitecture);
        }

        internal static string AppendProccessor(string fullName, ProcessorArchitecture targetProcessor)
        {
            string processor = targetProcessor == ProcessorArchitecture.None ? "MSIL" : targetProcessor.ToString();
            return AppendProccessor(fullName, processor);
        }

        internal static string AppendProccessor(string fullName, string targetProcessor)
        {
            if (HasProcessorArchitecture(fullName))
                return fullName;

            return fullName + ", ProcessorArchitecture=" + targetProcessor;
        }

        private static bool HasProcessorArchitecture(string fullName)
        {
            return fullName.IndexOf("ProcessorArchitecture=", 0,
                                    fullName.Length, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string GetProcessorArchitecture()
        {
            var lpSystemInfo = new SYSTEM_INFO();
            NativeMethods.GetSystemInfo(ref lpSystemInfo);

            switch (lpSystemInfo.wProcessorArchitecture)
            {
                case 0:
                    return "x86";
                case 6:
                    return "IA64";
                case 9:
                    return "AMD64";
                default:
                    return "MSIL";
            }
        }
    }
}