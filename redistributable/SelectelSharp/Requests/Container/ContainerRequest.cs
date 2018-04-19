using System;

namespace SelectelSharp.Requests.Container
{
    public abstract class ContainerRequest<T>: BaseRequest<T>
    {
        protected string ContainerName;        

        public ContainerRequest(string containerName)
        {
            if (string.IsNullOrEmpty(containerName))
            {
                throw new ArgumentException("Пустое имя контейнера.");
            }

            if (containerName.Length > 255 || containerName.EndsWith("/"))
            {
                throw new ArgumentException("Имя контейнера должно быть меньше 256 символов и не содержать завершающего слеша '/' в конце.");
            }

            this.ContainerName = containerName;
        }

        protected override string GetUrl(string storageUrl)
        {
            return string.Format("{0}/{1}", storageUrl, this.ContainerName);
        }
    }
}
