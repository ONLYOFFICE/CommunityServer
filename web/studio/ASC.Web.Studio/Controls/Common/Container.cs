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
using System.Web.UI;
using System.ComponentModel;
using System.Web.UI.WebControls;
using ASC.Web.Studio.Core;

namespace ASC.Web.Studio.Controls.Common
{
    [ToolboxData("<{0}:Container runat=\"server\"/>"),
     ParseChildren(true), PersistChildren(false)]
    public class Container : Control
    {
        #region public properties

        [Description("Provides Header items."),
         Category("Templates"), PersistenceMode(PersistenceMode.InnerProperty),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true)]
        public PlaceHolder Header { get; set; }

        [Description("Provides Body items."),
         Category("Templates"), PersistenceMode(PersistenceMode.InnerProperty),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true)]
        public PlaceHolder Body { get; set; }

        [Description("Provides Style options."),
         Category("Styles"), PersistenceMode(PersistenceMode.InnerProperty),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true)]
        public StyleOptions Options { get; set; }

        [Description("Bread Crumb item."),
         Category("Templates"), PersistenceMode(PersistenceMode.InnerProperty),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true)]
        public string CurrentPageCaption { get; set; }

        #endregion

        public Container()
        {
            Options = new StyleOptions
                {
                    HeadCssClass = "containerHeaderBlock",
                    BodyCssClass = "containerBodyBlock",
                    ContainerCssClass = "mainContainerClass",
                    PopupContainerCssClass = "popupContainerClass",
                    OnCancelButtonClick = "PopupKeyUpActionProvider.CloseDialog();"
                };
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (Header != null)
            {
                Controls.Add(Header);
            }

            if (Body != null)
            {
                Controls.Add(Body);
            }
        }

        #region Render

        public string GetInfoPanelClientID()
        {
            return string.Format("{0}_InfoPanel", ClientID);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            writer.Write("<div class=\"{0}\" style=\"{1}\">",
                         (Options.IsPopup && !string.IsNullOrEmpty(Options.PopupContainerCssClass)
                              ? Options.PopupContainerCssClass
                              : Options.ContainerCssClass),
                         Options.ContainerStyle);

            if (Header != null || !string.IsNullOrEmpty(CurrentPageCaption) || Options.IsPopup)
            {
                writer.Write("<div class=\"{0}\" style=\"{1}\">",
                             Options.HeadCssClass,
                             Options.HeadStyle);

                writer.Write("<table cellspacing=\"0\" cellpadding=\"0\" border=\"0\" style='width:100%; height:0px;'>");
                writer.Write("<tr valign=\"top\"><td>");

                if (!string.IsNullOrEmpty(CurrentPageCaption))
                {
                    writer.Write("<div>{0}</div>",
                                 string.IsNullOrEmpty(Options.HeaderBreadCrumbCaption)
                                     ? CurrentPageCaption.HtmlEncode()
                                     : Options.HeaderBreadCrumbCaption);
                }
                else if (Header != null)
                {
                    Header.RenderControl(writer);
                }

                writer.Write("</td>");

                if (Options.IsPopup)
                {
                    writer.Write("<td class=\"popupCancel\"><div class=\"cancelButton\" onclick=\"{0}\"></div></td>",
                                 Options.OnCancelButtonClick);
                }
                writer.Write("</tr></table>");

                writer.Write("</div>");
            }

            writer.Write(Options.IsPopup
                             ? "<div {1} id=\"{2}\" class=\"infoPanel {3}\"><div>{0}</div></div>"
                             : "<div class=\"infoPanel {3}\"><div {1} id=\"{2}\">{0}</div></div>",
                         Options.InfoMessageText,
                         string.IsNullOrEmpty(Options.InfoMessageText)
                             ? "style=\"display:none;\""
                             : string.Empty,
                         GetInfoPanelClientID(),
                         Options.InfoType.Equals(InfoType.Info)
                             ? string.Empty
                             : Options.InfoType.Equals(InfoType.Warning) ? "warn" : "alert");

            if (Body != null)
            {
                writer.Write("<div class=\"{0}\" style=\"{1}\">",
                             Options.BodyCssClass,
                             Options.BodyStyle);
                Body.RenderControl(writer);
                writer.Write("</div>");
            }

            writer.Write(@"</div>");
        }

        #endregion
    }

    public class StyleOptions
    {
        public string ContainerCssClass { get; set; }
        public string ContainerStyle { get; set; }

        public string PopupContainerCssClass { get; set; }

        public string HeadCssClass { get; set; }
        public string HeadStyle { get; set; }

        public string BodyCssClass { get; set; }
        public string BodyStyle { get; set; }

        public bool IsPopup { get; set; }

        public string OnCancelButtonClick { get; set; }

        public string HeaderBreadCrumbCaption { get; set; }

        public string InfoMessageText { get; set; }
        public InfoType InfoType { get; set; }
    }
}