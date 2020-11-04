/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System.ComponentModel;
using System.Web.UI;
using ASC.Web.Core;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.Controls.Common
{
    [DefaultProperty("Title")]
    [ToolboxData("<{0}:FeedControl runat=server></{0}:FeedControl>")]
    public class FeedControl : Control
    {
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public string Title { get; set; }

        [DefaultValue("")]
        [Localizable(false)]
        public string ProductId { get; set; }

        [DefaultValue("")]
        [Localizable(false)]
        public string ContainerId { get; set; }

        [DefaultValue("")]
        [Localizable(true)]
        public string ModuleId { get; set; }

        [DefaultValue(false)]
        [Localizable(false)]
        public bool ContentOnly { get; set; }

        [DefaultValue(true)]
        [Localizable(false)]
        public bool AutoFill { get; set; }

        public FeedControl()
        {
            ContentOnly = false;
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (AutoFill)
            {
                if (string.IsNullOrEmpty(ProductId))
                {
                    ProductId = CommonLinkUtility.GetProductID().ToString("D");
                }
                if (string.IsNullOrEmpty(ModuleId))
                {
                    IProduct product;
                    IModule module;
                    CommonLinkUtility.GetLocationByRequest(out product, out module);
                    if (module != null)
                    {
                        ModuleId = module.ID.ToString("D");
                    }
                }
            }

            writer.Write(Services.WhatsNew.feed.RenderRssMeta(Title, ProductId));
        }
    }
}