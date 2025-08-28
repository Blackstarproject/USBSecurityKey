using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace UsbSecurityKey
{
    /// <summary>
    /// A static helper class to provide strong AES-256 encryption and decryption functionality.
    /// This is used to protect the private key file on the USB drive.
    /// </summary>
    public static class CryptoHelper
    {
        // --- AES Configuration ---
        private const int KeySize = 256;    // AES key size in bits.
        private const int BlockSize = 128;  // AES block size in bits.
        private const int Iterations = 10000; // Number of iterations for the key derivation function. Higher is more secure.
        private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA256; // Specify the hash algorithm for PBKDF2.

        /// <summary>
        /// Encrypts a plaintext string using a password.
        /// </summary>
        /// <param name="plainText">The string to encrypt.</param>
        /// <param name="password">The password to derive the encryption key from.</param>
        /// <returns>A byte array containing the salt and the encrypted data.</returns>
        public static byte[] Encrypt(string plainText, string password)
        {
            // Generate a random salt using the modern RandomNumberGenerator.
            // A salt ensures that even if the same password is used twice, the resulting
            // encrypted data will be different, preventing rainbow table attacks.
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Create a key derivation object using the modern constructor.
            // Rfc2898DeriveBytes (PBKDF2) is used to stretch the password into a secure key and IV.
            using (var keyDerivation = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithm))
            // Use Aes.Create() which provides the recommended AES implementation.
            using (var aes = Aes.Create())
            {
                aes.KeySize = KeySize;
                aes.BlockSize = BlockSize;

                // Generate the key and initialization vector (IV) from the password and salt.
                aes.Key = keyDerivation.GetBytes(KeySize / 8); // 256 / 8 = 32 bytes
                aes.IV = keyDerivation.GetBytes(BlockSize / 8); // 128 / 8 = 16 bytes

                using (var memoryStream = new MemoryStream())
                {
                    // --- Construct the final byte array: [ 16-byte Salt | Encrypted Data ] ---
                    // First, write the salt to the beginning of the stream. This is crucial for decryption.
                    memoryStream.Write(salt, 0, salt.Length);

                    // Create a crypto stream to perform the encryption.
                    using (var cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        // Convert the plaintext to bytes and write it to the crypto stream.
                        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                        cryptoStream.Write(plainBytes, 0, plainBytes.Length);
                    }
                    // The memoryStream now contains the salt followed by the ciphertext.
                    return memoryStream.ToArray();
                }
            }
        }

        /// <summary>
        /// Decrypts a byte array using a password.
        /// </summary>
        /// <param name="cipherBytes">The encrypted data, which must include the prepended salt.</param>
        /// <param name="password">The password used for encryption.</param>
        /// <returns>The decrypted data as a byte array.</returns>
        /// <exception cref="CryptographicException">Thrown if decryption fails, often due to an incorrect password.</exception>
        public static byte[] Decrypt(byte[] cipherBytes, string password)
        {
            try
            {
                // Extract the 16-byte salt from the beginning of the cipher bytes.
                byte[] salt = new byte[16];
                Array.Copy(cipherBytes, 0, salt, 0, salt.Length);

                // Create key derivation and AES objects, just like in the encryption method.
                using (var keyDerivation = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithm))
                using (var aes = Aes.Create())
                {
                    aes.KeySize = KeySize;
                    aes.BlockSize = BlockSize;

                    // Re-generate the *same* key and IV using the provided password and the extracted salt.
                    // If the password is correct, these will match the ones used for encryption.
                    aes.Key = keyDerivation.GetBytes(KeySize / 8);
                    aes.IV = keyDerivation.GetBytes(BlockSize / 8);

                    // Create a memory stream to hold the decrypted data.
                    using (var memoryStream = new MemoryStream())
                    {
                        // Create a crypto stream to perform decryption.
                        // We read from a memory stream containing only the *ciphertext* (skipping the salt part).
                        using (var cryptoStream = new CryptoStream(new MemoryStream(cipherBytes, salt.Length, cipherBytes.Length - salt.Length), aes.CreateDecryptor(), CryptoStreamMode.Read))
                        {
                            // Decrypt the data and copy it to our output stream.
                            cryptoStream.CopyTo(memoryStream);
                        }
                        return memoryStream.ToArray();
                    }
                }
            }
            catch (CryptographicException ex)
            {
                // This exception is commonly thrown if the decryption fails, which often
                // means the password was incorrect, leading to a malformed final block.
                // We re-throw the exception to make the failure explicit to the caller.
                throw new CryptographicException("Decryption failed. The password may be incorrect.", ex);
            }
        }
    }
}