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
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;
using System.Web.UI;
using ASC.Web.Files.Classes;
using ASC.Web.Studio.Core.Quota;

namespace ASC.Web.Files.Controls
{
    public partial class ChunkUploadDialog : UserControl
    {
        public static string Location
        {
            get { return PathProvider.GetFileControlPath("ChunkUploadDialog/ChunkUploadDialog.ascx"); }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            InitScripts();
        }

        private void InitScripts()
        {
            var tenantQuota = GetQuotaString();

            var inlineScript = new StringBuilder();

            inlineScript.AppendFormat("ASC.Files.ChunkUploads.init({0});", tenantQuota);

            Page.RegisterInlineScript(inlineScript.ToString());
        }

        private static string GetQuotaString()
        {
            var quota = QuotaWrapper.GetCurrent();

            using (var ms = new MemoryStream())
            {
                var serializer = new DataContractJsonSerializer(typeof (QuotaWrapper));
                serializer.WriteObject(ms, quota);
                ms.Seek(0, SeekOrigin.Begin);

                return Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Length);
            }
        }
    }
}