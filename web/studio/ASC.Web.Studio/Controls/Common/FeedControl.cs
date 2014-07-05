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