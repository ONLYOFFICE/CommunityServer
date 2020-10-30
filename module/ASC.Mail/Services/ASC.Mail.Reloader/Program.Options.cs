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

namespace ASC.Mail.Reloader
{
    internal partial class Program
    {
        private sealed class Options
        {
            [Option('i', "id", Required = false, DefaultValue = -1, HelpText = "Id mailbox from MySQL DB.")]
            public int MailboxId { get; set; }

            [Option('m', "mid", Required = false, DefaultValue = -1, HelpText = "Message id for reload.")]
            public int MessageId { get; set; }

            [Option('j', "json_path", Required = false, DefaultValue = null, HelpText = "Path to json file (ex. { \"data\": [ { \"id_mailbox\": 2649,\"id\": 27325207 } ] } ).")]
            public string PathToJson { get; set; }

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
