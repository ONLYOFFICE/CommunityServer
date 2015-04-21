/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
                SubmitError(ex.Message);
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