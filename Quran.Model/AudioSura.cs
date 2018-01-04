using ProtoBuf;

namespace Quran.Model
{
    [ProtoContract]
    public class AudioSura
    {
        [ProtoMember(1)]
        public AudioSlice[] AudioSlices { get; set; }
    }
}
