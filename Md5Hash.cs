using System;
using System.IO;
using System.Security.Cryptography;

static string ComputeMD5Hash(string filePath)
{
    using (var md5 = MD5.Create())
    {
        using (var stream = File.OpenRead(filePath))
        {
            var hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}
