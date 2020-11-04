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


using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using ASC.Data.Storage;
using System;

namespace ASC.Web.Studio.Helpers
{
    public class ImageInfo
    {
        #region Members

        private string name;

        private int originalWidth;
        private int originalHeight;
        private int previewWidth;
        private int previewHeight;
        private int thumbnailWidth;
        private int thumbnailHeight;

        private string originalPath;
        private string previewPath;
        private string thumbnailPath;

        private string actionDate;

        #endregion

        #region Properties

        public virtual string Name
        {
            get { return name; }
            set { name = value; }
        }

        public virtual int OriginalWidth
        {
            get { return originalWidth; }
            set { originalWidth = value; }
        }
        public virtual int OriginalHeight
        {
            get { return originalHeight; }
            set { originalHeight = value; }
        }
        public virtual int PreviewWidth
        {
            get { return previewWidth; }
            set { previewWidth = value; }
        }
        public virtual int PreviewHeight
        {
            get { return previewHeight; }
            set { previewHeight = value; }
        }
        public virtual int ThumbnailWidth
        {
            get { return thumbnailWidth; }
            set { thumbnailWidth = value; }
        }
        public virtual int ThumbnailHeight
        {
            get { return thumbnailHeight; }
            set { thumbnailHeight = value; }
        }

        public virtual string OriginalPath
        {
            get { return originalPath; }
            set { originalPath = value; }
        }
        public virtual string PreviewPath
        {
            get { return previewPath; }
            set { previewPath = value; }
        }
        public virtual string ThumbnailPath
        {
            get { return thumbnailPath; }
            set { thumbnailPath = value; }
        }

        public virtual string ActionDate
        {
            get { return actionDate; }
            set { actionDate = value; }
        }

        #endregion
    }

    public class ThumbnailGenerator
    {
        bool _crop = false;
        int _width;
        int _heigth;
        int _widthPreview;
        int _heightPreview;

        public IDataStore store
        {
            get;
            set;
        }

        public ThumbnailGenerator(string tmpPath, bool crop, int width, int heigth, int widthPreview, int heightPreview)
        {
            _crop = crop;
            _width = width;
            _heigth = heigth;
            _widthPreview = widthPreview;
            _heightPreview = heightPreview;
        }

        public void DoThumbnail(string path, string outputPath, ref ImageInfo imageInfo)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                DoThumbnail(fs, outputPath, ref imageInfo);
            }
        }
        public void DoThumbnail(Stream image, string outputPath, ref ImageInfo imageInfo)
        {
            using (Image img = Image.FromStream(image))
            {
                DoThumbnail(img, outputPath, ref imageInfo);
            }
        }
        public void DoThumbnail(Image image, string outputPath, ref ImageInfo imageInfo)
        {
            int realWidth = image.Width;
            int realHeight = image.Height;

            imageInfo.OriginalWidth = realWidth;
            imageInfo.OriginalHeight = realHeight;

            EncoderParameters ep = new EncoderParameters(1);

            ep.Param[0] = new EncoderParameter(Encoder.Quality, (long)90);

            ImageCodecInfo icJPG = getCodecInfo("image/jpeg");

            if (!String.IsNullOrEmpty(imageInfo.Name) && imageInfo.Name.Contains("."))
            {
                int indexDot = imageInfo.Name.ToLower().LastIndexOf(".");

                if (imageInfo.Name.ToLower().IndexOf("png", indexDot) > indexDot)
                    icJPG = getCodecInfo("image/png");
                else if (imageInfo.Name.ToLower().IndexOf("gif", indexDot) > indexDot)
                    icJPG = getCodecInfo("image/png");
            }
            Bitmap thumbnail;

            if (realWidth < _width && realHeight < _heigth)
            {
                imageInfo.ThumbnailWidth = realWidth;
                imageInfo.ThumbnailHeight = realHeight;

                if (store == null)
                    image.Save(outputPath);
                else
                {

                    MemoryStream ms = new MemoryStream();
                    image.Save(ms, icJPG, ep);
                    ms.Seek(0, SeekOrigin.Begin);
                    store.Save(outputPath, ms);
                    ms.Dispose();
                }
                return;
            }
            else
            {
                thumbnail = new Bitmap(_width < realWidth ? _width : realWidth, _heigth < realHeight ? _heigth : realHeight);

                int maxSide = realWidth > realHeight ? realWidth : realHeight;
                int minSide = realWidth < realHeight ? realWidth : realHeight;

                bool alignWidth = true;
                if (_crop)
                    alignWidth = (minSide == realWidth);
                else
                    alignWidth = (maxSide == realWidth);

                double scaleFactor = (alignWidth) ? (realWidth / (1.0 * _width)) : (realHeight / (1.0 * _heigth));

                if (scaleFactor < 1) scaleFactor = 1;

                int locationX, locationY;
                int finalWidth, finalHeigth;

                finalWidth = (int)(realWidth / scaleFactor);
                finalHeigth = (int)(realHeight / scaleFactor);


                locationY = (int)(((_heigth < realHeight ? _heigth : realHeight) / 2.0) - (finalHeigth / 2.0));
                locationX = (int)(((_width < realWidth ? _width : realWidth) / 2.0) - (finalWidth / 2.0));

                Rectangle rect = new Rectangle(locationX, locationY, finalWidth, finalHeigth);

                imageInfo.ThumbnailWidth = thumbnail.Width;
                imageInfo.ThumbnailHeight = thumbnail.Height;


                using (Graphics graphic = Graphics.FromImage(thumbnail))
                {
                    graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    graphic.SmoothingMode = SmoothingMode.HighQuality;
                    graphic.DrawImage(image, rect);
                }
            }

            if (store == null)
                thumbnail.Save(outputPath, icJPG, ep);
            else
            {

                MemoryStream ms = new MemoryStream();
                thumbnail.Save(ms, icJPG, ep);
                ms.Seek(0, SeekOrigin.Begin);
                store.Save(outputPath, ms);
                ms.Dispose();
            }

            thumbnail.Dispose();
        }



        public void DoPreviewImage(string path, string outputPath, ref ImageInfo imageInfo)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                DoPreviewImage(fs, outputPath, ref imageInfo);
            }
        }
        public void DoPreviewImage(Stream image, string outputPath, ref ImageInfo imageInfo)
        {
            using (Image img = Image.FromStream(image))
            {
                DoPreviewImage(img, outputPath, ref imageInfo);
            }
        }
        public void DoPreviewImage(Image image, string outputPath, ref ImageInfo imageInfo)
        {
            int realWidth = image.Width;
            int realHeight = image.Height;

            int heightPreview = realHeight;
            int widthPreview = realWidth;

            EncoderParameters ep = new EncoderParameters(1);
            ImageCodecInfo icJPG = getCodecInfo("image/jpeg");
            ep.Param[0] = new EncoderParameter(Encoder.Quality, (long)90);

            if (realWidth <= _widthPreview && realHeight <= _heightPreview)
            {
                imageInfo.PreviewWidth = widthPreview;
                imageInfo.PreviewHeight = heightPreview;

                if (store == null)
                    image.Save(outputPath);
                else
                {

                    MemoryStream ms = new MemoryStream();
                    image.Save(ms, icJPG, ep);
                    ms.Seek(0, SeekOrigin.Begin);
                    store.Save(outputPath, ms);
                    ms.Dispose();
                }

                return;
            }
            else if ((double)realHeight / (double)_heightPreview > (double)realWidth / (double)_widthPreview)
            {
                if (heightPreview > _heightPreview)
                {
                    widthPreview = (int)(realWidth * _heightPreview * 1.0 / realHeight + 0.5);
                    heightPreview = _heightPreview;
                }
            }
            else
            {
                if (widthPreview > _widthPreview)
                {
                    heightPreview = (int)(realHeight * _widthPreview * 1.0 / realWidth + 0.5);
                    widthPreview = _widthPreview;
                }
            }

            imageInfo.PreviewWidth = widthPreview;
            imageInfo.PreviewHeight = heightPreview;

            Bitmap preview = new Bitmap(widthPreview, heightPreview);

            using (Graphics graphic = Graphics.FromImage(preview))
            {
                graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphic.SmoothingMode = SmoothingMode.HighQuality;
                graphic.DrawImage(image, 0, 0, widthPreview, heightPreview);
            }

            if (store == null)
                preview.Save(outputPath, icJPG, ep);
            else
            {

                MemoryStream ms = new MemoryStream();
                preview.Save(ms, icJPG, ep);
                ms.Seek(0, SeekOrigin.Begin);
                store.Save(outputPath, ms);
                ms.Dispose();
            }

            preview.Dispose();
        }

        public void RotateImage(string path, string outputPath, bool back)
        {
            try
            {
                using(var stream = store.GetReadStream(path))
                using (var image = Image.FromStream(stream))
                {
                    if (back)
                    {
                        image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    }
                    else
                    {
                        image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    }

                    var ep = new EncoderParameters(1);
                    var icJPG = getCodecInfo("image/jpeg");
                    ep.Param[0] = new EncoderParameter(Encoder.Quality, (long)100);

                    if (store == null)
                    {
                        image.Save(outputPath, icJPG, ep);
                    }
                    else
                    {
                        using (var ms = new MemoryStream())
                        {
                            image.Save(ms, icJPG, ep);
                            ms.Seek(0, SeekOrigin.Begin);
                            store.Save(outputPath, ms);
                        }
                    }

                    store.Delete(path);
                }
            }
            catch { }
        }

        private static ImageCodecInfo getCodecInfo(string mt)
        {
            ImageCodecInfo[] ici = ImageCodecInfo.GetImageEncoders();
            int idx = 0;
            for (int ii = 0; ii < ici.Length; ii++)
            {
                if (ici[ii].MimeType == mt)
                {
                    idx = ii;
                    break;
                }
            }
            return ici[idx];
        }
    }

    public class ImageHelper
    {
        public const int maxSize = 200;
        public const int maxWidthPreview = 933;
        public const int maxHeightPreview = 700;

        public static void GenerateThumbnail(string path, string outputPath, ref ImageInfo imageInfo)
        {
            ThumbnailGenerator _generator = new ThumbnailGenerator(null, true,
                maxSize,
                maxSize,
                maxWidthPreview,
                maxHeightPreview);

            _generator.DoThumbnail(path, outputPath, ref imageInfo);
        }

        public static void GenerateThumbnail(string path, string outputPath, ref ImageInfo imageInfo, int maxWidth, int maxHeight)
        {
            ThumbnailGenerator _generator = new ThumbnailGenerator(null, true,
                maxWidth,
                maxHeight,
                maxWidthPreview,
                maxHeightPreview);

            _generator.DoThumbnail(path, outputPath, ref imageInfo);
        }
        public static void GenerateThumbnail(Stream stream, string outputPath, ref ImageInfo imageInfo, int maxWidth, int maxHeight)
        {
            ThumbnailGenerator _generator = new ThumbnailGenerator(null, true,
                maxWidth,
                maxHeight,
                maxWidthPreview,
                maxHeightPreview);

            _generator.DoThumbnail(stream, outputPath, ref imageInfo);
        }

        public static void GenerateThumbnail(string path, string outputPath, ref ImageInfo imageInfo, int maxWidth, int maxHeight, IDataStore store)
        {
            ThumbnailGenerator _generator = new ThumbnailGenerator(null, true,
                maxWidth,
                maxHeight,
                maxWidthPreview,
                maxHeightPreview);
            _generator.store = store;

            _generator.DoThumbnail(path, outputPath, ref imageInfo);
        }
        public static void GenerateThumbnail(Stream stream, string outputPath, ref ImageInfo imageInfo, int maxWidth, int maxHeight, IDataStore store)
        {
            ThumbnailGenerator _generator = new ThumbnailGenerator(null, true,
                maxWidth,
                maxHeight,
                maxWidthPreview,
                maxHeightPreview);
            _generator.store = store;

            _generator.DoThumbnail(stream, outputPath, ref imageInfo);
        }


        public static void GenerateThumbnail(Stream stream, string outputPath, ref ImageInfo imageInfo)
        {
            ThumbnailGenerator _generator = new ThumbnailGenerator(null, true,
                maxSize,
                maxSize,
                maxWidthPreview,
                maxHeightPreview);

            _generator.DoThumbnail(stream, outputPath, ref imageInfo);
        }

        public static void GenerateThumbnail(Stream stream, string outputPath, ref ImageInfo imageInfo, IDataStore store)
        {
            ThumbnailGenerator _generator = new ThumbnailGenerator(null, true,
                maxSize,
                maxSize,
                maxWidthPreview,
                maxHeightPreview);

            _generator.store = store;
            _generator.DoThumbnail(stream, outputPath, ref imageInfo);
        }


        public static void GeneratePreview(string path, string outputPath, ref ImageInfo imageInfo)
        {
            ThumbnailGenerator _generator = new ThumbnailGenerator(null, true,
                maxSize,
                maxSize,
                maxWidthPreview,
                maxHeightPreview);

            _generator.DoPreviewImage(path, outputPath, ref imageInfo);

        }
        public static void GeneratePreview(Stream stream, string outputPath, ref ImageInfo imageInfo)
        {
            ThumbnailGenerator _generator = new ThumbnailGenerator(null, true,
                maxSize,
                maxSize,
                maxWidthPreview,
                maxHeightPreview);

            _generator.DoPreviewImage(stream, outputPath, ref imageInfo);

        }
        public static void GeneratePreview(Stream stream, string outputPath, ref ImageInfo imageInfo, IDataStore store)
        {
            ThumbnailGenerator _generator = new ThumbnailGenerator(null, true,
                maxSize,
                maxSize,
                maxWidthPreview,
                maxHeightPreview);

            _generator.store = store;
            _generator.DoPreviewImage(stream, outputPath, ref imageInfo);

        }


        public static void RotateImage(string path, string outputPath, bool back, IDataStore store)
        {
            ThumbnailGenerator _generator = new ThumbnailGenerator(null, true,
                maxSize,
                maxSize,
                maxWidthPreview,
                maxHeightPreview);
            _generator.store = store;

            _generator.RotateImage(path, outputPath, back);
        }
    }
}
