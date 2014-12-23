/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using DotNetOpenAuth.ApplicationBlock;
using DotNetOpenAuth.OAuth;

namespace ASC.Thrdparty.Web.Yahoo
{
    public partial class YahooImport : YahooBasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {

            
            if (!IsPostBack)
            {
                //if (AccessToken!=null)
                //{
                //    Response.Redirect("YahooAddressBook.aspx");
                //}

                var yahoo = new WebConsumer(YahooConsumer.ServiceDescription, this.TokenManager);

                // Is Google calling back with authorization?
                var accessTokenResponse = yahoo.ProcessUserAuthorization();
                if (accessTokenResponse != null)
                {
                    this.AccessToken = accessTokenResponse.AccessToken;
                    this.YahooGuid = accessTokenResponse.ExtraData["xoauth_yahoo_guid"];
                    //Redirecting to result page
                    Response.Redirect("YahooAddressBook.aspx");
                }
                //else if (this.AccessToken == null)
                //{
                    // If we don't yet have access, immediately request it.
                    YahooConsumer.RequestAuthorization(yahoo);
                //}
            }
            }
            catch (System.Threading.ThreadAbortException)
            {

            }
            catch (Exception ex)
            {
                ErrorScope = ex.Message;
                SubmitData();
            }
        }
    }
}
