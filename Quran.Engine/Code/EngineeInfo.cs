using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace Quran.Engine
{
    [ProtoContract]
    public class EngineeInfo
    {
        [ProtoMember(1)]
        public bool[] LockedSuras { get; set; }
    }
}
