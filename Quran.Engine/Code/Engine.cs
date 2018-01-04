using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;
using Quran.Model;

namespace Quran.Engine
{
    public static class Engine
    {
        public static void Run()
        {
            if (!File.Exists(QuranConfig.Current.MetaPath))
                MetaExtractor.Extract();

            LoadMeta();

            if (!File.Exists(QuranConfig.Current.FilePath))
                QuranExtractor.Extract();

            LoadEngineeInfo();

            if (!File.Exists(QuranConfig.Current.TransPath))
                TranslationExtractor.Extract();
        }

        private static void LoadMeta()
        {
            using (var file = File.OpenRead(QuranConfig.Current.MetaPath))
            {
                using (CryptoStream cs = new CryptoStream(file, Crypto.AES.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    QuranConfig.Current.Meta = Serializer.Deserialize<Meta>(cs);
                }
            }
        }

        public static void SaveMeta(Meta meta)
        {
            using (var file = File.Create(QuranConfig.Current.MetaPath))
            {
                using (CryptoStream cs = new CryptoStream(file, Crypto.AES.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    Serializer.Serialize<Meta>(cs, meta);
                }
            }
        }

        private static void LoadEngineeInfo()
        {
            if (!File.Exists(QuranConfig.Current.EngineeInfoPath))
            {
                var engineeInfo = new EngineeInfo();
                engineeInfo.LockedSuras = new bool[114];
                QuranConfig.Current.EngineeInfo = engineeInfo;
            }
            else
            {
                using (var file = File.OpenRead(QuranConfig.Current.EngineeInfoPath))
                {
                    QuranConfig.Current.EngineeInfo = Serializer.Deserialize<EngineeInfo>(file);
                }
            }
        }
    }
}
