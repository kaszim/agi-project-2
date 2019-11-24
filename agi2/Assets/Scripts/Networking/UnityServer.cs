using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Assets.Scripts.Networking;
using Lidgren.Network;
using UnityEngine;
using UnityEngine.Networking;

public class UnityServer : MonoBehaviour
{
    public bool Server = false;
    private NetServer _server;
    private bool _running;

    private Action<NetIncomingMessage>[] _packetResponse;

    public void Awake()
    {
        if (Server)
        {
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
            SendPacket(Packet.Instantiate, msg.SenderConnection, prefabId, uid, pos, rot, scale);
        };
        _packetResponse[Packet.TransformUpdate.ToByte()] = (msg) =>
        {
            var uid = msg.ReadInt64(); // Unique id for GameObject
            var pos = msg.ReadVector3();
            var rot = msg.ReadQuaternion();
            SendPacket(Packet.TransformUpdate, msg.SenderConnection, uid, pos, rot);
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

    private void SendPacket(Packet type, NetConnection except, params object[] args)
    {
        NetOutgoingMessage om = _server.CreateMessage();
        om.Write(type.ToByte());
        // Write all other types
        foreach (dynamic arg in args)
        {
            om.Write(arg);
        }
        _server.SendToAll(om, except, NetDeliveryMethod.ReliableOrdered, 0);
    }


}
