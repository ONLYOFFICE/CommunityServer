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
using System.Web;
using System.Text;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Configuration;

using ASC.Common.Notify.Patterns;
using ASC.Notify.Messages;
using ASC.Notify.Patterns;

using Textile;
using Textile.Blocks;


namespace ASC.Notify.Textile
{
    public class TextileStyler : IPatternStyler
    {
        private static readonly Regex VelocityArguments = new Regex(NVelocityPatternFormatter.NoStylePreffix + "(?<arg>.*?)" + NVelocityPatternFormatter.NoStyleSuffix, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

        static TextileStyler()
        {
            var file = "ASC.Notify.Textile.Resources.style.css";
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(file))
            using (var reader = new StreamReader(stream))
            {
                BlockAttributesParser.Styler = new StyleReader(reader.ReadToEnd().Replace("\n", "").Replace("\r", ""));
            }
        }

        public void ApplyFormating(NoticeMessage message)
        {
            bool isPromoTmpl = false;
            var output = new StringBuilderTextileFormatter();
            var formatter = new TextileFormatter(output);

            if (!string.IsNullOrEmpty(message.Subject))
            {
                message.Subject = VelocityArguments.Replace(message.Subject, m => m.Result("${arg}"));
            }

            if (!string.IsNullOrEmpty(message.Body))
            {
                formatter.Format(message.Body);

                var isPromo = message.GetArgument("isPromoLetter");
                if (isPromo != null && (string)isPromo.Value == "true")
                {
                    isPromoTmpl = true;
                }

                var logoMail = ConfigurationManager.AppSettings["web.logo.mail"];
                var logo = string.IsNullOrEmpty(logoMail) ? "http://cdn.teamlab.com/media/newsletters/images/header_05.jpg" : logoMail;
                var logoImg = isPromoTmpl ? "http://cdn.teamlab.com/media/newsletters/images/logo.png" : logoMail;
               
                var template = isPromoTmpl ? Resources.TemplateResource.HtmlMasterPromo : Resources.TemplateResource.HtmlMaster;
                message.Body = template.Replace("%CONTENT%", output.GetFormattedText()).Replace("%LOGO%", logoImg);

                var footer = message.GetArgument("WithPhoto");
                var partner = message.GetArgument("Partner");
                var res = String.Empty;

                if (partner != null) {
                    res = partner.Value.ToString();
                }

               
                if (String.IsNullOrEmpty(res) && footer != null)
                {
                    switch ((string)footer.Value)
                    {
                        case "photo":
                            res = Resources.TemplateResource.FooterWithPhoto;
                            break;
                        case "links":
                            res = Resources.TemplateResource.FooterWithLinks;
                            break;
                        case "personal":
                            res = Resources.TemplateResource.FooterPersonal;
                            break;
                        default:
                            res = String.Empty;
                            break;
                    }
                }
                message.Body = message.Body.Replace("%FOOTER%", res);

                var mail = message.Recipient.Addresses.FirstOrDefault(r => r.Contains("@"));
                var domain = ConfigurationManager.AppSettings["web.teamlab-site"];
                var site = string.IsNullOrEmpty(domain) ? "http://www.onlyoffice.com" : domain;
                var link = site + string.Format("/Unsubscribe.aspx?id={0}", HttpServerUtility.UrlTokenEncode(Security.Cryptography.InstanceCrypto.Encrypt(Encoding.UTF8.GetBytes(mail.ToLowerInvariant()))));
                var text = "";
                var isHosted = ConfigurationManager.AppSettings["core.payment-partners-hosted"];
                if (String.IsNullOrEmpty(isHosted) || isHosted == "false")
                {
                    text = string.Format(Resources.TemplateResource.TextForFooterWithUnsubscribe, link);
                }
                text += string.Format(Resources.TemplateResource.TextForFooter, DateTime.UtcNow.Year, string.Empty);
                message.Body = message.Body.Replace("%TEXTFOOTER%", text);
            }
        }


    }
}