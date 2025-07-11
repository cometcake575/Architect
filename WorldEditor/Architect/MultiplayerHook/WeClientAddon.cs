using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Architect.Configuration;
using Architect.Content.Elements.Custom.Behaviour;
using Architect.MultiplayerHook.Packets;
using Architect.Objects;
using Architect.Util;
using Hkmp.Api.Client;
using Hkmp.Networking.Packet;
using Newtonsoft.Json;
using UnityEngine;

namespace Architect.MultiplayerHook;

public class WeClientAddon : ClientAddon
{
    private const int SplitSize = 600;
    
    private IClientApi _api;

    private string _currentPacketGroup;
    private int _totalPacketCount;
    private readonly Dictionary<int, byte[]> _packets = new();

    public override void Initialize(IClientApi clientApi)
    {
        Logger.Info("Initializing client-side Architect addon!");
        _api = clientApi;

        var netReceiver = clientApi.NetClient.GetNetworkReceiver<PacketId>(this, InstantiatePacket);

        netReceiver.RegisterPacketHandler<RefreshPacketData>(PacketId.Refresh, packet =>
        {
            Architect.Instance.Log("Receiving Refresh Packet [CLIENT]");
            if (packet.Guid != _currentPacketGroup)
            {
                SceneSaveLoader.WipeScene(packet.SceneName);
                _packets.Clear();
                _totalPacketCount = packet.TotalPackets;
                _currentPacketGroup = packet.Guid;
            }

            _packets[packet.PacketId] = packet.Edits;

            if (_packets.Count == _totalPacketCount)
            {
                var bytes = new byte[SplitSize * (_packets.Count - 1) + _packets[_packets.Count - 1].Length];

                foreach (var data in _packets)
                {
                    data.Value.CopyTo(bytes, data.Key * SplitSize);
                }

                var json = ZipUtils.Unzip(bytes);
                
                SceneSaveLoader.Save(packet.SceneName, json);

                if (packet.SceneName == GameManager.instance.sceneName)
                {
                    PlacementManager.InvalidateCache();
                    EditorManager.ScheduleReloadScene();
                }
            }
        });

        netReceiver.RegisterPacketHandler<WinPacketData>(PacketId.Win,
            packet =>
            {
                Logger.Info("Receiving Edit Packet [CLIENT]");
                ZoteTrophy.WinScreen(packet.WinnerName);
            });

        netReceiver.RegisterPacketHandler<EditPacketData>(PacketId.Edit, packet =>
        {
            if (!Architect.GlobalSettings.CollaborationMode) return;
            Logger.Info("Receiving Edit Packet [CLIENT]");

            var json = ZipUtils.Unzip(packet.Edits);
            var edit = JsonConvert.DeserializeObject<ObjectPlacement>(json);
            
            if (packet.SceneName == GameManager.instance.sceneName)
            {
                PlacementManager.GetCurrentPlacements().Add(edit);
                if (EditorManager.IsEditing) edit.PlaceGhost();
            }
            else
            {
                SceneSaveLoader.ScheduleEdit(packet.SceneName, edit); // This will store it in a list to be added when the scene is next loaded, or saved when the game is next closed
            }
        });

        netReceiver.RegisterPacketHandler<ErasePacketData>(PacketId.Erase, packet =>
        {
            if (!Architect.GlobalSettings.CollaborationMode) return;
            Logger.Info("Receiving Erase Packet [CLIENT]");

            if (packet.SceneName == GameManager.instance.sceneName)
            {
                if (EditorManager.IsEditing) {
                    var objects = PlacementManager.GetCurrentPlacements()
                        .Where(obj => obj.GetId() == packet.Id).ToArray();

                    foreach (var obj in objects) obj.Destroy();
                }
                else
                {
                    PlacementManager.GetCurrentPlacements().RemoveAll(obj => obj.GetId() == packet.Id);
                }
            }
            else
            {
                SceneSaveLoader.ScheduleErase(packet.SceneName, packet.Id); // This will store it in a list to be added when the scene is next loaded, or saved when the game is next closed
            }
        });

        netReceiver.RegisterPacketHandler<UpdatePacketData>(PacketId.Update, packet =>
        {
            if (!Architect.GlobalSettings.CollaborationMode) return;
            Logger.Info("Receiving Update Packet [CLIENT]");

            if (packet.SceneName == GameManager.instance.sceneName)
            {
                var objects = PlacementManager.GetCurrentPlacements()
                    .Where(obj => obj.GetId() == packet.Id).ToArray();

                foreach (var obj in objects) obj.Move(new Vector3(packet.X, packet.Y, packet.Z));
            }
            else
            {
                SceneSaveLoader.ScheduleUpdate(packet.SceneName, packet.Id, new Vector3(packet.X, packet.Y, packet.Z)); // This will store it in a list to be added when the scene is next loaded, or saved when the game is next closed
            }
        });
    }

    public void Place(ObjectPlacement placement, string scene)
    {
        if (!_api.NetClient.IsConnected) return;

        Logger.Info("Sending Place Packet");
        
        var json = JsonConvert.SerializeObject(placement,
            SceneSaveLoader.Opc,
            SceneSaveLoader.V3C);
        var bytes = ZipUtils.Zip(json);

        var packet = new EditPacketData
        {
            Edits = bytes,
            SceneName = scene
        };

        _api.NetClient.GetNetworkSender<PacketId>(this)
            .SendSingleData(PacketId.Edit, packet);
    }

    public void Erase(string guid, string scene)
    {
        if (!_api.NetClient.IsConnected) return;
        
        Logger.Info("Sending Erase Packet");
        
        _api.NetClient.GetNetworkSender<PacketId>(this)
            .SendSingleData(PacketId.Erase, new ErasePacketData
            {
                SceneName = scene,
                Id = guid
            });
    }

    public void Update(string guid, string scene, Vector3 pos)
    {
        if (!_api.NetClient.IsConnected) return;
        
        Logger.Info("Sending Update Packet");
        
        _api.NetClient.GetNetworkSender<PacketId>(this)
            .SendSingleData(PacketId.Update, new UpdatePacketData
            {
                SceneName = scene,
                Id = guid,
                X = pos.x,
                Y = pos.y,
                Z = pos.z
            });
    }

    public async void Refresh()
    {
        try
        {
            if (!_api.NetClient.IsConnected) return;
        
            Logger.Info("Sending Refresh Packets");
        
            var scene = GameManager.instance.sceneName;
            var json = SceneSaveLoader.SerializeSceneData(PlacementManager.GetCurrentPlacements());
        
            var bytes = Split(ZipUtils.Zip(json), SplitSize);
        
            var count = bytes.Length;
            var i = 0;
            var guid = Guid.NewGuid().ToString();
        
            foreach (var byteGroup in bytes)
            {
                _api.NetClient.GetNetworkSender<PacketId>(this)
                    .SendSingleData(PacketId.Refresh, new RefreshPacketData
                    {
                        Edits = byteGroup,
                        Guid = guid,
                        TotalPackets = count,
                        PacketId = i,
                        SceneName = scene
                    });
                i++;
                await Task.Delay(100);
            }
        }
        catch (Exception e)
        {
            Architect.Instance.LogError(e);
        }
    }
    
    public static byte[][] Split(byte[] array, int size)
    {
        var count = Mathf.CeilToInt((float) array.Length / size);
        var bytes = new byte[count][];
        
        for (var i = 0; i < count; i++)
        {
            bytes[i] = array.Skip(i * size).Take(size).ToArray();
        }

        return bytes;
    }

    public void BroadcastWin()
    {
        if (!_api.NetClient.IsConnected) return;
        
        _api.NetClient.GetNetworkSender<PacketId>(this)
            .SendSingleData(PacketId.Win, new WinPacketData
            {
                WinnerName = _api.ClientManager.Username
            });
    }
    
    public IPacketData InstantiatePacket(PacketId packetId)
    {
        return packetId switch
        {
            PacketId.Refresh => new RefreshPacketData(),
            PacketId.Win => new WinPacketData(),
            PacketId.Edit => new EditPacketData(),
            PacketId.Erase => new ErasePacketData(),
            PacketId.Update => new UpdatePacketData(),
            _ => null
        };
    }

    protected override string Name => "Architect";
    protected override string Version => "1.8.5.4";
    public override bool NeedsNetwork => true;
}