/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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


using Microsoft.SharePoint.Client;

namespace ASC.Files.Thirdparty.SharePoint
{
    public class SharePointFileErrorEntry : File
    {
        public SharePointFileErrorEntry(ClientRuntimeContext cc, ObjectPath op)
            : base(cc, op)
        {
        }

        public string Error { get; set; }

        public object ID { get; set; }
    }

    public class SharePointFolderErrorEntry : Folder
    {
        public SharePointFolderErrorEntry(ClientRuntimeContext cc, ObjectPath op)
            : base(cc, op)
        {
        }

        public string Error { get; set; }

        public object ID { get; set; }
    }
}
