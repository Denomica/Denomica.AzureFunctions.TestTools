using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Denomica.AzureFunctions.TestTools.Core
{
    /// <summary>
    /// Extension methods.
    /// </summary>
    public static class ExtensionFunctions
    {

        /// <summary>
        /// Hashes the current string using SHA 512.
        /// </summary>
        public static string Hash(this string s)
        {
            var buffer = Encoding.UTF8.GetBytes(s);
            var hash = SHA512
                .Create()
                .ComputeHash(buffer);

            return Convert.ToBase64String(hash);
        }
    }
}
