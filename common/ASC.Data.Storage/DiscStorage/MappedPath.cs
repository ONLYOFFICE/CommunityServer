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
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ASC.Data.Storage.DiscStorage
{
    class MappedPath
    {
        public string PhysicalPath { get; set; }

        public Uri VirtualPath { get; set; }


        private MappedPath()
        {
        }

        public MappedPath(string tenant, bool appendTenant, string ppath, string vpath, IDictionary<string, string> storageConfig)
        {
            tenant = tenant.Trim('/');
            vpath = PathUtils.ResolveVirtualPath(vpath, false);
            vpath = !vpath.Contains('{') && appendTenant ? string.Format("{0}/{1}", vpath, tenant) : string.Format(vpath, tenant);
            VirtualPath = new Uri(vpath + "/", UriKind.RelativeOrAbsolute);

            ppath = PathUtils.ResolvePhysicalPath(ppath, storageConfig);
            PhysicalPath = ppath.IndexOf('{') == -1 && appendTenant ? Path.Combine(ppath, tenant) : string.Format(ppath, tenant);
        }

        public MappedPath AppendDomain(string domain)
        {
            domain = domain.Replace('.', '_'); //Domain prep. Remove dots
            return new MappedPath
            {
                PhysicalPath = Path.Combine(this.PhysicalPath, PathUtils.Normalize(domain, true)),
                VirtualPath = this.VirtualPath.IsAbsoluteUri ? new Uri(this.VirtualPath, domain) : new Uri(this.VirtualPath + domain, UriKind.Relative)
            };
        }
    }
}
