using SelectelSharp.Models;
using SelectelSharp.Models.Container;
using System.Collections.Generic;

namespace SelectelSharp.Models.Container
{
    /// <summary>
    /// Список контейнеров, так же содержащий информацию о хранилище
    /// </summary>
    public class ContainersList : List<Container>
    {
        /// <summary>
        /// Информация о хранилище
        /// </summary>
        public StorageInfo StorageInfo { get; internal set; }
    }
}
