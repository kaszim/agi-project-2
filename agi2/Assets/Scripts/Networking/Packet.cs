using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Networking
{
    public enum Packet
    {
        Instantiate = 0,
        TransformUpdate,
        Last
    }

    static class PacketExtensions
    {
        public static byte ToByte(this Packet type)
        {
            return (byte) type;
        }
    }
}
