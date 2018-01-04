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
    public static class QuranExtractor
    {
        public static void Extract(  )
        {
            if (QuranConfig.Current.IsUthmani )
                ExtractUthmani_MidStage();
            else
                ExtractSimple_MidStage();

            Finalize();
        }

        #region Middle Stage Methods

        private static void ExtractUthmani_MidStage()
        {
            string outPath = string.Format("../../Out/{0}-midstage.txt", QuranConfig.Current.FileName);

            if (File.Exists(outPath))
                File.Delete(outPath);

            string[] lines = LoadUthmani();

            int suraNo = 0;
            int ayaNo = 0;
            List<string> quran = new List<string>();

            foreach (string line in lines)
            {
                if (line.StartsWith("سُورَةُ "))
                {
                    suraNo++;
                    ayaNo = 0;

                    if ( suraNo != 1 && suraNo != 9)
                        quran.Add( suraNo.ToString().PadLeft(3, '0') + "|000|بِسۡمِ ٱللَّهِ ٱلرَّحۡمَٰنِ ٱلرَّحِيمِ" );
                    continue;
                }

                string currentLine = line;

                do
                {
                    ayaNo++;

                    string ayaNoArabic = Utility.GetArabicNo(ayaNo);
                    int index = currentLine.IndexOf(ayaNoArabic);

                    if (index > 0)
                    {
                        string aya = currentLine.Substring(0, index + ayaNoArabic.Length).TrimStart();
                        quran.Add(string.Format("{0}|{1}|{2}", suraNo.ToString().PadLeft(3, '0'), ayaNo.ToString().PadLeft(3, '0'), aya));
                        currentLine = currentLine.Substring(index + ayaNoArabic.Length);
                    }
                    else
                        break;
                } while (true);
            }

            using (StreamWriter sw = new StreamWriter(outPath, false, Encoding.UTF8))
            {
                foreach (string aya in quran)
                {
                    sw.WriteLine(aya);
                }
            }
        }

        private static string[] LoadUthmani()
        {
            StringBuilder sb = new StringBuilder();

            string quran = File.ReadAllText(string.Format( "../../In/{0}.txt", QuranConfig.Current.FileName ));
            string[] lines = quran.Split(new string[] { "\r\n" }, StringSplitOptions.None);

            string sura = "";
            foreach (string line in lines)
            {
                if (line.StartsWith("سُورَةُ ") )
                {
                    if (sura.Length > 0)
                    {
                        sb.AppendLine("سُورَةُ ");
                        sb.AppendLine(sura.Replace("\r\n", ""));
                        sura = "";
                    }
                }
                else if (line.Trim() == "بِسۡمِ ٱللَّهِ ٱلرَّحۡمَٰنِ ٱلرَّحِيمِ")
                    continue;
                else
                    sura += line;
            }

            sb.AppendLine("سُورَةُ ");
            sb.AppendLine(sura.Replace("\r\n", ""));

            return sb.ToString().Split(new string[] { "\r\n" }, StringSplitOptions.None);
        }

        private static void ExtractSimple_MidStage()
        {
            string outPath = string.Format("../../Out/{0}-midstage.txt", QuranConfig.Current.FileName);

            if (File.Exists(outPath))
                File.Delete(outPath);

            string quranContent = File.ReadAllText(string.Format("../../In/{0}.txt", QuranConfig.Current.FileName));
            string[] lines = quranContent.Split(new string[] { "\r\n" }, StringSplitOptions.None);

            int suraNo = 1;
            int ayaNo = -1;
            int lineNo = 0;
            string besm = lines[0];

            List<string> quran = new List<string>();

            foreach (var sura in QuranConfig.Current.Meta.Suras)
            {
                for (int i = 0; i < sura.TotalAyas; i++)
                {
                    ayaNo = i + 1;

                    if ( ayaNo == 1 && suraNo != 1 && suraNo != 9 )
                    {
                        quran.Add(string.Format("{0}|000|{1}", suraNo.ToString().PadLeft(3, '0'), besm ));
                        quran.Add(string.Format("{0}|001|{1} {2}", suraNo.ToString().PadLeft(3, '0'), lines[lineNo].Replace(besm + " ", ""), Utility.GetArabicNo(ayaNo)));
                    }
                    else
                        quran.Add(string.Format("{0}|{1}|{2} {3}", suraNo.ToString().PadLeft(3, '0'), ayaNo.ToString().PadLeft(3, '0'), lines[lineNo], Utility.GetArabicNo(ayaNo)));
                    
                    ++lineNo;
                }
                ++suraNo;
            }

            using (StreamWriter sw = new StreamWriter(outPath, false, Encoding.UTF8))
            {
                foreach (string aya in quran)
                {
                    sw.WriteLine(aya);
                }
            }

        }

        #endregion

        #region Finalize Methods

        private static void Finalize()
        {
            string path = string.Format("../../Out/{0}-midstage.txt", QuranConfig.Current.FileName);
            string[] lines = File.ReadAllText(path).Split(new string[] { "\r\n" }, StringSplitOptions.None);

            Model.Quran quran = new Model.Quran();
            quran.TextType = (int)QuranTextType.Uthmani;
            quran.Version = QuranConfig.Current.Version;
            quran.Created = DateTime.Today.ToShortDateString();
            quran.Description = "";

            List<Sura> suras = new List<Sura>();
            List<Aya> ayas = new List<Aya>();

            int suraNo = 1;
            int ayaNo = 1;
            foreach (string line in lines)
            {
                if ( line.Trim().Length == 0 || Int32.Parse(line.Substring(0, 3)) != suraNo)
                {
                    Sura sura = new Sura();
                    sura.Ayas = (Aya[])ayas.ToArray();
                    sura.Index = QuranConfig.Current.Meta.Suras[suraNo - 1].SuraNo;
                    sura.TotalAyas = QuranConfig.Current.Meta.Suras[suraNo - 1].TotalAyas;
                    sura.Order = QuranConfig.Current.Meta.Suras[suraNo - 1].Order;
                    sura.NameArabic = QuranConfig.Current.Meta.Suras[suraNo - 1].NameArabic;
                    sura.NameEnglish = QuranConfig.Current.Meta.Suras[suraNo - 1].NameEnglish;
                    sura.IsMeccan = QuranConfig.Current.Meta.Suras[suraNo - 1].IsMeccan;
                    suras.Add(sura);

                    // Reset 
                    ayas = new List<Aya>();
                    ++suraNo;

                    if (suraNo == 1 || suraNo == 9)
                        ayaNo = 1;
                    else
                        ayaNo = 0;
                }


                if (line.Trim().Length > 0)
                {
                    Aya aya = new Aya();
                    aya.AyaNo = ayaNo;
                    if ( ayaNo == 0 )
                        aya.Slices = new Slice[1] { new Slice() { SliceID = suraNo * 1000000 , AyaNo = ayaNo, Text = line.Remove(0, 8) + " " } };
                    else
                        aya.Slices = new Slice[1] { new Slice() { SliceID = suraNo * 1000000 + ayaNo * 1000 + 1, AyaNo = ayaNo, Text = line.Remove(0, 8) + " " }};

                    ApplySajda(suraNo, aya);

                    aya.Page = GetValue(QuranConfig.Current.Meta.Pages, suraNo, aya.AyaNo);
                    aya.Juz = GetValue(QuranConfig.Current.Meta.Juzs, suraNo, aya.AyaNo);
                    aya.Hizb = GetValue(QuranConfig.Current.Meta.Hizbs, suraNo, aya.AyaNo);
                    aya.Manzil = GetValue(QuranConfig.Current.Meta.Manzils, suraNo, aya.AyaNo);
                    aya.Ruku = GetValue(QuranConfig.Current.Meta.Rukus, suraNo, aya.AyaNo);

                    ayas.Add(aya);

                    ++ayaNo;
                }
            }

            quran.Suras = suras.ToArray();

            if (quran.Suras.Length != 114)
                throw new ApplicationException("There is no 114 Suras!");

            using (var file = File.Create(string.Format("../../Out/{0}.dat", QuranConfig.Current.FileName)))
            {
                using (CryptoStream cs = new CryptoStream(file, Crypto.AES.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    Serializer.Serialize<Model.Quran>(cs, quran);
                }
            }
        }

        #endregion

        #region Utility Methods

        private static void ApplySajda( int suraNo, Aya aya)
        {
            if (suraNo == 7 && aya.AyaNo == 206)
            {
                aya.HasSajda = true;
                aya.IsSajdaVajeb = false;
            }
            else if (suraNo == 13 && aya.AyaNo == 15)
            {
                aya.HasSajda = true;
                aya.IsSajdaVajeb = false;
            }
            else if (suraNo == 16 && aya.AyaNo == 50)
            {
                aya.HasSajda = true;
                aya.IsSajdaVajeb = false;
            }
            else if (suraNo == 17 && aya.AyaNo == 109)
            {
                aya.HasSajda = true;
                aya.IsSajdaVajeb = false;
            }
            else if (suraNo == 19 && aya.AyaNo == 58)
            {
                aya.HasSajda = true;
                aya.IsSajdaVajeb = false;
            }
            else if (suraNo == 22 && aya.AyaNo == 18)
            {
                aya.HasSajda = true;
                aya.IsSajdaVajeb = false;
            }
            else if (suraNo == 22 && aya.AyaNo == 77)
            {
                aya.HasSajda = true;
                aya.IsSajdaVajeb = false;
            }
            else if (suraNo == 25 && aya.AyaNo == 60)
            {
                aya.HasSajda = true;
                aya.IsSajdaVajeb = false;
            }
            else if (suraNo == 27 && aya.AyaNo == 26)
            {
                aya.HasSajda = true;
                aya.IsSajdaVajeb = false;
            }
            else if (suraNo == 32 && aya.AyaNo == 15)
            {
                aya.HasSajda = true;
                aya.IsSajdaVajeb = true;
            }
            else if (suraNo == 38 && aya.AyaNo == 24)
            {
                aya.HasSajda = true;
                aya.IsSajdaVajeb = false;
            }
            else if (suraNo == 41 && aya.AyaNo == 38)
            {
                aya.HasSajda = true;
                aya.IsSajdaVajeb = true;
            }
            else if (suraNo == 53 && aya.AyaNo == 62)
            {
                aya.HasSajda = true;
                aya.IsSajdaVajeb = true;
            }
            else if (suraNo == 84 && aya.AyaNo == 21)
            {
                aya.HasSajda = true;
                aya.IsSajdaVajeb = false;
            }
            else if (suraNo == 96 && aya.AyaNo == 19)
            {
                aya.HasSajda = true;
                aya.IsSajdaVajeb = true;
            }
        }

        private static int GetValue(PartMeta[] partMetas, int suraNo, int ayaNo)
        {
            int val = partMetas.Length;

            for (int i = 0; i < partMetas.Length - 1; i++)
            {
                int j = i+1;

                bool c1 = partMetas[i].Sura < suraNo;
                bool c2 = partMetas[i].Sura == suraNo && partMetas[i].Aya <= ayaNo;
                bool c3 = partMetas[j].Sura > suraNo;
                bool c4 = partMetas[j].Sura == suraNo && ayaNo < partMetas[j].Aya;
                
                if ( ( c1 || c2 ) && ( c3 || c4) )
                {
                    val = partMetas[i].Index;
                    break;
                }

            }

            return val;
        }

        #endregion
    }
}
