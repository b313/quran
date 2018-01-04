using ProtoBuf;

namespace Quran.Model
{
    [ProtoContract]
    public class Sura
    {
        [ProtoMember(1)]
        public Aya[] Ayas { get; set; } 

        [ProtoMember(2)]
        public int Index { get; set; }

        [ProtoMember(3)]
        public int TotalAyas { get; set; }

        [ProtoMember(4)]
        public int Order { get; set; }

        [ProtoMember(5)]
        public string NameArabic { get; set; }

        [ProtoMember(6)]
        public string NameEnglish { get; set; }

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
            return string.Format("{0} - {1}", Index.ToString().PadLeft(3, '0'), NameArabic);
        }

    }
}
