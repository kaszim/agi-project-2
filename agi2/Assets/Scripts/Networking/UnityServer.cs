using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lidgren.Network;
using Networking;
using UnityEngine;
using UnityEngine.Networking;

public enum GameState
{
    ARDetection,
    Game,
}

public class UnityServer : MonoBehaviour
{
    internal static UnityServer Instance { get; set; }
    public bool Server = false;
    private NetServer _server;
    private bool _running;
    private GameState _state;

    private Action<NetIncomingMessage>[] _packetResponse;

    public GameState GameState
    {
        get => _state;
        set
        {
            _state = value;
            BroadcastPacket(Packet.GameState, null, (int)_state);
        }
    }

    public void Awake()
    {
        Instance = this;
        if (Server)
        {
            _state = GameState.ARDetection;
            SetPacketResponses();
            var config = new NetPeerConfiguration("agi2");
            config.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
            config.MaximumConnections = 10;
            config.Port = 54677;
            _server = new NetServer(config);
            _running = false;
            Run();
        }
    }
    private void SetPacketResponses()
    {
        _packetResponse = new Action<NetIncomingMessage>[Packet.Last.ToByte() + 1];
        _packetResponse[Packet.Instantiate.ToByte()] = (msg) =>
        {
            var prefabId = msg.ReadInt32();
            var uid = msg.ReadInt64(); // Unique id for GameObject
            var pos = msg.ReadVector3();
            var rot = msg.ReadQuaternion();
            var scale = msg.ReadVector3();
            BroadcastPacket(Packet.Instantiate, msg.SenderConnection, prefabId, uid, pos, rot, scale);
        };
        _packetResponse[Packet.TransformUpdate.ToByte()] = (msg) =>
        {
            var uid = msg.ReadInt64(); // Unique id for GameObject
            var pos = msg.ReadVector3();
            var rot = msg.ReadQuaternion();
            BroadcastPacket(Packet.TransformUpdate, msg.SenderConnection, uid, pos, rot);
        };
        _packetResponse[Packet.ReadyAR.ToByte()] = (msg) =>
        {
            msg.SenderConnection.Tag = true;
            if (_state == GameState.ARDetection && _server.Connections.TrueForAll((con) => (bool)con.Tag))
            {
                Debug.Log("GameState = Game");
                // Everyone recognized AR Tag
                GameState = GameState.Game;
            }
        };
        _packetResponse[Packet.SyncVarUpdate.ToByte()] = (msg) =>
        {
            var uid = msg.ReadInt64();
            var varUid = msg.ReadInt32();
            var size = msg.ReadInt32();
            var bytes = msg.ReadBytes(size);
            BroadcastPacket(Packet.SyncVarUpdate, msg.SenderConnection, uid, varUid, bytes);
        };
        _packetResponse[Packet.Destroy.ToByte()] = (msg) =>
        {
            var uid = msg.ReadInt64();
            BroadcastPacket(Packet.Destroy, msg.SenderConnection, uid);
        };
        _packetResponse[Packet.Explode.ToByte()] = (msg) =>
        {
            var name = msg.ReadString();
            BroadcastPacket(Packet.Explode, msg.SenderConnection, name);
        };
    }

    public void Run()
    {
        _server.Start();
        _running = true;
        Task.Run(Listen);
    }

    private void Listen()
    {
        Debug.Log("Listening");
        while (_running)
        {
            NetIncomingMessage msg;
            while ((msg = _server.ReadMessage()) != null)
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.DiscoveryRequest:
                        // Simply answer the discovery
                        // TODO: Maybe do not answer while game is running? Set it somehow?
                        Debug.Log("Discovered");
                        _server.SendDiscoveryResponse(null, msg.SenderEndPoint);
                        break;
                    case NetIncomingMessageType.VerboseDebugMessage:
                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.WarningMessage:
                    case NetIncomingMessageType.ErrorMessage:
                        //
                        // Just print diagnostic messages to console
                        //
                        Debug.LogError(msg.ReadString());
                        break;
                    case NetIncomingMessageType.StatusChanged:
                        NetConnectionStatus status = (NetConnectionStatus) msg.ReadByte();
                        if (status == NetConnectionStatus.Connected)
                        {
                            msg.SenderConnection.Tag = false;
                            // New player connected
                        }

                        break;
                    case NetIncomingMessageType.Data:
                        var type = msg.ReadByte();
                        if (type < Packet.Last.ToByte())
                        {
                            _packetResponse[type](msg);
                        }
                        else
                        {
                            Debug.LogError($"Received unknown packet {type}");
                        }
                        break;
                }
            }
            Thread.Sleep(1);
        }
    }

    private NetOutgoingMessage WritePacket(Packet type, params object[] args)
    {
        NetOutgoingMessage om = _server.CreateMessage();
        om.Write(type.ToByte());
        // Write all other types
        foreach (dynamic arg in args)
        {
            om.Write(arg);
        }
        return om;
    }

    private void BroadcastPacket(Packet type, NetConnection except, params object[] args)
    {
        var om = WritePacket(type, args);
        _server.SendToAll(om, except, NetDeliveryMethod.ReliableOrdered, 0);
    }

    private void SendPacket(Packet type, NetConnection to, params object[] args)
    {
        var om = WritePacket(type, args);
        _server.SendMessage(om, to, NetDeliveryMethod.ReliableOrdered, 0);
    }

}
