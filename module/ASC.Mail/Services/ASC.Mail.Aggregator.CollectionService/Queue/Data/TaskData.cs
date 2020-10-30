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


using System.Threading.Tasks;
using ASC.Mail.Data.Contracts;

namespace ASC.Mail.Aggregator.CollectionService.Queue.Data
{
    public class TaskData
    {
        public TaskData(MailBoxData mailBoxData, Task task)
        {
            Mailbox = mailBoxData;
            Task = task;
        }

        public MailBoxData Mailbox { get; private set; }

        public Task Task { get; private set; }
    }
}
