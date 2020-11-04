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


#region Usings

using ASC.Web.Core.WhiteLabel;
using ASC.Web.Studio.Utility;
using System;
using System.Web;

#endregion

namespace ASC.Web.Studio.HttpHandlers
{
    public class TenantLogoHandler : IHttpHandler
    {
        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            var type = context.Request["logotype"];
            if (string.IsNullOrEmpty(type)) return;

            var whiteLabelType = (WhiteLabelLogoTypeEnum)Convert.ToInt32(type);
            var general = Convert.ToBoolean(context.Request["general"] ?? "true");
            var isDefIfNoWhiteLabel = Convert.ToBoolean(context.Request["defifnoco"] ?? "false");

            var imgUrl = TenantLogoHelper.GetLogo(whiteLabelType, general, isDefIfNoWhiteLabel);

            context.Response.ContentType = "image";
            context.Response.Redirect(imgUrl);
        }
    }
}
