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

using ASC.Web.Core.Client.Templates;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.UI;
using TMResourceData;

namespace ASC.Web.Core.Client.HttpHandlers
{
    public abstract class ClientScript
    {
        protected virtual string BaseNamespace
        {
            get { return "ASC.Resources"; }
        }


        protected abstract IEnumerable<KeyValuePair<string, object>> GetClientVariables(HttpContext context);


        public string GetData(HttpContext context)
        {
            var namespaces = BaseNamespace.Split('.');
            var builder = new StringBuilder();
            var content = string.Empty;

            for (var index = 1; index <= namespaces.Length; index++)
            {
                var ns = string.Join(".", namespaces, 0, index);
                builder.AppendFormat("if (typeof({0})==='undefined'){{{0} = {{}};}} ", ns);
            }

            var store = GetClientVariables(context);
            if (store != null)
            {
                foreach (var clientObject in store)
                {
                    var resourceSet = clientObject.Value as ClinetResourceSet;
                    if (resourceSet != null)
                    {
                        builder.AppendFormat("{0}.{1}={2};", BaseNamespace, clientObject.Key, JsonConvert.SerializeObject(resourceSet.GetResources()));
                        continue;
                    }

                    var templateSet = clientObject.Value as ClientTemplateSet;
                    if (templateSet != null)
                    {
                        builder.AppendFormat("{0}{1}", Environment.NewLine, templateSet.GetClientTemplates());
                        continue;
                    }

                    builder.AppendFormat("{0}.{1}={2};", BaseNamespace, clientObject.Key, JsonConvert.SerializeObject(clientObject.Value));
                }

                content = builder.ToString();
            }
            return content;
        }

        protected internal virtual string GetCacheHash()
        {
            return string.Empty;
        }


        protected KeyValuePair<string, object> RegisterObject(string key, object value)
        {
            return new KeyValuePair<string, object>(key, value);
        }

        protected KeyValuePair<string, object> RegisterResourceSet(string key, ResourceManager resourceManager)
        {
            return new KeyValuePair<string, object>(key, new ClinetResourceSet(resourceManager));
        }

        protected KeyValuePair<string, object> RegisterClientTemplatesPath(string virtualPathToControl, HttpContext context)
        {
            var page = new Page();
            page.Controls.Add(page.LoadControl(virtualPathToControl));

            var output = new StringWriter();
            context.Server.Execute(page, output, false);

            var doc = new HtmlDocument();
            doc.LoadHtml(output.GetStringBuilder().ToString());

            var nodes = doc.DocumentNode.SelectNodes("//script[@type='text/x-jquery-tmpl']");
            var templates = nodes.ToDictionary(x => x.Attributes["id"].Value, y => y.InnerHtml);
            return new KeyValuePair<string, object>(Guid.NewGuid().ToString(), new ClientTemplateSet(() => templates));
        }


        class ClientTemplateSet
        {
            private static readonly JqTemplateCompiler compiler = new JqTemplateCompiler();
            private readonly Func<Dictionary<string, string>> getTemplates;


            public ClientTemplateSet(Func<Dictionary<string, string>> clientTemplates)
            {
                getTemplates = clientTemplates;
            }

            public string GetClientTemplates()
            {
                var result = new StringBuilder();
                lock (compiler)
                {
                    foreach (var template in getTemplates())
                    {
                        // only for jqTmpl for now
                        result.AppendFormat("jQuery.template('{0}', {1});{2}", template.Key, compiler.GetCompiledCode(template.Value), Environment.NewLine);
                    }
                }
                return result.ToString();
            }
        }

        class ClinetResourceSet
        {
            private readonly ResourceManager manager;

            public ClinetResourceSet(ResourceManager manager)
            {
                this.manager = manager;
            }

            public IDictionary<string, string> GetResources()
            {
                var baseFromDbSet = manager.GetResourceSet(CultureInfo.InvariantCulture, true, true);

                var dbManager = manager as DBResourceManager;
                var baseNeutral = baseFromDbSet;

                if (dbManager != null)
                {
                    baseNeutral = dbManager.GetBaseNeutralResourceSet();
                }
                var set = manager.GetResourceSet(Thread.CurrentThread.CurrentCulture, true, true);
                var result = new Dictionary<string, string>();
                foreach (DictionaryEntry entry in baseNeutral)
                {
                    var value = set.GetString((string)entry.Key) ?? baseFromDbSet.GetString((string)entry.Key) ?? baseNeutral.GetString((string)entry.Key) ?? string.Empty;
                    result.Add(entry.Key.ToString(), value);
                }

                return result;
            }
        }
    }
}