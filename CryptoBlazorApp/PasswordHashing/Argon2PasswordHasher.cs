namespace CryptoBlazorApp.PasswordHashing
{
    using System;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using Konscious.Security.Cryptography;
    using Microsoft.AspNetCore.Identity;

    public class Argon2PasswordHasher<TUser> : IPasswordHasher<TUser> where TUser : class
    {
        private const int Argon2DegreeOfParallelism = 8;
        private const int Argon2Iterations = 4; // four cores
        private const int Argon2MemorySize = 1024 * 1024; // 1 GB

        public string HashPassword(TUser user, string password)
        {
            var salt = CreateSalt();
            var hash = HashPassword(password, salt);

            byte[] dst = new byte[33];
            Buffer.BlockCopy(salt, 0, dst, 1, 16);
            Buffer.BlockCopy(hash, 0, dst, 17, 16);
            return Convert.ToBase64String(dst);
        }

        public PasswordVerificationResult VerifyHashedPassword(TUser user, string hash,
            string providedPassword)
        {
            byte[] src = Convert.FromBase64String(hash);
            byte[] salt = new byte[0x10];
            Buffer.BlockCopy(src, 1, salt, 0, 0x10);
            byte[] hashedPassword = new byte[0x20];
            Buffer.BlockCopy(src, 17, hashedPassword, 0, 16);

            var success = VerifyHash(providedPassword, salt, hashedPassword);

            return success ? PasswordVerificationResult.Success : PasswordVerificationResult.Failed;
        }

        private byte[] CreateSalt()
        {
            var buffer = new byte[16];
            var rng = RandomNumberGenerator.Create();
            rng.GetBytes(buffer);
            return buffer;
        }

        private byte[] HashPassword(string password, byte[] salt)
        {
            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password));

            argon2.Salt = salt;
            argon2.DegreeOfParallelism = Argon2DegreeOfParallelism;
            argon2.Iterations = Argon2Iterations;
            argon2.MemorySize = Argon2MemorySize;

            return argon2.GetBytes(16);
        }

        private bool VerifyHash(string password, byte[] salt, byte[] hash)
        {
            var newHash = HashPassword(password, salt);
            return hash.SequenceEqual(newHash);
        }
    }
}