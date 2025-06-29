using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Architect.Content.Elements.Custom.Behaviour;
using Architect.MultiplayerHook.Packets;
using Architect.Objects;
using Architect.Util;
using Hkmp.Api.Client;
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

        var netReceiver = clientApi.NetClient.GetNetworkReceiver<PacketId>(this, HkmpHook.InstantiatePacket);

        netReceiver.RegisterPacketHandler<RefreshPacketData>(PacketId.Refresh, packet =>
        {
            if (packet.Guid != _currentPacketGroup)
            {
                Architect.GlobalSettings.Edits[packet.SceneName].Clear();
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
                Architect.GlobalSettings.Edits[packet.SceneName] =
                    JsonConvert.DeserializeObject<List<ObjectPlacement>>(json);

                if (packet.SceneName == GameManager.instance.sceneName)
                {
                    PlacementManager.InvalidateCache();
                    EditorManager.ScheduleReloadScene();
                }
            }
        });

        netReceiver.RegisterPacketHandler<WinPacketData>(PacketId.Win,
            packet => { ZoteTrophy.WinScreen(packet.WinnerName); });

        netReceiver.RegisterPacketHandler<EditPacketData>(PacketId.Edit, packet =>
        {
            if (!Architect.GlobalSettings.CollaborationMode) return;
            Architect.GlobalSettings.Edits[packet.SceneName].Add(packet.Edit);
            if (packet.SceneName == GameManager.instance.sceneName && EditorManager.IsEditing)
            {
                packet.Edit.PlaceGhost();
            }
        });

        netReceiver.RegisterPacketHandler<ErasePacketData>(PacketId.Erase, packet =>
        {
            if (!Architect.GlobalSettings.CollaborationMode) return;

            if (packet.SceneName == GameManager.instance.sceneName && EditorManager.IsEditing)
            {
                var objects = Architect.GlobalSettings.Edits[packet.SceneName]
                    .Where(obj => obj.GetId() == packet.Id).ToArray();
                
                foreach (var obj in objects) obj.Destroy();
            }
            else Architect.GlobalSettings.Edits[packet.SceneName].RemoveAll(obj => obj.GetId() == packet.Id);
        });
    }

    public void Place(ObjectPlacement placement, string scene)
    {
        _api.NetClient.GetNetworkSender<PacketId>(this)
            .SendSingleData(PacketId.Edit, new EditPacketData
            {
                SceneName = scene,
                Edit = placement
            });
    }

    public void Erase(string guid, string scene)
    {
        _api.NetClient.GetNetworkSender<PacketId>(this)
            .SendSingleData(PacketId.Erase, new ErasePacketData
            {
                SceneName = scene,
                Id = guid
            });
    }

    public async void Refresh()
    {
        try
        {
            if (!_api.NetClient.IsConnected) return;
        
            var scene = GameManager.instance.sceneName;
            var json = JsonConvert.SerializeObject(Architect.GlobalSettings.Edits[scene], 
                ObjectPlacement.ObjectPlacementConverter.Instance, 
                ObjectPlacement.ObjectPlacementConverter.Vector3Converter);
        
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

    protected override string Name => "Architect";
    protected override string Version => "1.4.0.1";
    public override bool NeedsNetwork => true;
}