using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.Runtime.InteropServices;

namespace MSBuild.Community.Tasks
{
    /// <summary>
    /// A task for embedded native resource.
    /// </summary>
    public class EmbedNativeResource : Task
    {
        #region Properties

        /// <summary>
        /// Gets or sets the target assembly path.
        /// </summary>
        [Required]
        public string TargetAssemblyPath { get; set; }

        /// <summary>
        /// Gets or sets the resource path.
        /// </summary>
        [Required]
        public string ResourcePath { get; set; }

        /// <summary>
        /// Gets or sets the resource name.
        /// </summary>
        public string ResourceName { get; set; }

        /// <summary>
        /// Gets or sets the resource type.
        /// </summary>
        public string ResourceType { get; set; }

        #endregion

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns>Success or failure of the task.</returns>
        public override bool Execute()
        {
            try
            {
                return ExecuteCore();
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex);
            }

            return Log.HasLoggedErrors;
        }

        #region P/Invoke

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool UpdateResource(IntPtr hUpdate, string lpType, string lpName, ushort wLanguage, IntPtr lpData, uint cbData);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr BeginUpdateResource(string pFileName, [MarshalAs(UnmanagedType.Bool)]bool bDeleteExistingResources);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool EndUpdateResource(IntPtr hUpdate, bool fDiscard);

        #endregion

        private bool ExecuteCore()
        {
            IntPtr filePtr = IntPtr.Zero;
            IntPtr resPtr = IntPtr.Zero;
            try
            {
                SetDefaults();

                // Read resource into unmanaged buffer
                byte[] resBytes = File.ReadAllBytes(ResourcePath);
                int resSize = Marshal.SizeOf(resBytes[0]) * resBytes.Length;
                resPtr = Marshal.AllocHGlobal(resSize);
                if (resPtr == IntPtr.Zero)
                {
                    Log.LogError("Failed to allocate memory for resource");
                    return false;
                }
                Marshal.Copy(resBytes, 0, resPtr, resBytes.Length);

                // Create target resources
                filePtr = BeginUpdateResource(TargetAssemblyPath, false);
                if (filePtr == IntPtr.Zero)
                {
                    Log.LogError("Failed to open file's resources: {0}", TargetAssemblyPath);
                    return false;
                }

                bool bResult = UpdateResource(
                     filePtr,
                     ResourceType,
                     ResourceName,
                     0, //MAKELANGID(LANG_NEUTRAL, SUBLANG_NEUTRAL),
                     resPtr,
                     (uint)resSize);
                if (!bResult)
                {
                    Log.LogError("Failed to update file's resources: {0}", TargetAssemblyPath);
                    return false;
                }

                bResult = EndUpdateResource(filePtr, false);
                if (!bResult)
                {
                    Log.LogError("Failed to close file's resources: {0}", TargetAssemblyPath);
                    return false;
                }
            }
            finally
            {
                if (filePtr != IntPtr.Zero)
                { 
                    Marshal.Release(filePtr); 
                }

                if (resPtr != IntPtr.Zero)
                {
                    Marshal.Release(resPtr);
                }
            }

            return Log.HasLoggedErrors;
        }

        private void SetDefaults()
        {
            if (string.IsNullOrEmpty(ResourceType))
            {
                ResourceType = "RC_DATA";
            }

            if (string.IsNullOrEmpty(ResourceName))
            {
                FileInfo fi = new FileInfo(ResourcePath);
                ResourceName = fi.Name;
            }
        }
    }
}
