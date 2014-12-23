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
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Web;
using ASC.FederatedLogin.LoginProviders;
using ASC.Thrdparty.Configuration;

namespace ASC.Thrdparty.Web.Google
{
    public partial class GoogleOAuthConnect : BaseImportPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                var scope = KeyStorage.Get("mail.googleScopes");
                var token = GoogleLoginProvider.Auth(HttpContext.Current, scope);

                var email = GetEmail(token.AccessToken);

                var emailInfo = new EmailAccessInfo
                    {
                        Email = email,
                        RefreshToken = token.RefreshToken
                    };
                SubmitEmailInfo(emailInfo);
            }
            catch (ThreadAbortException)
            {

            }
            catch (Exception ex)
            {
                ErrorScope = ex.Message;
                SubmitEmailInfo(new EmailAccessInfo());
            }
        }

        protected string GetEmail(string accessToken)
        {
            var email = "";

            if (!string.IsNullOrEmpty(accessToken))
            {
                var url = String.Format("https://www.googleapis.com/oauth2/v1/userinfo?access_token={0}", accessToken);

                var requestUserInfo = (HttpWebRequest)WebRequest.Create(url);
                var responseUserInfo = (HttpWebResponse)requestUserInfo.GetResponse();
                if (responseUserInfo.StatusCode == HttpStatusCode.OK)
                {
                    using (var receiveStream = responseUserInfo.GetResponseStream())
                    {
                        var encode = Encoding.GetEncoding("utf-8");
                        if (receiveStream != null)
                            using (var readStream = new StreamReader(receiveStream, encode))
                            {
                                var userInfo = Deserialise<GoogleUserInfo>(readStream.ReadToEnd());
                                responseUserInfo.Close();
                                readStream.Close();

                                if (!string.IsNullOrEmpty(userInfo.email)) email = userInfo.email;
                            }
                    }
                }
                else
                {
                    responseUserInfo.Close();
                }
            }

            return email;
        }

        public static T Deserialise<T>(string json)
        {
            using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(json)))
            {
                var serialiser = new DataContractJsonSerializer(typeof(T));
                return (T)serialiser.ReadObject(ms);
            }
        }
    }
}