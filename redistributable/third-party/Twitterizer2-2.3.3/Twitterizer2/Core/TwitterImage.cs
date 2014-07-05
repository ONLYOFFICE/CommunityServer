namespace Twitterizer
{
    using System;
    using System.IO;

    /// <summary>
    /// The image type that is being uploaded.
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    public enum TwitterImageImageType
    {
        /// <summary>
        /// JPEG
        /// </summary>
        Jpeg,

        /// <summary>
        /// GIF
        /// </summary>
        Gif,
        
        /// <summary>
        /// PNG
        /// </summary>
        PNG
    }

    /// <summary>
    /// Represents an image for uploading. Used to upload new profile and background images.
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    public class TwitterImage
    {
        /// <summary>
        /// Gets or sets the filename.
        /// </summary>
        /// <value>The filename.</value>
        public string Filename { get; set; }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>The data.</value>
        public byte[] Data { get; set; }

        /// <summary>
        /// Gets or sets the type of the image.
        /// </summary>
        /// <value>The type of the image.</value>
        public TwitterImageImageType ImageType { get; set; }

        /// <summary>
        /// Gets the image's MIME type.
        /// </summary>
        /// <returns></returns>
        public string GetMimeType()
        {
            switch (this.ImageType)
            {
                case TwitterImageImageType.Jpeg:
                    return "image/jpeg";
                case TwitterImageImageType.Gif:
                    return "image/gif";
                case TwitterImageImageType.PNG:
                    return "image/png";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Reads a file from the disk and returns a <see cref="TwitterImage"/> instance for uploading.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        public static TwitterImage ReadFromDisk(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new ArgumentException(string.Format("File does not be exist: {0}.", filePath));
            }

            TwitterImage newImage = new TwitterImage();
            newImage.Data = File.ReadAllBytes(filePath);

            FileInfo imageFileInfo = new FileInfo(filePath);
            newImage.Filename = imageFileInfo.Name;

            switch (imageFileInfo.Extension.ToLower())
            {
                case ".jpg":
                case ".jpeg":
                    newImage.ImageType = TwitterImageImageType.Jpeg;
                    break;
                case ".gif":
                    newImage.ImageType = TwitterImageImageType.Gif;
                    break;
                case ".png":
                    newImage.ImageType = TwitterImageImageType.PNG;
                    break;
                default:
                    throw new Exception("File is not a recognized type. Must be jpg, png, or gif.");
            }

            return newImage;
        }
    }
}
