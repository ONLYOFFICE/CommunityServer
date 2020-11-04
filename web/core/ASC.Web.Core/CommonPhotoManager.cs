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
                        graphic.SmoothingMode = SmoothingMode.HighQuality;
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
                        graphic.SmoothingMode = SmoothingMode.HighQuality;
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
