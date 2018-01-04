using ProtoBuf;

namespace Quran.Model
{
    [ProtoContract]
    public class Qari
    {
        [ProtoMember(1)]
        public int ID { get; set; }

        [ProtoMember(2)]
        public string Name { get; set; }

        [ProtoMember(3)]
        public string EnglishName { get; set; }

        [ProtoMember(4)]
        public bool[] Availability { get; set; }
    }
}
