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


using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using ASC.Api.Web.Help.DocumentGenerator;

namespace ASC.Api.Web.Help.Helpers
{
    public class BreadCrumbsBuilder
    {
        private readonly Controller _context;


        public class BreadCrumb
        {
            public string Text { get; set; }
            public string Link { get; set; }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != typeof (BreadCrumb)) return false;
                return Equals((BreadCrumb) obj);
            }

            public bool Equals(BreadCrumb other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Equals(other.Text, Text) && Equals(other.Link, Link);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((Text != null ? Text.GetHashCode() : 0)*397) ^ (Link != null ? Link.GetHashCode() : 0);
                }
            }
        }

        public List<BreadCrumb> BreadCrumbs { get; set; }

        public BreadCrumbsBuilder(Controller context)
        {
            _context = context;
        }


        public void Add(string text, string link)
        {
            if (_context.ViewData["breadcrumbs"] == null)
            {
                _context.ViewData["breadcrumbs"] = BreadCrumbs = new List<BreadCrumb>();
            }

            var breadCrumb = new BreadCrumb
                                 {
                                     Link = link,
                                     Text = text
                                 };

            if (!BreadCrumbs.Contains(breadCrumb))
            {
                BreadCrumbs.Add(breadCrumb);
            }
        }

        public void Add(string text, MsDocEntryPoint section, MsDocEntryPointMethod method, object controller)
        {
            Add(text, Url.GetDocUrl(section, method, controller, _context.ControllerContext.RequestContext));
        }

        public void Add(string text, string section, string category, string type, string path, object controller)
        {
            Add(text, Url.GetDocUrl(section, category, type, path, controller, _context.ControllerContext.RequestContext));
        }
    }
}