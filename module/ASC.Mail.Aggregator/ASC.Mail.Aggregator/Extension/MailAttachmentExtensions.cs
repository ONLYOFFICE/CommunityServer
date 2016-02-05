using System.IO;
using ActiveUp.Net.Mail;
using ASC.Common.Web;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Extension;
using ASC.Mail.Aggregator.DataStorage;
using ASC.Mail.Aggregator.Managers;

namespace ASC.Mail.Aggregator.Extension
{
    public static class MailAttachmentExtensions
    {
        public static MimePart ToMimePart(this MailAttachment attachment, bool loadAttachments)
        {
            var retVal = new MimePart();

            var s3Key = MailStoragePathCombiner.GerStoredFilePath(attachment);
            var fileName = attachment.fileName ?? Path.GetFileName(s3Key);

            if (loadAttachments)
            {
                var byteArray = attachment.data;

                if (byteArray == null || byteArray.Length == 0)
                {
                    using (var stream = StorageManager.GetDataStoreForAttachments(attachment.tenant).GetReadStream(s3Key))
                    {
                        byteArray = stream.ReadToEnd();
                    }
                }

                retVal = new MimePart(byteArray, fileName);

                if (!string.IsNullOrEmpty(attachment.contentId)) retVal.ContentId = attachment.contentId;
            }
            else
            {
                var conentType = MimeMapping.GetMimeMapping(s3Key);
                retVal.ContentType = new ContentType { Type = conentType };
                retVal.Filename = fileName;
                if (attachment.contentId != null) retVal.ContentId = attachment.contentId;
                retVal.TextContent = "";
            }

            return retVal;
        }
    }
}
