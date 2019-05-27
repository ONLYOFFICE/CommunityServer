using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace MSBuild.Community.Tasks.Git
{
    /// <summary>
    /// A task for git to get the current commit datetime.
    /// </summary>
    public class GitCommitDate : GitClient
    {
        public GitCommitDate()
        {
            Command = "show";
            Revision = "HEAD";
        }

        /// <summary>
        /// Gets or sets the revision to get the version from. Default is HEAD.
        /// </summary>
        public string Revision { get; set; }

        public string Format { get; set; }

        /// <summary>
        /// Gets or sets the commit datetime.
        /// </summary>
        [Output]
        public string CommitDate { get; set; }

        /// <summary>
        /// Generates the arguments.
        /// </summary>
        /// <param name="builder">The builder.</param>
        protected override void GenerateArguments(CommandLineBuilder builder)
        {
            builder.AppendSwitch("-s");
            builder.AppendSwitch("--format=%ci");

            base.GenerateArguments(builder);

            builder.AppendSwitch(Revision);
        }

		public override bool Execute()
		{
			bool result = base.Execute();
			if (result)
			{
				Parse();
			}
			return result;
		}

		private void Parse()
		{
			var stringBuilder = new StringBuilder();
			string consoleOutput = ConsoleOutput
				.Aggregate(stringBuilder, (sb, ti) => sb.AppendLine(ti.ItemSpec))
				.ToString();
			using (var reader = new StringReader(consoleOutput))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					DateTime dateTime;
					if (DateTime.TryParse(line, out dateTime))
					{
						CommitDate = dateTime.ToString(Format);
						Log.LogMessage(String.Format("CommitDate: {0}", CommitDate));
						return;
					}
				}
				if(String.IsNullOrEmpty(CommitDate))
					throw new FormatException(String.Format("wrong commit datetime string: '{0}'.", consoleOutput));
			}
		}
    }
}
