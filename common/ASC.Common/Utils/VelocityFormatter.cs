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
using System.IO;
using System.Text;
using ASC.Collections;
using NVelocity;
using NVelocity.App;
using NVelocity.Runtime.Resource.Loader;

namespace ASC.Common.Utils
{
    public class TextLoader : ResourceLoader
    {
        public override void Init(Commons.Collections.ExtendedProperties configuration)
        {
            //nothing to configure
        }

        public override Stream GetResourceStream(string source)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(source));
        }

        public override long GetLastModified(NVelocity.Runtime.Resource.Resource resource)
        {
            return 1;
        }

        public override bool IsSourceModified(NVelocity.Runtime.Resource.Resource resource)
        {
            return false;
        }
    }

    public class VelocityFormatter
    {
        private static bool _isVelocityInitialized;
        private static readonly CachedDictionary<Template> Patterns = new CachedDictionary<Template>("velocity_patterns");

        public static string FormatText(string templateText, IDictionary<string,object> values)
        {
            var nvelocityContext = new VelocityContext();
            foreach (var tagValue in values)
                nvelocityContext.Put(tagValue.Key, tagValue.Value);
            return FormatText(templateText, nvelocityContext);
        }

        public static string FormatText(string templateText, VelocityContext context)
        {
            if (!_isVelocityInitialized)
            {
                var properties = new Commons.Collections.ExtendedProperties();
                properties.AddProperty("resource.loader", "custom");
                properties.AddProperty("custom.resource.loader.class", "ASC.Common.Utils.TextLoader; ASC.Common");
                properties.AddProperty("input.encoding", Encoding.UTF8.WebName);
                properties.AddProperty("output.encoding", Encoding.UTF8.WebName);
                Velocity.Init(properties);
                _isVelocityInitialized = true;
            }
            using (var writer = new StringWriter())
            {
                var template = Patterns.Get(templateText.GetHashCode().ToString(), () => Velocity.GetTemplate(templateText));
                template.Merge(context, writer);
                return writer.GetStringBuilder().ToString();
            } 
        }
    }
}
