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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using ASC.Web.Mobile.Utils;

namespace ASC.Web.Mobile.Helpers
{

    public abstract class CombinedFileCollection
    {
        public int Version { get; set; }
        private readonly string _target;

        private readonly UrlHelper _urlHelper;
        private readonly bool _combine;
        private readonly List<string> _urls = new List<string>();
        private readonly HttpContextBase _context;
        private readonly string _targetFile;
        private readonly List<string> _urlsFile = new List<string>();

        protected CombinedFileCollection(string target, HtmlHelper helper, UrlHelper urlHelper, bool combine, int version)
        {
            Version = version;
            _urlHelper = urlHelper;
            _combine = combine;
            _context = helper.ViewContext.HttpContext;
            _target = _urlHelper.Content(target);
            _targetFile = _context.Server.MapPath(_target);
        }

        protected virtual string MapPath(string file)
        {
            return _context.Server.MapPath(file);
        }

        protected virtual CombinedFileCollection AddUrl(string url)
        {
            var urlR = _urlHelper.Content(url);
            _urls.Add(urlR);
            _urlsFile.Add(_context.Server.MapPath(urlR));
            return this;
        }

        protected virtual CombinedFileCollection AddUrls(IEnumerable<string> urls)
        {
            if (urls != null)
            {
                foreach (var url in urls)
                {
                    AddUrl(url);
                }
            }
            return this;
        }

        public MvcHtmlString Render()
        {
            PreRender();
            if (_combine)
            {
                var resultFile = _context.Server.MapPath(_target);
                bool shouldRebuild = HttpRuntime.Cache.Get(resultFile)==null || !File.Exists(resultFile);
                
                if (shouldRebuild)
                {
                    if (!Directory.Exists(Path.GetDirectoryName(_targetFile)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(_targetFile));
                    }
                    using (var fs = File.Create(_targetFile))
                    {
                        BuildResult(fs, _urlsFile);
                    }
                    HttpRuntime.Cache.Insert(resultFile,DateTime.UtcNow,null,Cache.NoAbsoluteExpiration,Cache.NoSlidingExpiration,CacheItemPriority.NotRemovable,null);
                }
                TagBuilder tagBuilder = BuildTag(AppendVersionTo(_target));
                return MvcHtmlString.Create(tagBuilder.ToString(Mode));
            }
            var builder = new StringBuilder();
            foreach (var url in _urls)
            {
                builder.AppendLine(BuildTag(AppendVersionTo(url)).ToString(Mode));
            }
            return MvcHtmlString.Create(builder.ToString());
        }

        private string AppendVersionTo(string url)
        {
            if (Version != -1)
            {
                if (url.IndexOf("?") != -1)
                {
                    return url + "&v=" + Version;
                }
                return url + "?v=" + Version;
            }
            return url;
        }

        protected virtual void PreRender()
        {
            
        }

        protected abstract TagBuilder BuildTag(string target);

        protected abstract TagRenderMode Mode { get; }

        protected abstract void BuildResult(Stream fs, List<string> filePaths);
    }

    public class CssFileCollection : CombinedFileCollection
    {


        public CssFileCollection(string target, HtmlHelper helper, UrlHelper urlHelper, bool combine, int version) : base(target, helper, urlHelper,combine,version)
        {
        }

        protected override TagBuilder BuildTag(string target)
        {
            var builder = new TagBuilder("link");
            builder.MergeAttribute("href",target);
            builder.MergeAttribute("rel", "stylesheet");
            builder.MergeAttribute("type", "text/css");
            return builder;
        }

        public CssFileCollection With(string url)
        {
            return (CssFileCollection) base.AddUrl(url);
        }

        public CssFileCollection WithMany(IEnumerable<string> urls)
        {
            return (CssFileCollection)base.AddUrls(urls);
        }

        protected override TagRenderMode Mode
        {
            get { return TagRenderMode.SelfClosing; }
        }

        protected override void BuildResult(Stream fs, List<string> filePaths)
        {
            var minifier = new Microsoft.Ajax.Utilities.Minifier();
            var styleBuilder = new StringBuilder();

            foreach (var style in filePaths)
            {
                styleBuilder.AppendLine(FileOperator.ReadAllText(style));
            }
            var styles = minifier.MinifyStyleSheet(styleBuilder.ToString());
            if (minifier.Errors.Count > 0)
            {
                styles = styleBuilder.ToString();
            }
            var buffer = Encoding.UTF8.GetBytes(styles);
            fs.Write(buffer,0,buffer.Length);
        }
    }

    public class JsFileCollection : CombinedFileCollection
    {
        public JsFileCollection(string target, HtmlHelper helper, UrlHelper urlHelper, bool combine, int version)
            : base(target, helper, urlHelper, combine,version)
        {
        }

        protected override TagBuilder BuildTag(string target)
        {
            var builder = new TagBuilder("script");
            builder.MergeAttribute("src", target);
            //builder.MergeAttribute("async","true");
            builder.MergeAttribute("type", "text/javascript");
            return builder;
        }

        protected override TagRenderMode Mode
        {
            get { return TagRenderMode.Normal; }
        }

        public JsFileCollection With(string url)
        {
            return (JsFileCollection)base.AddUrl(url);
        }

        public JsFileCollection WithMany(IEnumerable<string> urls)
        {
            return (JsFileCollection)base.AddUrls(urls);
        }

        protected override void BuildResult(Stream fs, List<string> filePaths)
        {
            var minifier = new Microsoft.Ajax.Utilities.Minifier();
            var styleBuilder = new StringBuilder();

            foreach (var style in filePaths)
            {
                styleBuilder.AppendLine(FileOperator.ReadAllText(style));
            }
            var styles = minifier.MinifyJavaScript(styleBuilder.ToString());
            if (minifier.Errors.Count > 0)
            {
                styles = styleBuilder.ToString();
            }
            var buffer = Encoding.UTF8.GetBytes(styles);
            fs.Write(buffer, 0, buffer.Length);
        }
    }
}