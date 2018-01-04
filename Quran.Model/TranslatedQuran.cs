using ProtoBuf;

namespace Quran.Model
{
    [ProtoContract]
    public class TranslatedQuran
    {
        [ProtoMember(1)]
        public TranslatedSura[] Suras{ get; set; }

        [ProtoMember(2)]
        public int LanguageID { get; set; }

        [ProtoMember(3)]
        public string Translator { get; set; }

        [ProtoMember(4)]
        public string TranslatorID { get; set; }

        [ProtoMember(5)]
        public string LastUpdate { get; set; }

        public Language Language
        {
            get
            {
                return (Language)LanguageID;
            }
        }
    }

    public enum Language
    {
        Persian = 1,
        English = 2
    }

}
