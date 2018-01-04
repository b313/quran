using ProtoBuf;

namespace Quran.Model
{
    [ProtoContract]
    public class Meta
    {
        [ProtoMember(1)]
        public SuraMeta[] Suras { get; set; }

        [ProtoMember(2)]
        public PartMeta[] Pages { get; set; }

        [ProtoMember(3)]
        public PartMeta[] Juzs { get; set; }

        [ProtoMember(4)]
        public PartMeta[] Hizbs { get; set; }

        [ProtoMember(5)]
        public PartMeta[] Manzils { get; set; }

        [ProtoMember(6)]
        public PartMeta[] Rukus { get; set; }

        [ProtoMember(7)]
        public Qari[] Qaris { get; set; }

        [ProtoMember(8)]
        public Translator[] Translators { get; set; }
    }

    [ProtoContract]
    public class SuraMeta
    {
        [ProtoMember(1)]
        public int SuraNo { get; set; }

        [ProtoMember(2)]
        public int TotalAyas { get; set; }

        [ProtoMember(3)]
        public int Order { get; set; }

        [ProtoMember(4)]
        public string NameArabic { get; set; }

        [ProtoMember(5)]
        public string NameEnglish { get; set; }

        [ProtoMember(6)]
        public string FullNameArabic
        {
            get;
            set;
        }

        [ProtoMember(7)]
        public bool IsMeccan { get; set; }

        public bool IsMedinan
        {
            get
            {
                return !IsMeccan;
            }
        }

        public override string ToString()
        {
            return string.Format("{0} - {1}", SuraNo.ToString().PadLeft(3, '0'), NameArabic);
        }
    }

    [ProtoContract]
    public class PartMeta
    {
        [ProtoMember(1)]
        public int Index { get; set; }

        [ProtoMember(2)]
        public int Sura { get; set; }

        [ProtoMember(3)]
        public int Aya { get; set; }
    }
}
