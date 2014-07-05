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
using System.IO;
using System.Web;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Studio;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.UserControls.Users.UserProfile;
using ASC.Web.Core.Users;
using ASC.Web.Core.Utility;
using ASC.Web.People.Resources;

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
                    var data = new byte[userPhoto.InputStream.Length];

                    var br = new BinaryReader(userPhoto.InputStream);
                    br.Read(data, 0, (int)userPhoto.InputStream.Length);
                    br.Close();

                    if (context.Request["autosave"] == "true")
                    {
                        var mainPhoto = UserPhotoManager.SaveOrUpdatePhoto(userId, data);
                        result.Data =
                            new
                                {
                                    main = mainPhoto,
                                    big = UserPhotoManager.GetBigPhotoURL(userId),
                                    medium = UserPhotoManager.GetMediumPhotoURL(userId),
                                    small = UserPhotoManager.GetSmallPhotoURL(userId),
                                };
                    }
                    else
                    {
                        result.Data = UserPhotoManager.SaveTempPhoto(data, SetupInfo.MaxImageUploadSize, UserPhotoManager.MaxFotoSize.Width, UserPhotoManager.MaxFotoSize.Height);
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
    }

    public partial class ProfileAction : MainPage
    {
        public ProfileHelper ProfileHelper;

        protected bool IsPageEditProfile()
        {
            return (Request["action"] == "edit");
        }

        protected bool IsAdmin()
        {
            return CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsAdmin();
        }

        private static bool CanEdit()
        {
            var curUser = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
            return curUser.IsAdmin() || curUser.IsOwner();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            ProfileHelper = new ProfileHelper(Request["user"]);
            var userInfo = ProfileHelper.UserInfo;

            if (userInfo.IsMe() && userInfo.IsVisitor() && IsPageEditProfile())
            {
                Response.Redirect("/my.aspx?action=edit");
            }

            if ((IsPageEditProfile() && !(userInfo.IsMe() || CanEdit()))
                || (!IsPageEditProfile() && !IsAdmin()))
            {
                Response.Redirect("~/products/people/", true);
            }

            var userProfileEditControl = LoadControl(UserProfileEditControl.Location) as UserProfileEditControl;


            _contentHolderForEditForm.Controls.Add(userProfileEditControl);
        }
    }
}