using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Driver.Extensions
{
    public static class DataExtensions
    {
        public static string Encrypt(string value, string key)
        {
            return Convert.ToBase64String(Encrypt(Encoding.UTF8.GetBytes(value), key));
        }

        private static byte[] Encrypt(byte[] key, string value)
        {
            SymmetricAlgorithm symmetricAlgorithm = Rijndael.Create();
            ICryptoTransform cryptoTransform =
                symmetricAlgorithm.CreateEncryptor(new Rfc2898DeriveBytes(value, new byte[16]).GetBytes(16), new byte[16]);

            byte[] result;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(key, 0, key.Length);
                    cryptoStream.FlushFinalBlock();

                    result = memoryStream.ToArray();

                    memoryStream.Close();
                    memoryStream.Dispose();
                }
            }

            return result;
        }

        [DebuggerNonUserCode]
        public static string Decrypt(string value, string key)
        {
            string result;

            try
            {
                using (CryptoStream cryptoStream = InternalDecrypt(Convert.FromBase64String(value), key))
                {
                    using (StreamReader streamReader = new StreamReader(cryptoStream))
                    {
                        result = streamReader.ReadToEnd();
                    }
                }
            }
            catch (CryptographicException e)
            {
                UnityEngine.Debug.LogException(e);
                return null;
            }

            return result;
        }

        private static CryptoStream InternalDecrypt(byte[] key, string value)
        {
            SymmetricAlgorithm symmetricAlgorithm = Rijndael.Create();
            ICryptoTransform cryptoTransform =
                symmetricAlgorithm.CreateDecryptor(new Rfc2898DeriveBytes(value, new byte[16]).GetBytes(16),
                    new byte[16]);

            MemoryStream memoryStream = new MemoryStream(key);
            return new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Read);
        }
    }
}