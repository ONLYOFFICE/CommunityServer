/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System.Text;
using System.Xml;
using ASC.Core.Users;
using System.Web;

namespace ASC.Web.Studio.Core.Users
{
    public static class SocialContactsManager
    {
        public const string ContactType_mail = "mail";
        public const string ContactType_facebook = "facebook";
        public const string ContactType_myspace = "myspace";
        public const string ContactType_livejournal = "livejournal";
        public const string ContactType_twitter = "twitter";
        public const string ContactType_yahoo = "yahoo";
        public const string ContactType_jabber = "jabber";
        public const string ContactType_blogger = "blogger";
        public const string ContactType_skype = "skype";
        public const string ContactType_msn = "msn";
        public const string ContactType_aim = "aim";
        public const string ContactType_icq = "icq";
        public const string ContactType_gmail = "gmail";
        public const string ContactType_gbuzz = "gbuzz";
        public const string ContactType_gtalk = "gtalk";
        public const string ContactType_phone = "phone";
        public const string ContactType_mobphone = "mobphone"; 


        public static XmlDocument xmlSocialContacts
        {
            get
            {
                StringBuilder xml = new StringBuilder();
                xml.Append("<!DOCTYPE contacts[<!ELEMENT contact ANY><!ATTLIST contact id ID #REQUIRED>]>");
                xml.Append("<contacts>");

                xml.Append("<contact id=\"mail\" title=\"" + Resources.Resource.TitleEmail + "\">");
                xml.Append("<pattern>" + Resources.Resource.HintEmail + "</pattern>");
                xml.Append("<template><a class=\"label mail\" title=\"{0}\" href=\"" + VirtualPathUtility.ToAbsolute("~/addons/mail/#composeto/email={0}") + "\" target=\"_blank\"><span class=\"inner-text\">{0}</span></a></template>");
                xml.Append("</contact>");

                xml.Append("<contact id=\"facebook\" title=\"" + Resources.Resource.TitleFacebook + "\">");
                xml.Append("<pattern>" + Resources.Resource.HintFacebook + "</pattern>");
                xml.Append("<template><a class=\"label facebook\" href=\"http://facebook.com/{0}\" target=\"_blank\"><span class=\"inner-text\">{0}</span></a></template>");
                xml.Append("</contact>");

                xml.Append("<contact id=\"myspace\" title=\"" + Resources.Resource.TitleMyspace + "\">");
                xml.Append("<pattern>" + Resources.Resource.HintMyspace + "</pattern>");
                xml.Append("<template><a class=\"label myspace\" href=\"http://myspace.com/{0}\" target=\"_blank\"><span class=\"inner-text\">{0}</span></a></template>");
                xml.Append("</contact>");

                xml.Append("<contact id=\"livejournal\" title=\"" + Resources.Resource.TitleLiveJournal + "\">");
                xml.Append("<pattern>" + Resources.Resource.HintLivejournal + "</pattern>");
                xml.Append("<template><a class=\"label livejournal\" href=\"http://{0}.livejournal.com\" target=\"_blank\"><span class=\"inner-text\">{0}</span></a></template>");
                xml.Append("</contact>");

                xml.Append("<contact id=\"twitter\" title=\"" + Resources.Resource.TitleTwitter + "\">");
                xml.Append("<pattern>" + Resources.Resource.HintTwitter + "</pattern>");
                xml.Append("<template><a class=\"label twitter\" href=\"http://twitter.com/{0}/\" target=\"_blank\"><span class=\"inner-text\">{0}</span></a></template>");
                xml.Append("</contact>");

                xml.Append("<contact id=\"yahoo\" title=\"" + Resources.Resource.TitleYahoo + "\">");
                xml.Append("<pattern>" + Resources.Resource.HintYahoo + "</pattern>");
                xml.Append("<template><a class=\"label yahoo\" href=\"" + VirtualPathUtility.ToAbsolute("~/addons/mail/#composeto/email={0}@yahoo.com") + "\" target=\"_blank\"><span class=\"inner-text\">{0}</span></a></template>");
                xml.Append("</contact>");

                xml.Append("<contact id=\"jabber\" title=\"" + Resources.Resource.TitleJabber + "\">");
                xml.Append("<pattern>" + Resources.Resource.HintJabber + "</pattern>");
                xml.Append("<template><span class=\"label jabber\"><span class=\"inner-text\">{0}</span></span></template>");
                xml.Append("</contact>");

                xml.Append("<contact id=\"blogger\" title=\"" + Resources.Resource.TitleBlogger + "\">");
                xml.Append("<pattern>" + Resources.Resource.HintBlogger + "</pattern>");
                xml.Append("<template><a class=\"label blogger\" href=\"http://{0}.blogspot.com\" target=\"_blank\"><span class=\"inner-text\">{0}</span></a></template>");
                xml.Append("</contact>");

                xml.Append("<contact id=\"skype\" title=\"" + Resources.Resource.TitleSkype + "\">");
                xml.Append("<pattern>" + Resources.Resource.HintSkype + "</pattern>");
                xml.Append("<template><span class=\"label skype\"><span class=\"inner-text\">{0}</span></span></template>");
                xml.Append("</contact>");

                xml.Append("<contact id=\"msn\" title=\"" + Resources.Resource.TitleMsn + "\">");
                xml.Append("<pattern>" + Resources.Resource.HintMsn + "</pattern>");
                xml.Append("<template><span class=\"label msn\"><span class=\"inner-text\">{0}</span></span></template>");
                xml.Append("</contact>");

                xml.Append("<contact id=\"aim\" title=\"" + Resources.Resource.TitleAim + "\">");
                xml.Append("<pattern>" + Resources.Resource.HintAim + "</pattern>");
                xml.Append("<template><span class=\"label aim\"><span class=\"inner-text\">{0}</span></span></template>");
                xml.Append("</contact>");

                xml.Append("<contact id=\"icq\" title=\"" + Resources.Resource.TitleIcq + "\">");
                xml.Append("<pattern>" + Resources.Resource.HintIcq + "</pattern>");
                xml.Append("<template><a class=\"label icq\" href=\"http://www.icq.com/people/{0}\" target=\"_blank\"><span class=\"inner-text\">{0}</span></a></template>");
                xml.Append("</contact>");

                xml.Append("<contact id=\"gmail\" title=\"" + Resources.Resource.TitleGooglemail + "\">");
                xml.Append("<pattern>" + Resources.Resource.HintGooglemail + "</pattern>");
                xml.Append("<template><a class=\"label gmail\" href=\"" + VirtualPathUtility.ToAbsolute("~/addons/mail/#composeto/email={0}") + "\" target=\"_blank\"><span class=\"inner-text\">{0}</span></a></template>");
                xml.Append("</contact>");

                xml.Append("<contact id=\"gbuzz\" title=\"" + Resources.Resource.TitleGooglebuzz + "\">");
                xml.Append("<pattern>" + Resources.Resource.HintGooglebuzz + "</pattern>");
                xml.Append("<template><a class=\"label gbuzz\" href=\"{0}\" target=\"_blank\"><span class=\"inner-text\">{0}</span></a></template>");
                xml.Append("</contact>");

                xml.Append("<contact id=\"gtalk\" title=\"" + Resources.Resource.TitleGoogletalk + "\">");
                xml.Append("<pattern>" + Resources.Resource.HintGoggletalk + "</pattern>");
                xml.Append("<template><span class=\"label gtalk\"><span class=\"inner-text\">{0}</span></span></template>");
                xml.Append("</contact>");

                xml.Append("<contact id=\"phone\" title=\"" + Resources.Resource.TitlePhone + "\">");
                xml.Append("<pattern>" + Resources.Resource.HintPhone + "</pattern>");
                xml.Append("<template><span class=\"label phone\"><span class=\"inner-text\">{0}</span></span></template>");
                xml.Append("</contact>");

                xml.Append("<contact id=\"mobphone\" title=\"" + Resources.Resource.TitleMobphone + "\">");
                xml.Append("<pattern>" + Resources.Resource.HintMobphone + "</pattern>");
                xml.Append("<template><span class=\"label mobphone\"><span class=\"inner-text\">{0}</span></span></template>");
                xml.Append("</contact>");

                xml.Append("</contacts>");

                XmlDocument sc = new XmlDocument();
                sc.LoadXml(xml.ToString());

                return sc;
            }
        }

        public static void AddSocialContact(this UserInfo user, string contactType, string value)
        {
            user.Contacts.Add(contactType);
            user.Contacts.Add(value);
        }

    }
}
