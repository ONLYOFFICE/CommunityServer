/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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

namespace ASC.Web.Studio.Core.Notify
{
    static class Tags
    {
        public const string UserName = "UserName";
        public const string UserLastName = "UserLastName";
        public const string UserEmail = "UserEmail";
        public const string UserPosition = "Position";
        public const string Phone = "Phone";
        public const string Website = "Website";
        public const string CompanyTitle = "CompanyTitle";
        public const string CompanySize = "CompanySize";
        public const string Subject = "Subject";
        public const string Body = "Body";
        public const string MyStaffLink = "MyStaffLink";
        public const string SettingsLink = "SettingsLink";
        public const string InviteLink = "InviteLink";
        public const string Date = "Date";
        public const string IP = "IP";
        public const string WebStudioLink = "WebStudioLink";
        public const string AuthorLink = "AuthorLink";
        public const string Activities = "Activities";
        public const string BackupUrl = "BackupUrl";
        public const string BackupHours = "BackupHours";

        public const string DeactivateUrl = "DeactivateUrl";
        public const string ActivateUrl = "ActivateUrl";
        public const string DeleteUrl = "DeleteUrl";
        public const string AutoRenew = "AutoRenew";
        public const string OwnerName = "OwnerName";
        public const string RegionName = "RegionName";

        public const string ActiveUsers = "ActiveUsers";
        public const string Price = "Price";
        public const string PricePeriod = "PricePeriod";
        public const string PortalUrl = "PortalUrl";
        public const string UserDisplayName = "UserDisplayName";
        public const string PricingPage = "PricingPage";
        public const string BlogLink = "TagBlogLink";
        public const string DueDate = "DueDate";
        public const string DelayDueDate = "DelayDueDate";

        public const string FromUserName = "FromUserName";
        public const string FromUserLink = "FromUserLink";
        public const string ToUserName = "ToUserName";
        public const string ToUserLink = "ToUserLink";
        public const string Message = "Message";

        public const string Coupon = "Coupon";

        public const string Address = "Address";
        public const string Login = "Login";
        public const string Server = "Server";
        public const string Encryption = "Encryption";
        public const string ImapPort = "ImapPort";
        public const string SmtpPort = "SmtpPort";
    }

    public sealed class CommonTags
    {
        public const string VirtualRootPath = "__VirtualRootPath";

        public const string ProductID = "__ProductID";

        public const string ModuleID = "__ModuleID";

        public const string ProductUrl = "__ProductUrl";

        public const string DateTime = "__DateTime";

        public const string AuthorID = "__AuthorID";

        public const string AuthorName = "__AuthorName";

        public const string SendFrom = "MessageFrom";

        public const string AuthorUrl = "__AuthorUrl";

        public const string RecipientID = "__RecipientID";

        public const string ProfileUrl = "ProfileUrl";

        public const string RecipientSubscriptionConfigURL = "RecipientSubscriptionConfigURL";

        public const string Priority = "Priority";

        public const string Culture = "Culture";

        public static string Footer = "Footer";

        public static string MasterTemplate = "MasterTemplate";

        public const string HelpLink = "__HelpLink";

        public const string WithoutUnsubscribe = "WithoutUnsubscribe";

        public const string LetterLogo = "LetterLogo";

        public const string LetterLogoText = "LetterLogoText";

        public const string MailWhiteLabelSettings = "MailWhiteLabelSettings";

        public const string EmbeddedAttachments = "EmbeddedAttachments";

        public const string Analytics = "Analytics";

        public const string ImagePath = "ImagePath";
    }
}
