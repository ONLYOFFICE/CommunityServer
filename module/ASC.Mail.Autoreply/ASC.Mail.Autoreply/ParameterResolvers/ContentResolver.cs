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


using System;
using ASC.Mail.Autoreply.Utility;
using ASC.Mail.Autoreply.Utility.Html;
using ASC.Mail.Net.Mail;
using HtmlAgilityPack;

namespace ASC.Mail.Autoreply.ParameterResolvers
{
    internal class HtmlContentResolver : IParameterResolver
    {
        public object ResolveParameterValue(Mail_Message mailMessage)
        {
            var messageText = !string.IsNullOrEmpty(mailMessage.BodyHtmlText)
                                  ? mailMessage.BodyHtmlText
                                  : Text2HtmlConverter.Convert(mailMessage.BodyText.Trim(' '));

            messageText = messageText.Replace(Environment.NewLine, "").Replace(@"\t", "");
            messageText = HtmlEntity.DeEntitize(messageText);
            messageText = HtmlSanitizer.Sanitize(messageText);

            return messageText.Trim("<br>").Trim("</br>").Trim(' ');
        }
    }

    internal class PlainTextContentResolver : IParameterResolver
    {
        public object ResolveParameterValue(Mail_Message mailMessage)
        {
            var messageText = new HtmlContentResolver().ResolveParameterValue(mailMessage) as string;

            if (!string.IsNullOrEmpty(messageText))
                messageText = Html2TextConverter.Convert(messageText);

            return messageText;
        }
    }
}
