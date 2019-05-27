namespace SelectelSharp.Models.Container
{
    public class Container
    {
        /// <summary>
        /// Место занимаемое содержимым контейнера
        /// </summary>
        public long Bytes { get; set; }

        /// <summary>
        /// Число объектов в контейнере
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Имя контейнера
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Всего закачанных в контейнер байт
        /// </summary>
        public long RxBytes { get; set; }

        /// <summary>
        /// Всего скачанных из контейнера байт
        /// </summary>
        public long TxBytes { get; set; }

        /// <summary>
        /// Тип контейнера
        /// </summary>
        public ContainerType Type { get; set; }
    }
}
