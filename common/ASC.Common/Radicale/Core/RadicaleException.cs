using System;

namespace ASC.Common.Radicale
{
    [Serializable]
    public class RadicaleException : Exception
    {
        public RadicaleException(string message)
            : base(message)
        {
        }


    }
}
