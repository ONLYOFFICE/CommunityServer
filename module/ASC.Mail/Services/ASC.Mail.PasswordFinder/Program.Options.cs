/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


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
