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
using ASC.Web.Core.WhiteLabel;
using Textile;
using Textile.Blocks;
using ASC.Notify.Textile.Resources;

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
            var template = NotifyTemplateResource.HtmlMaster;
            var isPersonalTmpl = false;

            var output = new StringBuilderTextileFormatter();
            var formatter = new TextileFormatter(output);

            if (!string.IsNullOrEmpty(message.Subject))
            {
                message.Subject = VelocityArguments.Replace(message.Subject, m => m.Result("${arg}"));
            }

            if (string.IsNullOrEmpty(message.Body)) return;

            formatter.Format(message.Body);

            var isPersonal = message.GetArgument("IsPersonal");
            if (isPersonal != null && (string)isPersonal.Value == "true")
            {
                isPersonalTmpl = true;
            }

            var templateTag = message.GetArgument("MasterTemplate");
            if (templateTag != null)
            {
                var templateTagValue = templateTag.Value as string;
                if (!string.IsNullOrEmpty(templateTagValue))
                {
                    var templateValue = NotifyTemplateResource.ResourceManager.GetString(templateTagValue);
                    if (!string.IsNullOrEmpty(templateValue))
                        template = templateValue;
                }
            }

            string logoImg;

            if (isPersonalTmpl)
            {
                logoImg = "https://static.onlyoffice.com/media/newsletters/images/mail_logo.png";
            }
            else
            {
                logoImg = ConfigurationManager.AppSettings["web.logo.mail"];
                if (String.IsNullOrEmpty(logoImg))
                {
                    var logo = message.GetArgument("LetterLogo");
                    if (logo != null && (string) logo.Value != "")
                    {
                        logoImg = (string) logo.Value;
                    }
                    else
                    {
                        logoImg = "https://static.onlyoffice.com/media/newsletters/images/mail_logo.png";
                    }
                }
            }

            var logoText = ConfigurationManager.AppSettings["web.logotext.mail"];
            if (String.IsNullOrEmpty(logoText))
            {
                var llt = message.GetArgument("LetterLogoText");
                if (llt != null && (string)llt.Value != "")
                {
                    logoText = (string)llt.Value;
                }
                else
                {
                    logoText = TenantWhiteLabelSettings.DefaultLogoText;
                }
            }

            var mailWhiteLabelTag = message.GetArgument("MailWhiteLabelSettings");
            var mailWhiteLabelSettings = mailWhiteLabelTag == null ? null : mailWhiteLabelTag.Value as MailWhiteLabelSettings;

            message.Body = template.Replace("%CONTENT%", output.GetFormattedText())
                                   .Replace("%LOGO%", logoImg)
                                   .Replace("%LOGOTEXT%", logoText)
                                   .Replace("%SITEURL%", mailWhiteLabelSettings == null ? MailWhiteLabelSettings.DefaultMailSiteUrl : mailWhiteLabelSettings.SiteUrl);

            var footer = message.GetArgument("Footer");
            var partner = message.GetArgument("Partner");

            var footerContent = string.Empty;
            var footerSocialContent = string.Empty;

            if (partner != null) {
                footerContent = partner.Value.ToString();
            }

            if (String.IsNullOrEmpty(footerContent) && footer != null)
            {
                switch ((string)footer.Value)
                {
                    case "common":
                        InitCommonFooter(mailWhiteLabelSettings, out footerContent, out footerSocialContent);
                        break;
                    case "personal":
                        footerSocialContent = NotifyTemplateResource.FooterSocial;
                        break;
                    case "freecloud":
                        footerContent = NotifyTemplateResource.FooterFreeCloud;
                        footerSocialContent = NotifyTemplateResource.FooterSocial;
                        break;
                    case "opensource":
                        footerContent = NotifyTemplateResource.FooterOpensource;
                        footerSocialContent = NotifyTemplateResource.FooterSocial;
                        break;
                }
            }

            message.Body = message.Body
                                  .Replace("%FOOTER%", footerContent)
                                  .Replace("%FOOTERSOCIAL%", footerSocialContent);

            var text = "";

            if (ConfigurationManager.AppSettings["core.base-domain"] != "localhost")
            {
                var noUnsubscribeLink = message.GetArgument("noUnsubscribeLink");
                if (noUnsubscribeLink == null || (string) noUnsubscribeLink.Value == "false")
                {
                    var isHosted = ConfigurationManager.AppSettings["core.payment-partners-hosted"];
                    if (String.IsNullOrEmpty(isHosted) || isHosted == "false")
                    {
                        var mail = message.Recipient.Addresses.FirstOrDefault(r => r.Contains("@"));
                        var domain = ConfigurationManager.AppSettings["web.teamlab-site"];
                        var site = string.IsNullOrEmpty(domain) ? "http://www.onlyoffice.com" : domain;
                        var link = site +
                                   string.Format("/Unsubscribe.aspx?id={0}",
                                                 HttpServerUtility.UrlTokenEncode(
                                                     Security.Cryptography.InstanceCrypto.Encrypt(
                                                         Encoding.UTF8.GetBytes(mail.ToLowerInvariant()))));

                        text = string.Format(NotifyTemplateResource.TextForFooterWithUnsubscribe, link);
                    }
                }

                text += string.Format(NotifyTemplateResource.TextForFooter, DateTime.UtcNow.Year, string.Empty);
            }

            message.Body = message.Body.Replace("%TEXTFOOTER%", text);
        }

        private static void InitCommonFooter(MailWhiteLabelSettings settings, out string footerContent, out string footerSocialContent)
        {
            footerContent = String.Empty;
            footerSocialContent = String.Empty;
            
            if (settings == null)
            {
                footerContent =
                    NotifyTemplateResource.FooterCommon
                                          .Replace("%SUPPORTURL%", MailWhiteLabelSettings.DefaultMailSupportUrl)
                                          .Replace("%SALESEMAIL%", MailWhiteLabelSettings.DefaultMailSalesEmail)
                                          .Replace("%DEMOURL%", MailWhiteLabelSettings.DefaultMailDemotUrl);
                footerSocialContent = NotifyTemplateResource.FooterSocial;

            }
            else if (settings.FooterEnabled)
            {
                footerContent =
                    NotifyTemplateResource.FooterCommon
                    .Replace("%SUPPORTURL%", String.IsNullOrEmpty(settings.SupportUrl) ? "mailto:" + settings.SalesEmail : settings.SupportUrl)
                    .Replace("%SALESEMAIL%", settings.SalesEmail)
                    .Replace("%DEMOURL%", String.IsNullOrEmpty(settings.DemotUrl) ? "mailto:" + settings.SalesEmail : settings.DemotUrl);
                footerSocialContent = settings.FooterSocialEnabled ? NotifyTemplateResource.FooterSocial : String.Empty;
            }
        }
    }
}