/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using System.Drawing;

namespace ASC.Web.Core.Users
{
    public class UserPhotoThumbnailManager
    {
        public static List<ThumbnailItem> SaveThumbnails(int x, int y, int width, int height, Guid userId)
        {
            return SaveThumbnails(new UserPhotoThumbnailSettings(x, y, width, height), userId);
        }

        public static List<ThumbnailItem> SaveThumbnails(Point point, Size size, Guid userId)
        {
            return SaveThumbnails(new UserPhotoThumbnailSettings(point, size), userId);
        }

        public static List<ThumbnailItem> SaveThumbnails(UserPhotoThumbnailSettings thumbnailSettings, Guid userId)
        {
            if (thumbnailSettings.Size.IsEmpty) return null;

            var thumbnailsData = new ThumbnailsData(userId);

            var resultBitmaps = new List<ThumbnailItem>();

            var img = thumbnailsData.MainImgBitmap;

            if (img == null) return null;

            foreach (var thumbnail in thumbnailsData.ThumbnailList)
            {
                thumbnail.Bitmap = GetBitmap(img, thumbnail.Size, thumbnailSettings);

                resultBitmaps.Add(thumbnail);
            }

            thumbnailsData.Save(resultBitmaps);

            thumbnailSettings.SaveForUser(userId);

            return thumbnailsData.ThumbnailList;
        }

        public static Bitmap GetBitmap(Image mainImg, Size size, UserPhotoThumbnailSettings thumbnailSettings)
        {
            var thumbnailBitmap = new Bitmap(size.Width, size.Height);

            var scaleX = size.Width/(1.0*thumbnailSettings.Size.Width);
            var scaleY = size.Height/(1.0*thumbnailSettings.Size.Height);

            var rect = new Rectangle(-(int) (scaleX*(1.0*thumbnailSettings.Point.X)),
                                     -(int) (scaleY*(1.0*thumbnailSettings.Point.Y)),
                                     (int) (scaleX*mainImg.Width),
                                     (int) (scaleY*mainImg.Height));

            using (var graphic = Graphics.FromImage(thumbnailBitmap))
            {
                graphic.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphic.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                graphic.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphic.DrawImage(mainImg, rect);

                using (var wrapMode = new System.Drawing.Imaging.ImageAttributes())
                {
                    wrapMode.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
                    graphic.DrawImage(mainImg, rect, 0, 0, mainImg.Width, mainImg.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return thumbnailBitmap;
        }
    }

    public class ThumbnailItem
    {
        public Size Size { get; set; }
        public string ImgUrl { get; set; }
        public Bitmap Bitmap { get; set; }
    }

    public class ThumbnailsData
    {
        private Guid UserId { get; set; }

        public ThumbnailsData(Guid userId)
        {
            UserId = userId;
        }

        public Bitmap MainImgBitmap
        {
            get { return UserPhotoManager.GetPhotoBitmap(UserId); }
        }

        public string MainImgUrl
        {
            get { return UserPhotoManager.GetPhotoAbsoluteWebPath(UserId); }
        }

        public List<ThumbnailItem> ThumbnailList
        {
            get
            {
                return new List<ThumbnailItem>
                    {
                        new ThumbnailItem
                            {
                                Size = UserPhotoManager.RetinaFotoSize,
                                ImgUrl = UserPhotoManager.GetRetinaPhotoURL(UserId)
                            },
                        new ThumbnailItem
                            {
                                Size = UserPhotoManager.MaxFotoSize,
                                ImgUrl = UserPhotoManager.GetMaxPhotoURL(UserId)
                            },
                        new ThumbnailItem
                            {
                                Size = UserPhotoManager.BigFotoSize,
                                ImgUrl = UserPhotoManager.GetBigPhotoURL(UserId)
                            },
                        new ThumbnailItem
                            {
                                Size = UserPhotoManager.MediumFotoSize,
                                ImgUrl = UserPhotoManager.GetMediumPhotoURL(UserId)
                            },
                        new ThumbnailItem
                            {
                                Size = UserPhotoManager.SmallFotoSize,
                                ImgUrl = UserPhotoManager.GetSmallPhotoURL(UserId)
                            }
                    };
            }
        }

        public void Save(List<ThumbnailItem> bitmaps)
        {
            foreach (var item in bitmaps)
                UserPhotoManager.SaveThumbnail(UserId, item.Bitmap, MainImgBitmap.RawFormat);
        }
    }
}