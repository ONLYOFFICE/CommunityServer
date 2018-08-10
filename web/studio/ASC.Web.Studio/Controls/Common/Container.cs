/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
			Header = new PlaceHolder ();
			Body = new PlaceHolder ();
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
                    writer.Write("<td class=\"popupCancel\"><div class=\"cancelButton\" onclick=\"{0}\">&times;</div></td>",
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