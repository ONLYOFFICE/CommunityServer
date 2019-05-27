using System;
using System.Security.Cryptography;
using System.Text;

namespace SelectelSharp.Common
{
    public static class Helpers
    {
        public static long DateToUnixTimestamp(DateTime date)
        {
            var ts = date - new DateTime(1970, 1, 1, 0, 0, 0);
            return (long)ts.TotalSeconds;
        }

        public static string CalculateSHA1(string input)
        {
            if (string.IsNullOrEmpty(input))
                return null;

            using (var sha1 = SHA1.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha1.ComputeHash(bytes);
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                    sb.Append(b.ToString("X2"));

                return sb.ToString().ToLower();
            }
        }
    }
}
