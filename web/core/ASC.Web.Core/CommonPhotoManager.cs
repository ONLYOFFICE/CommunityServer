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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace ASC.Web.Core
{
    public class CommonPhotoManager
    {

        public static Image DoThumbnail(Image image, Size size, bool crop, bool transparent, bool rectangle)
        {
            var width = size.Width;
            var height = size.Height;
            var realWidth = image.Width;
            var realHeight = image.Height;

            var thumbnail = new Bitmap(width, height);

            var maxSide = realWidth > realHeight ? realWidth : realHeight;
            var minSide = realWidth < realHeight ? realWidth : realHeight;

            var alignWidth = true;
            if (crop) alignWidth = (minSide == realWidth);
            else alignWidth = (maxSide == realWidth);

            double scaleFactor = (alignWidth) ? (realWidth / (1.0 * width)) : (realHeight / (1.0 * height));

            if (scaleFactor < 1) scaleFactor = 1;

            int locationX, locationY;
            int finalWidth, finalHeigth;

            finalWidth = (int)(realWidth / scaleFactor);
            finalHeigth = (int)(realHeight / scaleFactor);


            if (rectangle)
            {
                locationY = (int)((height / 2.0) - (finalHeigth / 2.0));
                locationX = (int)((width / 2.0) - (finalWidth / 2.0));

                var rect = new Rectangle(locationX, locationY, finalWidth, finalHeigth);

                using (var graphic = Graphics.FromImage(thumbnail))
                {
                    if (!transparent)
                    {
                        graphic.Clear(Color.White);
                        graphic.SmoothingMode = SmoothingMode.AntiAlias;
                    }
                    graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphic.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    using (ImageAttributes wrapMode = new ImageAttributes())
                    {
                        wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                        graphic.DrawImage(image, rect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                    }
                    
                    //graphic.DrawImage(image, rect);
                }
            }
            else
            {
                thumbnail = new Bitmap(finalWidth, finalHeigth);

                using (var graphic = Graphics.FromImage(thumbnail))
                {
                    if (!transparent)
                    {
                        graphic.Clear(Color.White);
                        graphic.SmoothingMode = SmoothingMode.AntiAlias;
                    }
                    graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    graphic.DrawImage(image, 0, 0, finalWidth, finalHeigth);
                }
            }

            return thumbnail;
        }

        public static byte[] SaveToBytes(Image img)
        {
            using (var memoryStream = new MemoryStream())
            {
                img.Save(memoryStream, ImageFormat.Png);
                return memoryStream.ToArray();
            }
        }

        public static byte[] SaveToBytes(Image img, String formatName)
        {
            byte[] data;
            using (var memoryStream = new MemoryStream())
            {
                var encParams = new EncoderParameters(1);
                encParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)100);
                img.Save(memoryStream, GetCodecInfo(formatName), encParams);
                data = memoryStream.ToArray();
            }
            return data;
        }

        public static ImageCodecInfo GetCodecInfo(String formatName)
        {
            var mimeType = string.Format("image/{0}", formatName);
            if (mimeType == "image/jpg") mimeType = "image/jpeg";
            var encoders = ImageCodecInfo.GetImageEncoders();
            foreach (var e in
                encoders.Where(e => e.MimeType.Equals(mimeType, StringComparison.InvariantCultureIgnoreCase)))
            {
                return e;
            }
            return 0 < encoders.Length ? encoders[0] : null;
        }

        public static string GetImgFormatName(ImageFormat format)
        {
            if (format.Equals(ImageFormat.Bmp)) return "bmp";
            if (format.Equals(ImageFormat.Emf)) return "emf";
            if (format.Equals(ImageFormat.Exif)) return "exif";
            if (format.Equals(ImageFormat.Gif)) return "gif";
            if (format.Equals(ImageFormat.Icon)) return "icon";
            if (format.Equals(ImageFormat.Jpeg)) return "jpeg";
            if (format.Equals(ImageFormat.Png)) return "png";
            if (format.Equals(ImageFormat.Tiff)) return "tiff";
            if (format.Equals(ImageFormat.Wmf)) return "wmf";
            return "jpg";
        }
    }
}
