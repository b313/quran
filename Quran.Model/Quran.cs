using ProtoBuf;

namespace Quran.Model
{
    [ProtoContract]
    public class Quran
    {
        [ProtoMember(1)]
        public Sura[] Suras { get; set; }
 
        [ProtoMember(2)]
        public int TextType { get; set; }

        [ProtoMember(3)]
        public string Version { get; set; }

        [ProtoMember(4)]
        public string Created { get; set; }

        [ProtoMember(5)]
        public string Description { get; set; }

        public QuranTextType Type
        {
            get
            {
                return (QuranTextType)TextType;
            }
        }
    }

    public enum QuranTextType
    {
        Simple= 0,
        Uthmani = 1
    }
}
