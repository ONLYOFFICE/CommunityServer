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


using System;

namespace ASC.Mail.Core.Entities
{
    public class ServerDns
    {
        public int Id { get; set; }
        public int Tenant { get; set; }
        public string User { get; set; }
        public int DomainId { get; set; }

        public string DomainCheck { get; set; }
        
        public string DkimSelector { get; set; }
        public string DkimPrivateKey { get; set; }
        public string DkimPublicKey { get; set; }
        public int DkimTtl { get; set; }
        public bool DkimVerified { get; set; }
        public DateTime? DkimDateChecked { get; set; }

        public string Spf { get; set; }
        public int SpfTtl { get; set; }
        public bool SpfVerified { get; set; }
        public DateTime? SpfDateChecked { get; set; }

        public string Mx { get; set; }
        public int MxTtl { get; set; }
        public bool MxVerified { get; set; }
        public DateTime? MxDateChecked { get; set; }

        public DateTime TimeModified { get; set; }
    }
}
