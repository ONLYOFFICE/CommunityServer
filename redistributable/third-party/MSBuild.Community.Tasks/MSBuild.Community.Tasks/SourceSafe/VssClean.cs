

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Utilities;

namespace MSBuild.Community.Tasks.SourceSafe
{
    /// <summary>
    /// Task that can strip the source control information from a
    /// Visual Studio Solution and subprojects.
    /// </summary>
    /// <remarks>This task is useful if you keep an archive of the
    /// source tree at each build milestone, because it's irritating to have
    /// to remove source control binding manually once you've copied a
    /// version of the code from your archive.</remarks>
    public class VssClean : Task
    {
        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns><see langword="true"/> if the task ran successfully; 
        /// otherwise <see langword="false"/>.</returns>
        public override bool Execute()
        {
            throw new NotImplementedException();
        }
    }
}
