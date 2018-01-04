using ProtoBuf;

namespace Quran.Model
{
    [ProtoContract]
    public class TranslatedSura
    {
        [ProtoMember(1)]
        public TranslatedAya[] Ayas { get; set; }

        [ProtoMember(2)]
        public TranslatedSlice[] AllSlices { get; set; }
    }
}
