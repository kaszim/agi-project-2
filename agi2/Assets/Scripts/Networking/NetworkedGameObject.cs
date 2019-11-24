using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Assets.Scripts.Networking;
using UnityEditor;
using UnityEngine;

namespace Networking
{
    class NetworkedGameObject : MonoBehaviour
    {
        public int TypeId;
        [HideInInspector]
        public long UID { get; set; }
        [HideInInspector]
        public bool Owner { get; set; }

        private Vector3 _pos;
        private Quaternion _rot;

        void Start()
        {
            _pos = transform.position;
            _rot = transform.rotation;
            Owner = false;
            if (UID == 0)
            {
                // Start is delayed to next frame, UID is set if we received this from server
                // Thus we did not receive from server, and we send it to the server
                UID = UnityClient._instance.InstantiateNetwork(this);
                // We also own this object
                Owner = true;
                Task.Run(SendUpdate);
            }
            var rb = GetComponent<Rigidbody>();
            if (!Owner && rb != null)
            {
                rb.isKinematic = true;
            }
        }

        private void Update()
        {
            // Why are you doing this you might ask
            // Well, it appears that once Unity tries to retrieve these while doing it from another thread
            // The internal c++ code will hit some kind of error, but will not say this.
            // Therefore the thread will stop running!
            _pos = transform.position;
            _rot = transform.rotation;
        }

        async void SendUpdate()
        {
            while (Owner)
            {
                UnityClient._instance.SendPacket(Packet.TransformUpdate, UID, _pos, _rot);
                await Task.Delay(33); // Roughly 30 times a second
            }
        }
    }
}
