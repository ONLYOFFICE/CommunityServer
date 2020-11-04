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