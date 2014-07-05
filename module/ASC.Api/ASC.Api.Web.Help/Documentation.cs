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

using ASC.Api.Interfaces;
using ASC.Api.Web.Help.DocumentGenerator;
using ASC.Api.Web.Help.Helpers;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Routing;

namespace ASC.Api.Web.Help
{
    internal static class Documentation
    {
        static List<MsDocEntryPoint> _points = new List<MsDocEntryPoint>();
        static readonly Dictionary<string, MsDocEntryPointMethod> MethodList = new Dictionary<string, MsDocEntryPointMethod>();

        public static void Load(string msDocFolder, string searchIndexDir)
        {
            MethodList.Clear();
            //Load documentation
            _points = GenerateDocs(msDocFolder);

            var basePath = ConfigurationManager.AppSettings["apiprefix"] ?? "api";
            if (_points != null) _points.ForEach(x => x.Methods.ForEach(y => y.Path = basePath + y.Path));
        }

        public static List<MsDocEntryPoint> GenerateDocs(string msDocFolder)
        {
            //Generate the docs first
            var container = ApiSetup.ConfigureEntryPoints();
            var entries = container.Resolve<IEnumerable<IApiMethodCall>>();

            var apiEntryPoints = container.Registrations.Where(x => x.RegisteredType == typeof(IApiEntryPoint)).ToList();

            var generator = new MsDocDocumentGenerator(Path.Combine(msDocFolder, "help.xml"), msDocFolder, container);

            foreach (var apiEntryPoint in entries.GroupBy(x => x.ApiClassType))
            {
                var point = apiEntryPoint;
                generator.GenerateDocForEntryPoint(
                    apiEntryPoints.SingleOrDefault(x => x.MappedToType == point.Key),
                    apiEntryPoint.AsEnumerable());
            }

            return generator.Points;
        }

        public static MsDocEntryPoint GetDocs(string name)
        {
            return _points.SingleOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public static IEnumerable<MsDocEntryPoint> GetAll()
        {
            return _points;
        }

        public static Dictionary<MsDocEntryPoint, Dictionary<MsDocEntryPointMethod, string>> Search(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return new Dictionary<MsDocEntryPoint, Dictionary<MsDocEntryPointMethod, string>>();
            }

            var terms = Regex.Split(query ?? String.Empty, @"\W+").Where(x => !String.IsNullOrEmpty(x));
            var result = _points.ToDictionary(
                x => x,
                ep => ep.Methods.Where(m => terms.All(
                        term => (m.Summary != null && 0 <= m.Summary.IndexOf(term, StringComparison.OrdinalIgnoreCase)) ||
                            (m.Category != null && 0 <= m.Category.IndexOf(term, StringComparison.OrdinalIgnoreCase)) ||
                            (m.FunctionName != null && 0 <= m.FunctionName.IndexOf(term, StringComparison.OrdinalIgnoreCase)) ||
                            (m.Notes != null && 0 <= m.Notes.IndexOf(term, StringComparison.OrdinalIgnoreCase)) ||
                            (m.Path != null && 0 <= m.Path.IndexOf(term, StringComparison.OrdinalIgnoreCase)) ||
                            (m.Remarks != null && 0 <= m.Remarks.IndexOf(term, StringComparison.OrdinalIgnoreCase)) ||
                            (m.Returns != null && 0 <= m.Returns.IndexOf(term, StringComparison.OrdinalIgnoreCase)))
                    )
                    .ToDictionary(key => key, value => string.Empty)
            );
            return result;

        }

        public static void GenerateRouteMap()
        {
            if (!MethodList.Any())
            {
                //Build list
                var reqContext = new RequestContext(new HttpContextWrapper(HttpContext.Current), new RouteData());
                foreach (var msDocEntryPoint in _points)
                {
                    MvcApplication.CacheManifest.AddCached(new Uri(Url.GetDocUrl(msDocEntryPoint.Name, null, null, reqContext), UriKind.Relative));
                    foreach (var method in msDocEntryPoint.Methods)
                    {
                        method.Parent = msDocEntryPoint;
                        var url = Url.GetDocUrl(msDocEntryPoint.Name, method.HttpMethod, method.Path, reqContext);
                        MethodList.Add(url, method);
                        //MvcApplication.CacheManifest.AddCached(new Uri(url, UriKind.Relative));
                    }
                }
            }
        }

        public static MsDocEntryPointMethod GetByUri(Uri uri)
        {
            MsDocEntryPointMethod pointMethod;
            MethodList.TryGetValue(uri.AbsolutePath, out pointMethod);
            return pointMethod;
        }
    }
}