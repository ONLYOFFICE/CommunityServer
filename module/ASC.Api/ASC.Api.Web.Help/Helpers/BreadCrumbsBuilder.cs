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