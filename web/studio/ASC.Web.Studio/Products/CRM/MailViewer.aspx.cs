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


#region Import

using System.Text;
using ASC.CRM.Core;
using ASC.Web.CRM.Classes;
using ASC.Web.Studio.Utility;
using Newtonsoft.Json;
using System.Web;

#endregion

namespace ASC.Web.CRM
{
    public partial class MailViewer : BasePage
    {
        #region Properies

        #endregion

        #region Events

        protected override void PageLoad()
        {
            int eventID;
            if (int.TryParse(UrlParameters.ID, out eventID))
            {
                var targetEvent = DaoFactory.RelationshipEventDao.GetByID(eventID);

                //Title = HeaderStringHelper.GetPageTitle("");

                if (targetEvent == null || !CRMSecurity.CanAccessTo(targetEvent) || targetEvent.CategoryID != (int)HistoryCategorySystem.MailMessage)
                    Response.Redirect(PathProvider.StartURL());

                var sb = new StringBuilder();
                sb.AppendFormat(@"ASC.CRM.HistoryMailView.init(""{0}"");", Global.EncodeTo64(JsonConvert.SerializeObject(targetEvent)));
                Page.RegisterInlineScript(sb.ToString());
            }
            else
            {
                Response.Redirect(PathProvider.StartURL());
            }
        }

        #endregion

    }
}