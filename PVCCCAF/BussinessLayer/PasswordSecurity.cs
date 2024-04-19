using System;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System.Security.Cryptography;

namespace CCAF.BussinessLayer
{
    /// <summary>
    /// Contains the relevant Bouncy Castle Methods required to encrypt a password.
    /// References NuGet Package BouncyCastle.Crypto.dll
    /// </summary>
    public class BouncyCastleHashing
    {
        private SecureRandom _cryptoRandom;

        public BouncyCastleHashing()
        {
            _cryptoRandom = new SecureRandom();
        }

        public byte[] CreateSalt(int size)
        {
            byte[] salt = new byte[size];
            _cryptoRandom.NextBytes(salt);
            return salt;
        }

        //overload
        public string PBKDF2_SHA256_GetHash(string password, string saltAsBase64String, int iterations, int hashByteSize)
        {
            var saltBytes = Convert.FromBase64String(saltAsBase64String);
            var hash = PBKDF2_SHA256_GetHash(password, saltBytes, iterations, hashByteSize);
            return Convert.ToBase64String(hash);
        }

        
        public byte[] PBKDF2_SHA256_GetHash(string password, byte[] salt, int iterations, int hashByteSize)
        {
            var pdb = new Pkcs5S2ParametersGenerator(new Org.BouncyCastle.Crypto.Digests.Sha256Digest());
            pdb.Init(PbeParametersGenerator.Pkcs5PasswordToBytes(password.ToCharArray()), salt, iterations);
            var key = (KeyParameter)pdb.GenerateDerivedMacParameters(hashByteSize * 8);
            return key.GetKey();
        }

      
        public bool ValidatePassword(string password, string salt, int iterations, int hashByteSize, string hashAsBase64String)
        {
            byte[] saltBytes = Convert.FromBase64String(salt);
            byte[] actualHashBytes = Convert.FromBase64String(hashAsBase64String);
            return ValidatePassword(password, saltBytes, iterations, hashByteSize, actualHashBytes);
        }

        public bool ValidatePassword(string password, byte[] saltBytes, int iterations, int hashByteSize, byte[] actualGainedHasAsByteArray)
        {
            byte[] testHash = PBKDF2_SHA256_GetHash(password, saltBytes, iterations, hashByteSize);
            return SlowEquals(actualGainedHasAsByteArray, testHash);
        }
        
        private bool SlowEquals(byte[] a, byte[] b)
        {
            uint diff = (uint)a.Length ^ (uint)b.Length;
            for (int i = 0; i < a.Length && i < b.Length; i++)
                diff |= (uint)(a[i] ^ b[i]);
            return diff == 0;
        }

    }
}