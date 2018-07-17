using CommandLine;
using CommandLine.Text;

namespace ASC.Mail.PasswordFinder
{
    partial class Program
    {
        private sealed class Options
        {
            [Option('j', "json", Required = false, HelpText = "Save settings to mailbox.json")]
            public bool NeedJson { get; set; }

            [Option('e', "exit", Required = false, HelpText = "Exit on end of any operations")]
            public bool ExitOnEnd { get; set; }

            [ParserState]
            public IParserState LastParserState { get; set; }

            [HelpOption]
            public string GetUsage()
            {
                return HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
            }
        }
    }
}
