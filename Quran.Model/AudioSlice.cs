using System;
using ProtoBuf;

namespace Quran.Model
{
    [ProtoContract]
    public class AudioSlice
    {
        [ProtoMember(1)]
        public int SliceID { get; set; }

        [ProtoMember(2)]
        public TimeSpan Start { get; set; }

        [ProtoMember(3)]
        public TimeSpan Finish { get; set; }

    }
}
