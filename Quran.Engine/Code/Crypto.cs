using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Quran.Engine
{
    public static class Crypto
    {
        private static string AesIV = @"  SOBHAN ALLAH  ";
        private static string AesKey = @"  QURAN  KARIM  ";

        private static AesCryptoServiceProvider aes = null;

        static Crypto()
        {
            aes = new AesCryptoServiceProvider();

            aes.BlockSize = 128;
            aes.KeySize = 128;
            aes.IV = Encoding.UTF8.GetBytes(AesIV);
            aes.Key = Encoding.UTF8.GetBytes(AesKey);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
        }

        public static AesCryptoServiceProvider AES
        {
            get
            {
                return aes;
            }
        }

        public static string Decrypt(string text)
        {
            byte[] src = System.Convert.FromBase64String(text);
            byte[] dest = AES.CreateDecryptor().TransformFinalBlock(src, 0, src.Length);
            return Encoding.Unicode.GetString(dest);
        }

        public static string Encrypt(string text)
        {
            byte[] src = Encoding.Unicode.GetBytes(text);
            byte[] dest = AES.CreateEncryptor().TransformFinalBlock(src, 0, src.Length);
            return Convert.ToBase64String(dest);
        }

        /// Sample Write & Read

        // using (var file = File.Create("s.bin"))
        // {
        //    using (CryptoStream cs = new CryptoStream(file, Crypto.AES.CreateEncryptor(), CryptoStreamMode.Write))
        //    {
        //        Serializer.Serialize<Student>(cs, s);
        //    }

        // }

        // using (var file = File.OpenRead("s.bin"))
        // {
        //    using (CryptoStream cs = new CryptoStream(file, Crypto.AES.CreateDecryptor(), CryptoStreamMode.Read))
        //    {
        //        s = Serializer.Deserialize<Student>(cs);
        //    }
        // }
    }
}
