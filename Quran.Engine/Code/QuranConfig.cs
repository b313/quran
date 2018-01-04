using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quran.Model;

namespace Quran.Engine
{
    public static class QuranConfig
    {
        public const char stop_Mim = 'ۘ';
        public const char stop_La = ' ';
        public const char stop_Sali = 'ۖ';
        public const char stop_Gholi = 'ۗ';
        public const char stop_Jim = 'ۚ';
        public const char unknown = '۞';

        public static class Current
        {
            private static string version_Uthmani = "0.9";
            private static string version_Simple = "1.0.2";
            private static string fileName_Uthmani;
            private static string fileName_Simple;

            static Current()
            {
                fileName_Uthmani = "quran-uthmani-" + version_Uthmani;
                fileName_Simple = "quran-simple-enhanced-" + version_Simple;

                if (CurrentTextType == Model.QuranTextType.Simple)
                {
                    FileName = fileName_Simple;
                    Version = version_Simple;
                }
                else
                {
                    FileName = fileName_Uthmani;
                    Version = version_Uthmani;
                }

                FilePath = string.Format("../../Out/{0}.dat", FileName);
                MetaPath = "../../Out/meta.dat";
                TransPath = string.Format("../../Out/{0}.dat", Translator); ;
                EngineeInfoPath = "../../Out/engineeinfo.dat";
            }

            public static Meta Meta { get; set; }

            public static EngineeInfo EngineeInfo { get; set; }

            public static QuranTextType CurrentTextType
            {
                get
                {
                    return Model.QuranTextType.Uthmani;
                }
            }

            public static bool IsUthmani
            {
                get
                {
                    return CurrentTextType == QuranTextType.Uthmani;
                }
            }

            public static string Translator
            {
                get
                {
                    return "fa.makarem";
                }
            }
            
            public static string FileName
            {
                get;
                private set;
            }

            public static string MetaPath
            {
                get;
                private set;
            }

            public static string TransPath
            {
                get;
                private set;
            }

            public static string EngineeInfoPath
            {
                get;
                private set;
            }

            public static string FilePath
            {
                get;
                private set;
            }

            public static string Version
            {
                get;
                private set;
            }
        }
    }
}
