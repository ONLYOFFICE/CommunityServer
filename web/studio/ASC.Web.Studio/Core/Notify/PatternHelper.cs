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


using System;
using ASC.Common.Utils;

namespace ASC.Web.Studio.Core.Notify
{
    public class PatternHelper
    {
        public string Unhtml(object htmlString)
        {
            if (htmlString == null || Convert.ToString(htmlString) == String.Empty)
                return "";

            var html = htmlString.ToString();
            try
            {
                return HtmlUtil.ToPlainText(html);
            }
            catch
            {
                return HtmlUtil.GetText(html);
            }
        }

        public string Right(object str, int count)
        {
            if (str == null || Convert.ToString(str) == String.Empty)
                return "";

            if (count > str.ToString().Length)
            {
                return str.ToString();
            }
            var cutTo = str.ToString().LastIndexOfAny(new[] { ' ', ',' }, count, count);
            if (cutTo == -1)
            {
                cutTo = count;
            }
            return str.ToString().Substring(0, cutTo);
        }

        public string Left(object str, int count)
        {
            if (str == null || Convert.ToString(str) == String.Empty)
                return "";

            if (count > str.ToString().Length)
            {
                return str.ToString();
            }
            var cutTo = str.ToString().IndexOfAny(new[] { ' ', ',' }, count);
            if (cutTo == -1)
            {
                cutTo = count;
            }
            return str.ToString().Substring(0, cutTo);
        }

    }
}