using ProtoBuf;

namespace Quran.Model
{
    [ProtoContract]
    public class Settings
    {
        [ProtoMember(1)]
        public int Repeat { get; set; }

        [ProtoMember(2)]
        public int Wait { get; set; }

        [ProtoMember(3)]
        public int Qari { get; set; }

        [ProtoMember(4)]
        public int Translator { get; set; }

        [ProtoMember(5)]
        public int TextType { get; set; }

        [ProtoMember(6)]
        public int TextZoom { get; set; }
    }
}
