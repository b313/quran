using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;
using Quran.Util;

namespace Quran.Code
{
    public static class AppSettings
    {
        private static Quran.Model.Settings settings;

        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

        public static Dictionary<int, string> TextZooms = new Dictionary<int, string>();

        static AppSettings()
        {
            TextZooms.Add( 14, "50%");
            TextZooms.Add( 26, "100%" );
            TextZooms.Add(32, "200%");
        }

        public static int Repeat 
        {
            get
            {
                return settings.Repeat;
            }
            set
            {
                settings.Repeat = value;
            }
        }
        
        public static int Wait
        {
            get
            {
                return settings.Wait;
            }
            set
            {
                settings.Wait = value;
            }
        }

        public static int Translator
        {
            get
            {
                return settings.Translator;
            }
            set
            {
                settings.Translator = value;
            }
        }

        public static int Qari
        {
            get
            {
                return settings.Qari;
            }
            set
            {
                settings.Qari = value;
            }
        }

        public static bool HasAudio
        {
            get
            {
                return (Qari > 0);
            }
        }

        public static bool HasTranslation
        {
            get
            {
                return (Translator > 0);
            }
        }

        public static int TextZoom
        {
            get
            {
                return settings.TextZoom;               
            }
            set
            {
                if (value != TextZoom)
                {
                    settings.TextZoom = value;
                    NotifyStaticPropertyChanged("TextZoom");
                }
            }
        }

        public static void NotifyStaticPropertyChanged(string propertyName)
        {
            if (StaticPropertyChanged != null)
                StaticPropertyChanged(null, new PropertyChangedEventArgs(propertyName));
        }

        internal static void Load()
        {
            using (var file = File.OpenRead("Settings.dat"))
            {
                using (CryptoStream cs = new CryptoStream(file, Crypto.AES.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    settings = Serializer.Deserialize<Quran.Model.Settings>(cs);
                }
            }
        }


        internal static void Save()
        {
            using (var file = File.Create("Settings.dat"))
            {
                using (CryptoStream cs = new CryptoStream(file, Crypto.AES.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    Serializer.Serialize<Model.Settings>(cs, settings);
                }
            }

        }



    }
}
