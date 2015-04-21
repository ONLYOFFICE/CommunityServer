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


#region Import

using System;
using ASC.CRM.Core;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Controls.Common;
using System.Web;

#endregion

namespace ASC.Web.CRM.Controls.Cases
{
    public partial class CasesFullCardView : BaseUserControl
    {
        #region Property

        public static string Location
        {
            get { return PathProvider.GetFileStaticRelativePath("Cases/CasesFullCardView.ascx"); }
        }

        public ASC.CRM.Core.Entities.Cases TargetCase { get; set; }

        #endregion

        #region Events


        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterClientScriptHelper.DataCasesFullCardView(Page, TargetCase);
            ExecHistoryView();
            RegisterScript();
        }

        #endregion

        #region Methods

        public void ExecHistoryView()
        {
            var historyViewControl = (HistoryView) LoadControl(HistoryView.Location);

            historyViewControl.TargetEntityType = EntityType.Case;
            historyViewControl.TargetEntityID = TargetCase.ID;
            historyViewControl.TargetContactID = 0;

            _phHistoryView.Controls.Add(historyViewControl);
        }

        private void RegisterScript()
        {
            var script = @"ASC.CRM.CasesFullCardView.init();";

            Page.RegisterInlineScript(script);
        }

        #endregion
    }
}