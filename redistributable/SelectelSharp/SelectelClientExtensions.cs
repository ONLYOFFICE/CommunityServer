#region Import

using SelectelSharp.Common;
using SelectelSharp.Headers;
using SelectelSharp.Models.Container; 
using SelectelSharp.Models.File;
using SelectelSharp.Requests.Container;
using SelectelSharp.Requests.File;
using SelectelSharp.Requests.CDN;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SelectelSharp.Models.Link;

#endregion

namespace SelectelSharp
{
    public static class SelectelClientExtensions
    {
        #region Container

        /// <summary>
        /// Получиение информации по контейнеру
        /// </summary>
        /// <param name="container">Имя контейнера</param>
        public static Task<ContainerInfo> GetContainerInfoAsync(this SelectelClient client, string container)
        {
            return client.ExecuteAsync(new GetContainerInfoRequest(container));
        }

        /// <summary>
        /// Получение списка контейнеров
        /// </summary>
        /// <param name="limit">Число, ограничивает количество объектов в результате (по умолчанию 10000)</param>
        /// <param name="marker">Cтрока, результат будет содержать объекты по значению больше указанного маркера (полезно использовать для постраничной навигации и при большом количестве контейнеров)</param>
        public static Task<ContainersList> GetContainersListAsync(this SelectelClient client, int limit = 1000, string marker = null)
        {
            return client.ExecuteAsync(new GetContainersRequest(limit, marker));
        }

        /// <summary>
        /// Создание нового контейнера
        /// </summary>
        /// <param name="container">Имя контейнера</param>
        /// <param name="customHeaders">Произвольные мета-данные через передачу заголовков с префиксом X-Container-Meta-.</param>
        /// <param name="type">X-Container-Meta-Type: Тип контейнера (public, private, gallery)</param>
        /// <param name="corsHeaders">Дополнительные заголовки кэшировани и CORS</param>
        public static Task<CreateContainerResult> CreateContainerAsync(this SelectelClient client, string container, ContainerType type = ContainerType.Private, Dictionary<string, object> customHeaders = null, CORSHeaders corsHeaders = null)
        {
            return client.ExecuteAsync(new CreateContainerRequest(container, type, customHeaders, corsHeaders));
        }

        /// <summary>
        /// Удаление контейнера
        /// </summary>
        /// <param name="container">Имя контейнера</param>
        public static Task<DeleteContainerResult> DeleteContainerAsync(this SelectelClient client, string container)
        {
            return client.ExecuteAsync(new DeleteContainerRequest(container));
        }

        /// <summary>
        /// Обновление мета-данных контейнера
        /// </summary>
        /// <param name="container">Имя контейнера</param>
        /// <param name="type">X-Container-Meta-Type: Тип контейнера (public, private, gallery)</param>
        /// <param name="customHeaders">Произвольные мета-данные через передачу заголовков с префиксом X-Container-Meta-.</param>
        /// <param name="corsHeaders">Дополнительные заголовки кэшировани и CORS</param>
        public static Task<UpdateContainerResult> SetContainerMetaAsync(this SelectelClient client, string container, ContainerType type = ContainerType.Private, Dictionary<string, object> customHeaders = null, CORSHeaders corsHeaders = null)
        {
            return client.ExecuteAsync(new UpdateContainerMetaRequest(container, type, customHeaders, corsHeaders));
        }

        /// <summary>
        /// Преобразование контейнера в галерею
        /// </summary>
        /// <param name="container">Имя контейнера</param>
        /// <param name="password">Дополнительно можно установить пароль, по которому будет ограничен доступ</param>
        public static Task<UpdateContainerResult> SetContainerToGalleryAsync(this SelectelClient client, string container, string password = null)
        {
            return client.ExecuteAsync(new UpdateContainerToGalleryRequest(container, password));
        }

        /// <summary>
        /// Получение списка файлов в контейнере
        /// </summary>
        /// <param name="container">Имя контейнера</param>
        /// <param name="limit">Число, ограничивает количество объектов в результате (по умолчанию 10000)</param>
        /// <param name="marker">Cтрока, результат будет содержать объекты по значению больше указанного маркера (полезно использовать для постраничной навигации и при большом количестве контейнеров)</param>        
        /// <param name="prefix">Строка, вернуть объекты имена которых начинаются с указанного префикса</param>
        /// <param name="path">Строка, вернуть объекты в указанной папке(виртуальные папки)</param>
        /// <param name="delimeter">Символ, вернуть объекты до указанного разделителя в их имени</param>
        public static Task<ContainerFilesList> GetContainerFilesAsync(this SelectelClient client, string container, int limit = 10000, string marker = null, string prefix = null, string path = null, string delimeter = null)
        {
            return client.ExecuteAsync(new GetContainerFilesRequest(container, limit, marker, prefix, path, delimeter));
        }

        #endregion

        #region Files

        public static Task<bool> CreateSymLink(this SelectelClient client, 
            String container,
            String link, 
            Symlink.SymlinkType symlinkType, 
            String objectLocation, 
            String password = null,
            String contentDisposition = null,         
            DateTime? deleteAt = null)
        {         
            return client.ExecuteAsync(new SymlinkRequest(container, new Symlink(link, symlinkType, objectLocation, deleteAt, password, contentDisposition)));
        }

        /// <summary>
        /// Получение файла
        /// </summary>
        /// <param name="container">Имя контейнера</param>
        /// <param name="path">Путь к файлу в контейнере</param>
        /// <param name="conditionalHeaders">Условные заголовки GET-запроса</param>
        /// <param name="allowAnonymously">Для файлов в публичных контейнерах, скачиваемых без токена</param>
        /// <returns></returns>
        public static Task<GetFileResult> GetFileAsync(this SelectelClient client, string container, string path, ConditionalHeaders conditionalHeaders = null, bool allowAnonymously = false)
        {
            return client.ExecuteAsync(new GetFileRequest(container, path, conditionalHeaders, allowAnonymously));
        }


        private static string CreateRandomString(int length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="container"></param>
        /// <param name="paths"></param>
        /// <returns></returns>
        public static async Task<CDNIvalidationResult> CDNIvalidationAsync(this SelectelClient client, string container, string[] paths)
        {
            if (String.IsNullOrEmpty(container))
            {
                throw new ArgumentNullException("container");
            }

            if (paths == null || paths.Count() == 0)
            {
                throw new ArgumentNullException("paths");
            }

            if (container.Length > 255 || container.EndsWith("/"))
            {
                throw new ArgumentException("Имя контейнера должно быть меньше 256 символов и не содержать завершающего слеша '/' в конце.");
            }
            
            var uri = paths.Select(x => new Uri(String.Format("{0}{1}/{2}", client.CDNUrl, container, x)));

            return await client.ExecuteAsync(new CDNIvalidationRequest(uri.ToArray()));     
        }


        public static async Task<bool> CopyFileAsync(this SelectelClient client, string container, string path, String newContainer, String newPath)
        {
            return await client.ExecuteAsync(new CopyFileRequest(container, path, newContainer, newPath));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="container"></param>
        /// <param name="path"></param>
        /// <param name="expire"></param>
        /// <returns></returns>
        public static async Task<Uri> GetPreSignUriAsync(this SelectelClient client, string container, string path, TimeSpan expire)
        {
            var containerInfo = await client.ExecuteAsync(new GetContainerInfoRequest(container));

            var customHeaders = containerInfo.CustomHeaders;

            if (customHeaders == null)
                customHeaders = new Dictionary<String, String>();

            var customHeader = "X-Container-Meta-Temp-Url-Key";

            String secretKey = String.Empty;

            if (customHeaders.ContainsKey(customHeader))
            {
                secretKey = customHeaders[customHeader];
            }
            else
            {
                secretKey = CreateRandomString(64);
                customHeaders.Add(customHeader, secretKey);
                await client.SetContainerMetaAsync(container, containerInfo.Type, customHeaders.ToDictionary(k => k.Key, v => (object)v.Value));
            }

            var expires = Helpers.DateToUnixTimestamp(DateTime.UtcNow.Add(expire));               

            String sig;

            using (var hasher = new System.Security.Cryptography.HMACSHA1(Encoding.UTF8.GetBytes(secretKey)))
            {
                var method = "GET";
                var relPath = String.Format("/{0}/{1}", container, path);

                var hmac_body = String.Format("{0}\n{1}\n{2}", method, expires, relPath);

                sig = BitConverter.ToString(hasher.ComputeHash(Encoding.UTF8.GetBytes(hmac_body))).Replace("-", "").ToLower();
            }

            return new Uri(String.Format("{0}{1}/{2}?temp_url_sig={3}&temp_url_expires={4}", client.StorageUrl, container, path, sig, expires));
        }

        public static Uri GetInternalUri(this SelectelClient client, string container, string path)
        {
            return new Uri(String.Format("{0}{1}/{2}", "https://private.selcdn.ru/", container, path));
        }
        
        /// <summary>
        /// Сохранение файла
        /// </summary>
        /// <param name="container">Имя контейнера</param>
        /// <param name="pathToSave"></param>
        /// <param name="AutoCloseStream"></param>
        /// <param name="AutoResetStreamPosition">If this value is set to true then the stream's position will be reset to the start before being read for upload. Default: true.</param>
        /// <param name="inputStream"></param>
        /// <param name="ETag"></param>
        /// <param name="contentDisposition">Content-Disposition</param>
        /// <param name="contentType">Content-Type</param>
        /// <param name="deleteAt">Время удаления</param>
        /// <param name="deleteAfter">Промежуток времени до удаления в секундах</param>
        /// <param name="customHeaders">Заголовки файла</param>
        /// <returns></returns>
        public static Task<UploadFileResult> UploadFileAsync(this SelectelClient client,
            string container,
            string pathToSave,
            bool AutoCloseStream = true,
            bool AutoResetStreamPosition = true,
            Stream inputStream = null,
            String ETag = null,
            string contentDisposition = null,
            string contentType = null,
            DateTime? deleteAt = null,
            long? deleteAfter = null,
            IDictionary<string, object> customHeaders = null)
        {
            return client.ExecuteAsync(new UploadFileRequest(
                container,
                pathToSave,
                AutoCloseStream,
                AutoResetStreamPosition,
                inputStream,
                ETag,
                contentDisposition,
                contentType,
                deleteAt.HasValue ? Helpers.DateToUnixTimestamp(deleteAt.Value) : (long?)null,
                deleteAfter,
                customHeaders));
        }

        /// <summary>
        /// Удаление файла
        /// </summary>
        /// <param name="container">Имя контейнера</param>
        /// <param name="path">Путь к файлу в контейнере</param>
        /// <returns></returns>
        public static Task<DeleteFileResult> DeleteFileAsync(this SelectelClient client, string container, string path)
        {
            return client.ExecuteAsync(new DeleteFileRequest(container, path));
        }

        /// <summary>
        /// Обновление мета-данных файла
        /// </summary>
        /// <param name="container">Имя контейнера</param>
        /// <param name="path">Путь к файлу в контейнере</param>
        /// <param name="customHeaders">Произвольные мета-данные через передачу заголовков с префиксом X-Container-Meta-.</param>
        /// <param name="corsHeaders">Дополнительные заголовки кэшировани и CORS</param>
        public static Task<UpdateFileResult> SetFileMetaAsync(this SelectelClient client,
            string container,
            string path,
            IDictionary<string, object> customHeaders = null,
            CORSHeaders corsHeaders = null)
        {
            return client.ExecuteAsync(new UpdateFileMetaRequest(container, path, customHeaders, corsHeaders));
        }

        /// <summary>
        /// Загрузка архива с последующей распаковкой на сервере
        /// </summary>
        /// <param name="container">Имя контейнера</param>
        /// <param name="file">Архив</param>
        /// <param name="format">Формат архива</param>
        /// <param name="pathToSave">Путь для распаковки в контейнере</param>
        /// <returns></returns>
        public static Task<UploadArchiveResult> UploadArchiveAsync(this SelectelClient client,
            string container,
            bool AutoCloseStream = true,
            bool AutoResetStreamPosition = true,
            Stream inputStream = null,
            FileArchiveFormat format = FileArchiveFormat.TarGz,
            string pathToSave = null)
        {
            return client.ExecuteAsync(new UploadArchiveRequest(AutoCloseStream, AutoResetStreamPosition, inputStream, format, pathToSave));
        }       
        
        #endregion
    }
}
