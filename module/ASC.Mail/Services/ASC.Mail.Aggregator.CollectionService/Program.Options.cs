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


using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace ASC.Mail.Aggregator.CollectionService
{
    public class Options
    {
        [OptionList('u', "users", MetaValue = "STRING ARRAY", Required = false, HelpText = "An array of users for which the aggregator will take tasks. " +
                                                                                           "Separator = ';' " +
                                                                                           "Example: -u\"{tl_userId_1}\";\"{tl_userId_2}\";\"{tl_userId_3}\"", Separator = ';')]
        public IList<string> OnlyUsers { get; set; }

        [Option("console", Required = false, HelpText = "Console state")]
        public bool IsConsole { get; set; }

        [Option("unlimit", Required = false, HelpText = "Unlimit messages per mailbox session")]
        public bool NoMessagesLimit { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
