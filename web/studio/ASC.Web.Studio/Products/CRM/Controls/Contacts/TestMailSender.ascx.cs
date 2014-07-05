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

#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using ASC.CRM.Core;
using ASC.Common.Threading.Progress;
using ASC.Web.CRM.Classes;
using AjaxPro;


#endregion

namespace ASC.Web.CRM.Controls.Contacts
{
    [AjaxNamespace("AjaxPro.TestMailSender")]
    public partial class TestMailSender : BaseUserControl
    {

        public static string Location
        {
            get { return PathProvider.GetFileStaticRelativePath("Contacts/TestMailSender.ascx"); }
        }

        protected void Page_Load(object sender, EventArgs e)
        {

            Utility.RegisterTypeForAjax(typeof(TestMailSender));

        }

        [AjaxMethod]
        public IProgressItem Start()
        {

            var contactIDs = Global.DaoFactory.GetContactInfoDao()
            .GetList(0, ContactInfoType.Email, null, true).Select(item => item.ContactID);

            
            
            // return  MailSender.Start(null, new HashSet<int>(contactIDs), subjectTemplate, bodyTemplate);
            throw new NotImplementedException();

        }




        [AjaxMethod]
        public String TestTemplate()
        {
            var temp = new MailTemplateManager();

            return temp.Apply(@"
                                asdfasdf
asdfasdfasdf
asdfasdf
asdf
$(Person.First Name)
asd
f
asdf
as
dfas


                              ", 2328);

        //    throw new NotImplementedException();
            //    return temp.

        }


        [AjaxMethod]
        public IProgressItem GetStatus()
        {
            return MailSender.GetStatus();

        }
        
        [AjaxMethod]
        public IProgressItem Cancel()
        {
             MailSender.Cancel();

            return GetStatus();

        }

    }
}