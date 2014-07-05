using System;
using System.Collections.Generic;
using System.Text;
using DaveSexton.DocProject;
using DaveSexton.DocProject.Engine;

namespace HAPDocumentation
{
    /// <summary>
    /// Hooks into the DocProject build process for the project in which it's defined.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class must be registered with the DocProject in the <em>Active Projects</em>
    /// tools options page in order for DocProject to instantiate it during a help build.
    /// </para>
    /// <para>
    /// To cancel the build at any time call the <see cref="BuildContext.Cancel" /> 
    /// method.  The build process will end after the current step is executed, 
    /// unless the step is being executed in the background.  In that case, it may 
    /// end immediately.
    /// </para>
    /// <para>
    /// Note: Do not cache instances of the <see cref="BuildContext" /> class.  A new 
    /// <see cref="BuildContext" /> is created each time the project is built.
    /// </para>
    /// </remarks>
    public class BuildProcess : BuildProcessComponent
    {
        DateTime buildStart, stepStart;

        /// <summary>
        /// Called before the project's help build starts.
        /// </summary>
        /// <param name="context">Provides information about the build process.</param>
        public override void BuildStarting(BuildContext context)
        {
            // Uncomment the following line to break into the debugger: 
            // System.Diagnostics.Debugger.Break();

            buildStart = DateTime.Now;
        }

        /// <summary>
        /// Called before a <paramref name="step" /> is executed during a help build.
        /// </summary>
        /// <param name="step"><see cref="IBuildStep" /> implementation to be executed.</param>
        /// <param name="context">Provides information about the build process.</param>
        /// <returns><b>true</b> indicates that the process should continue, otherwise, 
        /// <b>false</b> indicates that the process should skip this step.</returns>
        public override bool BeforeExecuteStep(IBuildStep step, BuildContext context)
        {
            stepStart = DateTime.Now;

            return true;
        }

        /// <summary>
        /// Called after a <paramref name="step" /> has been executed during a help build.
        /// </summary>
        /// <param name="step"><see cref="IBuildStep" /> implementation that was executed.</param>
        /// <param name="context">Provides information about the build process.</param>
        public override void AfterExecuteStep(IBuildStep step, BuildContext context)
        {
            TraceLine();
            TraceLine("Step {0} Time Elapsed: {1}", context.CurrentStepIndex + 1, DateTime.Now - stepStart);
        }

        /// <summary>
        /// Called after the project's help build has finished.
        /// </summary>
        /// <remarks>
        /// The <see cref="BuildContext.Cancel" /> method has no affect at this 
        /// point in the build process.  This method is the final step before the 
        /// build statistics are displayed.
        /// <para>
        /// This method is always invoked if <see cref="BuildStarting" /> is invoked, 
        /// regardless of whether an exception is thrown in any of the other methods, 
        /// <see cref="BuildContext.Cancel" /> has been called, or an exeception has
        /// been thrown by the build engine.
        /// </para>
        /// <para>
        /// To determine whether a help build failed or succeeded, examine the value of the
        /// <see cref="BuildContext.BuildState" /> property.
        /// </para>
        /// </remarks>
        /// <param name="context">Provides information about the build process.</param>
        public override void BuildCompleted(BuildContext context)
        {
            TraceLine();
            TraceLine("Total Time Elapsed: {0}", DateTime.Now - buildStart);
        }
    }
}
