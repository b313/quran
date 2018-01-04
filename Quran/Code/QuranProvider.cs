using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Windows.Documents;
using ProtoBuf;
using Quran.Model;
using Quran.Util;

namespace Quran.Code
{
    internal static class QuranProvider
    {
        private static Dictionary<int, AyaSpan[]> dic = new Dictionary<int, AyaSpan[]>();
        internal static Dictionary<int, Run[]> runs = new Dictionary<int, Run[]>();
        internal static Dictionary<int, string> trans = new Dictionary<int, string>();
        internal static Dictionary<int, AudioSlice> audio = new Dictionary<int, AudioSlice>();

        #region Properties 

        internal static Quran.Model.Quran Quran
        {
            get;
            private set;
        }

        internal static Quran.Model.TranslatedQuran TranslatedQuran
        {
            get;
            private set;
        }

        private static Quran.Model.AudioQuran AudioQuran
        {
            get;
            set;
        }

        internal static Quran.Model.Meta Meta
        {
            get;
            private set;
        }

        #endregion

        #region Init Methods 
        
        internal static void Init()
        {
            Init(false);
        }

        internal static void Init( bool warmStartup )
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            InitMeta(assembly);

            InitQuran(assembly);

            AppSettings.Load();

            InitTrans(assembly);

            InitAudio(assembly, Meta.Qaris[4]);

            if (warmStartup)
            {
                Paragraph paragraph = new Paragraph();

                for (int suraNo = 1; suraNo < 115; suraNo++)
                {
                    System.Threading.Tasks.Task.Run( () => InitProvider(suraNo, paragraph) );
                }
            }

        }

        private static void InitMeta( Assembly assembly )
        {
            //using (var file = File.OpenRead("Data/meta.dat"))
            using (var file = assembly.GetManifestResourceStream("Quran.Data.meta.dat"))
            {
                using (CryptoStream cs = new CryptoStream(file, Crypto.AES.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    Meta = Serializer.Deserialize<Quran.Model.Meta>(cs);
                }
            }
        }

        private static void InitQuran( Assembly assembly )
        {
            //using (var file = File.OpenRead("Data/quran-uthmani.dat"))
            using (var file = assembly.GetManifestResourceStream("Quran.Data.quran-uthmani-0.9.dat"))
            {
                using (CryptoStream cs = new CryptoStream(file, Crypto.AES.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    Quran = Serializer.Deserialize<Quran.Model.Quran>(cs);
                }
            }
        }

        private static void InitTrans(Assembly assembly)
        {
            using (var file = assembly.GetManifestResourceStream("Quran.Data.fa.makarem.dat"))
            {
                using (CryptoStream cs = new CryptoStream(file, Crypto.AES.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    TranslatedQuran = Serializer.Deserialize<Quran.Model.TranslatedQuran>(cs);
                }
            }
        }

        internal static void InitAudio()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            InitAudio(assembly, Meta.Qaris[AppSettings.Qari]);
        }

        internal static void InitAudio(Assembly assembly, Quran.Model.Qari qari)
        {
            try
            {
                audio.Clear();

                if (qari.ID == 0)
                    return;

                //using (var file = File.OpenRead( string.Format( "Data/{0}.dat", qari.EnglishName ) ))
                string fileName = string.Format("Quran.Data.{0}.dat", qari.EnglishName);
                using (var file = assembly.GetManifestResourceStream(fileName))
                {
                    using (CryptoStream cs = new CryptoStream(file, Crypto.AES.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        AudioQuran = Serializer.Deserialize<Quran.Model.AudioQuran>(cs);
                    }
                }

                foreach (var sura in AudioQuran.AudioSuras)
                {
                    // TO DO : S H O U L D   B E    R E M O V E D
                    if (sura.AudioSlices == null)
                        continue;

                    foreach (var slice in sura.AudioSlices)
                    {
                        audio.Add(slice.SliceID, slice);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorManager.Handle("QuranProvider >> InitAudio >> Qari:" + qari.ID, ex);
            }
        }


        #endregion

        #region Quran Methods
        
        internal static void InitProvider( int suraNo, Paragraph mainParagraph )
        {
            try
            {
                if (!dic.ContainsKey(suraNo))
                {
                    List<Run> tempRunList = new List<Run>();
                    

                    int sliceIndex = 0;
                    List<AyaSpan> list = new List<AyaSpan>();

                    foreach (var aya in Quran.Suras[suraNo - 1].Ayas)
                    {
                        AyaSpan ayaSpan = new AyaSpan() { Tag = aya.AyaNo };

                        foreach (var slice in aya.Slices)
                        {
                            SliceInfo sliceInfo = new SliceInfo() { AyaNo = aya.AyaNo, SliceID = slice.SliceID, SliceIndex = sliceIndex };
                            Run run = new Run(slice.Text) { Tag = sliceInfo };
                            tempRunList.Add(run);
                            trans.Add(slice.SliceID, TranslatedQuran.Suras[suraNo - 1].AllSlices[sliceIndex++].Text);
                            ayaSpan.Inlines.Add(new Span(run));
                        }

                        if (aya.AyaNo != 0)
                        {
                            list.Add(ayaSpan);
                            mainParagraph.Inlines.Add(ayaSpan);
                        }
                    }

                    dic[suraNo] = list.ToArray<AyaSpan>();
                    runs.Add(suraNo, tempRunList.ToArray());
                }
                else
                {
                    foreach (var ayaSpan in dic[suraNo])
                    {
                        mainParagraph.Inlines.Add(ayaSpan);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorManager.Handle("QuranProvider >> InitProvider >> Sura:" + suraNo, ex);
                throw new ApplicationException();
            }
        }

        internal static string GetSuraName(int suraNo)
        {
            return Meta.Suras[suraNo - 1].NameArabic;
        }

        internal static Span GetIntroAya(int suraNo)
        {
            return new Span(runs[suraNo][0]);
        }

        internal static Run GetRun(int suraNo, int ayaNo)
        {
            AyaSpan ayaSpan = dic[suraNo][ayaNo - 1] as AyaSpan;
            Span span = ayaSpan.Inlines.FirstInline as Span;
            return span.Inlines.FirstInline as Run;
        }
        
        #endregion

        #region Helper Methods

        internal static string GetQariName()
        {
            return Meta.Qaris[AppSettings.Qari].Name;
        }

        internal static string GetAudioPath( int suraNo )
        {
            string qariName = Meta.Qaris[AppSettings.Qari].EnglishName;
            
            return string.Format("Sounds//{0}//{1}.mp3", qariName, suraNo.ToString().PadLeft(3, '0'));
        }

        internal static void ChangeQari(int suraNo)
        {
            if (!Meta.Qaris[AppSettings.Qari].Availability[suraNo - 1] || !IsAudioFileExist(suraNo))
            {
                int nextQari = 0;

                for (int i = 1; i < Meta.Qaris.Length; i++)
                {
                    if (Meta.Qaris[i].Availability[suraNo - 1])
                    {
                        if (IsAudioFileExist(Meta.Qaris[i], suraNo))
                        {
                            nextQari = i;
                            break;
                        }
                    }
                }

                AppSettings.Qari = nextQari;
            }
        }

        internal static SajdaType GetSajdaInfo(int suraNo, int ayaNo )
        {
            if ( suraNo == 9 && ayaNo > 0 )
                ayaNo--;
            if (suraNo == 1)
                ayaNo--;

            if (!Quran.Suras[suraNo - 1].Ayas[ayaNo].HasSajda)
                return SajdaType.None;

            if (Quran.Suras[suraNo - 1].Ayas[ayaNo].IsSajdaMostahabi)
                return SajdaType.Mostahab;

            if (Quran.Suras[suraNo - 1].Ayas[ayaNo].IsSajdaVajeb)
                return SajdaType.Vajeb;
            
            return SajdaType.None;
        }


        internal static string GetCurrentAudioFilePath( int suraNo )
        {
            return string.Format("Sounds//{0}//{1}.mp3", Meta.Qaris[AppSettings.Qari].EnglishName, suraNo.ToString().PadLeft(3, '0'));
        }

        internal static bool IsAudioFileExist(Qari qari, int suraNo)
        {
            string path = string.Format("Sounds//{0}//{1}.mp3", qari.EnglishName, suraNo.ToString().PadLeft(3, '0'));
            return File.Exists(path);
        }

        internal static bool IsAudioFileExist(int suraNo)
        {
            return System.IO.File.Exists(GetCurrentAudioFilePath(suraNo ) );
        }
        #endregion


        internal enum SajdaType
        {
            None,
            Mostahab,
            Vajeb

        }



    }
}
