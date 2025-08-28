using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace UsbSecurityKey
{
    /// <summary>
    /// Handles the logic of authenticating a connected USB drive.
    /// </summary>
    public class UsbAuthenticator
    {
        /// <summary>
        /// Checks all connected drives to find and validate the security key.
        /// </summary>
        /// <returns>True if the correct key is present and authenticated.</returns>
        public bool Authenticate()
        {
            var keyDrive = DriveInfo.GetDrives()
                .FirstOrDefault(d => d.IsReady && d.DriveType == DriveType.Removable && d.VolumeLabel == AppSettings.VolumeLabel);

            if (keyDrive == null)
            {
                return false; // Correctly named drive not found.
            }

            // Fast Path: Check for a valid token first.
            if (CheckToken(keyDrive))
            {
                return true;
            }

            // Secure Path: If token fails, perform full RSA challenge-response.
            if (PerformRsaChallenge(keyDrive))
            {
                // If successful, create a new token for subsequent fast checks.
                GenerateNewToken(keyDrive);
                return true;
            }

            return false;
        }

        private bool CheckToken(DriveInfo drive)
        {
            try
            {
                string tokenPath = Path.Combine(drive.Name, AppSettings.TokenFile);
                if (!File.Exists(tokenPath)) return false;

                string tokenString = File.ReadAllText(tokenPath);
                return AuthenticationToken.Validate(tokenString, SecureConfig.GetHmacKey(), AppSettings.TokenValidity);
            }
            catch
            {
                return false; // I/O errors, etc.
            }
        }

        private bool PerformRsaChallenge(DriveInfo drive)
        {
            try
            {
                string publicKeyPath = Path.Combine(drive.Name, AppSettings.PublicKeyFile);
                string privateKeyPath = Path.Combine(drive.Name, AppSettings.PrivateKeyFile);
                string passwordPath = Path.Combine(drive.Name, AppSettings.PasswordFile);

                if (!File.Exists(publicKeyPath) || !File.Exists(privateKeyPath) || !File.Exists(passwordPath))
                {
                    return false;
                }

                // --- SECURITY WARNING ---
                // Reading the password directly from a file is highly insecure.
                // This is implemented as per the prompt's specifications.
                // A better system would prompt the user or use a different secret exchange mechanism.
                string password = File.ReadAllText(passwordPath);

                string publicKeyXml = File.ReadAllText(publicKeyPath);
                byte[] encryptedPrivateKey = File.ReadAllBytes(privateKeyPath);

                // Decrypt the private key in memory
                byte[] privateKeyBytes = CryptoHelper.Decrypt(encryptedPrivateKey, password);
                if (privateKeyBytes == null) return false; // Decryption failed
                string privateKeyXml = Encoding.UTF8.GetString(privateKeyBytes);

                // Perform cryptographic handshake
                using (var rsa = new RSACryptoServiceProvider())
                {
                    // Create a random challenge
                    byte[] challenge = Guid.NewGuid().ToByteArray();

                    // Sign the challenge with the private key
                    rsa.FromXmlString(privateKeyXml);
                    byte[] signature = rsa.SignData(challenge, SHA256.Create());

                    // Verify the signature with the public key
                    rsa.FromXmlString(publicKeyXml);
                    return rsa.VerifyData(challenge, SHA256.Create(), signature);
                }
            }
            catch
            {
                return false;
            }
        }

        private void GenerateNewToken(DriveInfo drive)
        {
            try
            {
                string tokenPath = Path.Combine(drive.Name, AppSettings.TokenFile);
                string newToken = AuthenticationToken.Generate(SecureConfig.GetHmacKey());
                File.WriteAllText(tokenPath, newToken);
            }
            catch (Exception ex)
            {
                // Handle potential I/O errors (e.g., drive removed during write)
                Console.WriteLine($"Error writing token: {ex.Message}");
            }
        }
    }
}