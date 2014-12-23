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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxPro;
using ASC.Core;
using ASC.Core.Users;
using ASC.MessagingSystem;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.UserControls.Users
{
    public class ThumbnailItem
    {
        public string id { get; set; }
        public Size size { get; set; }
        public string imgUrl { get; set; }
        public Bitmap bitmap { get; set; }
    }

    public interface IThumbnailsData
    {
        Guid UserID { get; set; }
        string MainImgUrl { get; }
        Bitmap MainImgBitmap { get; }
        List<ThumbnailItem> ThumbnailList { get; }
        void Save(List<ThumbnailItem> bitmaps);
    }

    [AjaxNamespace("ThumbnailEditor")]
    public partial class ThumbnailEditor : UserControl
    {
        #region Propertie

        public static string Location
        {
            get { return "~/UserControls/Users/ThumbnailEditor/ThumbnailEditor.ascx"; }
        }

        public Type SaveFunctionType { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string BehaviorID { get; set; }

        public Size JcropMinSize { get; set; }

        public Size JcropMaxSize { get; set; }

        public double JcropAspectRatio { get; set; }

        #endregion

        #region Members

        protected string MainImgUrl
        {
            get { return GetThumbnailsData(SaveFunctionType).MainImgUrl; }
        }

        private string JsObjName
        {
            get { return String.IsNullOrEmpty(BehaviorID) ? "__thumbnailEditor" + UniqueID : BehaviorID; }
        }

        protected string SelectorID = Guid.NewGuid().ToString().Replace('-', '_');

        #endregion

        #region Events

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            _container.Options.IsPopup = true;

            if (SaveFunctionType == null)
                return;

            var saveClass = GetThumbnailsData(SaveFunctionType);

            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/users/thumbnaileditor/js/thumbnaileditor.js"));
            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/users/thumbnaileditor/js/jquery.Jcrop.js"));
            Page.RegisterStyleControl(VirtualPathUtility.ToAbsolute("~/usercontrols/users/thumbnaileditor/css/thumbnaileditor.less"));

            var script = new StringBuilder();
            script.Append("window." + JsObjName + " = new ASC.Studio.ThumbnailEditor.ThumbnailEditorPrototype('" + SelectorID + "','" + JsObjName + "'); ");

            var sb = new StringBuilder();
            sb.Append("<table width='100%'><tr>");

            foreach (var item in saveClass.ThumbnailList)
            {
                sb.AppendFormat(@"<td valign='top'><div class='thumbnailImg' style='height:{0}px; width:{1}px'>
													<img id='preview_{3}' src='{2}'/>
												</div></td>",
                                item.size.Height,
                                item.size.Width,
                                item.imgUrl,
                                SelectorID);

                script.AppendFormat(" {0}.ThumbnailItems.push(new ASC.Studio.ThumbnailEditor.ThumbnailItem({1}, {2}, '{3}')); ",
                                    JsObjName,
                                    item.size.Height,
                                    item.size.Width,
                                    item.imgUrl);
            }

            sb.Append("</tr></table>");

            placeThumbnails.Controls.Add(new Literal {Text = sb.ToString()});

            script.AppendFormat(" {0}.JcropMinSize = [ {1}, {2} ]; ", JsObjName, JcropMinSize.Width, JcropMinSize.Height);

            script.AppendFormat(" {0}.JcropMaxSize = [ {1}, {2} ]; ", JsObjName, JcropMaxSize.Width, JcropMaxSize.Height);

            script.AppendFormat(" {0}.JcropAspectRatio = {1}; ", JsObjName, JcropAspectRatio);

            script.AppendFormat(" {0}.SaveThumbnailsFunction = '{1}'; ", JsObjName, SaveFunctionType.FullName);

            Page.RegisterInlineScript(script.ToString());
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(GetType());
        }

        #endregion

        #region Methods

        public ThumbnailEditor()
        {
            Description = Resources.Resource.DescriptionThumbnail;
            Title = Resources.Resource.TitleThumbnailPhoto;
        }

        #region Thumbnails Data factory

        private IThumbnailsData GetThumbnailsData(string saveFunctionType, Guid userID)
        {
            var saveFunction = Type.GetType(saveFunctionType);
            return GetThumbnailsData(saveFunction, userID);
        }

        private IThumbnailsData GetThumbnailsData(Type saveFunctionType)
        {
            return GetThumbnailsData(saveFunctionType, Guid.Empty);
        }

        private IThumbnailsData GetThumbnailsData(Type saveFunctionType, Guid userID)
        {
            var thumb = (IThumbnailsData)Activator.CreateInstance(saveFunctionType);
            thumb.UserID = Guid.Empty.Equals(userID) ? UserID : userID;
            return thumb;
        }

        protected Guid UserID
        {
            get
            {
                var userID = CoreContext.UserManager.GetUserByUserName(HttpContext.Current.Request[CommonLinkUtility.ParamName_UserUserName]).ID;
                return !Constants.LostUser.ID.Equals(userID) ? userID : SecurityContext.CurrentAccount.ID;
            }
        }

        #endregion

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public List<string> SaveThumbnails(int x, int y, int width, int height, string saveFunctionType, Guid userID)
        {
            if (x < 0 || y < 0 || width <= 0 || height <= 0) return null;

            var pointSelect = new Point(x, y);
            var sizeSelect = new Size(width, height);

            var saveClass = GetThumbnailsData(saveFunctionType, userID);

            var resaltBitmaps = new List<ThumbnailItem>();

            System.Drawing.Image img = saveClass.MainImgBitmap;
            if (img == null) return null;

            foreach (var thumbnail in saveClass.ThumbnailList)
            {
                var thumbnailBitmap = new Bitmap(thumbnail.size.Width, thumbnail.size.Height);

                var scaleX = thumbnail.size.Width / (1.0 * sizeSelect.Width);
                var scaleY = thumbnail.size.Height / (1.0 * sizeSelect.Height);

                var rect = new Rectangle(-(int)(scaleX * pointSelect.X),
                                         -(int)(scaleY * pointSelect.Y),
                                         (int)(scaleX * img.Width),
                                         (int)(scaleY * img.Height));

                using (var graphic = Graphics.FromImage(thumbnailBitmap))
                {
                    graphic.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    graphic.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                    graphic.FillRectangle(new SolidBrush(Color.Black), 0, 0, thumbnail.size.Width, thumbnail.size.Height);
                    graphic.DrawImage(img, rect);
                }
                thumbnail.bitmap = thumbnailBitmap;

                resaltBitmaps.Add(thumbnail);
            }

            saveClass.Save(resaltBitmaps);

            var user = CoreContext.UserManager.GetUsers(userID);
            MessageService.Send(HttpContext.Current.Request, MessageAction.UserUpdatedAvatarThumbnails, user.DisplayUserName(false));

            return saveClass.ThumbnailList.Select(t => t.imgUrl).ToList();
        }

        #endregion
    }
}