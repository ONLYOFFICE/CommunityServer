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