using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lidgren.Network;
using UnityEngine;

namespace Networking
{
    class UnityClient : MonoBehaviour
    {
        internal static UnityClient Instance;

        private Func<NetIncomingMessage, Action>[] _packetResponse;
        private NetClient _client;
        private bool _running;
        private Dictionary<long, GameObject> _gameObjects;
        // A queue for running Unity stuff in main thread
        private Queue<Action> _networkQueue;
        private GameObject GameWorld => GameObject.FindWithTag("GameWorld");

        // Just a helper to return no action
        private readonly Action DoNothing = () => { };

        private Dictionary<int, GameObject> _prefabs;
        // TODO: Detect these
        public GameObject[] NetworkedPrefabs;

        public GameState GameState { get; set; }
        // Event for GameState change
        public event Action<GameState> OnGameStateChange;

        public void Awake()
        {
            // TODO: Ensure singelton
            Instance = this;

            GameState = GameState.ARDetection;

            SetPacketResponses();
            _prefabs = new Dictionary<int, GameObject>();
            for (byte i = 0; i < NetworkedPrefabs.Length; i++)
            {
                _prefabs.Add(NetworkedPrefabs[i].GetComponent<NetworkedGameObject>().TypeId, NetworkedPrefabs[i]);
            }

            _gameObjects = new Dictionary<long, GameObject>();
            _networkQueue = new Queue<Action>();
            NetPeerConfiguration config = new NetPeerConfiguration("agi2");
            config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
            _client = new NetClient(config);
            _client.Start();
            _running = true;
            _client.DiscoverLocalPeers(54677);
            Task.Run(Listen);
        }

        private void SetPacketResponses()
        {
            _packetResponse = new Func<NetIncomingMessage, Action>[Packet.Last.ToByte() + 1];
            _packetResponse[Packet.Instantiate.ToByte()] = (msg) =>
            {
                var prefabId = msg.ReadInt32();
                var uid = msg.ReadInt64(); // Unique id for GameObject
                var pos = msg.ReadVector3();
                var rot = msg.ReadQuaternion();
                var scale = msg.ReadVector3();
                var s = _prefabs[prefabId];
                if (!_gameObjects.ContainsKey(uid))
                {
                    // If it does not already exist there create it
                    return () =>
                    {
                        var go = Instantiate(_prefabs[prefabId], pos, rot, GameWorld.transform);
                        go.transform.localScale = scale;
                        var ngo = go.GetComponent<NetworkedGameObject>();
                        ngo.UID =
                            uid; // Set UID so we don't send this object to server
                        ngo.IsOwned = false;
                        _gameObjects.Add(uid, go);
                    };
                }

                return DoNothing;
            };
            _packetResponse[Packet.TransformUpdate.ToByte()] = (msg) =>
            {
                var uid = msg.ReadInt64(); // Unique id for GameObject
                var pos = msg.ReadVector3();
                var rot = msg.ReadQuaternion();
                return () => _gameObjects[uid].GetComponent<NetworkedGameObject>().UpdateTransform(pos, rot);
            };
            _packetResponse[Packet.SyncVarUpdate.ToByte()] = (msg) =>
            {
                var uid = msg.ReadInt64();
                var go = _gameObjects[uid];
                var ngo = go.GetComponent<NetworkedGameObject>();
                return () => ngo.ReceiveSyncVarUpdate(msg.ReadInt32(), msg);
            };
            _packetResponse[Packet.Destroy.ToByte()] = (msg) =>
            {
                var uid = msg.ReadInt64();
                var go = _gameObjects[uid];
                _gameObjects.Remove(uid);
                return () => Destroy(go);
            };
            _packetResponse[Packet.GameState.ToByte()] = (msg) =>
            {
                GameState = (GameState)msg.ReadInt32();
                return () => OnGameStateChange(GameState);
            };
            _packetResponse[Packet.Explode.ToByte()] = (msg) =>
            {
                var name = msg.ReadString();
                return () =>
                {
                    var obj = GameWorld.transform.Find(name);
                    obj.GetComponent<DestroyMesh>().Explode(obj.transform.position);
                };
            };
        }

        public void FixedUpdate()
        {
            while (_networkQueue.Count > 0)
            {
                _networkQueue.Dequeue().Invoke();
            }
        }

        private void Listen()
        {
            Debug.Log("Client Started");
            while (_running)
            {
                // read messages
                NetIncomingMessage msg;
                while ((msg = _client.ReadMessage()) != null)
                {
                    switch (msg.MessageType)
                    {
                        case NetIncomingMessageType.DiscoveryResponse:
                            //TODO: Currently, just connect to first server found
                            Debug.Log($"Connected to server: {msg.SenderEndPoint}");
                            _client.Connect(msg.SenderEndPoint);
                            break;
                        case NetIncomingMessageType.Data:
                            var type = msg.ReadByte();
                            if (type < Packet.Last.ToByte())
                            {
                                _networkQueue.Enqueue(_packetResponse[type](msg));
                            }
                            else
                            {
                                Debug.LogError($"Received unknown packet {type}");
                            }
                            break;
                    }
                }
            }
        }

        public void SendPacket(Packet type, params object[] args)
        {
            NetOutgoingMessage om = _client.CreateMessage();
            om.Write(type.ToByte());
            // Write all other types
            foreach (dynamic arg in args)
            {
                om.Write(arg);
            }
            _client.SendMessage(om, NetDeliveryMethod.ReliableOrdered);
        }

        public long InstantiateNetwork(NetworkedGameObject go)
        {
            // Get a new uid
            var uid = DateTime.UtcNow.Ticks;
            SendPacket(Packet.Instantiate, go.TypeId, uid, go.transform.position, go.transform.rotation, go.transform.localScale);
            return uid;
        }

        public void DestroyNetwork(long UID)
        {
            SendPacket(Packet.Destroy, UID);
            _gameObjects.Remove(UID);
        }
    }
}
