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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Web;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core;
using ASC.Web.Studio;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.UserControls.Users.UserProfile;
using ASC.Web.Core.Users;
using ASC.Web.Core.Utility;
using ASC.Web.People.Resources;
using Resources;

namespace ASC.Web.People
{
    internal class UserPhotoUploader : IFileUploadHandler
    {
        public FileUploadResult ProcessUpload(HttpContext context)
        {
            var result = new FileUploadResult();
            try
            {
                if (context.Request.Files.Count != 0)
                {
                    Guid userId;
                    try
                    {
                        userId = new Guid(context.Request["userId"]);
                    }
                    catch 
                    {
                        userId = SecurityContext.CurrentAccount.ID;
                    }
                    SecurityContext.DemandPermissions(new UserSecurityProvider(userId), Constants.Action_EditUser);

                    var userPhoto = context.Request.Files[0];

                    if (userPhoto.InputStream.Length > SetupInfo.MaxImageUploadSize)
                    {
                        result.Success = false;
                        result.Message = FileSizeComment.FileImageSizeExceptionString;
                        return result;
                    }
                    
                    var data = new byte[userPhoto.InputStream.Length];

                    var br = new BinaryReader(userPhoto.InputStream);
                    br.Read(data, 0, (int)userPhoto.InputStream.Length);
                    br.Close();

                    CheckImgFormat(data);

                    if (context.Request["autosave"] == "true")
                    {
                        if (data.Length > SetupInfo.MaxImageUploadSize)
                            throw new ImageSizeLimitException();

                        var mainPhoto = UserPhotoManager.SaveOrUpdatePhoto(userId, data);

                        result.Data =
                            new
                                {
                                    main = mainPhoto,
                                    retina = UserPhotoManager.GetRetinaPhotoURL(userId),
                                    max = UserPhotoManager.GetMaxPhotoURL(userId),
                                    big = UserPhotoManager.GetBigPhotoURL(userId),
                                    medium = UserPhotoManager.GetMediumPhotoURL(userId),
                                    small = UserPhotoManager.GetSmallPhotoURL(userId),
                                };
                    }
                    else
                    {
                        result.Data = UserPhotoManager.SaveTempPhoto(data, SetupInfo.MaxImageUploadSize, UserPhotoManager.OriginalFotoSize.Width, UserPhotoManager.OriginalFotoSize.Height);
                    }

                    result.Success = true;
                }
                else
                {
                    result.Success = false;
                    result.Message = PeopleResource.ErrorEmptyUploadFileSelected;
                }

            }
            catch (UnknownImageFormatException)
            {
                result.Success = false;
                result.Message = PeopleResource.ErrorUnknownFileImageType;
            }
            catch (ImageWeightLimitException)
            {
                result.Success = false;
                result.Message = PeopleResource.ErrorImageWeightLimit;
            }
            catch (ImageSizeLimitException)
            {
                result.Success = false;
                result.Message = PeopleResource.ErrorImageSizetLimit;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message.HtmlEncode();
            }

            return result;
        }

        private static void CheckImgFormat(byte[] data)
        {
            ImageFormat imgFormat;

            try
            {
                using (var stream = new MemoryStream(data))
                using (var img = new Bitmap(stream))
                {
                    imgFormat = img.RawFormat;
                }
            }
            catch (OutOfMemoryException)
            {
                throw new ImageSizeLimitException();
            }
            catch (ArgumentException error)
            {
                throw new UnknownImageFormatException(error);
            }

            if (!imgFormat.Equals(ImageFormat.Png) && !imgFormat.Equals(ImageFormat.Jpeg))
            {
                throw new UnknownImageFormatException();
            }
        }
    }

    public partial class ProfileAction : MainPage
    {
        public ProfileHelper ProfileHelper;

        protected string PageTitle { get; private set; }

        protected bool IsPageEditProfile()
        {
            return (Request["action"] == "edit");
        }

        protected bool IsAdmin()
        {
            return CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsAdmin() ||
                WebItemSecurity.IsProductAdministrator(WebItemManager.PeopleProductID, SecurityContext.CurrentAccount.ID);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            ProfileHelper = new ProfileHelper(Request["user"]);

            var userInfo = ProfileHelper.UserInfo;

            if (userInfo.IsMe() && userInfo.IsVisitor() && IsPageEditProfile())
            {
                Response.Redirect("/My.aspx?action=edit");
            }

            if (IsPageEditProfile() ? !userInfo.IsMe() && (!IsAdmin() || userInfo.IsOwner()) : !IsAdmin())
            {
                Response.Redirect("~/Products/People/", true);
            }

            PageTitle = IsPageEditProfile() ? userInfo.DisplayUserName(false) + " - " + Resource.EditUserDialogTitle : Resource.CreateNewProfile;

            var userProfileEditControl = LoadControl(UserProfileEditControl.Location) as UserProfileEditControl;

            _contentHolderForEditForm.Controls.Add(userProfileEditControl);
        }
    }
}