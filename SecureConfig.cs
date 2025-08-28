using System;
using System.IO;
using System.Security.Cryptography;

namespace UsbSecurityKey
{
    /// <summary>
    /// Manages the application's secret HMAC key, using DPAPI to encrypt it.
    /// This ties the key to the current user's Windows profile for security.
    /// </summary>
    public static class SecureConfig
    {
        private static readonly string AppDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "UsbSecurityKey");
        private static readonly string KeyFilePath = Path.Combine(AppDataFolder, "hmac.key");
        private static readonly byte[] Entropy = { 0x18, 0x2a, 0xf3, 0x9c, 0x4a, 0x50, 0x6e, 0x71 }; // Salt for DPAPI

        private static byte[] _hmacKey;

        /// <summary>
        /// Gets the HMAC key, generating and saving it if it doesn't exist.
        /// </summary>
        public static byte[] GetHmacKey()
        {
            if (_hmacKey != null)
            {
                return _hmacKey;
            }

            Directory.CreateDirectory(AppDataFolder);

            if (File.Exists(KeyFilePath))
            {
                try
                {
                    byte[] encryptedKey = File.ReadAllBytes(KeyFilePath);
                    _hmacKey = ProtectedData.Unprotect(encryptedKey, Entropy, DataProtectionScope.CurrentUser);
                    return _hmacKey;
                }
                catch (Exception)
                {
                    // If decryption fails (e.g., corruption), generate a new key.
                }
            }

            // Generate a new, cryptographically secure key
            _hmacKey = new byte[64]; // 512 bits for HMACSHA256
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(_hmacKey);
            }

            // Encrypt and save the new key
            byte[] encryptedNewKey = ProtectedData.Protect(_hmacKey, Entropy, DataProtectionScope.CurrentUser);
            File.WriteAllBytes(KeyFilePath, encryptedNewKey);

            return _hmacKey;
        }
    }
}