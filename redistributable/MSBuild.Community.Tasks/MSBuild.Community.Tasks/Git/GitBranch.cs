using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace MSBuild.Community.Tasks.Git
{

	/// <summary>
	/// A task to get the name of the branch or tag of git repository
	/// </summary>
	public class GitBranch : GitClient
	{

		/// <summary>
		/// Default constructor
		/// </summary>
		public GitBranch()
		{
			Command = "status";
			Arguments = "branch";
		}

		/// <summary>
		/// Return the branch or tag.
		/// </summary>
		[Output]
		public string Branch
		{
			get;
			set;
		}

		/// <summary>
		/// Parse the output of the console and gets the Branch or Tag.
		/// </summary>
		/// <param name="singleLine">the line being parsed</param>
		/// <param name="messageImportance">message importance</param>
		protected override void LogEventsFromTextOutput(string singleLine, MessageImportance messageImportance)
		{
			bool isError = messageImportance == StandardErrorLoggingImportance;

			if (isError)
				base.LogEventsFromTextOutput(singleLine, messageImportance);
			else if (IsBranchStatusLine(singleLine))
				Branch = ParseStatusLineOutput(singleLine);
		}

		/// <summary>
		/// Check if a stdout Git line shows the current branch
		/// </summary>
		/// <param name="singleLine">the stdout line</param>
		/// <returns>true if is branch line</returns>
		public bool IsBranchStatusLine(string singleLine)
		{
			return singleLine.StartsWith("# On branch") || singleLine.StartsWith("On branch");
		}

		/// <summary>
		/// Parses a branch status line and returns the branch or tag
		/// </summary>
		/// <param name="singleLine">the stdout line of git command line tool</param>
		/// <returns>the branch or tag</returns>
		public string ParseStatusLineOutput(string singleLine)
		{
			int indexOfBranch = singleLine.IndexOf("branch");
			int branchLength = "branch".Length;
			return singleLine.Substring(indexOfBranch + branchLength + 1, singleLine.Length - indexOfBranch - branchLength - 1).Trim();
		}

		/// <summary>
		/// Generates the arguments.
		/// </summary>
		/// <param name="builder">The builder.</param>
		protected override void GenerateArguments(CommandLineBuilder builder)
		{			
			base.GenerateArguments(builder);			
		}
	}
}
