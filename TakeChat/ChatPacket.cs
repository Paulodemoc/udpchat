using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TakeChat
{
    public enum PACKET_TYPE
    {
        DISCOVERY = 0, CONFIRMATION, SETUP, SETUP_ERROR, SETUP_CONFIRMATION, TEXT
    }

    public class ChatPacket
    {
        public string Message { get; set; }
        public string Nickname { get; set; }
        public PACKET_TYPE PacketType { get; set; }
        public byte[] RawData { get; set; }
    }
}
