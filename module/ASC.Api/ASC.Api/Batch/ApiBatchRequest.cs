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


using System.Net.Mime;
using System.Text;

namespace ASC.Api.Batch
{
    public class ApiBatchRequest
    {
        public ApiBatchRequest()
        {
            //Defaults
            BodyContentType = new ContentType("application/x-www-form-urlencoded"){CharSet = Encoding.UTF8.WebName}.ToString();
            Method = "GET";
        }

        public int Order { get; set; }

        public string[] After { get; set; }
        public string[] Before { get; set; }

        public string RelativeUrl { get; set; }

        public string Name { get; set; }

        private string _method;
        public string Method
        {
            get { return string.IsNullOrEmpty(_method)?"GET":_method; }
            set { _method = value; }
        }

        public string Body { get; set; }

        public string BodyContentType { get; set; }
    }
}