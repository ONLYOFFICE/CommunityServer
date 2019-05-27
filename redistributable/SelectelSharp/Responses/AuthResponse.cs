using SelectelSharp.Common;
using SelectelSharp.Headers;

namespace SelectelSharp.Responses
{
    internal class AuthResponse
    {
        /// <summary>
        /// Базовый адрес для выполнения операций с хранилищем
        /// </summary>
        [Header(HeaderKeys.XStorageUrl)]
        public string StorageUrl { get; private set; }

        /// <summary>
        /// Идентификатор авторизации(действителен 24 часа с момента первой аутентификации)
        /// </summary>
        [Header(HeaderKeys.XAuthToken)]
        public string AuthToken { get; private set; }

        /// <summary>
        ///  Точное время действия идентификатора авторизации в секундах
        /// </summary>
        [Header(HeaderKeys.XExpireAuthToken)]
        public long ExpireAuthToken { get; private set; }
    }
}
