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
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Resources;

namespace ASC.Web.Studio.UserControls.Common.PollForm
{
    [DefaultProperty("Name")]
    [ToolboxData("<{0}:PollFormMaster runat=server></{0}:PollFormMaster>")]
    public class PollFormMaster : WebControl
    {
        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public string Name
        {
            get
            {
                var s = (String)ViewState["Name"];
                return s ?? String.Empty;
            }
            set { ViewState["Name"] = value; }
        }

        [Bindable(true)]
        [Category("Appearance")]
        public bool Singleton
        {
            get
            {
                var o = ViewState["Singleton"];
                return o != null && (bool)o;
            }
            set { ViewState["Singleton"] = value; }
        }

        [Bindable(true)]
        [Category("Appearance")]
        public List<AnswerViarint> AnswerVariants
        {
            get
            {
                var o = ViewState["AnswerVariants"];
                return o == null ? null : (List<AnswerViarint>)o;
            }
            set { ViewState["AnswerVariants"] = value; }
        }

        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public string VariantPanelCSSClass
        {
            get
            {
                var s = (string)ViewState["VariantPanelCSSClass"];
                return s ?? String.Empty;
            }
            set { ViewState["VariantPanelCSSClass"] = value; }
        }

        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public string VariantLabelCSSClass
        {
            get
            {
                var s = (string)ViewState["VariantLabelCSSClass"];
                return s ?? String.Empty;
            }
            set { ViewState["VariantLabelCSSClass"] = value; }
        }

        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public string HeaderCSSClass
        {
            get
            {
                var s = (string)ViewState["HeaderCSSClass"];
                return s ?? String.Empty;
            }
            set { ViewState["HeaderCSSClass"] = value; }
        }

        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public string TextEditCSSClass
        {
            get
            {
                var s = (string)ViewState["TextEditCSSClass"];
                return s ?? String.Empty;
            }
            set { ViewState["TextEditCSSClass"] = value; }
        }

        public string QuestionFieldID { get; set; }
        public string PollID { get; set; }
        private readonly string _uniqueID;

        public PollFormMaster()
        {
            AnswerVariants = new List<AnswerViarint>();
            Singleton = true;
            _uniqueID = Guid.NewGuid().ToString().Replace('-', '_');
            TextEditCSSClass = "textEdit";
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Page.RegisterBodyScripts("~/usercontrols/common/pollform/js/pollform.js")
                .RegisterStyle("~/usercontrols/common/pollform/css/style.css");
        }

        protected override void OnInit(EventArgs e)
        {
            InitProperties();
        }

        protected override void LoadViewState(object savedState)
        {
            base.LoadViewState(savedState);
            InitProperties();
        }

        private void InitProperties()
        {
            if (!Page.IsPostBack) return;

            if (!String.IsNullOrEmpty(Page.Request["poll_question"]))
                Name = Page.Request["poll_question"];

            try
            {
                Singleton = (Convert.ToInt32(Page.Request["questiontype"]) == 1);
            }
            catch
            {
                Singleton = true;
            }

            AnswerVariants.Clear();
            for (var i = 1; i < 20; i++)
            {
                if (String.IsNullOrEmpty(Page.Request["q" + i])) continue;

                var id = Page.Request["qid_" + i] ?? "";
                AnswerVariants.Add(new AnswerViarint { ID = id, Name = Page.Request["q" + i] });
            }
        }

        protected override void RenderContents(HtmlTextWriter output)
        {
            var labelCSSClass = String.IsNullOrEmpty(VariantLabelCSSClass) ? "poll_variantLabel" : VariantLabelCSSClass;
            var divCSSClass = String.IsNullOrEmpty(VariantPanelCSSClass) ? "poll_variantDiv" : VariantPanelCSSClass;
            var headerCSSClass = String.IsNullOrEmpty(HeaderCSSClass) ? "block-cnt-splitter" : HeaderCSSClass;

            var qInputID = String.IsNullOrEmpty(QuestionFieldID) ? ("__poll_" + _uniqueID + "_question") : QuestionFieldID;

            var sb = new StringBuilder();
            sb.Append("<div class=\"block-cnt-splitter requiredField\">");
            sb.Append("<span class=\"requiredErrorText\">" + UserControlsCommonResource.EmptyQuestion + "</span>");
            sb.Append("<input name=\"" + UniqueID + "\" type='hidden' value=''/>");

            sb.Append("<div class=\"" + headerCSSClass + " headerPanelSmall\">");
            sb.Append("<b>");
            sb.Append(UserControlsCommonResource.PollQuestion);
            sb.Append(":</b>");
            sb.Append("</div>");

            sb.Append("<input class=\"" + TextEditCSSClass + "\" style=\"width:100%;\" maxlength=\"100\" name=\"poll_question\" id=\"" + qInputID + "\" type=\"text\" value=\"" + HttpUtility.HtmlEncode(Name) + "\" />");
            sb.Append("</div>");

            sb.Append("<div class=\"block-cnt-splitter requiredField\">");
            sb.Append("<div class=\"" + headerCSSClass + " headerPanelSmall\">");
            sb.Append("<b>");
            sb.Append(UserControlsCommonResource.AnswerVariants);
            sb.Append(":</b>");
            sb.Append("</div>");

            sb.Append("<div id=\"__poll_" + _uniqueID + "_qbox\">");

            var i = 1;
            var leftMargin = 10;
            foreach (var answerVariant in AnswerVariants)
            {
                sb.Append("<div style='clear:left;' class=\"" + divCSSClass + " poll_clearFix\">");
                sb.Append("<label for=\"__poll_" + _uniqueID + "_q1\" style='float:left;' class=\"" + labelCSSClass + "\">" + UserControlsCommonResource.Variant + " " + i + ":</label>");
                if (AnswerVariants.Count < 3)
                {
                    sb.Append("<span style='display:none;' class='poll_remove_span_" + i + "' onclick='PollMaster.RemoveAnswerVariant(this)'></span>");
                }
                else
                {
                    sb.Append("<span class='poll_remove_span_" + i + "' onclick='PollMaster.RemoveAnswerVariant(this)'></span>");
                }
                if (i > 9) leftMargin = 3;
                sb.Append("<input id=\"__poll_" + _uniqueID + "_q" + i + "\" name=\"q" + i + "\" maxlength=\"100\" value=\"" + HttpUtility.HtmlEncode(answerVariant.Name) + "\" type=\"text\" class=\"" + TextEditCSSClass + "\" style=\"margin:2px 0 0 " + leftMargin + "px;  float:left; width:500px\"/>");
                sb.Append("<input id=\"__poll_" + _uniqueID + "_qid" + i + "\" name=\"qid_" + i + "\" value=\"" + (answerVariant.ID ?? "") + "\" type=\"hidden\"/>");
                sb.Append("</div>");
                i++;
            }

            if (i <= 2)
            {
                for (var j = i; j <= 2; j++)
                {
                    sb.AppendFormat("<div class=\"" + divCSSClass + " poll_clearFix\">"
                                    + "<label for=\"__poll_{1}_q{0}\" style='float:left;' class=\"" + labelCSSClass + "\">" + UserControlsCommonResource.Variant + " {0}:</label><span style='display:none;' class='poll_remove_span_" + j + "' onclick='PollMaster.RemoveAnswerVariant(this)'></span><input id=\"__poll_{1}_q{0}\" name=\"q{0}\" maxlength=\"100\" type=\"text\" class=\"" + TextEditCSSClass + "\" style=\"margin:2px 0 0 10px; float:left; width:500px\"/>"
                                    + "<input id=\"__poll_" + _uniqueID + "_qid" + j + "\" name=\"qid_" + j + "\" value=\"\" type=\"hidden\"/>"
                                    + " </div>", j, _uniqueID);
                }

                i = 2;
            }
            sb.Append("</div>");

            sb.AppendFormat(@"<input type=""hidden"" id=""__poll_{1}_variantCaption"" value=""{0}""/>", UserControlsCommonResource.Variant, _uniqueID);

            sb.Append("<div class=\"poll_clearFix\" style='padding: 0 0 0 63px;'>");
            sb.Append("<div id=\"__poll_" + _uniqueID + "_addButton\" style=\"" + ((i > 15) ? "display:none;" : "") + " \"><a class=\"link plus dotline\" href='#' onclick=\"PollMaster.AddAnswerVariant('" + _uniqueID + "','" + divCSSClass + "','" + labelCSSClass + "','" + TextEditCSSClass + "');\">" + UserControlsCommonResource.AddAnswerVariantButton + "</a></div>");
            sb.Append("</div>");

            sb.Append("</div>");

            var multipleState = !Singleton ? "checked=\"checked\"" : "";
            var singleState = Singleton ? "checked=\"checked\"" : "";

            sb.Append("<div class=\"block-cnt-splitter\">");
            sb.Append("<div class=\"" + headerCSSClass + " headerPanelSmall\">");
            sb.Append("<b>");
            sb.Append(UserControlsCommonResource.PollType);
            sb.Append(":</b>");
            sb.Append("</div>");

            sb.Append("<div style=\"padding-bottom:5px;\">");
            sb.Append("<input style=\"vertical-align:middle; margin:-3px 8px 0 0\" type=\"radio\" " + singleState + " id=\"__poll_" + _uniqueID + "_qt1\" value=\"1\" name=\"questiontype\"/>");
            sb.Append("<label for=\"__poll_" + _uniqueID + "_qt1\" style='padding-right:20px;'>" + UserControlsCommonResource.OneAnswerVariant + "</label>");
            sb.Append("</div>");

            sb.Append("<div>");
            sb.Append("<input style=\"vertical-align:middle; margin:-3px 8px 0 0;\" type=\"radio\" " + multipleState + " id=\"__poll_" + _uniqueID + "_qt2\" value=\"2\" name=\"questiontype\"/>");
            sb.Append("<label for=\"__poll_" + _uniqueID + "_qt2\">" + UserControlsCommonResource.FewAnswerVariant + "</label>");
            sb.Append("</div>");

            sb.Append("</div>");

            output.Write(sb.ToString());
        }

        public class AnswerViarint
        {
            public string ID { get; set; }
            public string Name { get; set; }
        }
    }
}