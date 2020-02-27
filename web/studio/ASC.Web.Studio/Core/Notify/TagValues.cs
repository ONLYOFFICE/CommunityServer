/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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


using ASC.Notify.Patterns;
using System;

namespace ASC.Web.Studio.Core.Notify
{
    internal static class TagValues
    {
        public static ITagValue WithoutUnsubscribe()
        {
            return new TagValue(CommonTags.WithoutUnsubscribe, true);
        }

        public static ITagValue PersonalHeaderStart()
        {
            return new TagValue("PersonalHeaderStart",
                                "<table style=\"margin: 0; border-spacing: 0; empty-cells: show; width: 540px; width: 100%;\" cellspacing=\"0\" cellpadding=\"0\"><tbody><tr><td style=\"width: 100%;color: #333333;font-size: 18px;font-weight: bold;margin: 0;height: 71px;padding-right:165px;background: url('https://d2nlctn12v279m.cloudfront.net/media/newsletters/images/personal-header-bg.png') top right no-repeat;\">");
        }

        public static ITagValue PersonalHeaderEnd()
        {
            return new TagValue("PersonalHeaderEnd", "</td></tr></tbody></table>");
        }

        public static ITagValue BlueButton(Func<string> btnTextFunc, string btnUrl)
        {
            Func<string> action = () =>
                {
                    var btnText = btnTextFunc != null ? btnTextFunc() ?? string.Empty : string.Empty;

                    return
                        string.Format(
                            @"<table style=""height: 48px; width: 540px; border-collapse: collapse; empty-cells: show; vertical-align: middle; text-align: center; margin: 30px auto; padding: 0;""><tbody><tr cellpadding=""0"" cellspacing=""0"" border=""0"">{2}<td style=""height: 48px; width: 380px; margin:0; padding:0; background-color: #66b76d; -moz-border-radius: 2px; -webkit-border-radius: 2px; border-radius: 2px;""><a style=""{3}"" target=""_blank"" href=""{0}"">{1}</a></td>{2}</tr></tbody></table>",
                            btnUrl,
                            btnText,
                            "<td style=\"height: 48px; width: 80px; margin:0; padding:0;\">&nbsp;</td>",
                            "color: #fff; font-family: Helvetica, Arial, Tahoma; font-size: 18px; font-weight: 600; vertical-align: middle; display: block; padding: 12px 0; text-align: center; text-decoration: none; background-color: #66b76d;");
                };

            return new TagActionValue("BlueButton", action);
        }

        public static ITagValue GreenButton(Func<string> btnTextFunc, string btnUrl)
        {
            Func<string> action = () =>
                {
                    var btnText = btnTextFunc != null ? btnTextFunc() ?? string.Empty : string.Empty;

                    return
                        string.Format(
                            @"<table style=""height: 48px; width: 540px; border-collapse: collapse; empty-cells: show; vertical-align: middle; text-align: center; margin: 30px auto; padding: 0;""><tbody><tr cellpadding=""0"" cellspacing=""0"" border=""0"">{2}<td style=""height: 48px; width: 380px; margin:0; padding:0; background-color: #66b76d; -moz-border-radius: 2px; -webkit-border-radius: 2px; border-radius: 2px;""><a style=""{3}"" target=""_blank"" href=""{0}"">{1}</a></td>{2}</tr></tbody></table>",
                            btnUrl,
                            btnText,
                            "<td style=\"height: 48px; width: 80px; margin:0; padding:0;\">&nbsp;</td>",
                            "color: #fff; font-family: Helvetica, Arial, Tahoma; font-size: 18px; font-weight: 600; vertical-align: middle; display: block; padding: 12px 0; text-align: center; text-decoration: none; background-color: #66b76d;");
                };

            return new TagActionValue("GreenButton", action);
        }

        public static ITagValue TableTop()
        {
            return new TagValue("TableItemsTop",
                                "<table cellpadding=\"0\" cellspacing=\"0\" style=\"margin: 20px 0 0; border-spacing: 0; empty-cells: show; width: 540px; font-size: 14px;\">");
        }

        public static ITagValue TableBottom()
        {
            return new TagValue("TableItemsBtm", "</table>");
        }

        public static ITagValue TableItem(
            int number,
            Func<string> linkTextFunc,
            string linkUrl,
            string imgSrc,
            Func<string> commentFunc,
            Func<string> bottomlinkTextFunc,
            string bottomlinkUrl)
        {
            Func<string> action = () =>
                {
                    var linkText = linkTextFunc != null ? linkTextFunc() ?? string.Empty : string.Empty;
                    var comment = commentFunc != null ? commentFunc() ?? string.Empty : string.Empty;
                    var bottomlinkText = bottomlinkTextFunc != null
                                             ? bottomlinkTextFunc() ?? string.Empty
                                             : string.Empty;

                    var imgHtml = string.Format(
                        "<img style=\"border: 0; padding: 0 15px 0 5px; width: auto; height: auto;\" alt=\"{1}\" src=\"{0}\"/>",
                        imgSrc ?? string.Empty,
                        linkText);

                    var linkHtml = string.Empty;

                    if (!string.IsNullOrEmpty(linkText))
                    {
                        linkHtml =
                            !string.IsNullOrEmpty(linkUrl)
                                ? string.Format(
                                    "<a target=\"_blank\" style=\"color:#0078bd; font-family: Arial; font-size: 14px; font-weight: bold;\" href=\"{0}\">{1}</a><br/>",
                                    linkUrl,
                                    linkText)
                                : string.Format(
                                    "<div style=\"display:block; color:#333333; font-family: Arial; font-size: 14px; font-weight: bold;margin-bottom: 10px;\">{0}</div>",
                                    linkText);
                    }

                    var underCommentLinkHtml =
                        string.IsNullOrEmpty(bottomlinkUrl)
                            ? string.Empty
                            : string.Format(
                                "<br/><a target=\"_blank\" style=\"color: #0078bd; font-family: Arial; font-size: 14px;\" href=\"{0}\">{1}</a>",
                                bottomlinkUrl,
                                bottomlinkText);

                    var html =
                        "<tr><td style=\"vertical-align: top; padding: 5px 10px; width: 70px;\">" +
                        imgHtml +
                        "</td><td style=\" vertical-align: middle; padding: 5px 10px; font-size: 14px; width: 470px; color: #333333;\">" +
                        linkHtml +
                        comment +
                        underCommentLinkHtml +
                        "</td></tr>";

                    return html;
                };

            return new TagActionValue("TableItem" + number, action);
        }
    }
}