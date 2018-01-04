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
    public static class TranslationExtractor
    {
        private const string idLabel = "#  ID: ";
        private const string nameLabel = "#  Name: ";
        private const string langLabel = "#  Language: ";
        private const string dateLabel = "#  Last Update: ";

        private static int lineNo = 0;
        private static TranslatedQuran translatedQuran = new TranslatedQuran();

        public static void Extract()
        {
            string text = File.ReadAllText( string.Format( "../../In/{0}.txt", QuranConfig.Current.Translator ) );

            string[] ayas = text.Split(new string[] { "\n"  }, StringSplitOptions.RemoveEmptyEntries);

            lineNo = 0;

            GetTranslation(ayas);

            Modify();

            MakeAllSlices();

            GetInfo(ayas);
            
            Save();

        }

        private static void Modify()
        {
            for (int i = 0; i < 114; i++)
            {
                int suraNo = i+1;
                if (suraNo == 1 || suraNo == 9)
                    continue;

                TranslatedAya besmAya = new TranslatedAya();
                besmAya.Slices = new TranslatedSlice[1];
                besmAya.Slices[0] = new TranslatedSlice(){ SliceID = suraNo * 1000000, Text = translatedQuran.Suras[0].Ayas[0].Slices[0].Text };

                List<TranslatedAya> list = translatedQuran.Suras[i].Ayas.ToList();
                list.Insert(0, besmAya);
                translatedQuran.Suras[i].Ayas = list.ToArray<TranslatedAya>();
            }
        }

        private static void GetTranslation(string[] ayas)
        {
            translatedQuran.Suras = new TranslatedSura[114];

            int i = 0;
            string starter = "";
            foreach (SuraMeta sura in QuranConfig.Current.Meta.Suras)
            {
                i = sura.SuraNo - 1;
                
                translatedQuran.Suras[i] = new TranslatedSura() { Ayas = new TranslatedAya[sura.TotalAyas] };

                for (int j = 0; j < sura.TotalAyas; j++)
                {
                    int ayaNo = j + 1;
                    string line = ayas[lineNo];
                    starter = sura.SuraNo + "|" + (j + 1) + "|";

                    if (!line.StartsWith(starter))
                        throw new ApplicationException("Starter has not found! " + starter);

                    line = line.Substring(starter.Length);
                    (translatedQuran.Suras[i]).Ayas[j] = new TranslatedAya() { Slices = new TranslatedSlice[1] { new TranslatedSlice() {  SliceID =sura.SuraNo * 1000000 + ayaNo * 1000 + 1, Text = line } } };

                    ++lineNo;
                }
            }
        }

        private static void MakeAllSlices()
        {
            foreach (var sura in translatedQuran.Suras)
            {
                List<TranslatedSlice> tempList = new List<TranslatedSlice>();
                foreach (var aya in sura.Ayas)
                {
                    TranslatedSlice ts = new TranslatedSlice();
                    ts.SliceID = aya.Slices[0].SliceID;
                    ts.Text = aya.Slices[0].Text;
                    tempList.Add(ts);
                }

                sura.AllSlices = tempList.ToArray();
            }
        }

        private static void GetInfo(string[] ayas)
        {
            for (int k = lineNo; k < ayas.Length; k++)
            {
                string line = ayas[k];

                if (line.StartsWith(nameLabel))
                {
                    translatedQuran.Translator = line.Substring(nameLabel.Length);
                }
                else if (line.StartsWith(idLabel))
                {
                    translatedQuran.TranslatorID = line.Substring(idLabel.Length);
                }
                else if (line.StartsWith(langLabel))
                {
                    string lang = line.Substring(langLabel.Length);
                    translatedQuran.LanguageID = (int)Enum.Parse(typeof(Language), lang);
                }
                else if (line.StartsWith(dateLabel))
                {
                    translatedQuran.LastUpdate = line.Substring(dateLabel.Length);
                }
            }
        }

        private static void Save()
        {

            using (var file = File.Create( QuranConfig.Current.TransPath ))
            {
                using (CryptoStream cs = new CryptoStream(file, Crypto.AES.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    Serializer.Serialize<Model.TranslatedQuran>(cs, translatedQuran);
                }
            }
        }
    }
}
