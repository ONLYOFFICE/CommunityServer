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
using ASC.Web.Core.Helpers;
using ASC.Web.Projects.Resources;

namespace ASC.Web.Projects.Controls.Common
{
    public partial class AddMilestoneContainer : BaseUserControl
    {
        public bool Edit { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.RegisterBodyScripts("~/products/projects/js/addmilestonecontainer.js")
                .RegisterStyle("~/products/projects/app_themes/default/css/addmilestonecontainer.css");
        }

        protected string ChooseMonthNumeralCase(double count)
        {
            return count == 0 ? string.Empty : count + " " + ChooseNumeralCase(count, GrammaticalResource.MonthNominative,
                GrammaticalResource.MonthGenitiveSingular, GrammaticalResource.MonthGenitivePlural);
        }
        protected static string ChooseNumeralCase(double number, string nominative, string genitiveSingular, string genitivePlural)
        {
            if (number == 0.5)
            {
                if (
                    String.Compare(System.Threading.Thread.CurrentThread.CurrentUICulture.ThreeLetterISOLanguageName, "rus", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return genitiveSingular;
                }
            }
            if (number == 1)
            {
                return nominative;
            }
            var count = (int)number;
            return GrammaticalHelper.ChooseNumeralCase(count, nominative, genitiveSingular, genitivePlural);
        }
    }
}