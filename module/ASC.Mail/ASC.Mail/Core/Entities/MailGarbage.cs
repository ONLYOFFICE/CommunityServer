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


using ASC.Mail.Data.Storage;

namespace ASC.Mail.Core.Entities
{
    public abstract class MailGarbage
    {
        public abstract int Id { get; }
        public abstract string Path { get; }
    }

    public class MailAttachGarbage : MailGarbage
    {
        private readonly int _id;
        public override int Id {
            get { return _id; }
        }

        private readonly string _path;
        public override string Path {
            get { return _path; }
        }

        public MailAttachGarbage(string user, int attachId, string stream, int number, string storedName)
        {
            _id = attachId;
            _path = MailStoragePathCombiner.GetFileKey(user, stream, number, storedName);
        }
    }

    public class MailMessageGarbage : MailGarbage
    {
        private readonly int _id;
        public override int Id
        {
            get { return _id; }
        }

        private readonly string _path;
        public override string Path
        {
            get { return _path; }
        }

        public MailMessageGarbage(string user, int id, string stream)
        {
            _id = id;
            _path = MailStoragePathCombiner.GetBodyKey(user, stream);
        }
    }
}
