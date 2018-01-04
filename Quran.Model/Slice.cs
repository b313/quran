using ProtoBuf;

namespace Quran.Model
{
    [ProtoContract]
    public class Slice
    {
        [ProtoMember(1)]
        public int SliceID { get; set; }

        [ProtoMember(2)]
        public int SliceIndex { get; set; }

        [ProtoMember(3)]
        public int AyaNo { get; set; }

        [ProtoMember(4)]
        public string Text { get; set; }
    }
}
