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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ASC.Web.Studio.UserControls.Common.ChooseTimePeriod
{
    public partial class ChooseTimePeriod : UserControl
    {
        public static string Location { get { return "~/usercontrols/common/choosetimeperiod/choosetimeperiod.ascx"; } }
        
        protected void Page_Load(object sender, EventArgs e)
        {
            Page.RegisterStyleControl(VirtualPathUtility.ToAbsolute("~/usercontrols/common/choosetimeperiod/css/choosetimeperiod.less"));
            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/common/choosetimeperiod/js/choosetimeperiod.js"));
        }
        
    }
    public static class TimePeriodInitialiser
    {
        public static string InitHoursCombobox()
        {
            var sb = new StringBuilder();

            for (var i = 0; i < 24; i++)
            {
                var value = string.Format(i < 10 ? "0{0}:00" : "{0}:00", i);

                sb.AppendFormat(
                    i == 12
                        ? "<option value='{0}' selected='selected'>{1}</option>"
                        : "<option value='{0}' >{1}</option>", i, value);
            }

            return sb.ToString();
        }
        public static string InitDaysOfWeek()
        {
            var sb = new StringBuilder();
            var format = Thread.CurrentThread.CurrentCulture.DateTimeFormat;
            //in cron expression week day 1-7 (not 0-6)
            var firstday = (int)format.FirstDayOfWeek;
            for (var i = firstday; i < firstday + 7; i++)
            {
                sb.AppendFormat("<option value='{0}'>{1}</option>", i % 7 + 1, format.GetDayName((DayOfWeek)(i % 7)));
            }
            return sb.ToString();
        }
        public static string InitDaysOfMonth()
        {
            var sb = new StringBuilder();
            for (var i = 1; i < 32; i++)
            {
                sb.AppendFormat("<option value='{0}'>{0}</option>", i);
            }
            return sb.ToString();
        }
    }
}