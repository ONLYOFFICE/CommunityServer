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
using System.Web;
using ASC.Web.Core.Client.HttpHandlers;
using ASC.Web.Sample.Resources;

namespace ASC.Web.Sample.Masters.ClientScripts
{
    public class ClientLocalizationResources : ClientScriptLocalization
    {
        protected override string BaseNamespace
        {
            get { return "ASC.Sample.Resources"; }
        }

        protected override IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context)
        {
            return new List<KeyValuePair<string, object>>(1)
                {
                    RegisterResourceSet("SampleResource", SampleResource.ResourceManager)
                };
        }
    }
}