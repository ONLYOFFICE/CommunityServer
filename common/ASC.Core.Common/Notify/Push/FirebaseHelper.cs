/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using System.Collections.Generic;

using ASC.Common.Logging;
using ASC.Core.Common.Notify.FireBase.Dao;
using ASC.Notify.Messages;

using FirebaseAdmin;
using FirebaseAdmin.Messaging;

using Google.Apis.Auth.OAuth2;

using Newtonsoft.Json;

namespace ASC.Core.Common.Notify.Push
{
    public class FirebaseHelper
    {
        private static FirebaseHelper _instance;
        public static FirebaseHelper Instance
        {
            get
            {
                if (_instance != null) return _instance;

                _instance = new FirebaseHelper();
                return _instance;
            }
        }

        private FirebaseHelper()
        {
            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromJson(JsonConvert.SerializeObject(new FirebaseApiKey()).Replace("\\\\", "\\"))
            });
        }

        public void SendMessage(NotifyMessage msg)
        {
            var user = CoreContext.UserManager.GetUserByUserName(msg.To);
            var fbDao = new FirebaseDao();

            Guid productID;

            if (!Guid.TryParse(msg.ProductID, out productID)) return;

            var fireBaseUser = new List<FireBaseUser>();

            if (productID == new Guid("{E67BE73D-F9AE-4ce1-8FEC-1880CB518CB4}")) //documents product
            {
                fireBaseUser = fbDao.GetUserDeviceTokens(user.ID, msg.Tenant, PushConstants.PushDocAppName);
            }

            if (productID == new Guid("{1e044602-43b5-4d79-82f3-fd6208a11960}")) //projects product
            {
                fireBaseUser = fbDao.GetUserDeviceTokens(user.ID, msg.Tenant, PushConstants.PushProjAppName);
            }

            foreach (var fb in fireBaseUser)
            {
                if ((bool)fb.IsSubscribed)
                {
                    var m = new Message()
                    {
                        Data = new Dictionary<string, string>{
                            { "data", msg.Data }
                        },
                        Token = fb.FirebaseDeviceToken,
                        Notification = new Notification()
                        {
                            Body = msg.Content
                        }
                    };
                    FirebaseMessaging.DefaultInstance.SendAsync(m);
                }
            }
        }
    }
}
