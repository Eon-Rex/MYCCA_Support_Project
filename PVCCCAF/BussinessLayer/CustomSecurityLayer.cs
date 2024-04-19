using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace PVCCCAF.BussinessLayer
{
    public class CustomSecurityLayer
    {
        public const int SaltByteSize = 20;
        public const int HashByteSize = 20; // This is to match the size of the PBKDF2-HMAC-SHA-1 hash 
        public const int Pbkdf2Iterations = 1000;
        public const int IterationIndex = 0; //these are not be changed
        public const int SaltIndex = 1; //these are not be changed
        public const int Pbkdf2Index = 2; //these are not be changed

        public static string HashPassword(string password)
        {
            var cryptoProvider = new RNGCryptoServiceProvider();
            byte[] salt = new byte[SaltByteSize];
            cryptoProvider.GetBytes(salt);

            var hash = GetPbkdf2Bytes(password, salt, Pbkdf2Iterations, HashByteSize);
            string var1 =Pbkdf2Iterations + ":" +
                   Convert.ToBase64String(salt) + ":" +
                   Convert.ToBase64String(hash);
            var hash2 = GetPbkdf2Bytes(password, salt, Pbkdf2Iterations, HashByteSize);
            string var2 =Pbkdf2Iterations + ":" +
                   Convert.ToBase64String(salt) + ":" +
                   Convert.ToBase64String(hash2);
            return var1;
        }

        public static bool ValidatePassword(string password, string correctHash)
        {
            char[] delimiter = { ':' };
            var split = correctHash.Split(delimiter);
            var iterations = Int32.Parse(split[IterationIndex]);
            var salt = Convert.FromBase64String(split[SaltIndex]);
            var hash = Convert.FromBase64String(split[Pbkdf2Index]);

            var testHash = GetPbkdf2Bytes(password, salt, iterations, hash.Length);
            return SlowEquals(hash, testHash);
        }

        private static bool SlowEquals(byte[] a, byte[] b)
        {
            var diff = (uint)a.Length ^ (uint)b.Length;
            for (int i = 0; i < a.Length && i < b.Length; i++)
            {
                diff |= (uint)(a[i] ^ b[i]);
            }
            return diff == 0;
        }

        private static byte[] GetPbkdf2Bytes(string password, byte[] salt, int iterations, int outputBytes)
        {
            //var pbkdf2 = new Rfc2898DeriveBytes(password, salt);
            //pbkdf2.IterationCount = iterations;
            //return pbkdf2.GetBytes(outputBytes);

            using (HMACSHA256 hmac = new HMACSHA256())
            {
                var df = new PBKDF2(password, salt, iterations, "HMACSHA256");
                return df.GetBytes(outputBytes);
            }
            //using (SHA256 hash = SHA256Managed.Create())
            //{
            //    Encoding enc = Encoding.UTF8;
            //    return hash.ComputeHash(enc.GetBytes(password));
            //}
        }

        //public static String sha256_hash(String value)
        //{
        //    StringBuilder Sb = new StringBuilder();
        //    using (SHA256 hash = SHA256Managed.Create())
        //    {
        //        Encoding enc = Encoding.UTF8;
        //        //byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
        //        //hash.c
        //        Byte[] result = hash.ComputeHash(enc.GetBytes(value));

        //        foreach (Byte b in result)
        //            Sb.Append(b.ToString("x2"));
        //    }

        //    return Sb.ToString();
        //}
    }
}