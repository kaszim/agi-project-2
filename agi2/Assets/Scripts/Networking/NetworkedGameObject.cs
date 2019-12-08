using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Lidgren.Network;
using UnityEngine;

namespace Networking
{
    public enum Player
    {
        Red,
        Blue
    }

    class NetworkedGameObject : MonoBehaviour
    {
        public int TypeId;
        [HideInInspector]
        public long UID { get; set; }
        [HideInInspector]
        public bool IsOwned { get; set; } = true;
        public static Player Player => UnityServer.Instance.enabled ? Player.Red : Player.Blue;

        private Vector3 _pos;
        private Quaternion _rot;
        private Dictionary<int, SyncVar> _syncVars;

        public void Start()
        {
            _syncVars = new Dictionary<int, SyncVar>();
            _pos = transform.position;
            _rot = transform.rotation;
            if (IsOwned)
            {
                // Start is delayed to next frame, Owner is false if received from server
                // Thus we did not receive from server, and we send it to the server
                UID = UnityClient.Instance.InstantiateNetwork(this);
                Task.Run(SendUpdate);
            }
            
            // Find SyncVars
            foreach (var nc in GetComponents<NetworkedBehaviour>())
            {
                var ncType = nc.GetType();
                // Loop through all properties
                foreach (var p in ncType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public))
                {
                    var syncVar = p.GetCustomAttribute<SyncVar>();
                    if (syncVar != null)
                    {
                        syncVar.Component = nc;
                        syncVar.Field = p;
                        _syncVars.Add(syncVar.UID, syncVar);
                        if (IsOwned)
                        {
                            Task.Run(() => SyncVarUpdate(syncVar));
                        }
                    }
                }
            }
        }

        private void OnDestroy()
        {
            if (IsOwned)
            {
                // To stop updates
                IsOwned = false;
                // Send one last position update
                UnityClient.Instance.SendPacket(Packet.TransformUpdate, UID, transform.localPosition, transform.localRotation);
                UnityClient.Instance.DestroyNetwork(UID);
            }
        }

        private void Update()
        {
            // Why are you doing this you might ask
            // Well, it appears that once Unity tries to retrieve these while doing it from another thread
            // The internal c++ code will hit some kind of error, but will not say this.
            // Therefore the thread will stop running!
            _pos = transform.localPosition;
            _rot = transform.localRotation;
        }

        async void SendUpdate()
        {
            while (IsOwned)
            {
                UnityClient.Instance.SendPacket(Packet.TransformUpdate, UID, _pos, _rot);
                await Task.Delay(16); // Roughly 60 times a second
            }
        }

        async void SyncVarUpdate(SyncVar syncVar)
        {
            while (IsOwned)
            {
                UnityClient.Instance.SendPacket(Packet.SyncVarUpdate, UID, syncVar.UID, Marshal.SizeOf(syncVar.Field.FieldType), syncVar.Field.GetValue(syncVar.Component));
                await Task.Delay(syncVar.UpdateFrequency);
            }
        }

        public void UpdateTransform(Vector3 pos, Quaternion rot)
        {
            transform.localPosition = pos;
            transform.localRotation = rot;
        }

        public void ReceiveSyncVarUpdate(int UID, NetIncomingMessage msg)
        {
            var syncVar = _syncVars[UID];
            switch (Type.GetTypeCode(syncVar.Field.FieldType))
            {
                case TypeCode.Int32:
                    syncVar.Field.SetValue(syncVar.Component, msg.ReadInt32());
                    break;
            }
        }
    }
}
