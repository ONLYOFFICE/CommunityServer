using SelectelSharp.Models.File;
using System.Collections.Generic;

namespace SelectelSharp.Models.Container
{
    /// <summary>
    /// Список контейнеров, так же содержащий информацию о хранилище
    /// </summary>
    public class ContainerFilesList : List<FileInfo>
    {
        /// <summary>
        /// Информация о хранилище
        /// </summary>
        public StorageInfo StorageInfo { get; internal set; }
    }
}
