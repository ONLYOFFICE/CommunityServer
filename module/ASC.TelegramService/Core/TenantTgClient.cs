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


using Telegram.Bot;

namespace ASC.TelegramService.Core
{
    public class TenantTgClient
    {
        public string Token { get; set; }
        public TelegramBotClient Client { get; set; }
        public string Proxy { get; set; }
        public int TokenLifeSpan { get; set; }
        public int TenantId { get; set; }
    }
}
