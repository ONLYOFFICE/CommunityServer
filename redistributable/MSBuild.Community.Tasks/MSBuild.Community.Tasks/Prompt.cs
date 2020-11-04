 
using System;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;


namespace MSBuild.Community.Tasks
{
    /// <summary>
    /// Displays a message on the console and waits for user input.
    /// </summary>
    /// <remarks>It is important to note that the message is not written to the MSBuild logger, 
    /// it is always written to the console, no matter which logger is configured.
    /// <para>This task requires user input from the console. Do not use this task for projects
    /// that will be executed unattended. It is recommended that you always add a Condtion so that
    /// this task is only enabled when a custom property is set through the command line.
    /// This will ensure that the other users do not attempt to execute the task in unattended mode.
    /// </para></remarks>
    /// <example>Pause the build if the interactive property is set:
    /// <code><![CDATA[
    /// <!-- Pause when invoked with the interactive property: msbuild myproject.proj /property:interactive=true -->
    /// 
    /// <Prompt Text="You can now attach the debugger to the msbuild.exe process..." Condition="'$(Interactive)' == 'True'" />
    /// ]]></code>
    /// </example>
    /// <example>Obtain user input during the build: 
    /// (Note: in most cases, it is recommended that users instead provide custom values to your build through the /property argument of msbuild.exe)
    /// <code><![CDATA[
    /// <Prompt Text="Tell me your name:" Condition="'$(Interactive)' == 'True'" >
    ///   <Output TaskParameter="UserInput" PropertyName="PersonName" />
    /// </Prompt>
    /// <Message Text="Hello $(PersonName)" />
    /// ]]></code>
    /// </example>
    public class Prompt : Task
    {
        private string text = "Press Enter to continue...";

        /// <summary>
        /// The message to display in the console.
        /// </summary>
        public string Text
        {
            get { return text; }
            set { text = value; }
        }

        private string userInput;

        /// <summary>
        /// The text entered at the console.
        /// </summary>
        [Output]
        public string UserInput
        {
            get { return userInput; }
        }

        /// <summary>
        /// When overridden in a derived class, executes the task.
        /// </summary>
        /// <returns>
        /// true if the task successfully executed; otherwise, false.
        /// </returns>
        public override bool Execute()
        {
            Console.WriteLine(text);
            userInput = Console.ReadLine();
            return true;
        }
    }
}
