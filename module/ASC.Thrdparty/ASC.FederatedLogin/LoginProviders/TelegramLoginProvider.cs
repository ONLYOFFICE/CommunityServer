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

using ASC.Core;
using ASC.Core.Common.Configuration;
using ASC.Core.Common.Notify;
using ASC.Core.Common.Notify.Telegram;

namespace ASC.FederatedLogin.LoginProviders
{
    public class TelegramLoginProvider : Consumer, IValidateKeysProvider, ITelegramLoginProvider
    {
        public string TelegramBotToken
        {
            get { return Instance["telegramBotToken"]; }
        }

        public string TelegramBotName
        {
            get { return Instance["telegramBotName"]; }
        }

        public int TelegramAuthTokenLifespan
        {
            get { return int.Parse(Instance["telegramAuthTokenLifespan"]); }
        }

        public string TelegramProxy
        {
            get { return Instance["telegramProxy"]; }
        }

        public static TelegramLoginProvider Instance
        {
            get { return ConsumerFactory.Get<TelegramLoginProvider>(); }
        }

        public bool IsEnabled()
        {
            return !string.IsNullOrEmpty(TelegramBotToken) && !string.IsNullOrEmpty(TelegramBotName);
        }

        public TelegramLoginProvider() { }

        public TelegramLoginProvider(string name, int order, Dictionary<string, string> props, Dictionary<string, string> additional = null)
            : base(name, order, props, additional)
        {
        }

        public bool ValidateKeys()
        {
            return TelegramHelper.Instance.CheckConnection(CoreContext.TenantManager.GetCurrentTenant().TenantId, TelegramBotToken, TelegramAuthTokenLifespan, TelegramProxy);
        }
    }
}
