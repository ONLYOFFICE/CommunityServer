/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
using AjaxPro;
using Resources;

namespace ASC.Web.Studio.UserControls.Common.PollForm
{
    public interface IVoteHandler
    {
        bool VoteCallback(string pollID, List<string> selectedVariantIDs, string additionalParams, out string errorMessage);
    }

    [AjaxNamespace("PollFormControl")]
    [DefaultProperty("Name")]
    [ToolboxData("<{0}:PollForm runat=server></{0}:PollForm>")]
    public class PollForm : WebControl
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
        public bool Answered
        {
            get
            {
                var o = ViewState["Answered"];
                return o != null && (bool)o;
            }
            set { ViewState["Answered"] = value; }
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
        public Type VoteHandlerType
        {
            get
            {
                var o = ViewState["VoteHandlerType"];
                return (o == null) ? null : (Type)o;
            }
            set { ViewState["VoteHandlerType"] = value; }
        }

        [Bindable(true)]
        [Category("Appearance")]
        public List<AnswerViarint> AnswerVariants
        {
            get
            {
                var o = ViewState["AnswerVariants"];
                return (o == null) ? null : (List<AnswerViarint>)o;
            }
            set { ViewState["AnswerVariants"] = value; }
        }

        [Bindable(true)]
        [Category("Appearance")]
        public string AdditionalParams
        {
            get
            {
                var o = ViewState["AdditionalParams"];
                return (o == null) ? null : (string)o;
            }
            set { ViewState["AdditionalParams"] = value; }
        }

        [Bindable(true)]
        [Category("Appearance")]
        public string ButtonCSSClass
        {
            get
            {
                var o = ViewState["ButtonCSSClass"];
                return (o == null) ? null : (string)o;
            }
            set { ViewState["ButtonCSSClass"] = value; }
        }

        [Bindable(true)]
        [Category("Appearance")]
        public string StatisticBarCSSClass
        {
            get
            {
                var o = ViewState["StatisticBarCSSClass"];
                return (o == null) ? null : (string)o;
            }
            set { ViewState["StatisticBarCSSClass"] = value; }
        }

        [Bindable(true)]
        [Category("Appearance")]
        public string LiderStatisticBarCSSClass
        {
            get
            {
                var o = ViewState["LiderStatisticBarCSSClass"];
                return ((o == null) ? null : (string)o);
            }
            set { ViewState["LiderStatisticBarCSSClass"] = value; }
        }

        [Bindable(true)]
        [Category("Appearance")]
        public string VariantTextCSSClass
        {
            get
            {
                var o = ViewState["VariantTextCSSClass"];
                return (o == null) ? null : (string)o;
            }
            set { ViewState["VariantTextCSSClass"] = value; }
        }

        [Bindable(true)]
        [Category("Appearance")]
        public string VoteCountTextCSSClass
        {
            get
            {
                var o = ViewState["VoteCountTextCSSClass"];
                return (o == null) ? null : (string)o;
            }
            set { ViewState["VoteCountTextCSSClass"] = value; }
        }

        public string PollID { get; set; }
        public string BehaviorID { get; set; }
        private string _jsObjName;

        public PollForm()
        {
            AnswerVariants = new List<AnswerViarint>();
            VoteCountTextCSSClass = "pollSmallDescribe";
            VariantTextCSSClass = "VariantTextCSSClass";
            LiderStatisticBarCSSClass = "statisticBar liaderBar";
            StatisticBarCSSClass = "statisticBar";
            ButtonCSSClass = "button gray";
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            AjaxPro.Utility.RegisterTypeForAjax(GetType());
            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/common/pollform/js/pollform.js"));
        }

        protected override void OnPreRender(EventArgs e)
        {
            _jsObjName = String.IsNullOrEmpty(BehaviorID) ? ("__pollForm" + UniqueID) : BehaviorID;

            var script = new StringBuilder();
            script.Append(" window." + _jsObjName + " = new VotingPollPrototype('" + _jsObjName + "','" + VoteHandlerType.AssemblyQualifiedName + "','" + PollID + "','" + UserControlsCommonResource.EmptySelect + "','" + StatisticBarCSSClass + "','" + LiderStatisticBarCSSClass + "','" + VariantTextCSSClass + "','" + VoteCountTextCSSClass + "','" + AdditionalParams + "'); ");

            foreach (var variant in AnswerVariants)
                script.Append(_jsObjName + ".RegistryVariant(new AnswerVariantPrototype('" + variant.ID + "','" + ReplaceSingleQuote(HttpUtility.HtmlEncode(variant.Name)) + "'," + variant.VoteCount + ")); ");

            Page.RegisterInlineScript(script.ToString());
        }

        protected override void RenderContents(HtmlTextWriter output)
        {
            output.Write(RenderPollForm());
        }

        private string RenderPollForm()
        {
            var sb = new StringBuilder();

            sb.Append("<div>");
            sb.AppendFormat(@"<div id=""__pollForm_{0}_result""></div>", PollID);
            sb.AppendFormat(@"<div id=""__pollForm_{0}_voteBox"">", PollID);

            if (!Answered)
            {
                foreach (var variant in AnswerVariants)
                {
                    sb.Append(@"<div style=""padding:6px 0px"">");

                    sb.AppendFormat(@"<input name=""__pollForm_{0}_av"" value=""{1}"" id=""__pollForm_{0}_av_{1}"" style=""float: left; margin: 0 10px 0 0;"" type=""{2}""/><label for=""__pollForm_{0}_av_{1}"">{3}</label>",
                                    PollID, variant.ID, (Singleton ? "radio" : "checkbox"), ReplaceSingleQuote(HttpUtility.HtmlEncode(variant.Name)));

                    sb.Append("</div>");
                }
            }
            else
            {
                sb.Append(RenderAnsweredVariants(AnswerVariants, StatisticBarCSSClass, LiderStatisticBarCSSClass, VariantTextCSSClass));
            }


            if (!Answered)
                sb.AppendFormat(@"<div id=""__pollForm_{0}_answButtonBox"" class=""small-button-container""><a class=""{3}"" href=""javascript:{1}.Vote();"">{2}</a></div>",
                                PollID, _jsObjName, UserControlsCommonResource.VoteButton, ButtonCSSClass);

            sb.Append(@"</div>");
            sb.Append(@"</div>");
            return sb.ToString();
        }

        private string RenderAnsweredVariants(List<AnswerViarint> variants, string statBarCSSClass, string liderBarCSSClass, string variantNameCSSClass)
        {
            const float width = 70;
            double k = 0;
            double userCount;
            double max = 0, fullCount = 0;

            foreach (var variant in variants)
            {
                userCount = variant.VoteCount;
                fullCount += userCount;
                if (max < userCount)
                    max = userCount;
            }

            if (max != 0)
                k = width/max;

            var sb = new StringBuilder();

            foreach (var variant in variants)
            {
                userCount = variant.VoteCount;

                sb.AppendFormat(@"<div class=""{0}"">", variantNameCSSClass);
                sb.AppendFormat("<b>{0}</b>", HttpUtility.HtmlEncode(variant.Name));
                sb.Append(@"<span class=""splitter""></span>");
                sb.AppendFormat(@"<span class=""gray-text"">{0} ({1}%)</span>", userCount, fullCount != 0 ? Math.Round((userCount*100)/fullCount) : 0);
                sb.Append("</div>");
                sb.AppendFormat(@"<div class=""{1}"" style=""width:{0}%;"">&nbsp;</div>", Math.Round(k*userCount), max == userCount ? liderBarCSSClass : statBarCSSClass);
                sb.Append(@"<div style=""clear:both;"">&nbsp;</div>");
            }
            sb.AppendFormat(@"<div class=""{2}"">{0}: {1}</div>", UserControlsCommonResource.AllVoting, fullCount, variantNameCSSClass);

            return sb.ToString();
        }

        public class AnswerViarint
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public int VoteCount { get; set; }
        }

        private class VoteResult
        {
            public string Message { get; set; }
            public string Success { get; set; }
            public string HTML { get; set; }
            public string PollID { get; set; }
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public object Vote(string voteHandlerTypeName, string pollID, string variantIDs, List<AnswerViarint> allVariants,
                           string statBarCSSClass, string liderBarCSSClass, string variantNameCSSClass, string voteCountCSSClass,
                           string additionalParams)
        {
            var result = new VoteResult { PollID = pollID };

            var voteHandler = (IVoteHandler)Activator.CreateInstance(Type.GetType(voteHandlerTypeName));
            var selectedVariantIDs = new List<string>();

            selectedVariantIDs.AddRange(variantIDs.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));

            string errorMessage;
            if (voteHandler.VoteCallback(pollID, selectedVariantIDs, additionalParams, out errorMessage))
            {
                result.Success = "1";
                foreach (var var in allVariants)
                {
                    var selectedID = selectedVariantIDs.Find(id => String.Equals(id, var.ID, StringComparison.InvariantCultureIgnoreCase));
                    if (!String.IsNullOrEmpty(selectedID))
                        var.VoteCount++;
                }

                allVariants.ForEach(v => v.Name = HttpUtility.HtmlDecode(v.Name));

                result.HTML = RenderAnsweredVariants(allVariants, statBarCSSClass, liderBarCSSClass, variantNameCSSClass);
            }
            else
            {
                result.Success = "0";
                result.Message = HttpUtility.HtmlEncode(errorMessage);
            }

            return result;
        }

        public static string ReplaceSingleQuote(string str)
        {
            return str == null ? null : str.Replace("'", "′");
        }
    }
}