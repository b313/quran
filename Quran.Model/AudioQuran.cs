using ProtoBuf;

namespace Quran.Model
{
    [ProtoContract]
    public class AudioQuran
    {
        [ProtoMember(1)]
        public AudioSura[] AudioSuras{ get; set; }

        [ProtoMember(2)]
        public Qari Qari { get; set; }

        [ProtoMember(3)]
        public string Album { get; set; }

        [ProtoMember(4)]
        public string LastUpdate { get; set; }
    }
}
