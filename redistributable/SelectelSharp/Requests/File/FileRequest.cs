using SelectelSharp.Requests.Container;
using System;
using System.Text;

namespace SelectelSharp.Requests.File
{
    public class FileRequest<T> : ContainerRequest<T>
    {
        const int MAX_FILE_NAME_SIZE = 1024;

        protected string Path;
        protected string FileName;


        public FileRequest(string container, string path)
            : base(container)
        {
            var parts = path.Split('/');
            if (parts.Length > 1)
            {
                this.FileName = parts[parts.Length - 1];
            }
            else
            {
                this.FileName = path;
            }

            path = Uri.EscapeUriString(path);
            if (Encoding.UTF8.GetByteCount(path) > MAX_FILE_NAME_SIZE)
            {
                throw new Exception("Полное имя файла (включая виртуальные папки) не должно превышать 1024 байт после URL квотирования.");                
            }

            this.Path = path;
        }

        protected override string GetUrl(string storageUrl)
        {
            return string.Format("{0}/{1}/{2}", storageUrl, this.ContainerName, this.Path);
        }
    }
}
