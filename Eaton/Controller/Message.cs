using ProtoBuf;

namespace Controller
{
    [ProtoContract]
    public class Message
    {
        [ProtoMember(1)]
        public int Measurements { get; set; }

        [ProtoMember(2)]
        public Guid SenderID { get; set; }
    }
}
