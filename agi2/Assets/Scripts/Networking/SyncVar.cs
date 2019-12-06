using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


namespace Networking
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    class SyncVar : Attribute
    {
        public int UpdateFrequency { get; private set; }
        public int UID => $"{Component.GetType().Name}.{Field.Name}".GetHashCode();
        public NetworkedBehaviour Component { get; set; }
        public FieldInfo Field { get; set; }


        public SyncVar(int updateFrequency = 100)
        {
            UpdateFrequency = updateFrequency;
        }
    }
}

