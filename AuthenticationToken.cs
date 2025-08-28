using System;
using System.Security.Cryptography;
using System.Text;

namespace UsbSecurityKey
{
    /// <summary>
    /// Represents a short-lived, signed authentication token for fast checks.
    /// </summary>
    public class AuthenticationToken
    {
        public DateTime CreationTimeUtc { get; private set; }
        public string Signature { get; private set; }

        /// <summary>
        /// Generates a new signed token string.
        /// </summary>
        public static string Generate(byte[] hmacKey)
        {
            long unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            byte[] timestampBytes = Encoding.UTF8.GetBytes(unixTimestamp.ToString());

            using (var hmac = new HMACSHA256(hmacKey))
            {
                byte[] signatureBytes = hmac.ComputeHash(timestampBytes);
                string signature = Convert.ToBase64String(signatureBytes);
                return $"{unixTimestamp}.{signature}";
            }
        }

        /// <summary>
        /// Validates a token string's signature and expiration.
        /// </summary>
        public static bool Validate(string tokenString, byte[] hmacKey, TimeSpan validityPeriod)
        {
            if (string.IsNullOrEmpty(tokenString)) return false;

            var parts = tokenString.Split('.');
            if (parts.Length != 2) return false;

            if (!long.TryParse(parts[0], out long unixTimestamp)) return false;
            string signature = parts[1];

            // 1. Check expiration
            var creationTime = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).UtcDateTime;
            if (creationTime.Add(validityPeriod) < DateTime.UtcNow)
            {
                return false; // Token expired
            }

            // 2. Verify signature
            byte[] timestampBytes = Encoding.UTF8.GetBytes(unixTimestamp.ToString());
            using (var hmac = new HMACSHA256(hmacKey))
            {
                byte[] expectedSignatureBytes = hmac.ComputeHash(timestampBytes);
                string expectedSignature = Convert.ToBase64String(expectedSignatureBytes);
                return signature == expectedSignature;
            }
        }
    }
}