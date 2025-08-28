using System;

namespace UsbSecurityKey
{
    /// <summary>
    /// Centralizes all application configuration settings.
    /// </summary>
    public static class AppSettings
    {
        // The required name of the USB drive. The provisioning tool will set this.
        public const string VolumeLabel = "SECURE_KEY_V1";

        // --- File names on the USB key ---
        public const string PublicKeyFile = "public.key";
        public const string PrivateKeyFile = "private.key";
        public const string PasswordFile = "password.txt"; // WARNING: Storing a password in plaintext is insecure.
        public const string TokenFile = "token.txt";

        // How long a token is valid after being created.
        public static readonly TimeSpan TokenValidity = TimeSpan.FromSeconds(30);
    }
}