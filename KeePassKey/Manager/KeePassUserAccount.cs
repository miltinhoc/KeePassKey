using System;
using System.IO;
using System.Security.Cryptography;

namespace KeePassKey.Manager
{
    public class KeePassUserAccount
    {
        private const string _userKeyFileName = "ProtectedUserKey.bin";
        private static readonly byte[] _entropy = new byte[] { 222, 19, 91,95,24,163,70,112,178,87,36,41,105,136,152, 230 };

        private static string GetUserKeyFilePath()
        {
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string keyFilePath = Path.Combine(appDataFolder, "KeePass", _userKeyFileName);

            return (File.Exists(keyFilePath) ? keyFilePath : string.Empty);
        }

        public static byte[] UnprotectFile()
        {
            string keyFile = GetUserKeyFilePath();

            if (string.IsNullOrEmpty(keyFile))
                return new byte[0];

            byte[] fileContent = File.ReadAllBytes(keyFile);

            return ProtectedData.Unprotect(fileContent, _entropy, DataProtectionScope.CurrentUser);
        }

    }
}
