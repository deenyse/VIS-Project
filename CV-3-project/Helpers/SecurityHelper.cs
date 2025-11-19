using System.Security.Cryptography;

namespace CV_3_project.Security
{
    public static class SecurityHelper
    {
        public static byte[] GenerateSalt()
        {
            var salt = new byte[64]; // 512 bits
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        public static string HashPassword(string password, byte[] salt)
        {
            using (var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256))
            {
                return Convert.ToBase64String(rfc2898DeriveBytes.GetBytes(32)); // 256 bits
            }
        }

        public static bool VerifyPassword(string enteredPassword, string storedHash, byte[] storedSalt)
        {
            string newHash = HashPassword(enteredPassword, storedSalt);
            return newHash == storedHash;
        }
    }
}