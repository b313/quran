using ProtoBuf;

namespace Quran.Model
{
    [ProtoContract]
    public class TranslatedAya
    {
        [ProtoMember(1)]
        public int AyaNo { get; set; }

        [ProtoMember(2)]
        public TranslatedSlice[] Slices { get; set; }

        public override string ToString()
        {
            string result = "";
            for (int i = 0; i < Slices.Length; i++)
            {
                result += Slices[i];
            }
            return result;
        }

    }
}
