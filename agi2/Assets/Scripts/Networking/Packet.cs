using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Networking
{
    public enum Packet
    {
        Instantiate = 0,
        TransformUpdate,
        ReadyAR,
        SpawnWorld,
        SyncVarUpdate,
        Destroy,
        GameState,
        Explode,
        Last,
    }

    static class PacketExtensions
    {
        public static byte ToByte(this Packet type)
        {
            return (byte)type;
        }
    }
}
