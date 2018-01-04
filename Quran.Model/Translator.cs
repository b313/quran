using ProtoBuf;

namespace Quran.Model
{
    [ProtoContract]
    public class Translator
    {
        [ProtoMember(1)]
        public string ID { get; set; }

        [ProtoMember(2)]
        public string Name { get; set; }


        private static Translator noTranslator = new Translator() { ID = "", Name = "بدون ترجمه" };
        public static Translator NoTranslator
        {
            get
            {
                return noTranslator;
            }
        }
    }
}
