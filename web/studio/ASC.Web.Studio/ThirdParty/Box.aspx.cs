/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using System;
using System.Threading;
using System.Web;
using ASC.FederatedLogin.Helpers;
using ASC.FederatedLogin.LoginProviders;
using ASC.Web.Core;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.ThirdParty
{
    public partial class Box : BasePage
    {
        public static string Location
        {
            get { return CommonLinkUtility.ToAbsolute("~/thirdparty/box.aspx"); }
        }

        private const string Source = "box";

        protected void Page_Load(object sender, EventArgs e)
        {
            var code = Request["code"];
            try
            {
                var error = Request["error"];
                if (!string.IsNullOrEmpty(error))
                {
                    if (error == "access_denied")
                    {
                        error = "Canceled at provider";
                    }
                    throw new Exception(error);
                }

                if (string.IsNullOrEmpty(code))
                {
                    OAuth20TokenHelper.RequestCode(HttpContext.Current,
                                                   BoxLoginProvider.BoxOauthCodeUrl,
                                                   BoxLoginProvider.BoxOAuth20ClientId,
                                                   BoxLoginProvider.BoxOAuth20RedirectUrl);
                }
                else
                {
                    Master.SubmitToken(code, Source);
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception ex)
            {
                Master.SubmitError(ex.Message, Source);
            }
        }
    }
}