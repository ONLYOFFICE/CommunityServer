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

                var logoImg = "";
                if (isPromoTmpl) {
                    logoImg = "http://cdn.teamlab.com/media/newsletters/images/logo.png";
                } else {
                   logoImg = ConfigurationManager.AppSettings["web.logo.mail"];
                    if (String.IsNullOrEmpty(logoImg))
                    {
                        var logo = message.GetArgument("LetterLogo");
                        if (logo != null && (string)logo.Value != "")
                        {
                            logoImg = (string)logo.Value;
                        }
                        else
                        {
                            logoImg = "http://cdn.teamlab.com/media/newsletters/images/header_08.png";
                        }
                    }
                }


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


                var text = "";

                var noUnsubscribeLink = message.GetArgument("noUnsubscribeLink");
                if (noUnsubscribeLink == null || (string)noUnsubscribeLink.Value == "false")
                {
                    var isHosted = ConfigurationManager.AppSettings["core.payment-partners-hosted"];
                    if (String.IsNullOrEmpty(isHosted) || isHosted == "false")
                    {
                        var mail = message.Recipient.Addresses.FirstOrDefault(r => r.Contains("@"));
                        var domain = ConfigurationManager.AppSettings["web.teamlab-site"];
                        var site = string.IsNullOrEmpty(domain) ? "http://www.onlyoffice.com" : domain;
                        var link = site + string.Format("/Unsubscribe.aspx?id={0}", HttpServerUtility.UrlTokenEncode(Security.Cryptography.InstanceCrypto.Encrypt(Encoding.UTF8.GetBytes(mail.ToLowerInvariant()))));

                        text = string.Format(Resources.TemplateResource.TextForFooterWithUnsubscribe, link);
                    }
                }

                text += string.Format(Resources.TemplateResource.TextForFooter, DateTime.UtcNow.Year, string.Empty);
                message.Body = message.Body.Replace("%TEXTFOOTER%", text);
            }
        }


    }
}