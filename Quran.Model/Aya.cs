using ProtoBuf;

namespace Quran.Model
{
    [ProtoContract]
    public class Aya
    {
        [ProtoMember(1)]
        public int AyaNo { get; set; }

        [ProtoMember(2)]
        public Slice[] Slices { get; set; }

        [ProtoMember(3)]
        public bool HasSajda { get; set; }

        [ProtoMember(4)]
        public bool IsSajdaVajeb { get; set; }

        public bool IsSajdaMostahabi
        {
            get
            {
                return !IsSajdaVajeb;
            }
        }

        [ProtoMember(5)]
        public int Page { get; set; }

        [ProtoMember(6)]
        public int Juz { get; set; }

        [ProtoMember(7)]
        public int Hizb { get; set; }

        [ProtoMember(8)]
        public int Manzil { get; set; }

        [ProtoMember(9)]
        public int Ruku { get; set; }

        public override string ToString()
        {
            string result = "";
            for (int i = 0; i < Slices.Length; i++)
            {
                result += Slices[i].Text;
            }
            return result;
        }
    }
}
