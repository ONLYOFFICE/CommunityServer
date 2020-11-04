using System;
using System.Globalization;
using ASC.Web.Studio.Core;
using ASC.Web.UserControls.Wiki.Handlers;
using ASC.Data.Storage;
using System.IO;
using ASC.Core;
using ASC.Web.Core;
using ASC.Web.UserControls.Wiki.Resources;

namespace ASC.Web.UserControls.Wiki.UC
{
    public class FileUploadResult
    {
        public FileUploadResult()
        {
            ErrorText = string.Empty;
            WebPath = string.Empty;
            LocalPath = string.Empty;
        }

        public string WebPath { get; set; }
        public string LocalPath { get; set; }
        public string ErrorText { get; set; }
    }

    public partial class FileUploader : BasePage
    {
        public static long MaxUploadSize
        {
            get { return SetupInfo.MaxUploadSize; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Clear();
            var result = new FileUploadResult();
            if (Request.Files.Count > 0 && !string.IsNullOrEmpty(Request["hfUserID"]))
            {
                try
                {
                    var file = Request.Files[0];
                    var content = new byte[file.ContentLength];
                    var fileExt = Path.GetExtension(file.FileName);
                    var localPath = Guid.NewGuid() + fileExt;

                    if (content.Length > MaxUploadSize && MaxUploadSize > 0)
                    {
                        result.ErrorText = WikiUCResource.wikiErrorFileSizeLimitText;
                    }
                    else
                    {
                        file.InputStream.Read(content, 0, file.ContentLength);
                        result.WebPath = TempFileContentSave(content, localPath);
                        result.LocalPath = localPath;
                    }

                    Response.StatusCode = 200;
                    Response.Write(AjaxPro.JavaScriptSerializer.Serialize(result));
                }
                catch (Exception)
                {
                }
            }
            Response.End();
        }

        private static string TempFileContentSave(byte[] fileContent, string filaLocation)
        {
            var tenantId = CoreContext.TenantManager.GetCurrentTenant().TenantId.ToString(CultureInfo.InvariantCulture);
            var storage = StorageFactory.GetStorage(tenantId, WikiSection.Section.DataStorage.ModuleName);
            string result;

            using (var ms = new MemoryStream(fileContent))
            {
                result = storage.Save(WikiSection.Section.DataStorage.TempDomain, filaLocation, ms).ToString();
            }

            return result;
        }
    }
}