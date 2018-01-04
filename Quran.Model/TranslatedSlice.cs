using System;
using ProtoBuf;

namespace Quran.Model
{
    [ProtoContract]
    public class TranslatedSlice
    {
        [ProtoMember(1)]
        public int SliceID { get; set; }

        [ProtoMember(2)]
        public String Text { get; set; }
    }
}
